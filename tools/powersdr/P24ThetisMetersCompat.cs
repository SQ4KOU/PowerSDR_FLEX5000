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
}
