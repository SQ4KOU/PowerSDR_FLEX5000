# FLEX-5000 Meters/Gadgets — evidence audit

Sources are pinned. No claim below relies on a moving upstream branch.

- PowerSDR KE9NS: `d558979570c4c2e4572b63ac218d3d4477926cb8`
- Thetis: `852bf0ef0b4f3886a13fc2846489aee16f361872`
- SQ4KOU safe base: `fad6179aa1a15f7d7461b02675ba8caa600eb1ab`

## 1. Why the old P24 bridge cannot be repaired safely

A mechanical scan of pinned Thetis `MeterManager.cs` found 227 distinct `_console.*`
member references. Only 65 of those names occur in pinned KE9NS `Console`; 162 do not.

Thetis `MeterManager.addDelegates()` subscribes to a large event surface, including
`MeterReadingsChangedHandlers`. KE9NS does not expose that event surface.

Therefore replacing Thetis `Console` with a catch-all dynamic object is not an adapter.
It suppresses compile-time evidence of missing backend behaviour. The previous
`P24DynamicValue` returning 0/false/empty values explains visually valid but dead meters.

Decision: the new implementation contains no dynamic compatibility facade. Compilation must
fail on every unmapped dependency.

## 2. Native telemetry sources that actually exist in KE9NS

### RX1 signal

KE9NS already computes FLEX-5000 signal level from:

`DttSP.CalculateRXMeter(0, 0, DttSP.MeterType.SIGNAL_STRENGTH)`

plus:

- `rx1_meter_cal_offset`
- `rx1_preamp_offset[(int)rx1_preamp_mode]`
- `rx1_filter_size_cal_offset`
- `rx1_path_offset`
- `rx1_xvtr_gain_offset`
- `rx1_loop_offset`

The CAT `ZZSM` path independently confirms the same FLEX-5000 calibration family.

### RX2 signal

KE9NS `UpdateRX2MeterData()` uses thread 2 and the corresponding RX2 calibration fields.

### RX ADC

Pinned Thetis itself maps:
- `Reading.ADC_PK` -> `WDSP.MeterType.ADC_REAL`
- `Reading.ADC_AV` -> `WDSP.MeterType.ADC_IMAG`

Pinned KE9NS DttSP exposes both `ADC_REAL` and `ADC_IMAG`.
This mapping is therefore source-proven, not inferred from naming.

### TX DSP readings

Pinned KE9NS DttSP exposes:
`MIC, PWR, ALC, EQ, LEVELER, COMP, CPDR, ALC_G, LVL_G, MIC_PK, ALC_PK,
EQ_PK, LEVELER_PK, COMP_PK, CPDR_PK`.

Pinned Thetis applies explicit sign/clamp rules, for example:
- MIC: `max(-195, -CalculateTXMeter(MIC))`
- EQ/LEVELER/COMP/ALC: `max(-30, -CalculateTXMeter(...))`
- LVL_G: `max(0, CalculateTXMeter(LVL_G))`
- ALC_GROUP: `max(-30,-ALC_PK) + max(0,-ALC_G)`

KE9NS exposes the required DttSP meter enums for those readings.

### FLEX-5000 power and SWR

KE9NS already polls PA ADC while TX in `PollFWCPAPWR()`:
- PA ADC 7 -> forward
- PA ADC 6 -> reverse
- cached as `pa_fwd_power`, `pa_rev_power`

Existing meter functions use:
- `FWCPAPower(pa_fwd_power)`
- `FWCPAPower(pa_rev_power) * swr_table[(int)tx_band]`
- `FWCSWR(pa_fwd_power, pa_rev_power)`

The new adapter should consume these cached PA values. It must not create a second FWC polling loop.

### Supply voltage

KE9NS CAT `ZZRV` proves a native FLEX-5000 supply-voltage reading:
`FWC.ReadPAADC(2)`, converted as `adc / 4096 * 2.5 * 11`.

No equivalent current/AMPS source was found in the audited KE9NS paths. AMPS remains unsupported
until a source is proven.

## 3. Reading compatibility matrix

| Thetis Reading | Status for FLEX-5000 | Proven source |
|---|---|---|
| SIGNAL_STRENGTH | Native | KE9NS DttSP + FLEX calibration |
| AVG_SIGNAL_STRENGTH | Native | KE9NS DttSP enum + legacy average meter path |
| ADC_PK | Native mapping | Thetis maps to ADC_REAL; KE9NS exposes ADC_REAL |
| ADC_AV | Native mapping | Thetis maps to ADC_IMAG; KE9NS exposes ADC_IMAG |
| AGC_PK | Unsupported initially | no KE9NS DttSP enum |
| AGC_AV | Unsupported initially | no KE9NS DttSP enum |
| AGC_GAIN | Candidate/native API | KE9NS DttSP exposes AGC_GAIN; parity test required |
| ESTIMATED_PBSNR | Unsupported initially | Thetis requires spectral/noise-floor logic absent from audited KE9NS path |
| MIC / MIC_PK | Native | KE9NS DttSP |
| EQ / EQ_PK | Native | KE9NS DttSP |
| LEVELER / LEVELER_PK | Native | KE9NS DttSP |
| LVL_G | Native | KE9NS DttSP |
| COMP / COMP_PK | Native | KE9NS DttSP |
| ALC / ALC_PK / ALC_G | Native | KE9NS DttSP |
| ALC_GROUP | Adapted, deterministic | exact Thetis formula from ALC_PK + ALC_G |
| CFC_PK / CFC_AV / CFC_G | Unsupported initially | KE9NS DttSP has no CFC meter enums |
| PWR | Native FLEX-5000 | cached FWC ADC + FWCPAPower |
| REVERSE_PWR | Native FLEX-5000 | cached FWC ADC + FWCPAPower + SWR table |
| SWR | Native FLEX-5000 | FWCSWR |
| DRIVE/FWD/REV ADC family | Unsupported initially | Thetis semantics are ANAN/HPSDR-specific; do not equate by name |
| VOLTS | Native FLEX-5000 | CAT ZZRV proves FWC PA ADC 2 conversion |
| AMPS | Unsupported initially | no proven KE9NS source in audit |
| AZ / ELE | Unsupported in radio core | requires separate rotator service |
| SIGNAL_MAX_BIN | Unsupported initially | Thetis uses WDSP GetDetectMaxBin path not found in KE9NS |
| ADC_MAX_MAG | Unsupported initially | Thetis uses NetworkIO ADC magnitude path, HPSDR-specific |

