# Synthesis hardware-specific marker audit

Snapshot: `synthesis-clean`

This report is diagnostic only. A match does not mean the code will be removed;
it identifies places requiring semantic review before the clean synthesis is accepted.

## `ANVELINA` — 82 match(es)

- `Project Files/Source/Console/CAT/CATCommands.cs:2932:            else if (radio == "ANAN100" || radio == "ANAN100B" || radio == "ANAN100D" || radio == "ANAN200D" || radio == "ANAN7000D" || radio == "ANAN8000D" || radio == "ANVELINAPRO3" || radio == "ANAN_G2" || radio == "ANAN_G2_1K")  // DH1KLM_21a added 7000D`
- `Project Files/Source/Console/CAT/CATCommands.cs:6138:                   HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/PSForm.designer.cs:328:            this.toolTip1.SetToolTip(this.chkPSOutlierEnable, "Enable robust outlier rejection before cubic-spline fitting. Default ON at sigma 5.0 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3).");`
- `Project Files/Source/Console/PSForm.designer.cs:353:            this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Target Feedback Level for auto-attenuator and indicator. Default 22 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3) to keep ADC2208 and codec in their linear range; 152 for other hardware.");`
- `Project Files/Source/Console/clsHardwareSpecific.cs:178:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:250:                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2E || //N1GP G2E added`
- `Project Files/Source/Console/clsHardwareSpecific.cs:260:                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2E || //N1GP G2E added`
- `Project Files/Source/Console/clsHardwareSpecific.cs:273:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:323:        // Default 152 preserves legacy behaviour; Anvelina PRO3 needs ~22 for its typical feedback coupling.`
- `Project Files/Source/Console/clsHardwareSpecific.cs:328:                // Yurij_eu2av: Orion MK2 based rigs (ANAN-7000/8000/Anvelina PRO3)`
- `Project Files/Source/Console/clsHardwareSpecific.cs:336:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:352:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:368:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:405:                case "ANVELINA-PRO3":`
- `Project Files/Source/Console/clsHardwareSpecific.cs:406:                    return HPSDRModel.ANVELINAPRO3;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:445:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:446:                    return "ANVELINA-PRO3";`
- `Project Files/Source/Console/clsHardwareSpecific.cs:469:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:486:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:519:                _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:755:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:832:                    _model == HPSDRModel.ANAN_G2 || _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.REDPITAYA);`
- `Project Files/Source/Console/cmaster.cs:630:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:714:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:750:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:845:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:6771:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:8254:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:10067:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11042:                    HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11068:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11212:                    HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11237:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11715:                //        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:14874:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:14903:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:15467:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:18757:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:19345:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:19521:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:21075:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:21101:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:21608:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA;`
- `Project Files/Source/Console/console.cs:21691:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA;`
- `Project Files/Source/Console/console.cs:22573:                                             HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25050:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:25136:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:25866:                        HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25892:                        HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25928:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA)`
- `Project Files/Source/Console/console.cs:26065:                        case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:27812:                        case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:31430:                 HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:31825:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:32442:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:40947:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:40955:                HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:53101:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:53161:                                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E ||`
- `Project Files/Source/Console/console.cs:53320:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E ||`
- `Project Files/Source/Console/enums.cs:131:        ANVELINAPRO3,`
- `Project Files/Source/Console/enums.cs:400:        OrionMKII = 5,      // AMAM-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII Anvelina-Pro3 RedPitaya`
- `Project Files/Source/Console/frmAbout.Designer.cs:73:            "EU2AV, Yurij (PureSignal enhancements, feedback calibration, Anvelina PRO3 firmware update & firmware)",`
- `Project Files/Source/Console/setup.cs:6295:            //    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3)`
- `Project Files/Source/Console/setup.cs:6320:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6379:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6385:                    (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3) ||`
- `Project Files/Source/Console/setup.cs:6465:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:6516:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM`
- `Project Files/Source/Console/setup.cs:6552:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6586:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM`
- `Project Files/Source/Console/setup.cs:15742:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:15981:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:16040:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:20548:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:23687:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:23721:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:24088:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:24092:                            string sSetting = "udANVELINAPRO3PAGain" + mapBandToMeters(b).ToString();`
- `Project Files/Source/Console/setup.cs:24100:                            string sSetting = "udANVELINAPRO3PAGainVHF" + (n - (int)Band.VHF0).ToString();`
- `Project Files/Source/Console/setup.designer.cs:8576:            "ANVELINA-PRO3",`

## `Anvelina` — 82 match(es)

