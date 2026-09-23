using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace PowerSDR
{
    internal static class P44TCISettings
    {
        internal static bool Enabled = true;
        internal static int Port = 50001;
        internal static bool BindAll = true;
        private static bool loaded;

        internal static void Load()
        {
            if (loaded) return;
            loaded = true;
            ArrayList a = DB.GetVars("SQ4KOU_TCI");
            if (a == null) return;

            foreach (object o in a)
            {
                string s = o as string;
                if (String.IsNullOrEmpty(s)) continue;
                int slash = s.IndexOf('/');
                if (slash <= 0 || slash >= s.Length - 1) continue;
                string key = s.Substring(0, slash);
                string value = s.Substring(slash + 1);

                bool b;
                int n;
                if (key == "Enabled" && Boolean.TryParse(value, out b)) Enabled = b;
                else if (key == "Port" && Int32.TryParse(value, out n) && n >= 1024 && n <= 65535) Port = n;
                else if (key == "BindAll" && Boolean.TryParse(value, out b)) BindAll = b;
            }
        }

        internal static void Save()
        {
            ArrayList a = new ArrayList();
            a.Add("Enabled/" + Enabled.ToString());
            a.Add("Port/" + Port.ToString(CultureInfo.InvariantCulture));
            a.Add("BindAll/" + BindAll.ToString());
            DB.SaveVars("SQ4KOU_TCI", ref a);
        }
    }

    internal sealed class P44TCIState
    {
        internal bool Power;
        internal long VfoA;
        internal long VfoB;
        internal long TxFrequency;
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
        internal bool Nr;
        internal bool Nb;
        internal bool Anf;
        internal bool Nf;
        internal bool Sql;
        internal int SqlLevel;
        internal int Drive;
        internal int TuneDrive;
        internal bool Trx;
        internal bool Tune;
        internal double VolumeDb;
        internal bool Mute;
        internal bool Mon;
        internal double MonVolumeDb;
        internal bool VfoALock;
        internal bool VfoBLock;
        internal bool VfoSync;
        internal bool AppFocus;
        internal int IQSampleRate;

        internal string Fingerprint()
        {
            return String.Join("|", new string[] {
                Power.ToString(), VfoA.ToString(), VfoB.ToString(), TxFrequency.ToString(), Mode,
                FilterLow.ToString(), FilterHigh.ToString(), AgcMode, AgcGain.ToString(), Split.ToString(),
                Rit.ToString(), Xit.ToString(), RitOffset.ToString(), XitOffset.ToString(),
                Nr.ToString(), Nb.ToString(), Anf.ToString(), Nf.ToString(), Sql.ToString(), SqlLevel.ToString(),
                Drive.ToString(), TuneDrive.ToString(), Trx.ToString(), Tune.ToString(),
                VolumeDb.ToString("F1", CultureInfo.InvariantCulture), Mute.ToString(),
                Mon.ToString(), MonVolumeDb.ToString("F1", CultureInfo.InvariantCulture),
                VfoALock.ToString(), VfoBLock.ToString(), VfoSync.ToString(), AppFocus.ToString(),
                IQSampleRate.ToString()
            });
        }
    }

    internal sealed class P44TCIClient
    {
        private readonly TcpClient tcp;
        private readonly NetworkStream stream;
        private readonly object sendLock = new object();
        private readonly P44TCIServer owner;
        private volatile bool closed;

        internal P44TCIClient(P44TCIServer server, TcpClient client)
        {
            owner = server;
            tcp = client;
            tcp.NoDelay = true;
            stream = tcp.GetStream();
        }

        internal void Run()
        {
            try
            {
                if (!Handshake()) return;
                owner.ClientReady(this);

                while (!closed)
                {
                    byte opcode;
                    byte[] payload;
                    if (!ReadFrame(out opcode, out payload)) break;

                    if (opcode == 0x8) break;
                    if (opcode == 0x9)
                    {
                        SendFrame(0xA, payload);
                        continue;
                    }
                    if (opcode != 0x1) continue;

                    string text = Encoding.UTF8.GetString(payload);
                    owner.ProcessText(this, text);
                }
            }
            catch { }
            finally
            {
                Close();
                owner.ClientClosed(this);
            }
        }

        private bool Handshake()
        {
            StringBuilder sb = new StringBuilder();
            byte[] one = new byte[1];
            int state = 0;
            while (sb.Length < 16384)
            {
                int n = stream.Read(one, 0, 1);
                if (n <= 0) return false;
                char ch = (char)one[0];
                sb.Append(ch);
                if (state == 0 && ch == '\r') state = 1;
                else if (state == 1 && ch == '\n') state = 2;
                else if (state == 2 && ch == '\r') state = 3;
                else if (state == 3 && ch == '\n') break;
                else state = 0;
            }

            string request = sb.ToString();
            string key = null;
            string[] lines = request.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                int colon = line.IndexOf(':');
                if (colon <= 0) continue;
                if (line.Substring(0, colon).Trim().Equals("Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase))
                {
                    key = line.Substring(colon + 1).Trim();
                    break;
                }
            }
            if (String.IsNullOrEmpty(key)) return false;

            string accept;
            using (SHA1 sha = SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
                accept = Convert.ToBase64String(hash);
            }

            string response =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Accept: " + accept + "\r\n\r\n";
            byte[] bytes = Encoding.ASCII.GetBytes(response);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            return true;
        }

        private bool ReadExact(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int n = stream.Read(buffer, offset, count);
                if (n <= 0) return false;
                offset += n;
                count -= n;
            }
            return true;
        }

        private bool ReadFrame(out byte opcode, out byte[] payload)
        {
            opcode = 0;
            payload = null;
            byte[] h = new byte[2];
            if (!ReadExact(h, 0, 2)) return false;

            opcode = (byte)(h[0] & 0x0F);
            bool masked = (h[1] & 0x80) != 0;
            ulong length = (ulong)(h[1] & 0x7F);

            if (length == 126)
            {
                byte[] x = new byte[2];
                if (!ReadExact(x, 0, 2)) return false;
                length = (ulong)((x[0] << 8) | x[1]);
            }
            else if (length == 127)
            {
                byte[] x = new byte[8];
                if (!ReadExact(x, 0, 8)) return false;
                length = 0;
                for (int i = 0; i < 8; i++) length = (length << 8) | x[i];
            }

            if (length > 1024 * 1024) return false;

            byte[] mask = null;
            if (masked)
            {
                mask = new byte[4];
                if (!ReadExact(mask, 0, 4)) return false;
            }

            payload = new byte[(int)length];
            if (length > 0 && !ReadExact(payload, 0, (int)length)) return false;

            if (masked)
            {
                for (int i = 0; i < payload.Length; i++)
                    payload[i] = (byte)(payload[i] ^ mask[i & 3]);
            }
            return true;
        }

        internal void SendText(string text)
        {
            if (closed || String.IsNullOrEmpty(text)) return;
            SendFrame(0x1, Encoding.UTF8.GetBytes(text));
        }

        private void SendFrame(byte opcode, byte[] payload)
        {
            if (closed) return;
            try
            {
                lock (sendLock)
                {
                    MemoryStream ms = new MemoryStream();
                    ms.WriteByte((byte)(0x80 | opcode));
                    int length = payload == null ? 0 : payload.Length;
                    if (length <= 125)
                    {
                        ms.WriteByte((byte)length);
                    }
                    else if (length <= 65535)
                    {
                        ms.WriteByte(126);
                        ms.WriteByte((byte)((length >> 8) & 0xFF));
                        ms.WriteByte((byte)(length & 0xFF));
                    }
                    else
                    {
                        ms.WriteByte(127);
                        ulong len = (ulong)length;
                        for (int i = 7; i >= 0; i--) ms.WriteByte((byte)((len >> (8 * i)) & 0xFF));
                    }
                    if (length > 0) ms.Write(payload, 0, length);
                    byte[] frame = ms.ToArray();
                    stream.Write(frame, 0, frame.Length);
                    stream.Flush();
                }
            }
            catch { Close(); }
        }

        internal void Close()
        {
            if (closed) return;
            closed = true;
            try { stream.Close(); } catch { }
            try { tcp.Close(); } catch { }
        }
    }

    internal sealed class P44TCIServer
    {
        private readonly PowerSDR.Console console;
        private readonly int port;
        private readonly bool bindAll;
        private TcpListener listener;
        private Thread acceptThread;
        private System.Threading.Timer stateTimer;
        private volatile bool stopping;
        private readonly object clientsLock = new object();
        private readonly List<P44TCIClient> clients = new List<P44TCIClient>();
        private string lastFingerprint = String.Empty;

        internal P44TCIServer(PowerSDR.Console c, int serverPort, bool allowLan)
        {
            console = c;
            port = serverPort;
            bindAll = allowLan;
        }

        internal int ClientCount
        {
            get { lock (clientsLock) return clients.Count; }
        }

        internal bool Running
        {
            get { return listener != null && !stopping; }
        }

        internal bool Start()
        {
            try
            {
                stopping = false;
                listener = new TcpListener(bindAll ? IPAddress.Any : IPAddress.Loopback, port);
                listener.Start();
                acceptThread = new Thread(AcceptLoop);
                acceptThread.IsBackground = true;
                acceptThread.Name = "PowerSDR TCI Accept";
                acceptThread.Start();
                stateTimer = new System.Threading.Timer(StateTick, null, 100, 100);
                return true;
            }
            catch
            {
                Stop();
                return false;
            }
        }

        internal void Stop()
        {
            stopping = true;
            if (stateTimer != null)
            {
                try { stateTimer.Dispose(); } catch { }
                stateTimer = null;
            }
            if (listener != null)
            {
                try { listener.Stop(); } catch { }
                listener = null;
            }

            P44TCIClient[] copy;
            lock (clientsLock)
            {
                copy = clients.ToArray();
                clients.Clear();
            }
            foreach (P44TCIClient client in copy) client.Close();
        }

        private void AcceptLoop()
        {
            while (!stopping)
            {
                try
                {
                    TcpClient tcp = listener.AcceptTcpClient();
                    P44TCIClient client = new P44TCIClient(this, tcp);
                    lock (clientsLock) clients.Add(client);
                    Thread t = new Thread(client.Run);
                    t.IsBackground = true;
                    t.Name = "PowerSDR TCI Client";
                    t.Start();
                }
                catch
                {
                    if (stopping) break;
                    Thread.Sleep(50);
                }
            }
        }

        internal void ClientReady(P44TCIClient client)
        {
            P44TCIState state = console.P44CaptureTCIState();
            client.SendText(BuildInitial(state));
        }

        internal void ClientClosed(P44TCIClient client)
        {
            lock (clientsLock) clients.Remove(client);
        }

        internal void ProcessText(P44TCIClient client, string text)
        {
            if (String.IsNullOrEmpty(text)) return;
            string[] commands = text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            bool handled = false;
            foreach (string raw in commands)
            {
                string cmd = raw.Trim();
                if (cmd.Length == 0) continue;
                if (console.P44ApplyTCICommand(cmd)) handled = true;
            }
            if (handled)
            {
                P44TCIState state = console.P44CaptureTCIState();
                client.SendText(BuildState(state));
                lastFingerprint = state.Fingerprint();
            }
        }

        private void StateTick(object unused)
        {
            if (stopping) return;
            try
            {
                P44TCIState state = console.P44CaptureTCIState();
                string fp = state.Fingerprint();
                if (fp == lastFingerprint) return;
                lastFingerprint = fp;
                Broadcast(BuildState(state));
            }
            catch { }
        }

        private void Broadcast(string text)
        {
            P44TCIClient[] copy;
            lock (clientsLock) copy = clients.ToArray();
            foreach (P44TCIClient client in copy) client.SendText(text);
        }

        private static string B(bool value) { return value ? "true" : "false"; }

        private static string BuildInitial(P44TCIState s)
        {
            StringBuilder b = new StringBuilder();
            b.Append("protocol:ExpertSDR3,2.0;");
            b.Append("device:FLEX5000;");
            b.Append("receive_only:false;");
            b.Append("trx_count:1;");
            b.Append("channels_count:2;");
            b.Append("vfo_limits:0,65000000;");
            int half = Math.Max(24000, s.IQSampleRate / 2);
            b.Append("if_limits:-").Append(half).Append(",").Append(half).Append(";");
            b.Append("modulations_list:AM,SAM,DSB,LSB,USB,NFM,FM,DIGL,DIGU,CWL,CWU;");
            b.Append("iq_samplerate:").Append(s.IQSampleRate).Append(";");
            b.Append("audio_samplerate:48000;");
            b.Append(BuildState(s));
            b.Append("ready;");
            return b.ToString();
        }

        private static string BuildState(P44TCIState s)
        {
            StringBuilder b = new StringBuilder();
            b.Append(s.Power ? "start;" : "stop;");
            b.Append("vfo:0,0,").Append(s.VfoA).Append(";");
            b.Append("vfo:0,1,").Append(s.VfoB).Append(";");
            b.Append("tx_frequency:").Append(s.TxFrequency).Append(";");
            b.Append("modulation:0,").Append(s.Mode).Append(";");
            b.Append("rx_filter_band:0,").Append(s.FilterLow).Append(",").Append(s.FilterHigh).Append(";");
            b.Append("agc_mode:0,").Append(s.AgcMode).Append(";");
            b.Append("agc_gain:0,").Append(s.AgcGain).Append(";");
            b.Append("split_enable:0,").Append(B(s.Split)).Append(";");
            b.Append("rit_enable:0,").Append(B(s.Rit)).Append(";");
            b.Append("xit_enable:0,").Append(B(s.Xit)).Append(";");
            b.Append("rit_offset:0,").Append(s.RitOffset).Append(";");
            b.Append("xit_offset:0,").Append(s.XitOffset).Append(";");
            b.Append("rx_nr_enable:0,").Append(B(s.Nr)).Append(";");
            b.Append("rx_nb_enable:0,").Append(B(s.Nb)).Append(";");
            b.Append("rx_anf_enable:0,").Append(B(s.Anf)).Append(";");
            b.Append("rx_nf_enable:0,").Append(B(s.Nf)).Append(";");
            b.Append("sql_enable:0,").Append(B(s.Sql)).Append(";");
            b.Append("sql_level:0,").Append(s.SqlLevel).Append(";");
            b.Append("drive:0,").Append(s.Drive).Append(";");
            b.Append("tune_drive:0,").Append(s.TuneDrive).Append(";");
            b.Append("trx:0,").Append(B(s.Trx)).Append(";");
            b.Append("tune:0,").Append(B(s.Tune)).Append(";");
            b.Append("volume:").Append(s.VolumeDb.ToString("F1", CultureInfo.InvariantCulture)).Append(";");
            b.Append("mute:").Append(B(s.Mute)).Append(";");
            b.Append("mon_enable:").Append(B(s.Mon)).Append(";");
            b.Append("mon_volume:").Append(s.MonVolumeDb.ToString("F1", CultureInfo.InvariantCulture)).Append(";");
            b.Append("vfo_lock:0,0,").Append(B(s.VfoALock)).Append(";");
            b.Append("vfo_lock:0,1,").Append(B(s.VfoBLock)).Append(";");
            b.Append("vfo_sync_ex:").Append(B(s.VfoSync)).Append(";");
            b.Append("app_focus:").Append(B(s.AppFocus)).Append(";");
            return b.ToString();
        }
    }

    sealed unsafe public partial class Console
    {
        private P44TCIServer p44TCIServer;
        private bool p44TciInit;

        internal void P44InitTCI()
        {
            P44TCISettings.Load();
            if (!p44TciInit)
            {
                p44TciInit = true;
                this.FormClosed += P44TCIFormClosed;
            }
            P44ApplyTCISettings();
        }

        private void P44TCIFormClosed(object sender, FormClosedEventArgs e)
        {
            if (p44TCIServer != null)
            {
                p44TCIServer.Stop();
                p44TCIServer = null;
            }
        }

        internal void P44ApplyTCISettings()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(P44ApplyTCISettings));
                return;
            }

            if (p44TCIServer != null)
            {
                p44TCIServer.Stop();
                p44TCIServer = null;
            }

            if (P44TCISettings.Enabled)
            {
                p44TCIServer = new P44TCIServer(this, P44TCISettings.Port, P44TCISettings.BindAll);
                if (!p44TCIServer.Start()) p44TCIServer = null;
            }
        }

        internal string P44TCIStatus
        {
            get
            {
                if (!P44TCISettings.Enabled) return "Disabled";
                if (p44TCIServer == null || !p44TCIServer.Running) return "ERROR / port unavailable";
                return "Listening on " + (P44TCISettings.BindAll ? "LAN" : "localhost") + ":" +
                    P44TCISettings.Port.ToString(CultureInfo.InvariantCulture) +
                    "  Clients: " + p44TCIServer.ClientCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        internal P44TCIState P44CaptureTCIState()
        {
            if (InvokeRequired)
                return (P44TCIState)Invoke(new Func<P44TCIState>(P44CaptureTCIState));

            P44TCIState s = new P44TCIState();
            s.Power = PowerOn;
            s.VfoA = (long)Math.Round(VFOAFreq * 1000000.0);
            s.VfoB = (long)Math.Round(VFOBFreq * 1000000.0);
            s.TxFrequency = (long)Math.Round(TXFreq * 1000000.0);
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
            s.Nr = chkNR.Checked;
            s.Nb = chkNB.Checked || chkDSPNB2.Checked;
            s.Anf = chkANF.Checked;
            s.Nf = chkTNF != null && chkTNF.Checked;
            s.Sql = chkSquelch.Checked;
            s.SqlLevel = Squelch;
            s.Drive = PWR;
            s.TuneDrive = TunePower;
            s.Trx = MOX;
            s.Tune = TUN;
            s.VolumeDb = P44LevelToDb(AF);
            s.Mute = MUT;
            s.Mon = MON;
            s.MonVolumeDb = P44LevelToDb(TXAF);
            s.VfoALock = VFOLock;
            s.VfoBLock = VFOLockB;
            s.VfoSync = VFOSync;
            s.AppFocus = ContainsFocus;
            s.IQSampleRate = SampleRateRX1;
            return s;
        }

        private static double P44LevelToDb(int level)
        {
            if (level <= 0) return -60.0;
            double db = 20.0 * Math.Log10(level / 100.0);
            if (db < -60.0) db = -60.0;
            if (db > 0.0) db = 0.0;
            return db;
        }

        private static int P44DbToLevel(double db)
        {
            if (db > 0.0) return Math.Max(0, Math.Min(100, (int)Math.Round(db)));
            if (db <= -60.0) return 0;
            int level = (int)Math.Round(100.0 * Math.Pow(10.0, db / 20.0));
            return Math.Max(0, Math.Min(100, level));
        }

        private static string P44ModeToTCI(DSPMode mode)
        {
            switch (mode)
            {
                case DSPMode.LSB: return "LSB";
                case DSPMode.USB: return "USB";
                case DSPMode.DSB: return "DSB";
                case DSPMode.CWL: return "CWL";
                case DSPMode.CWU: return "CWU";
                case DSPMode.FM: return "FM";
                case DSPMode.AM: return "AM";
                case DSPMode.SAM: return "SAM";
                case DSPMode.DIGL: return "DIGL";
                case DSPMode.DIGU: return "DIGU";
                default: return mode.ToString().ToUpperInvariant();
            }
        }

        private static bool P44TryTCIMode(string text, out DSPMode mode)
        {
            string m = (text ?? String.Empty).Trim().ToUpperInvariant();
            if (m == "LSB") { mode = DSPMode.LSB; return true; }
            if (m == "USB") { mode = DSPMode.USB; return true; }
            if (m == "DSB") { mode = DSPMode.DSB; return true; }
            if (m == "CWL") { mode = DSPMode.CWL; return true; }
            if (m == "CWU" || m == "CW") { mode = DSPMode.CWU; return true; }
            if (m == "FM" || m == "NFM") { mode = DSPMode.FM; return true; }
            if (m == "AM") { mode = DSPMode.AM; return true; }
            if (m == "SAM") { mode = DSPMode.SAM; return true; }
            if (m == "DIGL") { mode = DSPMode.DIGL; return true; }
            if (m == "DIGU") { mode = DSPMode.DIGU; return true; }
            mode = DSPMode.USB;
            return false;
        }

        private static string P44AgcToTCI(AGCMode mode)
        {
            switch (mode)
            {
                case AGCMode.FIXD: return "off";
                case AGCMode.LONG: return "long";
                case AGCMode.SLOW: return "slow";
                case AGCMode.FAST: return "fast";
                case AGCMode.CUSTOM: return "custom";
                case AGCMode.MED: return "normal";
                default: return "normal";
            }
        }

        private static bool P44TryTCIAgc(string text, out AGCMode mode)
        {
            string m = (text ?? String.Empty).Trim().ToLowerInvariant();
            if (m == "off" || m == "fixed") { mode = AGCMode.FIXD; return true; }
            if (m == "long") { mode = AGCMode.LONG; return true; }
            if (m == "slow") { mode = AGCMode.SLOW; return true; }
            if (m == "fast") { mode = AGCMode.FAST; return true; }
            if (m == "custom") { mode = AGCMode.CUSTOM; return true; }
            if (m == "normal" || m == "medium" || m == "med") { mode = AGCMode.MED; return true; }
            mode = AGCMode.MED;
            return false;
        }

        private static bool P44Bool(string s, out bool value)
        {
            return Boolean.TryParse((s ?? String.Empty).Trim(), out value);
        }

        internal bool P44ApplyTCICommand(string command)
        {
            if (InvokeRequired)
                return (bool)Invoke(new Func<string, bool>(P44ApplyTCICommand), command);

            try
            {
                string name;
                string argText;
                int colon = command.IndexOf(':');
                if (colon < 0)
                {
                    name = command.Trim().ToLowerInvariant();
                    argText = String.Empty;
                }
                else
                {
                    name = command.Substring(0, colon).Trim().ToLowerInvariant();
                    argText = command.Substring(colon + 1).Trim();
                }

                string[] a = argText.Length == 0 ? new string[0] : argText.Split(',');
                bool bv;
                int iv;
                long hz;
                double dv;

                if (name == "start") { PowerOn = true; return true; }
                if (name == "stop") { PowerOn = false; return true; }

                if (name == "vfo" && a.Length >= 3 && Int64.TryParse(a[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out hz))
                {
                    if (a[0] == "0" && a[1] == "0") VFOAFreq = hz / 1000000.0;
                    else if (a[0] == "0" && a[1] == "1") VFOBFreq = hz / 1000000.0;
                    return true;
                }
                if (name == "tx_frequency" && a.Length >= 1 && Int64.TryParse(a[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out hz))
                {
                    TXFreq = hz / 1000000.0;
                    return true;
                }
                if (name == "modulation" && a.Length >= 2 && a[0] == "0")
                {
                    DSPMode mode;
                    if (P44TryTCIMode(a[1], out mode)) RX1DSPMode = mode;
                    return true;
                }
                if (name == "rx_filter_band" && a.Length >= 3 && a[0] == "0" &&
                    Int32.TryParse(a[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out iv))
                {
                    int hi;
                    if (Int32.TryParse(a[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out hi))
                        UpdateRX1Filters(iv, hi);
                    return true;
                }
                if (name == "agc_mode" && a.Length >= 2 && a[0] == "0")
                {
                    AGCMode agc;
                    if (P44TryTCIAgc(a[1], out agc)) RX1AGCMode = agc;
                    return true;
                }
                if (name == "agc_gain" && a.Length >= 2 && a[0] == "0" && Int32.TryParse(a[1], out iv))
                {
                    RF = Math.Max(-20, Math.Min(120, iv));
                    return true;
                }
                if (name == "split_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { VFOSplit = bv; return true; }
                if (name == "rit_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { RITOn = bv; return true; }
                if (name == "xit_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { XITOn = bv; return true; }
                if (name == "rit_offset" && a.Length >= 2 && a[0] == "0" && Int32.TryParse(a[1], out iv)) { RITValue = iv; return true; }
                if (name == "xit_offset" && a.Length >= 2 && a[0] == "0" && Int32.TryParse(a[1], out iv)) { XITValue = iv; return true; }
                if (name == "rx_nr_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { CATNR = bv ? 1 : 0; return true; }
                if (name == "rx_nb_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { CATNB1 = bv ? 1 : 0; return true; }
                if (name == "rx_anf_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { CATANF = bv ? 1 : 0; return true; }
                if (name == "rx_nf_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv))
                {
                    if (chkTNF != null) chkTNF.Checked = bv;
                    return true;
                }
                if (name == "sql_enable" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { CATSquelch = bv ? 1 : 0; return true; }
                if (name == "sql_level" && a.Length >= 2 && a[0] == "0" && Int32.TryParse(a[1], out iv)) { Squelch = iv; return true; }
                if (name == "drive" && a.Length >= 2 && a[0] == "0" && Int32.TryParse(a[1], out iv)) { PWR = iv; return true; }
                if (name == "tune_drive" && a.Length >= 2 && a[0] == "0" && Int32.TryParse(a[1], out iv)) { TunePower = Math.Max(0, Math.Min(100, iv)); return true; }
                if (name == "trx" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { MOX = bv; return true; }
                if (name == "tune" && a.Length >= 2 && a[0] == "0" && P44Bool(a[1], out bv)) { TUN = bv; return true; }
                if (name == "volume" && a.Length >= 1 && Double.TryParse(a[0], NumberStyles.Float, CultureInfo.InvariantCulture, out dv)) { AF = P44DbToLevel(dv); return true; }
                if (name == "mute" && a.Length >= 1 && P44Bool(a[0], out bv)) { MUT = bv; return true; }
                if (name == "mon_enable" && a.Length >= 1 && P44Bool(a[0], out bv)) { MON = bv; return true; }
                if (name == "mon_volume" && a.Length >= 1 && Double.TryParse(a[0], NumberStyles.Float, CultureInfo.InvariantCulture, out dv)) { TXAF = P44DbToLevel(dv); return true; }
                if (name == "vfo_lock" && a.Length >= 3 && a[0] == "0" && P44Bool(a[2], out bv))
                {
                    if (a[1] == "0") VFOLock = bv;
                    else if (a[1] == "1") VFOLockB = bv;
                    return true;
                }
                if (name == "lock" && a.Length >= 2 && P44Bool(a[1], out bv))
                {
                    if (a[0] == "0") VFOLock = bv;
                    return true;
                }
                if (name == "vfo_sync_ex" && a.Length >= 1 && P44Bool(a[0], out bv)) { VFOSync = bv; return true; }

                // TCI query or currently read-only command: return current state to caller.
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public partial class Setup
    {
        private TabPage p44TciTab;
        private CheckBox p44TciEnabled;
        private CheckBox p44TciLan;
        private NumericUpDown p44TciPort;
        private Label p44TciStatus;
        private System.Windows.Forms.Timer p44TciUiTimer;
        private bool p44TciLoading;

        internal void P44InitTCIUI()
        {
            if (p44TciTab != null) return;
            P44TCISettings.Load();

            p44TciTab = new TabPage("TCI");
            p44TciTab.Name = "p44TciTab";
            p44TciTab.UseVisualStyleBackColor = true;
            tcSetup.TabPages.Add(p44TciTab);

            GroupBox group = new GroupBox();
            group.Text = "TCI WebSocket Server";
            group.Location = new Point(18, 18);
            group.Size = new Size(570, 190);
            p44TciTab.Controls.Add(group);

            p44TciEnabled = new CheckBox();
            p44TciEnabled.Text = "Enable TCI server";
            p44TciEnabled.AutoSize = true;
            p44TciEnabled.Location = new Point(18, 30);
            p44TciEnabled.CheckedChanged += P44TCISettingChanged;
            group.Controls.Add(p44TciEnabled);

            Label portLabel = new Label();
            portLabel.Text = "Port:";
            portLabel.AutoSize = true;
            portLabel.Location = new Point(18, 63);
            group.Controls.Add(portLabel);

            p44TciPort = new NumericUpDown();
            p44TciPort.Minimum = 1024;
            p44TciPort.Maximum = 65535;
            p44TciPort.Location = new Point(62, 60);
            p44TciPort.Width = 90;
            p44TciPort.ValueChanged += P44TCISettingChanged;
            group.Controls.Add(p44TciPort);

            p44TciLan = new CheckBox();
            p44TciLan.Text = "Allow LAN clients (bind all interfaces)";
            p44TciLan.AutoSize = true;
            p44TciLan.Location = new Point(18, 92);
            p44TciLan.CheckedChanged += P44TCISettingChanged;
            group.Controls.Add(p44TciLan);

            p44TciStatus = new Label();
            p44TciStatus.AutoSize = true;
            p44TciStatus.Location = new Point(18, 124);
            group.Controls.Add(p44TciStatus);

            Label scope = new Label();
            scope.Text = "TCI Core/Control. Audio and IQ streaming are not enabled in this build.";
            scope.AutoSize = true;
            scope.Location = new Point(18, 151);
            group.Controls.Add(scope);

            p44TciLoading = true;
            try
            {
                p44TciEnabled.Checked = P44TCISettings.Enabled;
                p44TciPort.Value = P44TCISettings.Port;
                p44TciLan.Checked = P44TCISettings.BindAll;
            }
            finally { p44TciLoading = false; }

            p44TciUiTimer = new System.Windows.Forms.Timer();
            p44TciUiTimer.Interval = 500;
            p44TciUiTimer.Tick += P44TCIStatusTick;
            p44TciUiTimer.Start();
            P44TCIStatusTick(this, EventArgs.Empty);
        }

        private void P44TCISettingChanged(object sender, EventArgs e)
        {
            if (p44TciLoading) return;
            P44TCISettings.Enabled = p44TciEnabled.Checked;
            P44TCISettings.Port = (int)p44TciPort.Value;
            P44TCISettings.BindAll = p44TciLan.Checked;
            P44TCISettings.Save();
            if (console != null) console.P44ApplyTCISettings();
        }

        private void P44TCIStatusTick(object sender, EventArgs e)
        {
            if (p44TciStatus == null) return;
            p44TciStatus.Text = console == null ? "Console unavailable" : console.P44TCIStatus;
        }
    }
}
