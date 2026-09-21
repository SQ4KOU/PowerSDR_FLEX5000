# FLEX-5000 Meters/Gadgets — native implementation architecture

Base: `base/safe` / `fad6179aa1a15f7d7461b02675ba8caa600eb1ab`

Reference sources:
- PowerSDR KE9NS: `d558979570c4c2e4572b63ac218d3d4477926cb8`
- Thetis: `852bf0ef0b4f3886a13fc2846489aee16f361872`

## Proven failures of P23/P24

1. The previous port replaced the strongly typed Thetis `Console` dependency with a dynamic facade.
   Mechanical audit of pinned Thetis `MeterManager.cs` found 227 `_console.*` references.
   Only 65 names exist in pinned KE9NS `Console`; 162 are absent.
   The Thetis meter event surface, including `MeterReadingsChangedHandlers`, is absent in KE9NS.
   Therefore a dynamic fallback can only mask missing backend behaviour; it cannot provide it.

2. P24 created a fallback container when restore returned zero containers and inserted SIGNAL_TEXT.
   Native Thetis does not do this. The native setup creates containers only from the explicit
   RX1/RX2 Add Container buttons. `FinishSetupAndDisplay()` accepts zero containers.

3. KE9NS `DB.SaveVars()` adds/updates rows but does not remove rows absent from the supplied list.
   Removing meter keys from a temporary ArrayList therefore does not purge stale meter records.
   Native Thetis calls `DB.PurgeMeters()` before `MeterManager.StoreSettings2()`.

4. The native Thetis Meters/Gadgets tab is 724x410 with a 710x395 holder.
   KE9NS Appearance content is 592x318 and the Setup form is fixed at 606x448.
   Directly inserting the Thetis fixed-coordinate control set into KE9NS Setup cannot be 1:1
   without a separate layout host.

## Architecture decision

Do not emulate Thetis Console. Do not inject the 44k-line MeterManager directly into the legacy
PowerSDR form with dynamic shims.

Create one new module in the existing project lineage:

`FlexMeters.dll` — .NET Framework 4.8, x86

Thetis and KE9NS both target .NET Framework 4.8 and x86, so this is a compatible module boundary.
The exact Thetis renderer may keep SharpDX 4.2.0 dependencies inside this module.

### Contracts

`IMeterTelemetrySource`
- Supplies requested `Reading` values.
- No fabricated zero fallback.
- Unsupported readings return `Unsupported`, not a numeric value.
- Uses native KE9NS DttSP/FWC algorithms.

`IMeterRadioState`
- Supplies VFO, mode, band, filter, MOX, power, split, RX2 and other state required by visual gadgets.
- Snapshot/polling is allowed; Thetis' missing event list is not recreated in PowerSDR Console.

`IMeterCommandSink`
- Explicit typed operations for button gadgets.
- A command is exposed only when native PowerSDR has an equivalent operation.

`IMeterStore`
- Owns the complete set of containers.
- Save is replace-all, not update-only.
- Zero containers saves as zero containers.
- No bootstrap/default container is ever created.
- Container model, item groups, item settings and floating geometry are one logical transaction.

`IMeterWindowHost`
- Owns floating window lifecycle and geometry.
- Restores only windows present in the persisted model.

## Native telemetry mapping

RX1 signal uses the same KE9NS algorithm already used by `UpdateMultimeter()`:
`DttSP.CalculateRXMeter(0,0,SIGNAL_STRENGTH)` plus
`rx1_meter_cal_offset + rx1_preamp_offset + rx1_filter_size_cal_offset + rx1_path_offset + rx1_xvtr_gain_offset + rx1_loop_offset`
for FLEX-5000.

RX2 uses the corresponding existing KE9NS formula with thread 2 and RX2 offsets.

TX DSP readings use existing KE9NS `DttSP.CalculateTXMeter()` mappings and sign/clamp rules.

FLEX-5000 forward/reverse power and SWR use the existing native functions:
- `FWCPAPower(pa_fwd_power)`
- `FWCPAPower(pa_rev_power) * swr_table[(int)tx_band]`
- `FWCSWR(pa_fwd_power, pa_rev_power)`

Do not duplicate calibration formulas in two independently maintained implementations.
Extract or wrap the native KE9NS calculations so the legacy meter and FlexMeters consume the same code.

## Persistence

Preferred storage is a dedicated `FlexMeters` Key/Value DataTable in the existing PowerSDR DataSet.
It is persisted by the already accepted atomic DB layer.

Save algorithm:
1. Build complete current meter dictionary.
2. Clear/replace the `FlexMeters` table.
3. Write current container/model/item/settings/geometry only.
4. Database atomic write remains handled by the base/safe DB layer.

Restore algorithm:
1. Read the complete `FlexMeters` table.
2. Restore exactly the IDs present.
3. If table is empty, restore zero windows.
4. Any malformed container is reported and skipped/fails validation; never replaced by a default.

This avoids stale `Options` rows and avoids the split geometry persistence used by native Thetis
(`meter*` rows plus `MeterDisplay_<guid>` form tables).

## UI

Do not expand or overlay the legacy KE9NS Setup designer.

Create `ucFlexMetersGadgets` from the Thetis Meters/Gadgets UI with its own fixed internal
724x410 design surface. Host it through a dedicated Meters/Gadgets page/window from Appearance.
The legacy Setup controls keep their original geometry.

The native Thetis editor behaviour to preserve:
- explicit Add RX1 / Add RX2 container
- no automatic container
- available/in-use lists
- add/remove/reorder items
- copy/paste settings
- container RX/TX visibility and other container properties
- per-item configuration panels

## Capability policy

Never silently emulate unsupported functionality.

Each MeterType/Gadget is classified:
- Native: exact KE9NS/FLEX-5000 source exists.
- Adapted: deterministic mapping exists and is tested.
- Unsupported: UI item is disabled/hidden with an explicit capability reason until implemented.

No dynamic fallback object is permitted.

## Hard build gates

The build must fail if any of these are present in the new meter module:
- `dynamic` used as a substitute for PowerSDR/Thetis API compatibility
- `P24DynamicValue`, `P24ReflectiveDynamic` or equivalent catch-all fallback
- automatic creation of a meter container when restore count is zero
- numeric zero returned for an unsupported Reading
- restore path that can create an ID not present in persisted state

Required tests:
1. Empty store -> 0 restored containers.
2. Two containers -> exact IDs, order, item types/settings and geometry after round trip.
3. Delete one -> after save/reload only the remaining container exists.
4. Fake telemetry source changes SIGNAL_STRENGTH -> meter model value changes.
5. Unsupported Reading -> explicit unsupported result, never 0f.
6. Native FLEX-5000 RX calculation test uses the same helper as the legacy meter.
7. Persistence save replaces old rows, proving stale deleted IDs cannot return.

## Implementation order

1. Contracts + fake host/store tests.
2. Dedicated replace-all meter repository.
3. Native FLEX-5000 telemetry bridge (RX1 first, then TX/FWC, then RX2).
4. Thetis meter model/renderer isolated from Console behind contracts.
5. Floating window lifecycle.
6. Meters/Gadgets editor UI.
7. Remaining gadgets/actions one capability at a time.
8. MSI only after persistence + live-value gates pass.