- `Project Files/Source/Console/CAT/CATCommands.cs:2932:            else if (radio == "ANAN100" || radio == "ANAN100B" || radio == "ANAN100D" || radio == "ANAN200D" || radio == "ANAN7000D" || radio == "ANAN8000D" || radio == "ANVELINAPRO3" || radio == "ANAN_G2" || radio == "ANAN_G2_1K")  // DH1KLM_21a added 7000D`
- `Project Files/Source/Console/CAT/CATCommands.cs:6138:                   HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/PSForm.designer.cs:328:            this.toolTip1.SetToolTip(this.chkPSOutlierEnable, "Enable robust outlier rejection before cubic-spline fitting. Default ON at sigma 5.0 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3).");`
- `Project Files/Source/Console/PSForm.designer.cs:353:            this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Target Feedback Level for auto-attenuator and indicator. Default 22 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3) to keep ADC2208 and codec in their linear range; 152 for other hardware.");`
- `Project Files/Source/Console/clsHardwareSpecific.cs:178:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:250:                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2E || //N1GP G2E added`
- `Project Files/Source/Console/clsHardwareSpecific.cs:260:                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2E || //N1GP G2E added`
- `Project Files/Source/Console/clsHardwareSpecific.cs:273:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:323:        // Default 152 preserves legacy behaviour; Anvelina PRO3 needs ~22 for its typical feedback coupling.`
- `Project Files/Source/Console/clsHardwareSpecific.cs:328:                // Yurij_eu2av: Orion MK2 based rigs (ANAN-7000/8000/Anvelina PRO3)`
- `Project Files/Source/Console/clsHardwareSpecific.cs:336:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:352:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:368:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:405:                case "ANVELINA-PRO3":`
- `Project Files/Source/Console/clsHardwareSpecific.cs:406:                    return HPSDRModel.ANVELINAPRO3;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:445:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:446:                    return "ANVELINA-PRO3";`
- `Project Files/Source/Console/clsHardwareSpecific.cs:469:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:486:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:519:                _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:755:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:832:                    _model == HPSDRModel.ANAN_G2 || _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.REDPITAYA);`
- `Project Files/Source/Console/cmaster.cs:630:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:714:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:750:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:845:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:6771:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:8254:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:10067:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11042:                    HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11068:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11212:                    HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11237:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11715:                //        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:14874:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:14903:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:15467:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:18757:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:19345:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:19521:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:21075:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:21101:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:21608:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA;`
- `Project Files/Source/Console/console.cs:21691:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA;`
- `Project Files/Source/Console/console.cs:22573:                                             HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25050:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:25136:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:25866:                        HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25892:                        HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25928:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA)`
- `Project Files/Source/Console/console.cs:26065:                        case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:27812:                        case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:31430:                 HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:31825:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:32442:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:40947:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:40955:                HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:53101:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:53161:                                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E ||`
- `Project Files/Source/Console/console.cs:53320:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E ||`
- `Project Files/Source/Console/enums.cs:131:        ANVELINAPRO3,`
- `Project Files/Source/Console/enums.cs:400:        OrionMKII = 5,      // AMAM-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII Anvelina-Pro3 RedPitaya`
- `Project Files/Source/Console/frmAbout.Designer.cs:73:            "EU2AV, Yurij (PureSignal enhancements, feedback calibration, Anvelina PRO3 firmware update & firmware)",`
- `Project Files/Source/Console/setup.cs:6295:            //    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3)`
- `Project Files/Source/Console/setup.cs:6320:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6379:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6385:                    (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3) ||`
- `Project Files/Source/Console/setup.cs:6465:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:6516:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM`
- `Project Files/Source/Console/setup.cs:6552:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6586:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM`
- `Project Files/Source/Console/setup.cs:15742:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:15981:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:16040:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:20548:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:23687:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:23721:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:24088:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:24092:                            string sSetting = "udANVELINAPRO3PAGain" + mapBandToMeters(b).ToString();`
- `Project Files/Source/Console/setup.cs:24100:                            string sSetting = "udANVELINAPRO3PAGainVHF" + (n - (int)Band.VHF0).ToString();`
- `Project Files/Source/Console/setup.designer.cs:8576:            "ANVELINA-PRO3",`

## `anvelina` — 82 match(es)

