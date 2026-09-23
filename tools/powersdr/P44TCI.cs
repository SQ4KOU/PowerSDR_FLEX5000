using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PowerSDR
{
    internal static class P44TCISettings
    {
        internal static bool Enabled = true;
        internal static int Port = 50001;
        internal static bool BindAll = true;
        internal static int PollMs = 100;
        private static bool loaded;

        internal static void Load()
        {
            if (loaded) return;
            loaded = true;

            ArrayList stored = DB.GetVars("SQ4KOU_TCI");
            if (stored == null) return;

            foreach (object o in stored)
            {
                string s = o as string;
                if (String.IsNullOrEmpty(s)) continue;
                int slash = s.IndexOf('/');
                if (slash <= 0 || slash >= s.Length - 1) continue;

                string key = s.Substring(0, slash);
                string value = s.Substring(slash + 1);

                bool b;
                int n;
                switch (key)
                {
                    case "Enabled":
                        if (Boolean.TryParse(value, out b)) Enabled = b;
                        break;
                    case "Port":
                        if (Int32.TryParse(value, out n) && n >= 1024 && n <= 65535) Port = n;
                        break;
                    case "BindAll":
                        if (Boolean.TryParse(value, out b)) BindAll = b;
                        break;
                    case "PollMs":
                        if (Int32.TryParse(value, out n) && n >= 25 && n <= 1000) PollMs = n;
                        break;
                }
            }
        }

        internal static void Save()
        {
            ArrayList a = new ArrayList();
            a.Add("Enabled/" + Enabled.ToString());
            a.Add("Port/" + Port.ToString(CultureInfo.InvariantCulture));
            a.Add("BindAll/" + BindAll.ToString());
            a.Add("PollMs/" + PollMs.ToString(CultureInfo.InvariantCulture));
            DB.SaveVars("SQ4KOU_TCI", ref a);
        }
    }

    internal static class P44TCILog
    {
        private static readonly object gate = new object();

        internal static void Write(string text)
        {
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PowerSDR-SQ4KOU");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "P44_TCI.log");
                lock (gate)
                {
                    File.AppendAllText(path,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                        " " + text + Environment.NewLine);
                }
            }
            catch { }
        }
    }

    internal sealed class P44TCISnapshot
    {
        internal bool PowerOn;
        internal bool Focus;
        internal long MaxHz;
        internal int SampleRate;
        internal long VfoAHz;
        internal long VfoBHz;
        internal long TxHz;
        internal string Mode;
        internal int FilterLow;
        internal int FilterHigh;
        internal string AgcMode;
        internal int AgcGain;
        internal bool Split;
        internal bool Rit;
        internal bool Xit;
        internal int RitOffset;
        internal int XitOffset;
        internal bool NR;
        internal bool NB;
        internal bool ANF;
        internal bool SqlEnabled;
        internal int SqlLevel;
        internal int StepAtt;
        internal int PreampAtt;
        internal int Drive;
        internal int TuneDrive;
        internal bool Mox;
        internal bool Tune;
        internal bool Mute;
        internal bool Mon;
        internal int Volume;
        internal int MonVolume;
        internal bool VfoALock;
        internal bool VfoBLock;
        internal bool VfoSync;
    }

    internal sealed class P44WSFrame
    {
        internal int Opcode;
        internal byte[] Payload;
    }

    internal sealed class P44TCIClient
    {
        internal readonly TcpClient Tcp;
        internal readonly NetworkStream Stream;
        internal Thread Thread;
        private readonly object sendGate = new object();

        internal P44TCIClient(TcpClient tcp)
        {
            Tcp = tcp;
            Stream = tcp.GetStream();
        }

        internal void SendFrame(int opcode, byte[] payload)
        {
            if (payload == null) payload = new byte[0];
            lock (sendGate)
            {
                if (!Tcp.Connected) return;

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteByte((byte)(0x80 | (opcode & 0x0F)));
                    ulong length = (ulong)payload.Length;
                    if (length <= 125)
                    {
                        ms.WriteByte((byte)length);
                    }
                    else if (length <= UInt16.MaxValue)
                    {
                        ms.WriteByte(126);
                        ms.WriteByte((byte)((length >> 8) & 0xFF));
                        ms.WriteByte((byte)(length & 0xFF));
                    }
                    else
                    {
                        ms.WriteByte(127);
                        for (int i = 7; i >= 0; i--)
                            ms.WriteByte((byte)((length >> (8 * i)) & 0xFF));
                    }

                    if (payload.Length > 0)
                        ms.Write(payload, 0, payload.Length);

                    byte[] frame = ms.ToArray();
                    Stream.Write(frame, 0, frame.Length);
                    Stream.Flush();
                }
            }
        }

        internal void SendText(string text)
        {
            SendFrame(1, Encoding.UTF8.GetBytes(text ?? String.Empty));
        }

        internal void Close()
        {
            try { Tcp.Close(); } catch { }
        }
    }

    internal sealed class P44TCIServer : IDisposable
    {
        private readonly Console console;
        private readonly object clientsGate = new object();
        private readonly List<P44TCIClient> clients = new List<P44TCIClient>();
        private TcpListener listener;
        private Thread acceptThread;
        private Thread publishThread;
        private volatile bool running;
        private P44TCISnapshot lastSnapshot;

        internal P44TCIServer(Console c)
        {
            console = c;
        }

        internal bool Running { get { return running; } }

        internal int ClientCount
        {
            get
            {
                lock (clientsGate) return clients.Count;
            }
        }

        internal void Start(IPAddress address, int port)
        {
            if (running) return;

            listener = new TcpListener(address, port);
            listener.Start();
            running = true;

            acceptThread = new Thread(AcceptLoop);
            acceptThread.IsBackground = true;
            acceptThread.Name = "P44 TCI Accept";
            acceptThread.Start();

            publishThread = new Thread(PublishLoop);
            publishThread.IsBackground = true;
            publishThread.Name = "P44 TCI State";
            publishThread.Start();

            P44TCILog.Write("START " + address + ":" + port.ToString(CultureInfo.InvariantCulture));
        }

        internal void Stop()
        {
            running = false;
            try { if (listener != null) listener.Stop(); } catch { }

            P44TCIClient[] copy;
            lock (clientsGate)
            {
                copy = clients.ToArray();
                clients.Clear();
            }
            foreach (P44TCIClient c in copy) c.Close();

            P44TCILog.Write("STOP");
        }

        public void Dispose()
        {
            Stop();
        }

        private void AcceptLoop()
        {
            while (running)
            {
                try
                {
                    TcpClient tcp = listener.AcceptTcpClient();
                    tcp.NoDelay = true;
                    P44TCIClient client = new P44TCIClient(tcp);
                    client.Thread = new Thread(delegate() { ClientLoop(client); });
                    client.Thread.IsBackground = true;
                    client.Thread.Name = "P44 TCI Client";
                    client.Thread.Start();
                }
                catch (SocketException)
                {
                    if (!running) break;
                }
                catch (Exception ex)
                {
                    P44TCILog.Write("ACCEPT ERROR " + ex.Message);
                    if (!running) break;
                    Thread.Sleep(100);
                }
            }
        }

        private void ClientLoop(P44TCIClient client)
        {
            bool added = false;
            try
            {
                if (!Handshake(client)) return;

                lock (clientsGate)
                {
                    clients.Add(client);
                    added = true;
                }

                P44TCILog.Write("CLIENT CONNECT " + ClientEndpoint(client));
                SendInitial(client);

                while (running && client.Tcp.Connected)
                {
                    P44WSFrame frame = ReadFrame(client.Stream);
                    if (frame == null) break;

                    if (frame.Opcode == 8)
                    {
                        try { client.SendFrame(8, frame.Payload); } catch { }
                        break;
                    }
                    if (frame.Opcode == 9)
                    {
                        client.SendFrame(10, frame.Payload);
                        continue;
                    }
                    if (frame.Opcode != 1) continue;

                    string text = Encoding.UTF8.GetString(frame.Payload ?? new byte[0]);
                    HandleText(client, text);
                }
            }
            catch (Exception ex)
            {
                if (running) P44TCILog.Write("CLIENT ERROR " + ex.Message);
            }
            finally
            {
                if (added)
                {
                    lock (clientsGate) clients.Remove(client);
                }
                P44TCILog.Write("CLIENT DISCONNECT " + ClientEndpoint(client));
                client.Close();
            }
        }

        private static string ClientEndpoint(P44TCIClient client)
        {
            try { return client.Tcp.Client.RemoteEndPoint.ToString(); }
            catch { return "?"; }
        }

        private bool Handshake(P44TCIClient client)
        {
            MemoryStream header = new MemoryStream();
            int matched = 0;
            while (header.Length < 16384)
            {
                int b = client.Stream.ReadByte();
                if (b < 0) return false;
                header.WriteByte((byte)b);

                if ((matched == 0 && b == 13) ||
                    (matched == 1 && b == 10) ||
                    (matched == 2 && b == 13) ||
                    (matched == 3 && b == 10))
                {
                    matched++;
                    if (matched == 4) break;
                }
                else
                {
                    matched = b == 13 ? 1 : 0;
                }
            }

            string request = Encoding.ASCII.GetString(header.ToArray());
            string key = HeaderValue(request, "Sec-WebSocket-Key");
            if (String.IsNullOrEmpty(key)) return false;

            string accept;
            using (SHA1 sha = SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(
                    key.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
                accept = Convert.ToBase64String(hash);
            }

            string response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Accept: " + accept + "\r\n\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            client.Stream.Write(bytes, 0, bytes.Length);
            client.Stream.Flush();
            return true;
        }

        private static string HeaderValue(string request, string name)
        {
            string[] lines = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                int colon = line.IndexOf(':');
                if (colon <= 0) continue;
                if (line.Substring(0, colon).Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return line.Substring(colon + 1).Trim();
            }
            return null;
        }

        private static byte[] ReadExact(Stream stream, int count)
        {
            byte[] data = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int n = stream.Read(data, offset, count - offset);
                if (n <= 0) return null;
                offset += n;
            }
            return data;
        }

        private static P44WSFrame ReadFrame(Stream stream)
        {
            byte[] head = ReadExact(stream, 2);
            if (head == null) return null;

            int opcode = head[0] & 0x0F;
            bool masked = (head[1] & 0x80) != 0;
            ulong length = (ulong)(head[1] & 0x7F);

            if (length == 126)
            {
                byte[] ext = ReadExact(stream, 2);
                if (ext == null) return null;
                length = (ulong)((ext[0] << 8) | ext[1]);
            }
            else if (length == 127)
            {
                byte[] ext = ReadExact(stream, 8);
                if (ext == null) return null;
                length = 0;
                for (int i = 0; i < 8; i++) length = (length << 8) | ext[i];
            }

            if (length > 1024 * 1024) throw new InvalidDataException("TCI frame too large");

            byte[] mask = masked ? ReadExact(stream, 4) : null;
            if (masked && mask == null) return null;

            byte[] payload = ReadExact(stream, (int)length);
            if (payload == null) return null;

            if (masked)
            {
                for (int i = 0; i < payload.Length; i++)
                    payload[i] = (byte)(payload[i] ^ mask[i & 3]);
            }

            P44WSFrame frame = new P44WSFrame();
            frame.Opcode = opcode;
            frame.Payload = payload;
            return frame;
        }

        private void PublishLoop()
        {
            while (running)
            {
                try
                {
                    P44TCISnapshot now = console.P44ReadTCIState();
                    P44TCISnapshot before = lastSnapshot;
                    lastSnapshot = now;

                    if (before != null)
                    {
                        List<string> changed = BuildMessages(now, before, false);
                        foreach (string message in changed) Broadcast(message);
                    }
                }
                catch (Exception ex)
                {
                    if (running) P44TCILog.Write("STATE ERROR " + ex.Message);
                }

                Thread.Sleep(Math.Max(25, P44TCISettings.PollMs));
            }
        }

        private void Broadcast(string text)
        {
            P44TCIClient[] copy;
            lock (clientsGate) copy = clients.ToArray();
            foreach (P44TCIClient client in copy)
            {
                try { client.SendText(text); }
                catch
                {
                    lock (clientsGate) clients.Remove(client);
                    client.Close();
                }
            }
        }

        private void SendInitial(P44TCIClient client)
        {
            P44TCISnapshot s = console.P44ReadTCIState();

            client.SendText("protocol:ExpertSDR3,2.0;");
            client.SendText("device:FLEX-5000;");
            client.SendText("receive_only:false;");
            client.SendText("trx_count:1;");
            client.SendText("channels_count:2;");
            client.SendText("vfo_limits:0," + s.MaxHz.ToString(CultureInfo.InvariantCulture) + ";");
            client.SendText("if_limits:" + (-s.SampleRate / 2).ToString(CultureInfo.InvariantCulture) + "," +
                            (s.SampleRate / 2).ToString(CultureInfo.InvariantCulture) + ";");
            client.SendText("modulations_list:AM,SAM,DSB,LSB,USB,NFM,FM,DIGL,DIGU,CWL,CWU;");
            client.SendText("iq_samplerate:" + s.SampleRate.ToString(CultureInfo.InvariantCulture) + ";");
            client.SendText("audio_samplerate:48000;");

            List<string> all = BuildMessages(s, null, true);
            foreach (string message in all) client.SendText(message);
            client.SendText("ready;");
        }

        private static string B(bool value)
        {
            return value ? "true" : "false";
        }

        private static double VolumeToDb(int volume)
        {
            double v = Math.Max(0, Math.Min(100, volume));
            return -60.0 + v * 0.6;
        }

        private static int DbToVolume(double db)
        {
            db = Math.Max(-60.0, Math.Min(0.0, db));
            return (int)Math.Round((db + 60.0) / 0.6, MidpointRounding.AwayFromZero);
        }

        private static List<string> BuildMessages(P44TCISnapshot s, P44TCISnapshot old, bool all)
        {
            List<string> m = new List<string>();

            if (all || old.PowerOn != s.PowerOn) m.Add(s.PowerOn ? "start;" : "stop;");
            if (all || old.Focus != s.Focus) m.Add("app_focus:" + B(s.Focus) + ";");
            if (all || old.VfoAHz != s.VfoAHz) m.Add("vfo:0,0," + s.VfoAHz.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.VfoBHz != s.VfoBHz) m.Add("vfo:0,1," + s.VfoBHz.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.TxHz != s.TxHz) m.Add("tx_frequency:" + s.TxHz.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.Mode != s.Mode) m.Add("modulation:0," + s.Mode + ";");
            if (all || old.FilterLow != s.FilterLow || old.FilterHigh != s.FilterHigh)
                m.Add("rx_filter_band:0," + s.FilterLow.ToString(CultureInfo.InvariantCulture) + "," +
                      s.FilterHigh.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.AgcMode != s.AgcMode) m.Add("agc_mode:0," + s.AgcMode + ";");
            if (all || old.AgcGain != s.AgcGain) m.Add("agc_gain:0," + s.AgcGain.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.Split != s.Split) m.Add("split_enable:0," + B(s.Split) + ";");
            if (all || old.Rit != s.Rit) m.Add("rit_enable:0," + B(s.Rit) + ";");
            if (all || old.Xit != s.Xit) m.Add("xit_enable:0," + B(s.Xit) + ";");
            if (all || old.RitOffset != s.RitOffset) m.Add("rit_offset:0," + s.RitOffset.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.XitOffset != s.XitOffset) m.Add("xit_offset:0," + s.XitOffset.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.NR != s.NR) m.Add("rx_nr_enable:0," + B(s.NR) + ";");
            if (all || old.NB != s.NB) m.Add("rx_nb_enable:0," + B(s.NB) + ";");
            if (all || old.ANF != s.ANF) m.Add("rx_anf_enable:0," + B(s.ANF) + ";");
            if (all) m.Add("rx_nf_enable:0,false;");
            if (all || old.SqlEnabled != s.SqlEnabled) m.Add("sql_enable:0," + B(s.SqlEnabled) + ";");
            if (all || old.SqlLevel != s.SqlLevel) m.Add("sql_level:0," + s.SqlLevel.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.StepAtt != s.StepAtt)
            {
                m.Add("rx_step_att_ex:0," + s.StepAtt.ToString(CultureInfo.InvariantCulture) + ";");
                m.Add("rx_step_att:0," + s.StepAtt.ToString(CultureInfo.InvariantCulture) + ";");
            }
            if (all || old.PreampAtt != s.PreampAtt)
            {
                m.Add("rx_preamp_att_ex:0," + s.PreampAtt.ToString(CultureInfo.InvariantCulture) + ";");
                m.Add("rx_preamp_att:0," + s.PreampAtt.ToString(CultureInfo.InvariantCulture) + ";");
            }
            if (all || old.Drive != s.Drive) m.Add("drive:0," + s.Drive.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.TuneDrive != s.TuneDrive) m.Add("tune_drive:0," + s.TuneDrive.ToString(CultureInfo.InvariantCulture) + ";");
            if (all || old.Mox != s.Mox) m.Add("trx:0," + B(s.Mox) + ";");
            if (all || old.Tune != s.Tune) m.Add("tune:0," + B(s.Tune) + ";");
            if (all || old.Mute != s.Mute) m.Add("mute:" + B(s.Mute) + ";");
            if (all || old.Mon != s.Mon) m.Add("mon_enable:" + B(s.Mon) + ";");
            if (all || old.Volume != s.Volume) m.Add("volume:" + VolumeToDb(s.Volume).ToString("0.0", CultureInfo.InvariantCulture) + ";");
            if (all || old.MonVolume != s.MonVolume) m.Add("mon_volume:" + VolumeToDb(s.MonVolume).ToString("0.0", CultureInfo.InvariantCulture) + ";");
            if (all || old.VfoALock != s.VfoALock) m.Add("vfo_lock:0,0," + B(s.VfoALock) + ";");
            if (all || old.VfoBLock != s.VfoBLock) m.Add("vfo_lock:0,1," + B(s.VfoBLock) + ";");
            if (all || old.VfoSync != s.VfoSync) m.Add("vfo_sync_ex:" + B(s.VfoSync) + ";");

            return m;
        }

        private void HandleText(P44TCIClient client, string text)
        {
            if (String.IsNullOrWhiteSpace(text)) return;
            string[] commands = text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string raw in commands)
            {
                string command = raw.Trim();
                if (command.Length == 0) continue;

                int colon = command.IndexOf(':');
                string name = (colon >= 0 ? command.Substring(0, colon) : command).Trim().ToLowerInvariant();
                string argText = colon >= 0 ? command.Substring(colon + 1).Trim() : String.Empty;
                string[] args = argText.Length == 0 ? new string[0] : argText.Split(',');

                try { HandleCommand(client, name, args); }
                catch (Exception ex) { P44TCILog.Write("CMD " + name + " ERROR " + ex.Message); }
            }
        }

        private static bool TryBool(string s, out bool value)
        {
            if (Boolean.TryParse(s, out value)) return true;
            if (s == "1") { value = true; return true; }
            if (s == "0") { value = false; return true; }
            return false;
        }

        private static bool Rx0(string[] args)
        {
            int rx;
            return args.Length > 0 && Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out rx) && rx == 0;
        }

        private void SendCurrent(P44TCIClient client, string name)
        {
            P44TCISnapshot s = console.P44ReadTCIState();
            List<string> all = BuildMessages(s, null, true);
            foreach (string message in all)
            {
                if (message.StartsWith(name + ":", StringComparison.OrdinalIgnoreCase) ||
                    (name == "startstop" && (message == "start;" || message == "stop;")))
                    client.SendText(message);
            }
        }

        private void HandleCommand(P44TCIClient client, string name, string[] args)
        {
            long hz;
            int n;
            bool b;
            double d;

            switch (name)
            {
                case "start":
                    console.P44SetPower(true);
                    return;
                case "stop":
                    console.P44SetPower(false);
                    return;
                case "set_in_focus":
                    console.P44SetFocus();
                    return;
                case "vfo":
                    if (!Rx0(args) || args.Length < 2) return;
                    int chan;
                    if (!Int32.TryParse(args[1], out chan) || chan < 0 || chan > 1) return;
                    if (args.Length >= 3 && Int64.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out hz))
                        console.P44SetVFO(chan, hz);
                    else
                        client.SendText("vfo:0," + chan.ToString(CultureInfo.InvariantCulture) + "," +
                            (chan == 0 ? console.P44ReadTCIState().VfoAHz : console.P44ReadTCIState().VfoBHz).ToString(CultureInfo.InvariantCulture) + ";");
                    return;
                case "tx_frequency":
                    if (args.Length >= 1 && Int64.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out hz))
                        console.P44SetTXFrequency(hz);
                    else
                        SendCurrent(client, "tx_frequency");
                    return;
                case "rx_filter_band":
                    if (!Rx0(args)) return;
                    if (args.Length >= 3 &&
                        Int32.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                    {
                        int high;
                        if (Int32.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out high))
                            console.P44SetFilter(n, high);
                    }
                    else SendCurrent(client, "rx_filter_band");
                    return;
                case "modulation":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2) console.P44SetMode(args[1]);
                    else SendCurrent(client, "modulation");
                    return;
                case "agc_mode":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2) console.P44SetAgcMode(args[1]);
                    else SendCurrent(client, "agc_mode");
                    return;
                case "agc_gain":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                        console.P44SetAgcGain(n);
                    else SendCurrent(client, "agc_gain");
                    return;
                case "split_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetSplit(b);
                    else SendCurrent(client, "split_enable");
                    return;
                case "rit_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetRit(b);
                    else SendCurrent(client, "rit_enable");
                    return;
                case "xit_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetXit(b);
                    else SendCurrent(client, "xit_enable");
                    return;
                case "rit_offset":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetRitOffset(n);
                    else SendCurrent(client, "rit_offset");
                    return;
                case "xit_offset":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetXitOffset(n);
                    else SendCurrent(client, "xit_offset");
                    return;
                case "rx_nr_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetNR(b);
                    else SendCurrent(client, "rx_nr_enable");
                    return;
                case "rx_nb_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetNB(b);
                    else SendCurrent(client, "rx_nb_enable");
                    return;
                case "rx_anf_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetANF(b);
                    else SendCurrent(client, "rx_anf_enable");
                    return;
                case "rx_nf_enable":
                    client.SendText("rx_nf_enable:0,false;");
                    return;
                case "sql_enable":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetSqlEnabled(b);
                    else SendCurrent(client, "sql_enable");
                    return;
                case "sql_level":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetSqlLevel(n);
                    else SendCurrent(client, "sql_level");
                    return;
                case "rx_step_att":
                case "rx_step_att_ex":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetStepAtt(n);
                    else SendCurrent(client, name);
                    return;
                case "rx_preamp_att":
                case "rx_preamp_att_ex":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetPreampAtt(n);
                    else SendCurrent(client, name);
                    return;
                case "drive":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetDrive(n);
                    else SendCurrent(client, "drive");
                    return;
                case "tune_drive":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && Int32.TryParse(args[1], out n)) console.P44SetTuneDrive(n);
                    else SendCurrent(client, "tune_drive");
                    return;
                case "trx":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetMox(b);
                    else SendCurrent(client, "trx");
                    return;
                case "tune":
                    if (!Rx0(args)) return;
                    if (args.Length >= 2 && TryBool(args[1], out b)) console.P44SetTune(b);
                    else SendCurrent(client, "tune");
                    return;
                case "volume":
                    if (args.Length >= 1 && Double.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                        console.P44SetVolume(DbToVolume(d));
                    else SendCurrent(client, "volume");
                    return;
                case "mute":
                    if (args.Length >= 1 && TryBool(args[0], out b)) console.P44SetMute(b);
                    else SendCurrent(client, "mute");
                    return;
                case "mon_enable":
                    if (args.Length >= 1 && TryBool(args[0], out b)) console.P44SetMon(b);
                    else SendCurrent(client, "mon_enable");
                    return;
                case "mon_volume":
                    if (args.Length >= 1 && Double.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                        console.P44SetMonVolume(DbToVolume(d));
                    else SendCurrent(client, "mon_volume");
                    return;
                case "vfo_lock":
                    if (!Rx0(args) || args.Length < 2) return;
                    if (!Int32.TryParse(args[1], out chan) || chan < 0 || chan > 1) return;
                    if (args.Length >= 3 && TryBool(args[2], out b)) console.P44SetVfoLock(chan, b);
                    else
                    {
                        P44TCISnapshot s = console.P44ReadTCIState();
                        client.SendText("vfo_lock:0," + chan.ToString(CultureInfo.InvariantCulture) + "," +
                            B(chan == 0 ? s.VfoALock : s.VfoBLock) + ";");
                    }
                    return;
                case "vfo_sync_ex":
                    if (args.Length >= 1 && TryBool(args[0], out b)) console.P44SetVfoSync(b);
                    else SendCurrent(client, "vfo_sync_ex");
                    return;
                case "iq_samplerate":
                    client.SendText("iq_samplerate:" + console.P44ReadTCIState().SampleRate.ToString(CultureInfo.InvariantCulture) + ";");
                    return;
                case "audio_samplerate":
                    client.SendText("audio_samplerate:48000;");
                    return;
            }
        }
    }

    sealed unsafe public partial class Console
    {
        private P44TCIServer p44TCIServer;

        internal void P44StartTCI()
        {
            P44TCISettings.Load();
            if (!P44TCISettings.Enabled) return;
            if (p44TCIServer != null && p44TCIServer.Running) return;

            try
            {
                if (p44TCIServer != null) p44TCIServer.Stop();
                p44TCIServer = new P44TCIServer(this);
                IPAddress address = P44TCISettings.BindAll ? IPAddress.Any : IPAddress.Loopback;
                p44TCIServer.Start(address, P44TCISettings.Port);
                FormClosed -= P44TCIFormClosed;
                FormClosed += P44TCIFormClosed;
            }
            catch (Exception ex)
            {
                P44TCILog.Write("START ERROR " + ex.ToString());
                try { if (p44TCIServer != null) p44TCIServer.Stop(); } catch { }
                p44TCIServer = null;
            }
        }

        internal void P44StopTCI()
        {
            try
            {
                if (p44TCIServer != null) p44TCIServer.Stop();
            }
            finally
            {
                p44TCIServer = null;
            }
        }

        internal void P44RestartTCI()
        {
            P44StopTCI();
            if (P44TCISettings.Enabled) P44StartTCI();
        }

        private void P44TCIFormClosed(object sender, FormClosedEventArgs e)
        {
            P44StopTCI();
        }

        internal string P44TCIStatusText
        {
            get
            {
                if (!P44TCISettings.Enabled) return "Disabled";
                if (p44TCIServer == null || !p44TCIServer.Running) return "Enabled - server not running";
                return "Listening on " + (P44TCISettings.BindAll ? "0.0.0.0" : "127.0.0.1") + ":" +
                    P44TCISettings.Port.ToString(CultureInfo.InvariantCulture) +
                    " | clients: " + p44TCIServer.ClientCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        private T P44UI<T>(Func<T> f)
        {
            if (IsDisposed) return default(T);
            if (InvokeRequired) return (T)Invoke(f);
            return f();
        }

        private void P44UI(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) Invoke(a);
            else a();
        }

        internal P44TCISnapshot P44ReadTCIState()
        {
            return P44UI<P44TCISnapshot>(delegate
            {
                P44TCISnapshot s = new P44TCISnapshot();
                s.PowerOn = PowerOn;
                s.Focus = ContainsFocus;
                s.MaxHz = (long)Math.Round(MaxFreq * 1000000.0);
                s.SampleRate = SampleRate1;
                s.VfoAHz = (long)Math.Round(VFOAFreq * 1000000.0);
                s.VfoBHz = (long)Math.Round(VFOBFreq * 1000000.0);
                s.TxHz = (long)Math.Round(TXFreq * 1000000.0);
                s.Mode = P44ModeToTCI(RX1DSPMode);
                s.FilterLow = RX1FilterLow;
                s.FilterHigh = RX1FilterHigh;
                s.AgcMode = P44AgcToTCI(RX1AGCMode);
                s.AgcGain = RF;
                s.Split = VFOSplit;
                s.Rit = RITOn;
                s.Xit = XITOn;
                s.RitOffset = RITValue;
                s.XitOffset = XITValue;
                s.NR = chkNR.Checked;
                s.NB = chkNB.Checked;
                s.ANF = chkANF.Checked;
                s.SqlEnabled = chkSquelch.Checked;
                s.SqlLevel = Squelch;

                float preampOffset = 0.0f;
                try { preampOffset = rx1_preamp_offset[(int)RX1PreampMode]; } catch { }
                s.StepAtt = preampOffset > 0.5f ? (int)Math.Round(preampOffset) : 0;
                s.PreampAtt = preampOffset < -0.5f ? (int)Math.Round(preampOffset) : 0;

                s.Drive = PWR;
                s.TuneDrive = TunePower;
                s.Mox = MOX;
                s.Tune = TUN;
                s.Mute = MUT;
                s.Mon = MON;
                s.Volume = AF;
                s.MonVolume = TXAF;
                s.VfoALock = P32VFOALock;
                s.VfoBLock = P32VFOBLock;
                s.VfoSync = VFOSync;
                return s;
            });
        }

        private static string P44ModeToTCI(DSPMode mode)
        {
            switch (mode)
            {
                case DSPMode.LSB: return "LSB";
                case DSPMode.USB: return "USB";
                case DSPMode.DSB: return "DSB";
                case DSPMode.AM: return "AM";
                case DSPMode.SAM: return "SAM";
                case DSPMode.FM: return "FM";
                case DSPMode.DIGL: return "DIGL";
                case DSPMode.DIGU: return "DIGU";
                case DSPMode.CWL: return "CWL";
                case DSPMode.CWU: return "CWU";
                default: return mode.ToString().ToUpperInvariant();
            }
        }

        private static DSPMode P44ModeFromTCI(string mode)
        {
            string m = (mode ?? String.Empty).Trim().ToUpperInvariant();
            switch (m)
            {
                case "LSB": return DSPMode.LSB;
                case "USB": return DSPMode.USB;
                case "DSB": return DSPMode.DSB;
                case "AM": return DSPMode.AM;
                case "SAM": return DSPMode.SAM;
                case "FM":
                case "NFM": return DSPMode.FM;
                case "DIGL": return DSPMode.DIGL;
                case "DIGU": return DSPMode.DIGU;
                case "CWL": return DSPMode.CWL;
                case "CWU":
                case "CW": return DSPMode.CWU;
                default: return DSPMode.FIRST;
            }
        }

        private static string P44AgcToTCI(AGCMode mode)
        {
            switch (mode)
            {
                case AGCMode.FIXD: return "off";
                case AGCMode.LONG: return "long";
                case AGCMode.SLOW: return "slow";
                case AGCMode.FAST: return "fast";
                case AGCMode.MED: return "normal";
                default: return "normal";
            }
        }

        private static AGCMode P44AgcFromTCI(string mode)
        {
            string m = (mode ?? String.Empty).Trim().ToLowerInvariant();
            switch (m)
            {
                case "off":
                case "fixd":
                case "fixed": return AGCMode.FIXD;
                case "long": return AGCMode.LONG;
                case "slow": return AGCMode.SLOW;
                case "fast": return AGCMode.FAST;
                case "normal":
                case "med":
                case "medium":
                default: return AGCMode.MED;
            }
        }

        internal void P44SetPower(bool v) { P44UI(delegate { PowerOn = v; }); }
        internal void P44SetFocus() { P44UI(delegate { Activate(); BringToFront(); }); }
        internal void P44SetVFO(int chan, long hz) { P44UI(delegate { if (chan == 0) VFOAFreq = hz / 1000000.0; else VFOBFreq = hz / 1000000.0; }); }
        internal void P44SetTXFrequency(long hz) { P44UI(delegate { TXFreq = hz / 1000000.0; }); }
        internal void P44SetFilter(int low, int high) { P44UI(delegate { UpdateRX1Filters(low, high); }); }
        internal void P44SetMode(string mode) { P44UI(delegate { DSPMode m = P44ModeFromTCI(mode); if (m != DSPMode.FIRST) RX1DSPMode = m; }); }
        internal void P44SetAgcMode(string mode) { P44UI(delegate { RX1AGCMode = P44AgcFromTCI(mode); }); }
        internal void P44SetAgcGain(int gain) { P44UI(delegate { RF = Math.Max(-20, Math.Min(120, gain)); }); }
        internal void P44SetSplit(bool v) { P44UI(delegate { VFOSplit = v; }); }
        internal void P44SetRit(bool v) { P44UI(delegate { RITOn = v; }); }
        internal void P44SetXit(bool v) { P44UI(delegate { XITOn = v; }); }
        internal void P44SetRitOffset(int v) { P44UI(delegate { RITValue = Math.Max(-99999, Math.Min(99999, v)); }); }
        internal void P44SetXitOffset(int v) { P44UI(delegate { XITValue = Math.Max(-99999, Math.Min(99999, v)); }); }
        internal void P44SetNR(bool v) { P44UI(delegate { chkNR.Checked = v; }); }
        internal void P44SetNB(bool v) { P44UI(delegate { chkNB.Checked = v; }); }
        internal void P44SetANF(bool v) { P44UI(delegate { chkANF.Checked = v; }); }
        internal void P44SetSqlEnabled(bool v) { P44UI(delegate { chkSquelch.Checked = v; }); }
        internal void P44SetSqlLevel(int v) { P44UI(delegate { Squelch = Math.Max(ptbSquelch.Minimum, Math.Min(ptbSquelch.Maximum, v)); }); }
        internal void P44SetDrive(int v) { P44UI(delegate { PWR = Math.Max(0, Math.Min(100, v)); }); }
        internal void P44SetTuneDrive(int v) { P44UI(delegate { TunePower = Math.Max(0, Math.Min(100, v)); }); }
        internal void P44SetMox(bool v) { P44UI(delegate { MOX = v; }); }
        internal void P44SetTune(bool v) { P44UI(delegate { TUN = v; }); }
        internal void P44SetVolume(int v) { P44UI(delegate { AF = Math.Max(0, Math.Min(100, v)); }); }
        internal void P44SetMute(bool v) { P44UI(delegate { MUT = v; }); }
        internal void P44SetMon(bool v) { P44UI(delegate { MON = v; }); }
        internal void P44SetMonVolume(int v) { P44UI(delegate { TXAF = Math.Max(0, Math.Min(100, v)); }); }
        internal void P44SetVfoLock(int chan, bool v) { P44UI(delegate { if (chan == 0) P32VFOALock = v; else P32VFOBLock = v; }); }
        internal void P44SetVfoSync(bool v) { P44UI(delegate { VFOSync = v; }); }

        internal void P44SetStepAtt(int attenuation)
        {
            P44UI(delegate
            {
                attenuation = Math.Max(0, attenuation);
                PreampMode[] modes = new PreampMode[] { PreampMode.OFF, PreampMode.LOW, PreampMode.MED, PreampMode.HIGH };
                double best = Double.MaxValue;
                PreampMode selected = RX1PreampMode;
                foreach (PreampMode mode in modes)
                {
                    float value;
                    try { value = rx1_preamp_offset[(int)mode]; } catch { continue; }
                    if (value < -0.5f) continue;
                    double diff = Math.Abs(value - attenuation);
                    if (diff < best) { best = diff; selected = mode; }
                }
                RX1PreampMode = selected;
            });
        }

        internal void P44SetPreampAtt(int attenuation)
        {
            P44UI(delegate
            {
                attenuation = Math.Min(0, attenuation);
                PreampMode[] modes = new PreampMode[] { PreampMode.OFF, PreampMode.LOW, PreampMode.MED, PreampMode.HIGH };
                double best = Double.MaxValue;
                PreampMode selected = RX1PreampMode;
                foreach (PreampMode mode in modes)
                {
                    float value;
                    try { value = rx1_preamp_offset[(int)mode]; } catch { continue; }
                    if (value > 0.5f) continue;
                    double diff = Math.Abs(value - attenuation);
                    if (diff < best) { best = diff; selected = mode; }
                }
                RX1PreampMode = selected;
            });
        }
    }

    public partial class Setup
    {
        private TabPage p44TCITab;
        private CheckBox p44TCIEnabled;
        private NumericUpDown p44TCIPort;
        private CheckBox p44TCIBindAll;
        private Label p44TCIStatus;
        private bool p44TCILoading;

        internal void P44InitTCIUI()
        {
            if (p44TCITab != null) return;
            P44TCISettings.Load();

            foreach (TabPage page in tcSetup.TabPages)
            {
                if (page.Text.Equals("TCI", StringComparison.OrdinalIgnoreCase))
                {
                    p44TCITab = page;
                    return;
                }
            }

            p44TCITab = new TabPage("TCI");
            p44TCITab.Name = "p44TCITab";
            p44TCITab.UseVisualStyleBackColor = true;
            tcSetup.TabPages.Add(p44TCITab);

            GroupBox group = new GroupBox();
            group.Text = "TCI WebSocket server";
            group.Location = new Point(18, 18);
            group.Size = new Size(570, 190);
            p44TCITab.Controls.Add(group);

            p44TCIEnabled = new CheckBox();
            p44TCIEnabled.AutoSize = true;
            p44TCIEnabled.Text = "Enable TCI";
            p44TCIEnabled.Location = new Point(18, 30);
            p44TCIEnabled.CheckedChanged += P44TCISettingChanged;
            group.Controls.Add(p44TCIEnabled);

            Label portLabel = new Label();
            portLabel.AutoSize = true;
            portLabel.Text = "Port:";
            portLabel.Location = new Point(18, 64);
            group.Controls.Add(portLabel);

            p44TCIPort = new NumericUpDown();
            p44TCIPort.Minimum = 1024;
            p44TCIPort.Maximum = 65535;
            p44TCIPort.Location = new Point(65, 60);
            p44TCIPort.Width = 90;
            p44TCIPort.ValueChanged += P44TCISettingChanged;
            group.Controls.Add(p44TCIPort);

            p44TCIBindAll = new CheckBox();
            p44TCIBindAll.AutoSize = true;
            p44TCIBindAll.Text = "Listen on LAN (0.0.0.0)";
            p44TCIBindAll.Location = new Point(18, 94);
            p44TCIBindAll.CheckedChanged += P44TCISettingChanged;
            group.Controls.Add(p44TCIBindAll);

            p44TCIStatus = new Label();
            p44TCIStatus.AutoSize = true;
            p44TCIStatus.Location = new Point(18, 128);
            group.Controls.Add(p44TCIStatus);

            Label scope = new Label();
            scope.AutoSize = true;
            scope.Text = "P44: TCI control/status. Audio and IQ streaming are not enabled in this build.";
            scope.Location = new Point(18, 154);
            group.Controls.Add(scope);

            p44TCILoading = true;
            try
            {
                p44TCIEnabled.Checked = P44TCISettings.Enabled;
                p44TCIPort.Value = P44TCISettings.Port;
                p44TCIBindAll.Checked = P44TCISettings.BindAll;
                P44UpdateTCIStatus();
            }
            finally
            {
                p44TCILoading = false;
            }
        }

        private void P44TCISettingChanged(object sender, EventArgs e)
        {
            if (p44TCILoading) return;

            P44TCISettings.Enabled = p44TCIEnabled.Checked;
            P44TCISettings.Port = (int)p44TCIPort.Value;
            P44TCISettings.BindAll = p44TCIBindAll.Checked;
            P44TCISettings.Save();

            if (console != null) console.P44RestartTCI();
            P44UpdateTCIStatus();
        }

        private void P44UpdateTCIStatus()
        {
            if (p44TCIStatus == null) return;
            p44TCIStatus.Text = console == null ? "Status: console unavailable" : "Status: " + console.P44TCIStatusText;
        }
    }
}
