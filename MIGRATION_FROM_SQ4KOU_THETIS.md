# Migration audit — FLEX-5000

Migration date: 2026-09-12

Source repository: `SQ4KOU/SQ4KOU-THETIS`
Destination repository: `SQ4KOU/PowerSDR_FLEX5000`

Only project history was migrated. Foreign `.github/workflows/` files were intentionally removed during history filtering because CI definitions are repository-specific. This rewrites destination commit SHAs while retaining the project-source history and commit messages.

| Source branch | Source SHA | Destination branch | Destination SHA |
|---|---|---|---|
| `flex5000-wdsp-native` | `4153ccdcb6b341b809c8e95c2d93b367fb822401` | `baseline/flex5000-wdsp-native` | `e12c1609f0caf64a340fd6153ea7205975c0dd3b` |
| `flex5000-hpsdr-isolation-fix` | `33956754f572c8bccec1a35eda1f5721a2d963d6` | `archive/flex5000-hpsdr-isolation-fix` | `3d01192421d9f5a406e7bf7a44e48731d8c0e73a` |
| `powersdr-flex5000-display` | `9f7de9105d5cefe9f1e651e02bede7db42aa01e4` | `feature/powersdr-flex5000-display` | `1ef3ccf070f9e994b7c6d369649361b49057281d` |
| `powersdr-flex5000-thetis-ui` | `3cee4972e5aac5220f701e669213469e140f9bec` | `feature/powersdr-flex5000-thetis-ui` | `14e50babc0c681ce27414085e323e93b92dd2e2e` |

`main` is a project index/administrative branch and is not implicitly a hardware PASS baseline.