- `Project Files/Source/Console/CAT/CATCommands.cs:2932:            else if (radio == "ANAN100" || radio == "ANAN100B" || radio == "ANAN100D" || radio == "ANAN200D" || radio == "ANAN7000D" || radio == "ANAN8000D" || radio == "ANVELINAPRO3" || radio == "ANAN_G2" || radio == "ANAN_G2_1K")  // DH1KLM_21a added 7000D`
- `Project Files/Source/Console/CAT/CATCommands.cs:6138:                   HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/PSForm.designer.cs:328:            this.toolTip1.SetToolTip(this.chkPSOutlierEnable, "Enable robust outlier rejection before cubic-spline fitting. Default ON at sigma 5.0 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3).");`
- `Project Files/Source/Console/PSForm.designer.cs:353:            this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Target Feedback Level for auto-attenuator and indicator. Default 22 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3) to keep ADC2208 and codec in their linear range; 152 for other hardware.");`
- `Project Files/Source/Console/clsHardwareSpecific.cs:178:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:250:                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2E || //N1GP G2E added`
- `Project Files/Source/Console/clsHardwareSpecific.cs:260:                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2E || //N1GP G2E added`
- `Project Files/Source/Console/clsHardwareSpecific.cs:273:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:323:        // Default 152 preserves legacy behaviour; Anvelina PRO3 needs ~22 for its typical feedback coupling.`
- `Project Files/Source/Console/clsHardwareSpecific.cs:328:                // Yurij_eu2av: Orion MK2 based rigs (ANAN-7000/8000/Anvelina PRO3)`
- `Project Files/Source/Console/clsHardwareSpecific.cs:336:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:352:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:368:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:405:                case "ANVELINA-PRO3":`
- `Project Files/Source/Console/clsHardwareSpecific.cs:406:                    return HPSDRModel.ANVELINAPRO3;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:445:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:446:                    return "ANVELINA-PRO3";`
- `Project Files/Source/Console/clsHardwareSpecific.cs:469:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:486:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:519:                _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:755:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:832:                    _model == HPSDRModel.ANAN_G2 || _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.REDPITAYA);`
- `Project Files/Source/Console/cmaster.cs:630:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:714:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:750:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/cmaster.cs:845:                            case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:6771:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:8254:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:10067:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11042:                    HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11068:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11212:                    HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11237:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:11715:                //        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:14874:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:14903:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:15467:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:18757:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:19345:                        HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:19521:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:21075:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:21101:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:21608:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA;`
- `Project Files/Source/Console/console.cs:21691:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA;`
- `Project Files/Source/Console/console.cs:22573:                                             HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25050:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:25136:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:25866:                        HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25892:                        HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:25928:                            HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA)`
- `Project Files/Source/Console/console.cs:26065:                        case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:27812:                        case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:31430:                 HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:31825:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:32442:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN_G2 ||`
- `Project Files/Source/Console/console.cs:40947:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/console.cs:40955:                HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:53101:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/console.cs:53161:                                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E ||`
- `Project Files/Source/Console/console.cs:53320:                    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E ||`
- `Project Files/Source/Console/enums.cs:131:        ANVELINAPRO3,`
- `Project Files/Source/Console/enums.cs:400:        OrionMKII = 5,      // AMAM-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII Anvelina-Pro3 RedPitaya`
- `Project Files/Source/Console/frmAbout.Designer.cs:73:            "EU2AV, Yurij (PureSignal enhancements, feedback calibration, Anvelina PRO3 firmware update & firmware)",`
- `Project Files/Source/Console/setup.cs:6295:            //    HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3)`
- `Project Files/Source/Console/setup.cs:6320:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6379:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6385:                    (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3) ||`
- `Project Files/Source/Console/setup.cs:6465:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:6516:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM`
- `Project Files/Source/Console/setup.cs:6552:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/setup.cs:6586:                HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM`
- `Project Files/Source/Console/setup.cs:15742:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:15981:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:16040:                HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3 &&`
- `Project Files/Source/Console/setup.cs:20548:                case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:23687:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:23721:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:24088:                    case HPSDRModel.ANVELINAPRO3:`
- `Project Files/Source/Console/setup.cs:24092:                            string sSetting = "udANVELINAPRO3PAGain" + mapBandToMeters(b).ToString();`
- `Project Files/Source/Console/setup.cs:24100:                            string sSetting = "udANVELINAPRO3PAGainVHF" + (n - (int)Band.VHF0).ToString();`
- `Project Files/Source/Console/setup.designer.cs:8576:            "ANVELINA-PRO3",`

## `ORIONMKII` — 61 match(es)

