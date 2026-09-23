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
        internal static string BindAddress = "0.0.0.0";
        internal static int Port = 50001;
        internal static int PollMs = 100;
        internal static bool SendInitialStateOnConnect = true;
        internal static bool UseRX1VFOAForRX2VFOA = false;
        internal static bool CopyRX2VFOBToRX2VFOA = false;
        internal static bool ForgetRX2VFOB = false;
        internal static bool CWLUbecomesCW = false;
        internal static bool CWBecomesCWUAbove10MHz = false;
        internal static bool EmulateExpertSDR3Protocol = true;
        internal static bool EmulateSunSDR2Pro = false;

        // Stored now so the page follows the Thetis TCI layout. The current
        // PowerSDR display core does not yet render TCI spots, so that group
        // is shown but disabled until the native spot layer is ported.
        internal static bool ShowSpots = false;
        internal static int MaxSpots = 100;
        internal static int SpotLifetimeMinutes = 10;
        internal static bool SpotFlags = true;
        internal static bool SpotFlashNew = true;
        internal static int SpotBackPanelAlpha = 20;
        internal static bool OwnCallAppearance = true;
        internal static string OwnCall = "SQ4KOU";
        internal static int OwnCallColorArgb = Color.Yellow.ToArgb();
        internal static int CWSpotSideband = 0; // 0=default, 1=CWU, 2=CWL

        private static bool loaded;

        internal static void Load()
        {
            if (loaded) return;
            loaded = true;

            ArrayList stored = DB.GetVars("SQ4KOU_TCI");
            if (stored == null) return;

            bool hasBindAddress = false;
            bool? legacyBindAll = null;

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
                    case "BindAddress":
                        if (IsIPv4(value))
                        {
                            BindAddress = value;
                            hasBindAddress = true;
                        }
                        break;
                    case "BindAll":
                        if (Boolean.TryParse(value, out b)) legacyBindAll = b;
                        break;
                    case "Port":
                        if (Int32.TryParse(value, out n) && n >= 1024 && n <= 65535) Port = n;
                        break;
                    case "PollMs":
                        if (Int32.TryParse(value, out n) && n >= 25 && n <= 1000) PollMs = n;
                        break;
                    case "SendInitialStateOnConnect":
                        if (Boolean.TryParse(value, out b)) SendInitialStateOnConnect = b;
                        break;
                    case "UseRX1VFOAForRX2VFOA":
                        if (Boolean.TryParse(value, out b)) UseRX1VFOAForRX2VFOA = b;
                        break;
                    case "CopyRX2VFOBToRX2VFOA":
                        if (Boolean.TryParse(value, out b)) CopyRX2VFOBToRX2VFOA = b;
                        break;
                    case "ForgetRX2VFOB":
                        if (Boolean.TryParse(value, out b)) ForgetRX2VFOB = b;
                        break;
                    case "CWLUbecomesCW":
                        if (Boolean.TryParse(value, out b)) CWLUbecomesCW = b;
                        break;
                    case "CWBecomesCWUAbove10MHz":
                        if (Boolean.TryParse(value, out b)) CWBecomesCWUAbove10MHz = b;
                        break;
                    case "EmulateExpertSDR3Protocol":
                        if (Boolean.TryParse(value, out b)) EmulateExpertSDR3Protocol = b;
                        break;
                    case "EmulateSunSDR2Pro":
                        if (Boolean.TryParse(value, out b)) EmulateSunSDR2Pro = b;
                        break;
                    case "ShowSpots":
                        if (Boolean.TryParse(value, out b)) ShowSpots = b;
                        break;
                    case "MaxSpots":
                        if (Int32.TryParse(value, out n) && n >= 1 && n <= 1000) MaxSpots = n;
                        break;
                    case "SpotLifetimeMinutes":
                        if (Int32.TryParse(value, out n) && n >= 1 && n <= 1440) SpotLifetimeMinutes = n;
                        break;
                    case "SpotFlags":
                        if (Boolean.TryParse(value, out b)) SpotFlags = b;
                        break;
                    case "SpotFlashNew":
                        if (Boolean.TryParse(value, out b)) SpotFlashNew = b;
                        break;
                    case "SpotBackPanelAlpha":
                        if (Int32.TryParse(value, out n)) SpotBackPanelAlpha = Math.Max(0, Math.Min(255, n));
                        break;
                    case "OwnCallAppearance":
                        if (Boolean.TryParse(value, out b)) OwnCallAppearance = b;
                        break;
                    case "OwnCall":
                        OwnCall = value ?? String.Empty;
                        break;
                    case "OwnCallColorArgb":
                        if (Int32.TryParse(value, out n)) OwnCallColorArgb = n;
                        break;
                    case "CWSpotSideband":
                        if (Int32.TryParse(value, out n) && n >= 0 && n <= 2) CWSpotSideband = n;
                        break;
                }
            }

            if (!hasBindAddress && legacyBindAll.HasValue)
                BindAddress = legacyBindAll.Value ? "0.0.0.0" : "127.0.0.1";
        }

        internal static void Save()
        {
            ArrayList a = new ArrayList();
            a.Add("Enabled/" + Enabled.ToString());
            a.Add("BindAddress/" + BindAddress);
            a.Add("BindAll/" + (BindAddress == "0.0.0.0").ToString());
            a.Add("Port/" + Port.ToString(CultureInfo.InvariantCulture));
            a.Add("PollMs/" + PollMs.ToString(CultureInfo.InvariantCulture));
            a.Add("SendInitialStateOnConnect/" + SendInitialStateOnConnect.ToString());
            a.Add("UseRX1VFOAForRX2VFOA/" + UseRX1VFOAForRX2VFOA.ToString());
            a.Add("CopyRX2VFOBToRX2VFOA/" + CopyRX2VFOBToRX2VFOA.ToString());
            a.Add("ForgetRX2VFOB/" + ForgetRX2VFOB.ToString());
            a.Add("CWLUbecomesCW/" + CWLUbecomesCW.ToString());
            a.Add("CWBecomesCWUAbove10MHz/" + CWBecomesCWUAbove10MHz.ToString());
            a.Add("EmulateExpertSDR3Protocol/" + EmulateExpertSDR3Protocol.ToString());
            a.Add("EmulateSunSDR2Pro/" + EmulateSunSDR2Pro.ToString());
            a.Add("ShowSpots/" + ShowSpots.ToString());
            a.Add("MaxSpots/" + MaxSpots.ToString(CultureInfo.InvariantCulture));
            a.Add("SpotLifetimeMinutes/" + SpotLifetimeMinutes.ToString(CultureInfo.InvariantCulture));
            a.Add("SpotFlags/" + SpotFlags.ToString());
            a.Add("SpotFlashNew/" + SpotFlashNew.ToString());
            a.Add("SpotBackPanelAlpha/" + SpotBackPanelAlpha.ToString(CultureInfo.InvariantCulture));
            a.Add("OwnCallAppearance/" + OwnCallAppearance.ToString());
            a.Add("OwnCall/" + (OwnCall ?? String.Empty));
            a.Add("OwnCallColorArgb/" + OwnCallColorArgb.ToString(CultureInfo.InvariantCulture));
            a.Add("CWSpotSideband/" + CWSpotSideband.ToString(CultureInfo.InvariantCulture));
            DB.SaveVars("SQ4KOU_TCI", ref a);
        }

        internal static bool IsIPv4(string value)
        {
            IPAddress ip;
            return IPAddress.TryParse((value ?? String.Empty).Trim(), out ip) &&
                ip.AddressFamily == AddressFamily.InterNetwork;
        }
    }

    internal static class P44TCILog
    {
        private static readonly object gate = new object();

        internal static string LogPath
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PowerSDR-SQ4KOU");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "P44_TCI.log");
            }
        }

        internal static void Write(string text)
        {
            try
            {
                lock (gate)
                {
                    File.AppendAllText(LogPath,
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

                P46TCIStreaming.RegisterClient(client);
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
                    if (frame.Opcode == 2)
                    {
                        P46TCIStreaming.HandleBinary(client, frame.Payload);
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
                P46TCIStreaming.UnregisterClient(client);
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

            string protocol = P44TCISettings.EmulateExpertSDR3Protocol ? "ExpertSDR3" : "Thetis";
            string device = P44TCISettings.EmulateSunSDR2Pro ? "SunSDR2PRO" : "FLEX-5000";

            client.SendText("protocol:" + protocol + ",2.0;");
            client.SendText("device:" + device + ";");
            client.SendText("receive_only:false;");
            client.SendText("trx_count:1;");
            client.SendText("channels_count:2;");
            client.SendText("vfo_limits:0," + s.MaxHz.ToString(CultureInfo.InvariantCulture) + ";");
            client.SendText("if_limits:" + (-s.SampleRate / 2).ToString(CultureInfo.InvariantCulture) + "," +
                            (s.SampleRate / 2).ToString(CultureInfo.InvariantCulture) + ";");

            string cwModes = P44TCISettings.CWLUbecomesCW ? ",CW" : String.Empty;
            client.SendText("modulations_list:AM,SAM,DSB,LSB,USB,NFM,FM,DIGL,DIGU,CWL,CWU" + cwModes + ";");
            client.SendText("iq_samplerate:" + s.SampleRate.ToString(CultureInfo.InvariantCulture) + ";");
            client.SendText("audio_samplerate:48000;");
            P46TCIStreaming.SendInitial(client);

            if (P44TCISettings.SendInitialStateOnConnect)
            {
                List<string> all = BuildMessages(s, null, true);
                foreach (string message in all) client.SendText(message);
            }

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
            if (all || old.Mode != s.Mode)
            {
                string mode = s.Mode;
                if (P44TCISettings.CWLUbecomesCW && (mode == "CWL" || mode == "CWU")) mode = "CW";
                m.Add("modulation:0," + mode + ";");
            }
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

                try
                {
                    if (!P46TCIStreaming.HandleCommand(client, name, args))
                        HandleCommand(client, name, args);
                }
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
                IPAddress address;
                if (!IPAddress.TryParse(P44TCISettings.BindAddress, out address) ||
                    address.AddressFamily != AddressFamily.InterNetwork)
                    address = IPAddress.Any;
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
                return "Listening on " + P44TCISettings.BindAddress + ":" +
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
        internal void P44SetMode(string mode)
        {
            P44UI(delegate
            {
                DSPMode m;
                if (P44TCISettings.CWBecomesCWUAbove10MHz &&
                    String.Equals((mode ?? String.Empty).Trim(), "CW", StringComparison.OrdinalIgnoreCase))
                    m = VFOAFreq >= 10.0 ? DSPMode.CWU : DSPMode.CWL;
                else
                    m = P44ModeFromTCI(mode);

                if (m != DSPMode.FIRST) RX1DSPMode = m;
            });
        }
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
        private TextBox p44TCIBindSpec;
        private NumericUpDown p44TCIRate;
        private CheckBox p44TCIEnabled;
        private CheckBox p44TCISendInitial;
        private CheckBox p44TCIUseRx1ForRx2;
        private CheckBox p44TCICopyRx2;
        private CheckBox p44TCIForgetRx2;
        private CheckBox p44TCICWLAsCW;
        private CheckBox p44TCICWAbove10;
        private CheckBox p44TCIEmulateExpert;
        private CheckBox p44TCIEmulateSun;
        private Label p44TCIStatus;
        private System.Windows.Forms.Timer p44TCIUiTimer;
        private ToolTip p44TCIToolTip;
        private bool p44TCILoading;

        private CheckBox P44MakeCheck(Control parent, string text, int x, int y)
        {
            CheckBox cb = new CheckBox();
            cb.AutoSize = true;
            cb.Text = text;
            cb.Location = new Point(x, y);
            parent.Controls.Add(cb);
            return cb;
        }

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

            p44TCIToolTip = new ToolTip();

            p44TCITab = new TabPage("TCI");
            p44TCITab.Name = "p44TCITab";
            p44TCITab.UseVisualStyleBackColor = true;
            p44TCITab.AutoScroll = true;
            tcSetup.TabPages.Add(p44TCITab);

            GroupBox server = new GroupBox();
            server.Text = "TCI Server";
            server.Location = new Point(12, 10);
            server.Size = new Size(610, 184);
            p44TCITab.Controls.Add(server);

            Label bindLabel = new Label();
            bindLabel.AutoSize = true;
            bindLabel.Text = "Bind IP:Port";
            bindLabel.Location = new Point(16, 27);
            server.Controls.Add(bindLabel);

            p44TCIBindSpec = new TextBox();
            p44TCIBindSpec.Location = new Point(92, 23);
            p44TCIBindSpec.Size = new Size(150, 20);
            p44TCIBindSpec.Leave += P44TCIBindLeave;
            server.Controls.Add(p44TCIBindSpec);

            Button def = new Button();
            def.Text = "Def";
            def.Location = new Point(248, 21);
            def.Size = new Size(42, 24);
            def.Click += P44TCIDefaultsClick;
            server.Controls.Add(def);

            Button ipv4 = new Button();
            ipv4.Text = "IPv4";
            ipv4.Location = new Point(296, 21);
            ipv4.Size = new Size(48, 24);
            ipv4.Click += P44TCIIPv4Click;
            server.Controls.Add(ipv4);

            Label rateLabel = new Label();
            rateLabel.AutoSize = true;
            rateLabel.Text = "Rate Limit (ms)";
            rateLabel.Location = new Point(360, 27);
            server.Controls.Add(rateLabel);

            p44TCIRate = new NumericUpDown();
            p44TCIRate.Minimum = 25;
            p44TCIRate.Maximum = 1000;
            p44TCIRate.Increment = 25;
            p44TCIRate.Location = new Point(454, 23);
            p44TCIRate.Size = new Size(72, 20);
            p44TCIRate.ValueChanged += P44TCISettingChanged;
            server.Controls.Add(p44TCIRate);

            p44TCISendInitial = P44MakeCheck(server, "Send initial VFO state on connect (out)", 18, 54);
            p44TCISendInitial.CheckedChanged += P44TCISettingChanged;

            p44TCIUseRx1ForRx2 = P44MakeCheck(server, "Use RX1 VFOa for RX2 VFOa (in+out)", 18, 78);
            p44TCIUseRx1ForRx2.Enabled = false;
            p44TCICopyRx2 = P44MakeCheck(server, "Duplicate RX2 VFOb to RX2 VFOa (out)", 18, 102);
            p44TCICopyRx2.Enabled = false;
            p44TCIForgetRx2 = P44MakeCheck(server, "Forget RX2 VFOb", 266, 102);
            p44TCIForgetRx2.Enabled = false;

            p44TCICWLAsCW = P44MakeCheck(server, "CWL/CWU becomes CW (out)", 18, 126);
            p44TCICWLAsCW.CheckedChanged += P44TCISettingChanged;
            p44TCICWAbove10 = P44MakeCheck(server, "CW becomes CWU if 10MHz and above (in)", 18, 150);
            p44TCICWAbove10.CheckedChanged += P44TCISettingChanged;

            p44TCIEmulateExpert = P44MakeCheck(server, "Emulate ExpertSDR3 protocol", 342, 54);
            p44TCIEmulateExpert.CheckedChanged += P44TCISettingChanged;
            p44TCIEmulateSun = P44MakeCheck(server, "Emulate SunSDR2Pro device", 342, 78);
            p44TCIEmulateSun.CheckedChanged += P44TCISettingChanged;

            p44TCIToolTip.SetToolTip(p44TCIUseRx1ForRx2, "RX2 is intentionally not exposed in the current FLEX-5000 build.");
            p44TCIToolTip.SetToolTip(p44TCICopyRx2, "RX2 is intentionally not exposed in the current FLEX-5000 build.");
            p44TCIToolTip.SetToolTip(p44TCIForgetRx2, "RX2 is intentionally not exposed in the current FLEX-5000 build.");

            GroupBox spots = new GroupBox();
            spots.Text = "TCI Spots";
            spots.Location = new Point(12, 202);
            spots.Size = new Size(610, 174);
            spots.Enabled = false;
            p44TCITab.Controls.Add(spots);

            CheckBox showSpots = P44MakeCheck(spots, "Show TCI Spots", 18, 24);
            showSpots.Checked = P44TCISettings.ShowSpots;

            Label maxLabel = new Label();
            maxLabel.AutoSize = true;
            maxLabel.Text = "Max Spots:";
            maxLabel.Location = new Point(18, 52);
            spots.Controls.Add(maxLabel);

            NumericUpDown maxSpots = new NumericUpDown();
            maxSpots.Minimum = 1;
            maxSpots.Maximum = 1000;
            maxSpots.Value = P44TCISettings.MaxSpots;
            maxSpots.Location = new Point(82, 48);
            maxSpots.Size = new Size(62, 20);
            spots.Controls.Add(maxSpots);

            Button clearNonSwl = new Button();
            clearNonSwl.Text = "Clear non SWL";
            clearNonSwl.Location = new Point(160, 44);
            clearNonSwl.Size = new Size(86, 27);
            spots.Controls.Add(clearNonSwl);

            Button clearSwl = new Button();
            clearSwl.Text = "Clear SWL";
            clearSwl.Location = new Point(252, 44);
            clearSwl.Size = new Size(72, 27);
            spots.Controls.Add(clearSwl);

            CheckBox flags = P44MakeCheck(spots, "Flags", 340, 50);
            flags.Checked = P44TCISettings.SpotFlags;

            Label lifeLabel = new Label();
            lifeLabel.AutoSize = true;
            lifeLabel.Text = "Spot Lifetime:";
            lifeLabel.Location = new Point(18, 82);
            spots.Controls.Add(lifeLabel);

            NumericUpDown life = new NumericUpDown();
            life.Minimum = 1;
            life.Maximum = 1440;
            life.Value = P44TCISettings.SpotLifetimeMinutes;
            life.Location = new Point(92, 78);
            life.Size = new Size(55, 20);
            spots.Controls.Add(life);

            Label mins = new Label();
            mins.AutoSize = true;
            mins.Text = "mins";
            mins.Location = new Point(151, 82);
            spots.Controls.Add(mins);

            CheckBox flash = P44MakeCheck(spots, "Flash new", 205, 80);
            flash.Checked = P44TCISettings.SpotFlashNew;

            Button flashColour = new Button();
            flashColour.Location = new Point(290, 77);
            flashColour.Size = new Size(30, 22);
            flashColour.BackColor = Color.White;
            spots.Controls.Add(flashColour);

            Label alphaLabel = new Label();
            alphaLabel.AutoSize = true;
            alphaLabel.Text = "Spot back panel alpha:";
            alphaLabel.Location = new Point(18, 111);
            spots.Controls.Add(alphaLabel);

            TrackBar alpha = new TrackBar();
            alpha.Minimum = 0;
            alpha.Maximum = 255;
            alpha.TickStyle = TickStyle.None;
            alpha.Value = Math.Max(0, Math.Min(255, P44TCISettings.SpotBackPanelAlpha));
            alpha.Location = new Point(145, 103);
            alpha.Size = new Size(140, 28);
            spots.Controls.Add(alpha);

            CheckBox ownCall = P44MakeCheck(spots, "Own Call Appearance", 18, 139);
            ownCall.Checked = P44TCISettings.OwnCallAppearance;

            TextBox ownCallText = new TextBox();
            ownCallText.Text = P44TCISettings.OwnCall;
            ownCallText.Location = new Point(148, 136);
            ownCallText.Size = new Size(90, 20);
            spots.Controls.Add(ownCallText);

            Button ownColour = new Button();
            ownColour.Location = new Point(244, 134);
            ownColour.Size = new Size(30, 24);
            ownColour.BackColor = Color.FromArgb(P44TCISettings.OwnCallColorArgb);
            spots.Controls.Add(ownColour);

            GroupBox cwSpot = new GroupBox();
            cwSpot.Text = "CW Spot sideband";
            cwSpot.Location = new Point(345, 102);
            cwSpot.Size = new Size(245, 58);
            spots.Controls.Add(cwSpot);

            RadioButton cwU = new RadioButton();
            cwU.AutoSize = true;
            cwU.Text = "Force to CWU";
            cwU.Location = new Point(9, 24);
            cwSpot.Controls.Add(cwU);

            RadioButton cwL = new RadioButton();
            cwL.AutoSize = true;
            cwL.Text = "Force to CWL";
            cwL.Location = new Point(91, 24);
            cwSpot.Controls.Add(cwL);

            RadioButton cwDefault = new RadioButton();
            cwDefault.AutoSize = true;
            cwDefault.Text = "Default";
            cwDefault.Location = new Point(173, 24);
            cwSpot.Controls.Add(cwDefault);

            if (P44TCISettings.CWSpotSideband == 1) cwU.Checked = true;
            else if (P44TCISettings.CWSpotSideband == 2) cwL.Checked = true;
            else cwDefault.Checked = true;

            p44TCIEnabled = P44MakeCheck(p44TCITab, "TCIServer Running", 18, 388);
            p44TCIEnabled.CheckedChanged += P44TCISettingChanged;

            Button showLog = new Button();
            showLog.Text = "Show Log";
            showLog.Location = new Point(158, 383);
            showLog.Size = new Size(76, 27);
            showLog.Click += P44TCIShowLogClick;
            p44TCITab.Controls.Add(showLog);

            p44TCIStatus = new Label();
            p44TCIStatus.AutoSize = true;
            p44TCIStatus.Location = new Point(250, 390);
            p44TCITab.Controls.Add(p44TCIStatus);

            p44TCILoading = true;
            try
            {
                p44TCIBindSpec.Text = P44TCISettings.BindAddress + ":" +
                    P44TCISettings.Port.ToString(CultureInfo.InvariantCulture);
                p44TCIRate.Value = Math.Max(p44TCIRate.Minimum, Math.Min(p44TCIRate.Maximum, P44TCISettings.PollMs));
                p44TCISendInitial.Checked = P44TCISettings.SendInitialStateOnConnect;
                p44TCIUseRx1ForRx2.Checked = P44TCISettings.UseRX1VFOAForRX2VFOA;
                p44TCICopyRx2.Checked = P44TCISettings.CopyRX2VFOBToRX2VFOA;
                p44TCIForgetRx2.Checked = P44TCISettings.ForgetRX2VFOB;
                p44TCICWLAsCW.Checked = P44TCISettings.CWLUbecomesCW;
                p44TCICWAbove10.Checked = P44TCISettings.CWBecomesCWUAbove10MHz;
                p44TCIEmulateExpert.Checked = P44TCISettings.EmulateExpertSDR3Protocol;
                p44TCIEmulateSun.Checked = P44TCISettings.EmulateSunSDR2Pro;
                p44TCIEnabled.Checked = P44TCISettings.Enabled;
            }
            finally
            {
                p44TCILoading = false;
            }

            p44TCIUiTimer = new System.Windows.Forms.Timer();
            p44TCIUiTimer.Interval = 500;
            p44TCIUiTimer.Tick += delegate { P44UpdateTCIStatus(); };
            p44TCIUiTimer.Start();
            P44UpdateTCIStatus();
        }

        private bool P44TryReadBindSpec(out string address, out int port)
        {
            address = P44TCISettings.BindAddress;
            port = P44TCISettings.Port;

            string spec = (p44TCIBindSpec == null ? String.Empty : p44TCIBindSpec.Text).Trim();
            int colon = spec.LastIndexOf(':');
            if (colon <= 0 || colon >= spec.Length - 1) return false;

            string ipText = spec.Substring(0, colon).Trim();
            int p;
            if (!P44TCISettings.IsIPv4(ipText)) return false;
            if (!Int32.TryParse(spec.Substring(colon + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out p)) return false;
            if (p < 1024 || p > 65535) return false;

            address = ipText;
            port = p;
            return true;
        }

        private void P44TCIBindLeave(object sender, EventArgs e)
        {
            if (p44TCILoading) return;
            string address;
            int port;
            if (!P44TryReadBindSpec(out address, out port))
            {
                p44TCIBindSpec.Text = P44TCISettings.BindAddress + ":" +
                    P44TCISettings.Port.ToString(CultureInfo.InvariantCulture);
                P44UpdateTCIStatus();
                return;
            }

            P44TCISettings.BindAddress = address;
            P44TCISettings.Port = port;
            P44SaveAndRestartTCI();
        }

        private void P44TCIDefaultsClick(object sender, EventArgs e)
        {
            p44TCIBindSpec.Text = "0.0.0.0:50001";
            p44TCIRate.Value = 100;
            P44TCIBindLeave(sender, EventArgs.Empty);
        }

        private void P44TCIIPv4Click(object sender, EventArgs e)
        {
            string ip = "127.0.0.1";
            try
            {
                IPAddress[] addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                foreach (IPAddress candidate in addresses)
                {
                    if (candidate.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(candidate))
                    {
                        ip = candidate.ToString();
                        break;
                    }
                }
            }
            catch { }

            p44TCIBindSpec.Text = ip + ":" + P44TCISettings.Port.ToString(CultureInfo.InvariantCulture);
            P44TCIBindLeave(sender, EventArgs.Empty);
        }

        private void P44TCISettingChanged(object sender, EventArgs e)
        {
            if (p44TCILoading) return;

            P44TCISettings.Enabled = p44TCIEnabled.Checked;
            P44TCISettings.PollMs = (int)p44TCIRate.Value;
            P44TCISettings.SendInitialStateOnConnect = p44TCISendInitial.Checked;
            P44TCISettings.CWLUbecomesCW = p44TCICWLAsCW.Checked;
            P44TCISettings.CWBecomesCWUAbove10MHz = p44TCICWAbove10.Checked;
            P44TCISettings.EmulateExpertSDR3Protocol = p44TCIEmulateExpert.Checked;
            P44TCISettings.EmulateSunSDR2Pro = p44TCIEmulateSun.Checked;
            P44SaveAndRestartTCI();
        }

        private void P44SaveAndRestartTCI()
        {
            P44TCISettings.Save();
            if (console != null) console.P44RestartTCI();
            P44UpdateTCIStatus();
        }

        private void P44TCIShowLogClick(object sender, EventArgs e)
        {
            try
            {
                P44TCILog.Write("LOG OPEN");
                System.Diagnostics.Process.Start("notepad.exe", P44TCILog.LogPath);
            }
            catch { }
        }

        private void P44UpdateTCIStatus()
        {
            if (p44TCIStatus == null) return;
            p44TCIStatus.Text = console == null ? "Console unavailable" : console.P44TCIStatusText;
        }
    }
}
