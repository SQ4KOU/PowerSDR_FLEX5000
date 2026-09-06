// SQ4KOU PowerSDR / Thetis direct console layout
// UI geometry only. Native PowerSDR radio and DSP paths are unchanged.

using System.Drawing;

namespace PowerSDR
{
    public partial class Console
    {
        private const bool SQ4KOU_ThetisUiEnabled = true;

        // Called immediately after InitializeComponent() and before
        // GrabConsoleSizeBasis().  The .resx has already been rewritten with
        // the pinned Thetis geometry; this method handles PowerSDR-only items
        // that have no one-to-one donor control.
        private void SQ4KOU_ApplyThetisBaseLayout()
        {
            if (!SQ4KOU_ThetisUiEnabled) return;

            SuspendLayout();
            try
            {
                ClientSize = new Size(1018, 721);

                // Thetis reference geometry.  These assignments are deliberate
                // invariants and also act as a runtime guard against a stale skin
                // or an older resource file restoring the KE9NS arrangement.
                grpVFOA.Location = new Point(125, 24);
                grpVFOA.Size = new Size(232, 88);
                grpVFOBetween.Location = new Point(360, 24);
                grpVFOBetween.Size = new Size(240, 88);
                grpVFOB.Location = new Point(603, 24);
                grpVFOB.Size = new Size(232, 88);
                grpMultimeter.Location = new Point(841, 24);
                grpMultimeter.Size = new Size(172, 88);

                panelDisplay.Location = new Point(124, 118);
                panelDisplay.Size = new Size(710, 300);

                panelBandHF.Location = new Point(840, 150);
                panelBandGN.Location = new Point(840, 150);
                panelBandVHF.Location = new Point(840, 150);
                panelBandHF.Size = new Size(173, 128);
                panelBandGN.Size = new Size(173, 128);
                panelBandVHF.Size = new Size(173, 128);

                panelMode.Location = new Point(840, 284);
                panelMode.Size = new Size(173, 104);
                panelFilter.Location = new Point(840, 392);
                panelFilter.Size = new Size(173, 192);

                panelVFO.Location = new Point(128, 420);
                panelVFO.Size = new Size(130, 168);
                panelDSP.Location = new Point(264, 420);
                panelDSP.Size = new Size(120, 96);
                panelDisplay2.Location = new Point(386, 420);
                panelDisplay2.Size = new Size(110, 96);
                panelMultiRX.Location = new Point(264, 516);
                panelMultiRX.Size = new Size(232, 72);

                panelModeSpecificPhone.Location = new Point(499, 420);
                panelModeSpecificCW.Location = new Point(499, 420);
                panelModeSpecificDigital.Location = new Point(499, 420);
                panelModeSpecificFM.Location = new Point(499, 420);

                // PowerSDR has no Thetis panelPower container.  Keep the native
                // Power control as a direct form child at the equivalent absolute
                // position instead of changing its parent.
                chkPower.Location = new Point(6, 30);

                // PowerSDR-only controls remain available in unused Thetis space.
                panelAntenna.Location = new Point(6, 528);
                panelAntenna.Size = new Size(117, 58);
                panelDateTime.Location = new Point(6, 590);
                panelDateTime.Size = new Size(117, 126);

                // Preserve the KE9NS band-stack panel without allowing it to
                // displace VFO A/B or the Thetis BAND/MODE/FILTER column.
                panelTSBandStack.Location = new Point(840, 588);
                panelTSBandStack.Size = new Size(173, 128);

                // The analog VFO dials belong to the old KE9NS geometry and can
                // cover the Thetis VFO row when enabled.  The VFO text controls
                // remain native and fully interactive.
                VFODialA.Visible = false;
                VFODialB.Visible = false;
                VFODialAA.Visible = false;
                VFODialBB.Visible = false;
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        // This is the only resize path used by the SQ4KOU layout.  It is adapted
        // from the pinned Thetis Console resize geometry and operates on the
        // existing PowerSDR controls; no controls are reparented or recreated.
        private void SQ4KOU_ResizeThetis(int hDelta, int vDelta)
        {
            if (!SQ4KOU_ThetisUiEnabled) return;

            SuspendLayout();
            try
            {
                panelFilter.Location = new Point(gr_filter_basis_location.X + hDelta,
                    gr_filter_basis_location.Y + vDelta);

                panelBandHF.Location = new Point(gr_BandHF_basis_location.X + hDelta,
                    gr_BandHF_basis_location.Y + (vDelta / 4));
                panelBandGN.Location = new Point(gr_BandGEN_basis_location.X + hDelta,
                    gr_BandGEN_basis_location.Y + (vDelta / 4));
                panelBandVHF.Location = new Point(gr_BandVHF_basis_location.X + hDelta,
                    gr_BandVHF_basis_location.Y + (vDelta / 4));

                panelMode.Location = new Point(gr_Mode_basis_location.X + hDelta,
                    gr_Mode_basis_location.Y + (vDelta / 2));

                grpVFOA.Size = new Size(232, 88);
                grpVFOB.Size = new Size(232, 88);
                grpVFOBetween.Size = new Size(240, 88);
                grpMultimeter.Size = new Size(172, 88);

                grpVFOA.Location = new Point(gr_VFOA_basis_location.X + (hDelta / 4),
                    gr_VFOA_basis_location.Y);
                grpVFOBetween.Location = new Point(gr_vfobetween_basis_location.X + (hDelta / 2),
                    gr_vfobetween_basis_location.Y);
                grpVFOB.Location = new Point(gr_VFOB_basis_location.X + hDelta - (hDelta / 4),
                    gr_VFOB_basis_location.Y);
                grpMultimeter.Location = new Point(gr_Multimeter_basis_location.X + hDelta,
                    gr_Multimeter_basis_location.Y);

                panelDisplay.Size = new Size(gr_display_size_basis.Width + hDelta,
                    gr_display_size_basis.Height + vDelta);
                picDisplay.Size = new Size(pic_display_size_basis.Width + hDelta,
                    pic_display_size_basis.Height + vDelta);

                panelVFO.Location = new Point(gr_VFO_basis_location.X + (hDelta / 4),
                    gr_VFO_basis_location.Y + vDelta);
                panelDisplay2.Location = new Point(gr_display2_basis.X + (hDelta / 2),
                    gr_display2_basis.Y + vDelta);
                panelDSP.Location = new Point(gr_dsp_basis.X + (hDelta / 2),
                    gr_dsp_basis.Y + vDelta);
                panelMultiRX.Location = new Point(gr_multirx_basis.X + (hDelta / 2),
                    gr_multirx_basis.Y + vDelta);

                panelModeSpecificPhone.Location = new Point(gr_ModePhone_basis_location.X + hDelta - (hDelta / 4),
                    gr_ModePhone_basis_location.Y + vDelta);
                panelModeSpecificCW.Location = new Point(gr_ModeCW_basis_location.X + hDelta - (hDelta / 4),
                    gr_ModeCW_basis_location.Y + vDelta);
                panelModeSpecificDigital.Location = new Point(gr_ModeDig_basis_location.X + hDelta - (hDelta / 4),
                    gr_ModeDig_basis_location.Y + vDelta);
                panelModeSpecificFM.Location = new Point(gr_ModeFM_basis_location.X + hDelta - (hDelta / 4),
                    gr_ModeFM_basis_location.Y + vDelta);

                panelOptions.Location = new Point(gr_options_basis.X,
                    gr_options_basis.Y + (vDelta / 4));
                panelSoundControls.Location = new Point(gr_sound_controls_basis.X,
                    gr_sound_controls_basis.Y + (vDelta / 8) + (vDelta / 4));
                chkPower.Location = new Point(chk_power_basis.X,
                    chk_power_basis.Y + (vDelta / 8));
                chkSquelch.Location = new Point(chk_squelch_basis.X,
                    chk_squelch_basis.Y + (vDelta / 2));
                picSquelch.Location = new Point(pic_sql_basis.X,
                    pic_sql_basis.Y + (vDelta / 2));
                ptbSquelch.Location = new Point(tb_sql_basis.X,
                    tb_sql_basis.Y + (vDelta / 2));

                panelAntenna.Location = new Point(gr_antenna_basis.X,
                    gr_antenna_basis.Y + ((vDelta * 3) / 4));
                panelDateTime.Location = new Point(gr_date_time_basis.X,
                    gr_date_time_basis.Y + ((vDelta * 3) / 4));

                grpDisplaySplit.Location = new Point(gr_display_split_basis.X + (hDelta / 2),
                    gr_display_split_basis.Y + vDelta);

                grpRX2Meter.Location = new Point(gr_rx2_meter_basis.X + hDelta,
                    gr_rx2_meter_basis.Y + vDelta);
                panelBandHFRX2.Location = new Point(gr_BandHFRX2_basis_location.X + hDelta,
                    gr_BandHFRX2_basis_location.Y + vDelta);
                panelBandGNRX2.Location = new Point(gr_BandGENRX2_basis_location.X + hDelta,
                    gr_BandGENRX2_basis_location.Y + vDelta);
                panelBandVHFRX2.Location = new Point(gr_BandVHFRX2_basis_location.X + hDelta,
                    gr_BandVHFRX2_basis_location.Y + vDelta);

                panelRX2Filter.Location = new Point(gr_rx2_filter_basis.X + (int)(hDelta * 0.66),
                    gr_rx2_filter_basis.Y + vDelta);
                panelRX2Mode.Location = new Point(gr_rx2_mode_basis.X + (int)(hDelta * 0.492),
                    gr_rx2_mode_basis.Y + vDelta);
                panelRX2Display.Location = new Point(gr_rx2_display_basis.X + (int)(hDelta * 0.383),
                    gr_rx2_display_basis.Y + vDelta);
                panelRX2DSP.Location = new Point(gr_rx2_dsp_basis.X + (int)(hDelta * 0.258),
                    gr_rx2_dsp_basis.Y + vDelta);
                panelRX2Mixer.Location = new Point(gr_rx2_mixer_basis.X + (int)(hDelta * 0.078),
                    gr_rx2_mixer_basis.Y + vDelta);

                lblRX2RF.Location = new Point(lbl_rx2_rf_basis.X + (int)(hDelta * 0.164),
                    lbl_rx2_rf_basis.Y + vDelta);
                ptbRX2RF.Location = new Point(tb_rx2_rf_basis.X + (int)(hDelta * 0.164),
                    tb_rx2_rf_basis.Y + vDelta);
                chkRX2Squelch.Location = new Point(chk_rx2_squelch_basis.X + (int)(hDelta * 0.164),
                    chk_rx2_squelch_basis.Y + vDelta);
                ptbRX2Squelch.Location = new Point(tb_rx2_squelch_basis.X + (int)(hDelta * 0.164),
                    tb_rx2_squelch_basis.Y + vDelta);
                picRX2Squelch.Location = new Point(pic_rx2_squelch_basis.X + (int)(hDelta * 0.164),
                    pic_rx2_squelch_basis.Y + vDelta);
                chkRX2.Location = new Point(chk_rx2_enable_basis.X,
                    chk_rx2_enable_basis.Y + vDelta);
                chkRX2Preamp.Location = new Point(chk_rx2_preamp_basis.X,
                    chk_rx2_preamp_basis.Y + vDelta);
                lblRX2Band.Location = new Point(lbl_rx2_band_basis.X,
                    lbl_rx2_band_basis.Y + vDelta);
                comboRX2Band.Location = new Point(combo_rx2_band_basis.X,
                    combo_rx2_band_basis.Y + vDelta);

                // PowerSDR-only band stack occupies the otherwise unused area
                // below the Thetis filter column.
                panelTSBandStack.Location = new Point(840 + hDelta, 588 + vDelta);
                panelTSBandStack.Size = new Size(173, 128);

                VFODialA.Visible = false;
                VFODialB.Visible = false;
                VFODialAA.Visible = false;
                VFODialBB.Visible = false;
            }
            finally
            {
                ResumeLayout(true);
            }
        }
    }
}