- `Project Files/Source/Console/HPSDR/NetworkIO.cs:165:                        board_is_expected_for_model = BoardID == HPSDRHW.Hermes || BoardID == HPSDRHW.OrionMKII; // can be these two`
- `Project Files/Source/Console/HPSDR/NetworkIO.cs:567://                    board_is_expected_for_model = BoardID == HPSDRHW.Hermes || BoardID == HPSDRHW.OrionMKII; // can be these two`
- `Project Files/Source/Console/HPSDR/NetworkIO.cs:1017://                                            hpsdrd.deviceType = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/HPSDR/clsRadioDiscovery.cs:1210:            if (boardId == 10) return HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/MeterManager.cs:5390:                    ((_currentHPSDRmodel == HPSDRModel.ORIONMKII || _currentHPSDRmodel == HPSDRModel.ANAN8000D || _currentHPSDRmodel == HPSDRModel.ANAN_G2)`
- `Project Files/Source/Console/clsHardwareSpecific.cs:143:                    case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:148:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:155:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:162:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:183:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:190:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:468:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:485:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:541:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:831:                return !(_model == HPSDRModel.ORIONMKII || _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/cmaster.cs:627:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/cmaster.cs:711:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/cmaster.cs:747:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/cmaster.cs:842:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:6856:            if ((HardwareSpecific.Hardware == HPSDRHW.OrionMKII) || (HardwareSpecific.Hardware == HPSDRHW.Saturn)`
- `Project Files/Source/Console/console.cs:8249:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:8680:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII RedPitaya`
- `Project Files/Source/Console/console.cs:10063:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11038:                    HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11064:                        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11208:                    HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11234:                        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11710:                //        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:14850:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:14897:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:15462:            if (HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:19341:                        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:19515:                    HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:21070:                    HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:22572:                                             HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:25061:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:25145:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:25235:        public float computeOrionMkIIExciterPower()`
- `Project Files/Source/Console/console.cs:25860:                        HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:25886:                        HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:26059:                        case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:26067:                            drivepwr = computeOrionMkIIExciterPower();`
- `Project Files/Source/Console/console.cs:27716:                        case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:27807:                        case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:31424:                 HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:40943:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:40955:                HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:53097:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:53160:                                HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANAN_G2E || HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANAN_G2_1K || //N1GP G2E added`
- `Project Files/Source/Console/console.cs:53319:                    HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANAN_G2E || HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANAN_G2_1K || //N1GP G2E added`
- `Project Files/Source/Console/database.cs:10533:        //                        //else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelORIONMKII")) sRad = ""; // not implemented in comboRadioModel list items`
- `Project Files/Source/Console/database.cs:11093:                                //else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelORIONMKII")) sRad = ""; // not implemented in comboRadioModel list items`
- `Project Files/Source/Console/enums.cs:126:        ORIONMKII,`
- `Project Files/Source/Console/enums.cs:400:        OrionMKII = 5,      // AMAM-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII Anvelina-Pro3 RedPitaya`
- `Project Files/Source/Console/setup.cs:8963:                    if (((HardwareSpecific.Hardware == HPSDRHW.Orion || HardwareSpecific.Hardware == HPSDRHW.OrionMKII) &&`
- `Project Files/Source/Console/setup.cs:15978:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/setup.cs:16037:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/setup.cs:24218:                    case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/setup.cs:24222:                            string sSetting = "udORIONMKIIPAGain" + mapBandToMeters(b).ToString();`
- `Project Files/Source/Console/setup.cs:24230:                            string sSetting = "udORIONMKIIPAGainVHF" + (n - (int)Band.VHF0).ToString();`

## `ORIONMK2` — 0 match(es)

- none

## `Orion MK2` — 4 match(es)

- `Project Files/Source/Console/PSForm.designer.cs:304:            this.toolTip1.SetToolTip(this.udPSOutlierSigma, "Outlier-rejection sigma. Range 0.1–5.0 (lower = more aggressive culling). Default 5.0 for Orion MK2 rigs.");`
- `Project Files/Source/Console/PSForm.designer.cs:328:            this.toolTip1.SetToolTip(this.chkPSOutlierEnable, "Enable robust outlier rejection before cubic-spline fitting. Default ON at sigma 5.0 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3).");`
- `Project Files/Source/Console/PSForm.designer.cs:353:            this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Target Feedback Level for auto-attenuator and indicator. Default 22 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3) to keep ADC2208 and codec in their linear range; 152 for other hardware.");`
- `Project Files/Source/Console/clsHardwareSpecific.cs:328:                // Yurij_eu2av: Orion MK2 based rigs (ANAN-7000/8000/Anvelina PRO3)`

## `OrionMKII` — 61 match(es)

