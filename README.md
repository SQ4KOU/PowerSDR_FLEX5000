# SQ4KOU-THETIS

Clean synthesis workspace for Thetis.

## Stage 1 goal

Create a coherent, buildable synthesis combining the strongest parts of:

- `ramdor/Thetis`
- `eu2av/OpenHPSDR-Thetis-Enhanced`

Stage 1 is intentionally **free of SQ4KOU patches**. SQ4KOU-specific patches are allowed only after the clean synthesis builds successfully and is accepted as the new baseline.

## Pinned upstream commits

Pinned for the first synthesis on 2026-09-04:

- Ramdor `master`: `852bf0ef0b4f3886a13fc2846489aee16f361872`
- EU2AV `main`: `567a7ecd88bfb2ca41316dab9258fff0efeae7bf`

## Repository branches

- `base/ramdor-20260904` — immutable Ramdor reference snapshot with upstream history
- `base/eu2av-20260904` — immutable EU2AV reference snapshot with upstream history
- `synthesis-clean` — clean synthesis branch; Ramdor is the structural base, selected EU2AV improvements are integrated here
- `main` — repository control branch

## Integration rule

No SQ4KOU patches on `synthesis-clean` until the Ramdor + EU2AV synthesis is internally consistent and buildable.

The upstream baselines are imported automatically by GitHub Actions and remain separate so every integrated change can be traced back to its source.
