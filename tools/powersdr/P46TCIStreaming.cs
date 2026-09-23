using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace PowerSDR
{
    internal enum P46TCIStreamType : uint
    {
        IQ_STREAM = 0,
        RX_AUDIO_STREAM = 1,
        TX_AUDIO_STREAM = 2,
        TX_CHRONO = 3,
        LINEOUT_STREAM = 4
    }

    internal enum P46TCISampleType : uint
    {
        INT16 = 0,
        INT24 = 1,
        INT32 = 2,
        FLOAT32 = 3
    }

    internal sealed class P46TxBlock
    {
        internal float[] Left;
        internal float[] Right;
        internal int Offset;
        internal int SampleRate;
    }

    internal sealed class P46RxBlock
    {
        internal bool IsIQ;
        internal int SampleRate;
        internal int Frames;
        internal float[] A;
        internal float[] B;
    }

    internal sealed class P46ClientState
    {
        internal readonly P44TCIClient Client;
        internal readonly object StreamGate = new object();
        internal readonly object TxGate = new object();

        internal bool IQEnabled;
        internal bool AudioEnabled;
        internal int AudioSampleRate = 48000;
        internal P46TCISampleType AudioSampleType = P46TCISampleType.FLOAT32;
        internal int AudioChannels = 2;
        internal int AudioPacketFrames = 2048;
        internal int TxBufferingMs = 50;

        internal readonly List<float> PendingAudio = new List<float>(8192);

        internal bool TxActive;
        internal readonly Queue<P46TxBlock> TxBlocks = new Queue<P46TxBlock>();
        internal int TxQueuedFrames;
        internal bool TxHaveCurrent;
        internal float TxCurrentL;
        internal float TxCurrentR;
        internal double TxPhase;
        internal int TxCurrentRate = 48000;
        internal bool NeedChrono;

        internal P46ClientState(P44TCIClient client)
        {
            Client = client;
        }
    }

    internal static unsafe class P46TCIStreaming
    {
        private const int MaxRxQueueBlocks = 16;
        private const int MaxTxQueueFrames = 96000;

        private static readonly object clientsGate = new object();
        private static readonly Dictionary<P44TCIClient, P46ClientState> clients =
            new Dictionary<P44TCIClient, P46ClientState>();

        private static readonly object rxQueueGate = new object();
        private static readonly Queue<P46RxBlock> rxQueue = new Queue<P46RxBlock>();

        private static readonly AutoResetEvent workerEvent = new AutoResetEvent(false);
        private static Thread workerThread;
        private static bool workerStarted;

        private static volatile bool anyIQ;
        private static volatile bool anyAudio;
        private static volatile bool anyTx;
        private static P46ClientState txOwner;

        internal static void RegisterClient(P44TCIClient client)
        {
            if (client == null) return;

            lock (clientsGate)
            {
                if (!clients.ContainsKey(client))
                    clients.Add(client, new P46ClientState(client));
                RefreshFlagsLocked();
                EnsureWorkerLocked();
            }
        }

        internal static void UnregisterClient(P44TCIClient client)
        {
            if (client == null) return;

            lock (clientsGate)
            {
                P46ClientState state;
                if (clients.TryGetValue(client, out state))
                {
                    if (txOwner == state) txOwner = null;
                    clients.Remove(client);
                }
                RefreshFlagsLocked();
            }

            workerEvent.Set();
        }

        private static void EnsureWorkerLocked()
        {
            if (workerStarted) return;
            workerStarted = true;
            workerThread = new Thread(WorkerLoop);
            workerThread.IsBackground = true;
            workerThread.Name = "P46 TCI Streaming";
            workerThread.Priority = ThreadPriority.AboveNormal;
            workerThread.Start();
        }

        private static void RefreshFlagsLocked()
        {
            bool iq = false;
            bool audio = false;
            foreach (P46ClientState state in clients.Values)
            {
                iq |= state.IQEnabled;
                audio |= state.AudioEnabled;
            }

            anyIQ = iq;
            anyAudio = audio;
            anyTx = txOwner != null && txOwner.TxActive;
        }

        private static bool TryGetState(P44TCIClient client, out P46ClientState state)
        {
            lock (clientsGate)
                return clients.TryGetValue(client, out state);
        }

        internal static void SendInitial(P44TCIClient client)
        {
            P46ClientState state;
            if (!TryGetState(client, out state)) return;

            lock (state.StreamGate)
            {
                client.SendText("audio_stream_sample_type:" + SampleTypeName(state.AudioSampleType) + ";");
                client.SendText("audio_stream_channels:" + state.AudioChannels.ToString(CultureInfo.InvariantCulture) + ";");
                client.SendText("audio_stream_samples:" + state.AudioPacketFrames.ToString(CultureInfo.InvariantCulture) + ";");
                client.SendText("tx_stream_audio_buffering:" + state.TxBufferingMs.ToString(CultureInfo.InvariantCulture) + ";");
            }
        }

        internal static bool HandleCommand(P44TCIClient client, string name, string[] args)
        {
            P46ClientState state;
            if (!TryGetState(client, out state)) return false;

            int receiver;
            int n;

            switch (name)
            {
                case "iq_start":
                case "iq_stop":
                    if (args.Length != 1 ||
                        !Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out receiver) ||
                        receiver != 0)
                        return true;

                    lock (clientsGate)
                    {
                        state.IQEnabled = name == "iq_start";
                        RefreshFlagsLocked();
                    }
                    client.SendText((state.IQEnabled ? "iq_start:0;" : "iq_stop:0;"));
                    workerEvent.Set();
                    return true;

                case "audio_start":
                case "audio_stop":
                    if (args.Length != 1 ||
                        !Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out receiver) ||
                        receiver != 0)
                        return true;

                    lock (clientsGate)
                    {
                        state.AudioEnabled = name == "audio_start";
                        RefreshFlagsLocked();
                    }
                    if (!state.AudioEnabled)
                    {
                        lock (state.StreamGate) state.PendingAudio.Clear();
                    }
                    client.SendText((state.AudioEnabled ? "audio_start:0;" : "audio_stop:0;"));
                    workerEvent.Set();
                    return true;

                case "audio_samplerate":
                    lock (state.StreamGate)
                    {
                        if (args.Length == 1 &&
                            Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out n) &&
                            (n == 8000 || n == 12000 || n == 24000 || n == 48000))
                        {
                            state.AudioSampleRate = n;
                            state.PendingAudio.Clear();
                            if (state.AudioPacketFrames == 2048)
                                state.AudioPacketFrames = DefaultPacketFrames(n);
                        }
                        client.SendText("audio_samplerate:" + state.AudioSampleRate.ToString(CultureInfo.InvariantCulture) + ";");
                    }
                    return true;

                case "audio_stream_sample_type":
                    lock (state.StreamGate)
                    {
                        if (args.Length == 1)
                            state.AudioSampleType = ParseSampleType(args[0]);
                        client.SendText("audio_stream_sample_type:" + SampleTypeName(state.AudioSampleType) + ";");
                    }
                    return true;

                case "audio_stream_channels":
                    lock (state.StreamGate)
                    {
                        if (args.Length == 1 &&
                            Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out n) &&
                            (n == 1 || n == 2))
                        {
                            state.AudioChannels = n;
                            state.PendingAudio.Clear();
                        }
                        client.SendText("audio_stream_channels:" + state.AudioChannels.ToString(CultureInfo.InvariantCulture) + ";");
                    }
                    return true;

                case "audio_stream_samples":
                    lock (state.StreamGate)
                    {
                        if (args.Length == 1 &&
                            Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out n) &&
                            n >= 100 && n <= 2048)
                        {
                            state.AudioPacketFrames = n;
                            state.PendingAudio.Clear();
                        }
                        client.SendText("audio_stream_samples:" + state.AudioPacketFrames.ToString(CultureInfo.InvariantCulture) + ";");
                    }
                    return true;

                case "tx_stream_audio_buffering":
                    lock (state.StreamGate)
                    {
                        if (args.Length == 1 &&
                            Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out n) &&
                            n >= 50 && n <= 500)
                            state.TxBufferingMs = n;

                        client.SendText("tx_stream_audio_buffering:" + state.TxBufferingMs.ToString(CultureInfo.InvariantCulture) + ";");
                    }
                    return true;

                case "trx":
                    HandleTrxStreamingState(state, args);
                    return false;
            }

            return false;
        }

        private static void HandleTrxStreamingState(P46ClientState state, string[] args)
        {
            if (args == null || args.Length < 2) return;

            int receiver;
            bool mox;
            if (!Int32.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out receiver) || receiver != 0)
                return;
            if (!Boolean.TryParse(args[1], out mox))
                return;

            bool useTciAudio = args.Length > 2 &&
                String.Equals(args[2].Trim(), "tci", StringComparison.OrdinalIgnoreCase);

            lock (clientsGate)
            {
                if (mox && useTciAudio)
                {
                    if (txOwner == null || txOwner == state)
                    {
                        txOwner = state;
                        state.TxActive = true;
                        lock (state.TxGate)
                        {
                            ClearTxLocked(state);
                            state.NeedChrono = true;
                        }
                    }
                }
                else
                {
                    if (txOwner == state) txOwner = null;
                    state.TxActive = false;
                    lock (state.TxGate) ClearTxLocked(state);
                }

                RefreshFlagsLocked();
            }

            workerEvent.Set();
        }

        internal static void HandleBinary(P44TCIClient client, byte[] payload)
        {
            if (payload == null || payload.Length < 64) return;

            P46ClientState state;
            if (!TryGetState(client, out state)) return;
            if (!state.TxActive) return;

            int receiver = BitConverter.ToInt32(payload, 0);
            int sampleRate = BitConverter.ToInt32(payload, 4);
            P46TCISampleType sampleType = (P46TCISampleType)BitConverter.ToUInt32(payload, 8);
            int length = BitConverter.ToInt32(payload, 20);
            P46TCIStreamType streamType = (P46TCIStreamType)BitConverter.ToUInt32(payload, 24);
            int channels = BitConverter.ToInt32(payload, 28);

            if (receiver != 0 || streamType != P46TCIStreamType.TX_AUDIO_STREAM || length <= 0)
                return;

            if (channels != 1 && channels != 2) channels = 2;
            if (sampleRate <= 0) sampleRate = state.AudioSampleRate;

            int bytesPerSample = BytesPerSample(sampleType);
            int availableValues = (payload.Length - 64) / bytesPerSample;
            int valueCount = Math.Min(length, availableValues);
            if (channels == 2) valueCount -= valueCount & 1;
            if (valueCount <= 0) return;

            float[] decoded = DecodeSamples(payload, 64, valueCount, sampleType);
            int frames = channels == 1 ? decoded.Length : decoded.Length / 2;
            float[] left = new float[frames];
            float[] right = new float[frames];

            for (int i = 0; i < frames; i++)
            {
                float l = decoded[channels == 1 ? i : 2 * i];
                float r = channels == 1 ? l : decoded[2 * i + 1];
                left[i] = ClampStreamSample(l);
                right[i] = ClampStreamSample(r);
            }

            lock (state.TxGate)
            {
                P46TxBlock block = new P46TxBlock();
                block.Left = left;
                block.Right = right;
                block.SampleRate = sampleRate;
                state.TxBlocks.Enqueue(block);
                state.TxQueuedFrames += frames;
                state.NeedChrono = false;

                while (state.TxQueuedFrames > MaxTxQueueFrames && state.TxBlocks.Count > 1)
                {
                    P46TxBlock dropped = state.TxBlocks.Dequeue();
                    state.TxQueuedFrames -= Math.Max(0, dropped.Left.Length - dropped.Offset);
                }
            }
        }

        private static float ClampStreamSample(float v)
        {
            if (Single.IsNaN(v) || Single.IsInfinity(v)) return 0.0f;
            if (v > 4.0f) return 4.0f;
            if (v < -4.0f) return -4.0f;
            return v;
        }

        internal static void CaptureIQ(float* iPtr, float* qPtr, int frames, int sampleRate)
        {
            if (!anyIQ || iPtr == null || qPtr == null || frames <= 0) return;

            float[] iData = new float[frames];
            float[] qData = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                iData[i] = iPtr[i];
                qData[i] = qPtr[i];
            }

            EnqueueRxBlock(true, iData, qData, frames, sampleRate);
        }

        internal static void CaptureAudio(float* leftPtr, float* rightPtr, int frames, int sampleRate)
        {
            if (!anyAudio || leftPtr == null || rightPtr == null || frames <= 0) return;

            float[] left = new float[frames];
            float[] right = new float[frames];
            for (int i = 0; i < frames; i++)
            {
                left[i] = leftPtr[i];
                right[i] = rightPtr[i];
            }

            EnqueueRxBlock(false, left, right, frames, sampleRate);
        }

        private static void EnqueueRxBlock(bool iq, float[] a, float[] b, int frames, int sampleRate)
        {
            P46RxBlock block = new P46RxBlock();
            block.IsIQ = iq;
            block.A = a;
            block.B = b;
            block.Frames = frames;
            block.SampleRate = sampleRate;

            lock (rxQueueGate)
            {
                if (rxQueue.Count >= MaxRxQueueBlocks)
                    rxQueue.Dequeue();
                rxQueue.Enqueue(block);
            }

            workerEvent.Set();
        }

        internal static bool FillTxAudio(float* leftPtr, float* rightPtr, int frames, int targetSampleRate)
        {
            if (!anyTx || leftPtr == null || rightPtr == null || frames <= 0)
                return false;

            P46ClientState state;
            lock (clientsGate) state = txOwner;
            if (state == null || !state.TxActive) return false;

            bool underflow = false;

            lock (state.TxGate)
            {
                for (int i = 0; i < frames; i++)
                {
                    if (!state.TxHaveCurrent)
                    {
                        float l;
                        float r;
                        int rate;
                        if (!TryTakeTxFrameLocked(state, out l, out r, out rate))
                        {
                            leftPtr[i] = 0.0f;
                            rightPtr[i] = 0.0f;
                            underflow = true;
                            continue;
                        }

                        state.TxCurrentL = l;
                        state.TxCurrentR = r;
                        state.TxCurrentRate = rate > 0 ? rate : 48000;
                        state.TxHaveCurrent = true;
                        state.TxPhase = 0.0;
                    }

                    leftPtr[i] = state.TxCurrentL;
                    rightPtr[i] = state.TxCurrentR;

                    int dstRate = targetSampleRate > 0 ? targetSampleRate : state.TxCurrentRate;
                    double step = state.TxCurrentRate / (double)dstRate;
                    state.TxPhase += step;

                    while (state.TxPhase >= 1.0)
                    {
                        float l;
                        float r;
                        int rate;
                        if (!TryTakeTxFrameLocked(state, out l, out r, out rate))
                        {
                            state.TxHaveCurrent = false;
                            underflow = true;
                            break;
                        }

                        state.TxCurrentL = l;
                        state.TxCurrentR = r;
                        state.TxCurrentRate = rate > 0 ? rate : state.TxCurrentRate;
                        state.TxPhase -= 1.0;
                    }
                }

                int threshold = Math.Max(state.AudioPacketFrames * 2,
                    state.AudioSampleRate * state.TxBufferingMs / 1000);
                if (state.TxQueuedFrames < threshold || underflow)
                    state.NeedChrono = true;
            }

            workerEvent.Set();
            return true;
        }

        private static bool TryTakeTxFrameLocked(P46ClientState state, out float left, out float right, out int sampleRate)
        {
            while (state.TxBlocks.Count > 0)
            {
                P46TxBlock block = state.TxBlocks.Peek();
                if (block.Offset >= block.Left.Length)
                {
                    state.TxBlocks.Dequeue();
                    continue;
                }

                int pos = block.Offset++;
                left = block.Left[pos];
                right = block.Right[pos];
                sampleRate = block.SampleRate;
                state.TxQueuedFrames = Math.Max(0, state.TxQueuedFrames - 1);
                return true;
            }

            left = 0.0f;
            right = 0.0f;
            sampleRate = state.AudioSampleRate;
            return false;
        }

        private static void ClearTxLocked(P46ClientState state)
        {
            state.TxBlocks.Clear();
            state.TxQueuedFrames = 0;
            state.TxHaveCurrent = false;
            state.TxPhase = 0.0;
            state.NeedChrono = false;
        }

        private static void WorkerLoop()
        {
            while (true)
            {
                workerEvent.WaitOne(10);

                for (int n = 0; n < 16; n++)
                {
                    P46RxBlock block = null;
                    lock (rxQueueGate)
                    {
                        if (rxQueue.Count > 0)
                            block = rxQueue.Dequeue();
                    }

                    if (block == null) break;
                    try { ProcessRxBlock(block); }
                    catch (Exception ex) { P44TCILog.Write("P46 RX STREAM ERROR " + ex.Message); }
                }

                try { PumpTxChrono(); }
                catch (Exception ex) { P44TCILog.Write("P46 TX CHRONO ERROR " + ex.Message); }
            }
        }

        private static void ProcessRxBlock(P46RxBlock block)
        {
            P46ClientState[] snapshot;
            lock (clientsGate)
            {
                snapshot = new P46ClientState[clients.Count];
                clients.Values.CopyTo(snapshot, 0);
            }

            foreach (P46ClientState state in snapshot)
            {
                if (block.IsIQ)
                {
                    if (!state.IQEnabled) continue;
                    SendIQBlock(state, block);
                }
                else
                {
                    if (!state.AudioEnabled) continue;
                    SendAudioBlock(state, block);
                }
            }
        }

        private static void SendIQBlock(P46ClientState state, P46RxBlock block)
        {
            float[] interleaved = new float[block.Frames * 2];
            for (int i = 0; i < block.Frames; i++)
            {
                interleaved[2 * i] = block.A[i];
                interleaved[2 * i + 1] = block.B[i];
            }

            byte[] data = EncodeSamples(interleaved, P46TCISampleType.FLOAT32);
            state.Client.SendFrame(2, BuildStreamPayload(
                0,
                block.SampleRate,
                P46TCISampleType.FLOAT32,
                interleaved.Length,
                P46TCIStreamType.IQ_STREAM,
                2,
                data));
        }

        private static void SendAudioBlock(P46ClientState state, P46RxBlock block)
        {
            lock (state.StreamGate)
            {
                float[] left;
                float[] right;
                ResamplePair(block.A, block.B, block.Frames, block.SampleRate,
                    state.AudioSampleRate, out left, out right);

                if (state.AudioChannels == 1)
                {
                    for (int i = 0; i < left.Length; i++)
                        state.PendingAudio.Add(0.5f * (left[i] + right[i]));
                }
                else
                {
                    for (int i = 0; i < left.Length; i++)
                    {
                        state.PendingAudio.Add(left[i]);
                        state.PendingAudio.Add(right[i]);
                    }
                }

                int packetValues = state.AudioPacketFrames * state.AudioChannels;
                while (packetValues > 0 && state.PendingAudio.Count >= packetValues)
                {
                    float[] packet = state.PendingAudio.GetRange(0, packetValues).ToArray();
                    state.PendingAudio.RemoveRange(0, packetValues);

                    byte[] data = EncodeSamples(packet, state.AudioSampleType);
                    state.Client.SendFrame(2, BuildStreamPayload(
                        0,
                        state.AudioSampleRate,
                        state.AudioSampleType,
                        packet.Length,
                        P46TCIStreamType.RX_AUDIO_STREAM,
                        state.AudioChannels,
                        data));
                }
            }
        }

        private static void ResamplePair(
            float[] sourceLeft,
            float[] sourceRight,
            int sourceFrames,
            int sourceRate,
            int targetRate,
            out float[] left,
            out float[] right)
        {
            if (sourceFrames <= 0)
            {
                left = new float[0];
                right = new float[0];
                return;
            }

            if (sourceRate <= 0 || targetRate <= 0 || sourceRate == targetRate)
            {
                left = new float[sourceFrames];
                right = new float[sourceFrames];
                Array.Copy(sourceLeft, left, sourceFrames);
                Array.Copy(sourceRight, right, sourceFrames);
                return;
            }

            int targetFrames = Math.Max(1,
                (int)Math.Round(sourceFrames * targetRate / (double)sourceRate));
            left = new float[targetFrames];
            right = new float[targetFrames];

            if (targetFrames == 1 || sourceFrames == 1)
            {
                left[0] = sourceLeft[0];
                right[0] = sourceRight[0];
                return;
            }

            double scale = (sourceFrames - 1) / (double)(targetFrames - 1);
            for (int i = 0; i < targetFrames; i++)
            {
                double pos = i * scale;
                int p0 = (int)pos;
                int p1 = Math.Min(sourceFrames - 1, p0 + 1);
                float frac = (float)(pos - p0);
                left[i] = sourceLeft[p0] + (sourceLeft[p1] - sourceLeft[p0]) * frac;
                right[i] = sourceRight[p0] + (sourceRight[p1] - sourceRight[p0]) * frac;
            }
        }

        private static void PumpTxChrono()
        {
            P46ClientState state;
            lock (clientsGate) state = txOwner;
            if (state == null || !state.TxActive) return;

            bool send;
            int sampleRate;
            int frames;
            int channels;
            P46TCISampleType sampleType;

            lock (state.TxGate)
            {
                send = state.NeedChrono;
                if (send) state.NeedChrono = false;
            }

            if (!send) return;

            lock (state.StreamGate)
            {
                sampleRate = state.AudioSampleRate;
                frames = state.AudioPacketFrames;
                channels = state.AudioChannels;
                sampleType = state.AudioSampleType;
            }

            state.Client.SendFrame(2, BuildStreamPayload(
                0,
                sampleRate,
                sampleType,
                frames * Math.Max(1, channels),
                P46TCIStreamType.TX_CHRONO,
                channels,
                new byte[0]));
        }

        private static int DefaultPacketFrames(int rate)
        {
            switch (rate)
            {
                case 8000: return 256;
                case 12000: return 512;
                case 24000: return 1024;
                default: return 2048;
            }
        }

        private static P46TCISampleType ParseSampleType(string text)
        {
            switch ((text ?? String.Empty).Trim().ToLowerInvariant())
            {
                case "int16": return P46TCISampleType.INT16;
                case "int24": return P46TCISampleType.INT24;
                case "int32": return P46TCISampleType.INT32;
                default: return P46TCISampleType.FLOAT32;
            }
        }

        private static string SampleTypeName(P46TCISampleType type)
        {
            switch (type)
            {
                case P46TCISampleType.INT16: return "int16";
                case P46TCISampleType.INT24: return "int24";
                case P46TCISampleType.INT32: return "int32";
                default: return "float32";
            }
        }

        private static int BytesPerSample(P46TCISampleType type)
        {
            switch (type)
            {
                case P46TCISampleType.INT16: return 2;
                case P46TCISampleType.INT24: return 3;
                case P46TCISampleType.INT32:
                case P46TCISampleType.FLOAT32:
                default: return 4;
            }
        }

        private static void WriteUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static byte[] BuildStreamPayload(
            int receiver,
            int sampleRate,
            P46TCISampleType sampleType,
            int length,
            P46TCIStreamType streamType,
            int channels,
            byte[] samplePayload)
        {
            int dataLength = samplePayload == null ? 0 : samplePayload.Length;
            byte[] payload = new byte[64 + dataLength];
            int offset = 0;

            WriteUInt32(payload, offset, (uint)receiver); offset += 4;
            WriteUInt32(payload, offset, (uint)sampleRate); offset += 4;
            WriteUInt32(payload, offset, (uint)sampleType); offset += 4;
            WriteUInt32(payload, offset, 0); offset += 4;
            WriteUInt32(payload, offset, 0); offset += 4;
            WriteUInt32(payload, offset, (uint)length); offset += 4;
            WriteUInt32(payload, offset, (uint)streamType); offset += 4;
            WriteUInt32(payload, offset, (uint)channels); offset += 4;

            for (int i = 0; i < 8; i++, offset += 4)
                WriteUInt32(payload, offset, 0);

            if (dataLength > 0)
                Buffer.BlockCopy(samplePayload, 0, payload, 64, dataLength);

            return payload;
        }

        private static byte[] EncodeSamples(float[] samples, P46TCISampleType type)
        {
            if (samples == null || samples.Length == 0) return new byte[0];

            int bytes = BytesPerSample(type);
            byte[] data = new byte[samples.Length * bytes];

            if (type == P46TCISampleType.FLOAT32)
            {
                Buffer.BlockCopy(samples, 0, data, 0, data.Length);
                return data;
            }

            int offset = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float v = Math.Max(-1.0f, Math.Min(1.0f, samples[i]));
                switch (type)
                {
                    case P46TCISampleType.INT16:
                        short s16 = (short)Math.Round(v * Int16.MaxValue);
                        data[offset++] = (byte)(s16 & 0xFF);
                        data[offset++] = (byte)((s16 >> 8) & 0xFF);
                        break;

                    case P46TCISampleType.INT24:
                        int s24 = (int)Math.Round(v * 8388607.0f);
                        data[offset++] = (byte)(s24 & 0xFF);
                        data[offset++] = (byte)((s24 >> 8) & 0xFF);
                        data[offset++] = (byte)((s24 >> 16) & 0xFF);
                        break;

                    case P46TCISampleType.INT32:
                        int s32 = (int)Math.Round(v * Int32.MaxValue);
                        data[offset++] = (byte)(s32 & 0xFF);
                        data[offset++] = (byte)((s32 >> 8) & 0xFF);
                        data[offset++] = (byte)((s32 >> 16) & 0xFF);
                        data[offset++] = (byte)((s32 >> 24) & 0xFF);
                        break;
                }
            }

            return data;
        }

        private static float[] DecodeSamples(
            byte[] payload,
            int offset,
            int count,
            P46TCISampleType type)
        {
            float[] result = new float[count];

            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case P46TCISampleType.INT16:
                        result[i] = BitConverter.ToInt16(payload, offset) / 32768.0f;
                        offset += 2;
                        break;

                    case P46TCISampleType.INT24:
                        int s24 = payload[offset] |
                            (payload[offset + 1] << 8) |
                            (payload[offset + 2] << 16);
                        if ((s24 & 0x800000) != 0)
                            s24 |= unchecked((int)0xFF000000);
                        result[i] = s24 / 8388608.0f;
                        offset += 3;
                        break;

                    case P46TCISampleType.INT32:
                        result[i] = (float)(BitConverter.ToInt32(payload, offset) / 2147483648.0);
                        offset += 4;
                        break;

                    case P46TCISampleType.FLOAT32:
                    default:
                        result[i] = BitConverter.ToSingle(payload, offset);
                        offset += 4;
                        break;
                }
            }

            return result;
        }
    }
}