- `Project Files/Source/Console/HPSDR/NetworkIO.cs:165:                        board_is_expected_for_model = BoardID == HPSDRHW.Hermes || BoardID == HPSDRHW.OrionMKII; // can be these two`
- `Project Files/Source/Console/HPSDR/NetworkIO.cs:567://                    board_is_expected_for_model = BoardID == HPSDRHW.Hermes || BoardID == HPSDRHW.OrionMKII; // can be these two`
- `Project Files/Source/Console/HPSDR/NetworkIO.cs:1017://                                            hpsdrd.deviceType = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/HPSDR/clsRadioDiscovery.cs:1210:            if (boardId == 10) return HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/MeterManager.cs:5390:                    ((_currentHPSDRmodel == HPSDRModel.ORIONMKII || _currentHPSDRmodel == HPSDRModel.ANAN8000D || _currentHPSDRmodel == HPSDRModel.ANAN_G2)`
- `Project Files/Source/Console/clsHardwareSpecific.cs:143:                    case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:148:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:155:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:162:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:183:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:190:                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:468:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:485:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:541:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:831:                return !(_model == HPSDRModel.ORIONMKII || _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/cmaster.cs:627:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/cmaster.cs:711:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/cmaster.cs:747:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/cmaster.cs:842:                            case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:6856:            if ((HardwareSpecific.Hardware == HPSDRHW.OrionMKII) || (HardwareSpecific.Hardware == HPSDRHW.Saturn)`
- `Project Files/Source/Console/console.cs:8249:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:8680:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII RedPitaya`
- `Project Files/Source/Console/console.cs:10063:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11038:                    HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11064:                        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11208:                    HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11234:                        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:11710:                //        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:14850:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:14897:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:15462:            if (HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:19341:                        HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:19515:                    HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:21070:                    HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:22572:                                             HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:25061:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:25145:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:25235:        public float computeOrionMkIIExciterPower()`
- `Project Files/Source/Console/console.cs:25860:                        HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:25886:                        HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:26059:                        case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:26067:                            drivepwr = computeOrionMkIIExciterPower();`
- `Project Files/Source/Console/console.cs:27716:                        case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:27807:                        case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:31424:                 HardwareSpecific.Model == HPSDRModel.ORIONMKII ||`
- `Project Files/Source/Console/console.cs:40943:                case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/console.cs:40955:                HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 ||`
- `Project Files/Source/Console/console.cs:53097:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/console.cs:53160:                                HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANAN_G2E || HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANAN_G2_1K || //N1GP G2E added`
- `Project Files/Source/Console/console.cs:53319:                    HardwareSpecific.Model == HPSDRModel.ORIONMKII || HardwareSpecific.Model == HPSDRModel.ANAN_G2E || HardwareSpecific.Model == HPSDRModel.ANAN_G2 || HardwareSpecific.Model == HPSDRModel.ANAN_G2_1K || //N1GP G2E added`
- `Project Files/Source/Console/database.cs:10533:        //                        //else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelORIONMKII")) sRad = ""; // not implemented in comboRadioModel list items`
- `Project Files/Source/Console/database.cs:11093:                                //else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelORIONMKII")) sRad = ""; // not implemented in comboRadioModel list items`
- `Project Files/Source/Console/enums.cs:126:        ORIONMKII,`
- `Project Files/Source/Console/enums.cs:400:        OrionMKII = 5,      // AMAM-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII Anvelina-Pro3 RedPitaya`
- `Project Files/Source/Console/setup.cs:8963:                    if (((HardwareSpecific.Hardware == HPSDRHW.Orion || HardwareSpecific.Hardware == HPSDRHW.OrionMKII) &&`
- `Project Files/Source/Console/setup.cs:15978:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/setup.cs:16037:                HardwareSpecific.Model != HPSDRModel.ORIONMKII &&`
- `Project Files/Source/Console/setup.cs:24218:                    case HPSDRModel.ORIONMKII:`
- `Project Files/Source/Console/setup.cs:24222:                            string sSetting = "udORIONMKIIPAGain" + mapBandToMeters(b).ToString();`
- `Project Files/Source/Console/setup.cs:24230:                            string sSetting = "udORIONMKIIPAGainVHF" + (n - (int)Band.VHF0).ToString();`

## `7000DLE` — 9 match(es)

- `Project Files/Source/Console/clsHardwareSpecific.cs:397:                case "ANAN-7000DLE":`
- `Project Files/Source/Console/clsHardwareSpecific.cs:436:                    return "ANAN-7000DLE";`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:8680:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII RedPitaya`
- `Project Files/Source/Console/database.cs:10531:        //                        else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D")) sRad = "ANAN-7000DLE";`
- `Project Files/Source/Console/database.cs:11091:                                else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D")) sRad = "ANAN-7000DLE";`
- `Project Files/Source/Console/enums.cs:400:        OrionMKII = 5,      // AMAM-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII Anvelina-Pro3 RedPitaya`
- `Project Files/Source/Console/setup.designer.cs:8571:            "ANAN-7000DLE",`
- `Project Files/Source/Console/setup.designer.cs:11756:            this.toolTip1.SetToolTip(this.btnAmpDefault, "Set volt/sens to defaults (7000dle has unique values)");`

