// P24 compatibility boundary for native Thetis MeterManager on PowerSDR FLEX-5000.
// Only host/backend shims live here. Thetis UI/container/renderer code is kept separate.
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;

namespace PowerSDR
{
    public sealed class P24AdaptorInfo
    {
        public int VendorId { get; set; }
        public int DeviceId { get; set; }
    }

    public static class MeterScriptEngine
    {
        public sealed class Snapshot
        {
            public Dictionary<string, object> Common;
            public Dictionary<string, object>[] Banks;
        }

        private static readonly object Sync = new object();
        private static readonly Dictionary<int, string> Conditions = new Dictionary<int, string>();
        private static int _next;
        public static bool IsInBatch { get; private set; }

        public static void Start(Func<Snapshot> provider, int interval, int bankCount) { }
        public static void Stop() { lock(Sync) Conditions.Clear(); }
        public static void BeginBatch() { IsInBatch = true; }
        public static void EndBatch() { IsInBatch = false; }
        public static int RegisterLed() { lock(Sync) return ++_next; }
        public static void UnregisterLed(int id) { lock(Sync) Conditions.Remove(id); }
        public static bool SetCondition(int id, string condition)
        {
            lock(Sync) Conditions[id] = condition ?? "";
            return true;
        }
        public static bool ReadResult(int id) { return false; }
    }

    public class ImageFetcher
    {
        public enum State { OK=0, ERROR_NO_SUITABLE_IMAGE, ERROR_URL_ISSUE, ERROR_IMAGE_CONVERSION_PROBLEM, WAITING, GATHERING_IMAGES, IDLE=99 }
        public sealed class StateEventArgs : EventArgs
        {
            public Guid Guid { get; private set; }
            public State WebImageState { get; private set; }
            public StateEventArgs(Guid guid, State state) { Guid=guid; WebImageState=state; }
        }

        private sealed class Entry
        {
            public string Url;
            public int Interval;
            public bool File;
            public bool Bypass;
            public List<Image> Images = new List<Image>();
        }

        private readonly ConcurrentDictionary<Guid,Entry> _entries = new ConcurrentDictionary<Guid,Entry>();
        public event EventHandler<Guid> ImagesObtained;
        public event EventHandler<StateEventArgs> StateChanged;
        public string Version { get; set; }

        public Guid RegisterURL(string url, int timeout_secs, int image_limit, bool file, bool bypass_cache=false)
        {
            Guid id=Guid.NewGuid();
            Entry e=new Entry{Url=url,Interval=timeout_secs,File=file,Bypass=bypass_cache};
            _entries[id]=e;
            ThreadPool.QueueUserWorkItem(delegate { Fetch(id,e); });
            return id;
        }
        public List<Image> LatestImages(Guid id)
        {
            Entry e;
            if(!_entries.TryGetValue(id,out e)) return new List<Image>();
            lock(e.Images) return new List<Image>(e.Images);
        }
        public void UpdateInterval(Guid id,int interval) { Entry e; if(_entries.TryGetValue(id,out e)) e.Interval=interval; }
        public void UpdateBypassCache(Guid id,bool bypass) { Entry e; if(_entries.TryGetValue(id,out e)) e.Bypass=bypass; }
        public void StopFetching(Guid id)
        {
            Entry e;
            if(_entries.TryRemove(id,out e))
            {
                lock(e.Images){ foreach(Image i in e.Images) try{i.Dispose();}catch{} e.Images.Clear(); }
            }
        }
        public void Shutdown()
        {
            foreach(Guid id in _entries.Keys) StopFetching(id);
        }

