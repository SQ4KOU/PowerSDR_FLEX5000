# PowerSDR FLEX-5000 UI stability audit — P14

Date: 2026-09-19  
Upstream: KE9NS PowerSDR 2.8.0.336, pinned source d558979570c4c2e4572b63ac218d3d4477926cb8  
Target: FLEX-5000 / x86

## Observed failure

- Audio/radio path continues to run.
- Main Console WinForms UI can stop responding during BAND and other switch operations.
- Setup tabs were broken only by P11 input filtering; P11 is rejected.
- P13 removed P08 Sleep(0) catch-up and restored display thread BelowNormal, but the freeze remained.

This isolates the failure primarily to the UI thread / UI lifecycle rather than ASIO/DSP/audio.

## Structural audit of upstream handlers

Automated scan of Console/console.cs and Console/setup.cs found:
- 189 event handlers with synchronous risk operations.
- 89 high-risk handlers containing FWC/USBHID/I2C, blocking sleep/wait, network/spot I/O, or file I/O.
- 64 handlers contain Thread.Sleep.
- 20 handlers call FWC directly.
- 11 handlers call USBHID directly.
- 6 handlers call IIC_* helpers directly.
- 84 handlers touch DB routines.
- Event handlers frequently call other event handlers synchronously.

Representative runtime paths:
- BAND click -> SaveBandA -> StackForm.bandstackupdate -> DB.Get/SaveBandStack -> SetBand -> RX1DSPMode/RX1Filter/VFOAFreq -> RX1Band.
- RX1Band on FLEX-5000 can synchronously call optional FlexWire/I2C external-device writes (HERO/AMP), antenna state, FWC and DSP updates.
- SetBand can synchronously call SpotForm.processTCPMessage when spot activity is enabled.
- Several Setup audio handlers block the UI with 400–1000 ms Thread.Sleep by upstream design.

These upstream synchronous paths are latency-sensitive. Changing the WinForms form lifecycle or AutoStart ordering can expose reentrancy/race behavior even if the radio/audio threads continue normally.

## SQ4KOU patch stack classification

### Runtime-neutral / mostly shutdown-time
- Database reliability: transactional physical DB writes and recovery.
- Window-state persistence.
- P03 fast shutdown.
- P04 fast audio exit.
- P05 audio before FWC standby.
- P06 shutdown timing evidence.
- P07 indexed SaveVars + close-time window snapshot.

### Runtime display
- P08 display optimization.
- P13 keeps P08 memory improvements but restores safe Thread.Sleep(display_delay) pacing and BelowNormal renderer priority.
- Freeze still reproduced on P13, so aggressive P08 pacing is not the root cause.

### Global UI lifecycle changes — prime regression candidates
P09:
- enables main-form optimized buffering earlier;
- replaces several Show/Close or Show/Hide warm-up sequences with Handle creation;
- manually calls SpotControl_Load;
- adds a load-once guard.

P10:
- moves AutoStart power-on from constructor/startup flow to OnShown + BeginInvoke.

These two patches alter object lifecycle/event ordering globally rather than changing one control.

## P14 decision

P14 removes P09 and P10 completely from the build while retaining:
- DB reliability;
- confirmed shutdown fixes;
- window-state fixes;
- P07;
- P08 memory optimizations;
- P13 stable display pacing.

The P14 verifier fails the build if any P09/P10 marker remains.

## Interpretation of P14 test

If P14 is stable across BAND/MODE/AGC/ATT/RX2/TUN/MOX/Setup:
- regression interval is P09/P10;
- startup optimization must be redesigned without manual form priming and without moving AutoStart into OnShown.

If P14 still freezes while audio runs:
- move to shared synchronous UI I/O audit;
- instrument phase markers before/after FWC/FlexWire/I2C/DB/Spot calls;
- keep native radio command ordering;
- isolate only optional external I/O behind bounded/serialized execution where safe;
- then, if needed, roll back remaining P08 runtime changes for a second bisect.