## `ANAN-7000` — 10 match(es)

- `Project Files/Source/Console/PSForm.designer.cs:328:            this.toolTip1.SetToolTip(this.chkPSOutlierEnable, "Enable robust outlier rejection before cubic-spline fitting. Default ON at sigma 5.0 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3).");`
- `Project Files/Source/Console/PSForm.designer.cs:353:            this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Target Feedback Level for auto-attenuator and indicator. Default 22 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3) to keep ADC2208 and codec in their linear range; 152 for other hardware.");`
- `Project Files/Source/Console/clsHardwareSpecific.cs:328:                // Yurij_eu2av: Orion MK2 based rigs (ANAN-7000/8000/Anvelina PRO3)`
- `Project Files/Source/Console/clsHardwareSpecific.cs:397:                case "ANAN-7000DLE":`
- `Project Files/Source/Console/clsHardwareSpecific.cs:436:                    return "ANAN-7000DLE";`
- `Project Files/Source/Console/console.cs:8595:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII AnvelinaPro3`
- `Project Files/Source/Console/console.cs:8680:                    case HPSDRHW.OrionMKII: // ANAN-7000DLE 7000DLEMkII ANAN-8000DLE OrionMkII RedPitaya`
- `Project Files/Source/Console/database.cs:10531:        //                        else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D")) sRad = "ANAN-7000DLE";`
- `Project Files/Source/Console/database.cs:11091:                                else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D")) sRad = "ANAN-7000DLE";`
- `Project Files/Source/Console/setup.designer.cs:8571:            "ANAN-7000DLE",`

## `ANAN7000` — 77 match(es)

- `Project Files/Source/Console/CAT/CATCommands.cs:2932:            else if (radio == "ANAN100" || radio == "ANAN100B" || radio == "ANAN100D" || radio == "ANAN200D" || radio == "ANAN7000D" || radio == "ANAN8000D" || radio == "ANVELINAPRO3" || radio == "ANAN_G2" || radio == "ANAN_G2_1K")  // DH1KLM_21a added 7000D`
- `Project Files/Source/Console/CAT/CATCommands.cs:6137:            if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:150:                    case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:249:                return _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:259:                return _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:272:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:334:                    case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:350:                    case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:366:                    case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:398:                    return HPSDRModel.ANAN7000D;`
- `Project Files/Source/Console/clsHardwareSpecific.cs:435:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:466:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:483:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:518:                (_model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/clsHardwareSpecific.cs:752:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/clsHardwareSpecific.cs:831:                return !(_model == HPSDRModel.ORIONMKII || _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/cmaster.cs:628:                            case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/cmaster.cs:712:                            case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/cmaster.cs:748:                            case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/cmaster.cs:843:                            case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:6770:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:8250:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:10061:                HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:11036:                    HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:11062:                        HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:11206:                    HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:11232:                        HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:11711:                //        HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:14854:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:14898:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:15463:                HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:18756:                if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:19339:                        HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:19516:                    HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:21071:                    HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:21100:                if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:21606:                            HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:21689:                            HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:25049:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:25135:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:25861:                        HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:25887:                        HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:25918:                        if (HardwareSpecific.Model == HPSDRModel.ANAN_G2E || HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.REDPITAYA) //DH1KLM should be in P1  //N1GP G2E added`
- `Project Files/Source/Console/console.cs:25926:                        if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:26060:                        case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:27717:                        case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:27808:                        case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:31425:                 HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:31824:            if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:32441:            if (HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:40941:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/console.cs:40954:            if (HardwareSpecific.Model == HPSDRModel.ANAN100D || HardwareSpecific.Model == HPSDRModel.ANAN200D || HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/console.cs:53095:                HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/console.cs:53159:                bool use_sa = HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/console.cs:53318:                bool use_sa = HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/database.cs:10531:        //                        else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D")) sRad = "ANAN-7000DLE";`
- `Project Files/Source/Console/database.cs:11091:                                else if (getRadioSelectedFromOldRadButton(ref tempTable, "radGenModelANAN7000D")) sRad = "ANAN-7000DLE";`
- `Project Files/Source/Console/display.cs:1423:        //private static HPSDRModel _current_hpsdr_model = HPSDRModel.ANAN7000D;`
- `Project Files/Source/Console/enums.cs:127:        ANAN7000D,`
- `Project Files/Source/Console/frmAbout.Designer.cs:97:            "Radio Model : ANAN7000",`
- `Project Files/Source/Console/setup.cs:6291:            //    HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/setup.cs:6317:                HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/setup.cs:6377:            else if (HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/setup.cs:6384:                if ((HardwareSpecific.Model == HPSDRModel.ANAN7000D) ||`
- `Project Files/Source/Console/setup.cs:6463:                HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/setup.cs:6514:                HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/setup.cs:6551:                HardwareSpecific.Model == HPSDRModel.ANAN7000D || HardwareSpecific.Model == HPSDRModel.ANAN8000D ||`
- `Project Files/Source/Console/setup.cs:6584:            if (HardwareSpecific.Model == HPSDRModel.ANAN8000D || HardwareSpecific.Model == HPSDRModel.ANAN7000D ||`
- `Project Files/Source/Console/setup.cs:15740:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/setup.cs:15976:                HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/setup.cs:16035:                HardwareSpecific.Model != HPSDRModel.ANAN7000D &&`
- `Project Files/Source/Console/setup.cs:20341:                case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/setup.cs:23687:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:23721:            if (HardwareSpecific.Model == HPSDRModel.ANVELINAPRO3 || HardwareSpecific.Model == HPSDRModel.ANAN7000D)`
- `Project Files/Source/Console/setup.cs:24070:                    case HPSDRModel.ANAN7000D:`
- `Project Files/Source/Console/setup.cs:24074:                            string sSetting = "udANAN7000DPAGain" + mapBandToMeters(b).ToString();`
- `Project Files/Source/Console/setup.cs:24082:                            string sSetting = "udANAN7000DPAGainVHF" + (n - (int)Band.VHF0).ToString();`