Unsupported means the editor must not offer a functioning-looking meter returning 0.

## 4. Process lifecycle — exact integration points

KE9NS is `public partial class Console`. A new partial file can access private radio/calibration
fields without reflection and without making them public.

### Initialization

In the constructor, KE9NS:
1. initializes radio/audio,
2. calls `InitConsole()`,
3. inside `InitConsole()` creates `setupForm = new Setup(this)`,
4. calls `GetState()`,
5. completes calibration/state initialization.

Therefore the FlexMeters controller should be created after `InitConsole()` returns.
At that point DB, Setup and radio state exist.

Persisted model can be loaded then, but floating windows are not shown until the main Console
`Shown` event.

### POWER ON

KE9NS starts its meter infrastructure only after:
- FLEX-5000 leaves standby,
- audio starts successfully.

Then it starts:
- `UpdateMultimeter`
- RX2 meter thread when applicable
- `PollFWCPAPWR` for FLEX-5000/3000.

The FlexMeters telemetry worker must be started in this same POWER-ON phase.
Its loop terminates with Power OFF, matching the native meter workers.

### TX power data

`PollFWCPAPWR()` already samples PA ADC every 100 ms while MOX and clears the cached values
when the loop exits. FlexMeters reads those cached values and does not touch FWC ADC polling.

### Shutdown

KE9NS `Console_Closing()` powers the radio off, then later calls `SaveState()` and
`setupForm.SaveOptions()`. `ExitConsole()` subsequently calls `DB.Exit()`.

FlexMeters must commit its in-memory model to the `FlexMeters` DataTable before forms are
destroyed and before `DB.Exit()`.

## 5. Persistence — why a dedicated table is safer

KE9NS `DB.SaveVars()` is update/add only. It does not delete DB rows that are absent from the
input list. This was a direct cause of stale meter IDs surviving the P24 save cycle.

`DB.ds` is a public `DataSet`. `DB.AddFormTable()` is private, but the PowerSDR adapter can
create a `DataTable("FlexMeters")` directly inside the same assembly without changing DB code.

Save is therefore:
1. ensure `FlexMeters` table exists with Key/Value columns,
2. `Rows.Clear()`,
3. add exactly the current meter state,
4. let existing `DB.Exit()` persist the DataSet through the already accepted safe DB layer.

This guarantees that deleting a container removes its persisted ID.

Geometry is stored in the same table/model as the container. Do not use a second
`MeterDisplay_<guid>` form table.

## 6. DLL boundary

Both pinned projects target .NET Framework 4.8 and x86 is supported by both.

Use:
- `FlexMeters.dll`: model, renderer, editor, window host, interfaces.
- PowerSDR partial `Console.FlexMetersAdapter.cs`: implements telemetry/state/command/store
  interfaces with direct access to private KE9NS fields.
- PowerSDR references FlexMeters.dll.
- FlexMeters.dll does not reference PowerSDR.exe.

This direction has no circular dependency.

Thetis renderer's SharpDX 4.2.0 dependencies stay inside the FlexMeters module/package.

## 7. UI boundary

Thetis Meters/Gadgets editor surface is 724x410; its holder is 710x395.
KE9NS Appearance client area is only 592x318 and legacy Setup is FixedSingle.

Therefore the exact Thetis editor cannot fit in the current Setup viewport without clipping,
scrolling or changing the whole Setup geometry.

Recommended host:
- keep `Appearance -> Meters/Gadgets` as the entry point,
- open a dedicated editor form containing the full 724x410 editor surface,
- do not move Factory Defaults / Import Database / other legacy Setup controls.

This deliberately changes only the hosting surface, not the editor semantics.

## 8. Compiler as the compatibility auditor

When Thetis meter/model/renderer code is brought into FlexMeters.dll:
- replace Console/static-service coupling with explicit interfaces,
- remove unsupported hardware-specific feature blocks,
- do not add catch-all substitutes.

Every unresolved member must remain a compile error until it has one of:
1. a native adapter implementation,
2. a deterministic adapted implementation,
3. an explicit unsupported capability.

That turns the compiler into a hard dependency checklist and prevents a repeat of P24.