        private void Fetch(Guid id, Entry e)
        {
            EventHandler<StateEventArgs> sh=StateChanged;
            if(sh!=null) sh(this,new StateEventArgs(id,State.GATHERING_IMAGES));
            try
            {
                Image img=null;
                if(e.File)
                {
                    using(FileStream fs=new FileStream(e.Url,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
                    using(Image src=Image.FromStream(fs)) img=new Bitmap(src);
                }
                else
                {
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                    using(WebClient wc=new WebClient())
                    {
                        wc.Headers[HttpRequestHeader.UserAgent]="PowerSDR-SQ4KOU-P24-ThetisMeters";
                        string url=e.Url;
                        if(e.Bypass) url+=(url.Contains("?")?"&":"?")+"t="+DateTime.UtcNow.Ticks;
                        byte[] b=wc.DownloadData(url);
                        using(MemoryStream ms=new MemoryStream(b))
                        using(Image src=Image.FromStream(ms)) img=new Bitmap(src);
                    }
                }
                lock(e.Images){ foreach(Image old in e.Images) try{old.Dispose();}catch{} e.Images.Clear(); if(img!=null)e.Images.Add(img); }
                if(sh!=null) sh(this,new StateEventArgs(id,img==null?State.ERROR_NO_SUITABLE_IMAGE:State.OK));
                EventHandler<Guid> ih=ImagesObtained; if(ih!=null) ih(this,id);
            }
            catch
            {
                if(sh!=null) sh(this,new StateEventArgs(id,State.ERROR_URL_ISSUE));
            }
        }
    }

    public static class ThetisBotDiscord
    {
        public static string GetMessagesString(ulong channel, int part, bool general) { return ""; }
    }

    public static class BandStackManager
    {
        public static string BandToString(Band b) { return b.ToString(); }
    }

    public sealed class RecordingDetails
    {
        public string Band { get; set; }
        public string Frequency { get; set; }
        public string Mode { get; set; }
        public DateTime UtcTime { get; set; }
    }


    // Compile/runtime adapter for Thetis voice-record meter items.
    // The final P24 mapping will bridge these calls to PowerSDR's native wave subsystem.
    public sealed class clsAudioRecordPlayback
    {
        public sealed class RecordingJsonModel
        {
            public string utc_time { get; set; } = "";
            public string frequency { get; set; } = "";
            public string mode { get; set; } = "";
            public string band { get; set; } = "";
            public string wav_file { get; set; } = "";
            public long wav_file_size_bytes { get; set; }
            public string wav_file_last_write_utc { get; set; } = "";
            public double play_duration_seconds { get; set; }
            public int sample_rate { get; set; }
            public short bit_depth { get; set; }
            public short channels { get; set; }
            public short format_tag { get; set; }
            public string tag_description { get; set; } = "";
            public string mp3_file { get; set; } = "";
            public long mp3_file_size_bytes { get; set; }
        }

        public event Action<bool,string,string> RecordingChanged;
        public event Action<bool,string,string,bool> PlayingChanged;
        public event Action<string,RecordingJsonModel> RecordingJsonWritten;

        public string AudioFolder { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "PowerSDR");
        public int InputPCDeviceID { get; set; } = -1;
        public int OutputPCDeviceID { get; set; } = -1;
        public bool IsPlaying { get; private set; }
        public bool IsRecording { get; private set; }

        public bool CanBePlayed(string filepath) { return !String.IsNullOrWhiteSpace(filepath) && File.Exists(filepath); }
        public bool DeleteRecording(string full_path, out string error, bool delete_containing_folder_if_empty=false)
        {
            error=null;
            try { if(!String.IsNullOrWhiteSpace(full_path) && File.Exists(full_path)) File.Delete(full_path); return true; }
            catch(Exception ex){ error=ex.Message; return false; }
        }
        public bool GetJSONDetailsFromFile(string full_path_file, out RecordingJsonModel json_data)
        {
            json_data=null; return !String.IsNullOrWhiteSpace(full_path_file) && File.Exists(full_path_file);
        }
        public bool PlayFileViaWDSP(string play_id,string full_path,int wfw_id,out string error,double adjustGain_dB=0,bool ignore_temp_changes=false)
        { error="WDSP playback is not mapped on FLEX-5000"; return false; }
        public bool PlayFileViaPCAudio(string play_id,string full_path,int pcAudioDeviceOutputId,out string error)
        { error="PC playback adapter pending"; return false; }
        public string RecordToFileFromWDSP(string record_id,string full_path,int wfw_id,out string error,bool remove_if_file_exists=false,RecordingDetails details=null,bool ignore_temp_changes=false)
        { error="WDSP recording is not mapped on FLEX-5000"; return null; }
        public string RecordToFileFromPCAudio(string record_id,string full_path,int pcAudioDeviceInputId,out string error,bool remove_if_file_exists=false,RecordingDetails details=null)
        { error="PC recording adapter pending"; return null; }
        public bool StopPlayback(out string error) { error=null; IsPlaying=false; var h=PlayingChanged; if(h!=null) h(false,"","",false); return true; }
        public bool StopRecord(out string error) { error=null; IsRecording=false; var h=RecordingChanged; if(h!=null) h(false,"",""); return true; }
    }

    // HPSDR spectrum object compatibility surface. It deliberately does not
    // import ChannelMaster/WDSP. Mini-spectrum will be bound to PowerSDR display data later.
    public sealed class SpecHPSDR
    {
        public SpecHPSDR(int id) { }
        public bool Update { get; set; }
        public int FrameRate { get; set; }
        public int PixelOut { get; set; }
        public bool IgnoreFrequencyOffset { get; set; }
        public int Pixels { get; set; }
        public bool AverageOn { get; set; }
        public int DetTypePan { get; set; }
        public double AvTau { get; set; }
        public int FFTSize { get; set; }
        public int BlockSize { get; set; }
        public int SampleRate { get; set; }
        public int WindowType { get; set; }
        public int AverageMode { get; set; }
        public bool NormOneHzPan { get; set; }
        public int LowFreq { get; private set; }
        public int HighFreq { get; private set; }
        public double PanSlider { get; set; }
        public void initAnalyzer() { }
        public void resetPixelBuffers() { }
        public void ZoomToBandwidth(double hz) { }
        public (int,int) GetFrequencyExtents(double zoom,double pan) { return (LowFreq,HighFreq); }
    }

    public static unsafe class SpecHPSDRDLL
    {
        public static void GetPixels(int disp,int channel,float* ptr,ref int flag) { flag=0; }
    }

    public sealed class P24SpecRegistry
    {
        public SpecHPSDR GetSpecRX(int id) { return new SpecHPSDR(id); }
    }

    sealed unsafe partial class Console
    {
        private clsAudioRecordPlayback _p24Arp;
        private P24SpecRegistry _p24SpecRx;
        public clsAudioRecordPlayback ARP
        {
            get { if(_p24Arp==null) _p24Arp=new clsAudioRecordPlayback(); return _p24Arp; }
        }
        public P24SpecRegistry specRX
        {
            get { if(_p24SpecRx==null) _p24SpecRx=new P24SpecRegistry(); return _p24SpecRx; }
        }
    }
}