## `Feedback Level` — 16 match(es)

- `Project Files/Source/Console/PSForm.cs:469:                // need this incase single cal is unable to complete do to bad feedback level`
- `Project Files/Source/Console/PSForm.designer.cs:353:            this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Target Feedback Level for auto-attenuator and indicator. Default 22 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3) to keep ADC2208 and codec in their linear range; 152 for other hardware.");`
- `Project Files/Source/Console/PSForm.designer.cs:482:            this.toolTip1.SetToolTip(this.chkPSAutoAttenuate, "Automatically adjust attenuator for optimum feedback level. (Recommended)");`
- `Project Files/Source/Console/PSForm.designer.cs:525:            this.labelTS8.Text = "Feedback Level";`
- `Project Files/Source/Console/PSForm.designer.cs:526:            this.toolTip1.SetToolTip(this.labelTS8, "Indicates, by color, correct/incorrect RF feedback level");`
- `Project Files/Source/Console/PSForm.designer.cs:538:            this.toolTip1.SetToolTip(this.lblPSInfoFB, "Indicates, by color, correct/incorrect RF feedback level");`
- `Project Files/Source/Console/PSForm.designer.cs:836:            this.toolTip1.SetToolTip(this.lblPSfb2, "Indicator:  RF feedback level; drives red/yellow/green indicator.");`
- `Project Files/Source/Console/PSForm.designer.cs:847:            this.toolTip1.SetToolTip(this.labelTS1, "Indicator:  RF feedback level; drives red/yellow/green indicator.");`
- `Project Files/Source/Console/clsHardwareSpecific.cs:322:        // Yurij_eu2av: hardware-specific target Feedback Level for PureSignal auto-attenuator.`
- `Project Files/Source/Console/console.cs:3480:        //            a.Add("last_radio_protocol/" + Audio.LastRadioProtocol.ToString()); // MW0LGE [2.9.0.8] used incase protocol changes from last time. Used in audio.cs tp reset PS feedback level`
- `Project Files/Source/Console/setup.designer.cs:10663:            this.chkHideFeebackLevel.Text = "Hide feedback level number";`
- `Project Files/Source/Console/setup.designer.cs:10664:            this.toolTip1.SetToolTip(this.chkHideFeebackLevel, "Hide the feedback level from the info bar");`
- `Project Files/Source/Console/setup.designer.cs:10677:            this.toolTip1.SetToolTip(this.chkSwapREDBluePSAColours, "Swap the red/blue colours used in the feedback level");`
- `Project Files/Source/Console/setup.designer.cs:72620:            // Yurij_eu2av: hidden persisted target Feedback Level for PureSignal auto-attenuator`
- `Project Files/Source/Console/ucInfoBar.Designer.cs:62:            this.toolTip1.SetToolTip(this.lblFB, "Feedback level in order. Blue > 181, Green > 128, Yellow > 90, Red >= 0");`
- `Project Files/Source/wdsp/calcc.h:161://		 4 - feedback level warning`

## `feedback target` — 0 match(es)

- none

## `outlier_sigma` — 20 match(es)

