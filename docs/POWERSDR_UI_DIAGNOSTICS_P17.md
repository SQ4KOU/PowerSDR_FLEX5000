# PowerSDR FLEX-5000 P17 — UI diagnostics

Base: `feature/ke9ns-2.8.0.336-p14-stability-baseline`.

P17 is measurement-only. It does not move or parallelize PAL/FWC/FireWire, Audio, DttSP, database writes, band switching, AutoStart, or the native GDI+ renderer.

## Why this build exists

The PowerSDR display path has two distinct stages:

1. `RunDisplay()` on a worker obtains DSP spectrum/panadapter data and requests `picDisplay.Invalidate()`.
2. The WinForms UI thread later executes `picDisplay_Paint()` and `Display.RenderGDIPlus()`.

Audio/DSP can therefore continue while the console appears frozen if the WinForms message pump is occupied by synchronous control, database, layout, or hardware work.

## Measurements

The diagnostic helper records:

- constructor, InitializeComponent, DB init, PortAudio init, the fixed FWC settle sleep, InitConsole, SyncDSP and native AutoStart timing;
- UI heartbeat latency with stall thresholds at 250/500/1000/2000/5000/10000 ms;
- last relevant Win32 UI message/control before a stall;
- RunDisplay producer cycles, Invalidate requests, Paint count and RenderGDIPlus count;
- average/max Paint and RenderGDIPlus times, with slow-frame events;
- SetBand, SetRX1Band, SaveBandA and Console_Resize durations;
- working set, GDI/USER object counts and GC collection counts;
- UI-thread and AppDomain unhandled exceptions.

Telemetry is queued in memory and written by a BelowNormal background writer so normal UI/hardware calls do not synchronously write the log.

## Log location

`%APPDATA%\FlexRadio Systems\PowerSDR v2.8.0\SQ4KOU Diagnostics\`

The current run is `SQ4KOU_UI_DIAG_YYYYMMDD_HHMMSS.log`. `SQ4KOU_UI_DIAG_LATEST.txt` contains the full path of the latest log.

## Thetis reference

Thetis was reviewed as an architectural reference, not transplanted wholesale. Relevant patterns are:

- display encapsulated in a dedicated `PanDisplay : PictureBox`;
- a long-running display task separate from the normal form event path;
- dedicated graphics buffer ownership/update methods;
- explicit `InvokeRequired` / `BeginInvoke` patterns for cross-thread WinForms access;
- producer/data processing separated from the final UI paint step.

P17 measures where legacy PowerSDR diverges from those boundaries. Any functional rework will be based on the resulting trace rather than introduced into this diagnostic build.
