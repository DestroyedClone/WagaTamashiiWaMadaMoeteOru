﻿using RiskOfOptions;

namespace SillyHitboxViewer {
    public static class RiskOfOptionsCompat {

        private static bool? _enabled;

        public static bool enabled {
            get {
                if (_enabled == null) {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                }
                return (bool)_enabled;
            }
        }

        public static void doOptions() {

            ModSettingsManager.setPanelTitle("Hitbox Viewer");
            ModSettingsManager.setPanelDescription("Enable/disable hitbox or hurtbox viewer");

            ModSettingsManager.addOption(new ModOption(ModOption.OptionType.Bool, "Enable Hitboxes", $"Shows hitboxes on attacks.\nCan be overridden by pressing {Utils.cfg_toggleKey}", "1"));
            ModSettingsManager.addListener(ModSettingsManager.getOption("Enable Hitboxes"), new UnityEngine.Events.UnityAction<bool>(hitboxBoolEvent));

            ModSettingsManager.addOption(new ModOption(ModOption.OptionType.Bool, "Enable Hurtboxes", $"Shows hurtboxes on characters\nCan be overridden by pressing {Utils.cfg_toggleKey}", "0"));
            ModSettingsManager.addListener(ModSettingsManager.getOption("Enable Hurtboxes"), new UnityEngine.Events.UnityAction<bool>(hurtboxBoolEvent));
        }

        public static void hitboxBoolEvent(bool active) {

            HitboxViewerMod.setShowingHitboxes(!active);
        }
        public static void hurtboxBoolEvent(bool active) {

            HitboxViewerMod.setShowingHurtboxes(!active, true);
        }

        public static void readOptions() {

            string disableHit = ModSettingsManager.getOptionValue("Enable Hitboxes");
            if (!string.IsNullOrEmpty(disableHit)) {
                HitboxViewerMod.setShowingHitboxes(disableHit == "1");
            }

            string disableHurt = ModSettingsManager.getOptionValue("Enable Hurtboxes");
            if (!string.IsNullOrEmpty(disableHurt)) {
                HitboxViewerMod.setShowingHurtboxes(disableHurt == "1", false);
            }
        }
    }
}
