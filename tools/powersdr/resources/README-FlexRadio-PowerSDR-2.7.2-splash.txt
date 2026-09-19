FlexRadio PowerSDR original splash reference for SQ4KOU P20

Purpose
-------
This file preserves the form-level splash BackgroundImage used as the P20
reference. The P20 patch extracts only $this.BackgroundImage from the archived
RESX and injects that payload into the pinned KE9NS 2.8.0.336 source. No other
resource from the archive is copied into the executable.

Reference
---------
Repository: M0LTE/PowerSDR
Commit: f6aed32a5411798e93ed2d052c4e4ec83967e966
Path: Source/Console/splash.resx

Lineage
-------
The repository readme identifies its base as the GPL FlexRadio Systems
PowerSDR 2.7.2 SVN 5123 source. This resource is used specifically to restore
the pre-KE9NS FlexRadio-style opening artwork while preserving all SQ4KOU P19
startup sequencing fixes.

SQ4KOU P20 adds only the callsign overlay and retains the splash until the
stable main-window reveal.