- `Project Files/Source/wdsp/calcc.c:141:	a->outlier_sigma = 0.0;`
- `Project Files/Source/wdsp/calcc.c:479:		if (a->outlier_sigma > 0.0)`
- `Project Files/Source/wdsp/calcc.c:480:			n_filt = reject_outliers(tx_filt, rx_filt, n_filt, a->outlier_sigma);`
- `Project Files/Source/wdsp/calcc.c:1365:	a->outlier_sigma = sigma;`
- `Project Files/Source/wdsp/calcc.c:1379:	a->outlier_sigma = 0.0;`
- `Project Files/Source/wdsp/calcc.h:51:	double outlier_sigma;	// 0.0 = disabled`
- `Project Files/Source/wdsp/nurbs_fit.c:87:    cfg->outlier_sigma      = 3.0;`
- `Project Files/Source/wdsp/nurbs_fit.c:102:    cfg->local_outlier_sigma = 4.0;`
- `Project Files/Source/wdsp/nurbs_fit.c:598:                                 double outlier_sigma,`
- `Project Files/Source/wdsp/nurbs_fit.c:612:    double threshold = outlier_sigma * sigma;`
- `Project Files/Source/wdsp/nurbs_fit.c:631:                                double outlier_sigma)`
- `Project Files/Source/wdsp/nurbs_fit.c:647:    if (outlier_sigma > 0.0) {`
- `Project Files/Source/wdsp/nurbs_fit.c:654:        threshold = outlier_sigma * sigma;`
- `Project Files/Source/wdsp/nurbs_fit.c:893:                                              cfg.outlier_sigma, inlier_mask);`
- `Project Files/Source/wdsp/nurbs_fit.c:934:                cfg.local_outlier_sigma,`
- `Project Files/Source/wdsp/nurbs_fit.c:1086:                                               cfg.outlier_sigma);`
- `Project Files/Source/wdsp/nurbs_fit.c:1119:                                           cfg.outlier_sigma);`
- `Project Files/Source/wdsp/nurbs_fit.c:1503:                                        cfg.outlier_sigma, inlier_mask);`
- `Project Files/Source/wdsp/nurbs_fit.h:91:    double outlier_sigma;`
- `Project Files/Source/wdsp/nurbs_fit.h:94:    double local_outlier_sigma;`

## `Detector Cal` — 5 match(es)

- `Project Files/Source/Console/console.cs:25097:        // Yurij-eu2av - 2026-07-02: Per-band power-detector calibration multiplier.`
- `Project Files/Source/Console/setup.cs:23542:        // Yurij-eu2av - 2026-07-02: Per-band power-detector calibration multiplier.`
- `Project Files/Source/Console/setup.cs:23598:            grpDetCal.Text = "Power Detector Calibration by Band (multiplier)";`
- `Project Files/Source/Console/setup.cs:23726:            MessageBox.Show("Detector calibration reset to defaults.\n\n" + modelDefaults +`
- `Project Files/Source/Console/setup.designer.cs:75820:        // Yurij-eu2av - 2026-07-02: per-band power-detector calibration controls`

## `detector calibration` — 5 match(es)

- `Project Files/Source/Console/console.cs:25097:        // Yurij-eu2av - 2026-07-02: Per-band power-detector calibration multiplier.`
- `Project Files/Source/Console/setup.cs:23542:        // Yurij-eu2av - 2026-07-02: Per-band power-detector calibration multiplier.`
- `Project Files/Source/Console/setup.cs:23598:            grpDetCal.Text = "Power Detector Calibration by Band (multiplier)";`
- `Project Files/Source/Console/setup.cs:23726:            MessageBox.Show("Detector calibration reset to defaults.\n\n" + modelDefaults +`
- `Project Files/Source/Console/setup.designer.cs:75820:        // Yurij-eu2av - 2026-07-02: per-band power-detector calibration controls`

## `voltage calibration` — 5 match(es)

- `Project Files/Source/Console/console.cs:24957:        // Yurij-eu2av - 2026-07-03: per-device voltage calibration multipliers.`
- `Project Files/Source/Console/setup.cs:100:        // Yurij-eu2av - 2026-07-03: voltage calibration controls (built in initVoltsAmpsCalibration)`
- `Project Files/Source/Console/setup.cs:24797:        // Yurij-eu2av - 2026-07-03: Voltage calibration handlers.`
- `Project Files/Source/Console/setup.cs:24810:        // Yurij-eu2av - 2026-07-03: reset voltage calibration to 1.000 (no correction).`
- `Project Files/Source/Console/setup.cs:24820:            // Yurij-eu2av - 2026-07-03: build voltage calibration controls`

## `PA Volts` — 0 match(es)

- none

## `Supply 13.8` — 0 match(es)

- none
