using System.IO;
using MissionPlanner.Utilities;

namespace MissionPlanner.Aerostar
{
    /// <summary>
    /// Aerostar Dynamics branding, built into the program.
    /// </summary>
    /// <remarks>
    /// The images and the theme in Aerostar\Branding are copied next to MissionPlanner.exe by the build, where Mission
    /// Planner already looks for logo.png, logo2.png, icon.png, light_*_icon.png and *.mpsystheme. The name is set here
    /// instead of in a logo.txt file, and the standard settings file (config.xml) is kept: the 2025 Aerostar build used
    /// logo.txt, which also switched settings to "Aerostar Dynamics.xml", so whenever logo.txt went missing the program
    /// came back as plain Mission Planner with plain settings.
    /// </remarks>
    public static class AerostarBranding
    {
        public const string Name = "Aerostar Mission Planner";

        public const string ThemeFile = "AerostarDynamics.mpsystheme";

        /// <summary>Settings file of the 2025 Aerostar build (Dave Howie's), migrated once.</summary>
        const string LegacySettingsFile = "Aerostar Dynamics.xml";

        /// <summary>Set once all first-run defaults have been applied; after that the user's choices are kept.</summary>
        const string DefaultsAppliedKey = "aerostar_defaults_v1";

        /// <summary>Applied on the first run of the Aerostar build, only where the user has no value yet.</summary>
        static readonly string[,] Defaults =
        {
            { "altunits", "Feet" },
            { "distunits", "Feet" },
            { "speedunits", "mph" },
            { "APMFirmware", "ArduCopter" },
        };

        /// <summary>
        /// Copies the 2025 Aerostar build's settings into the standard settings file, once, if the user has no standard
        /// settings yet.
        /// </summary>
        public static void MigrateLegacySettings()
        {
            try
            {
                var dir = Settings.GetUserDataDirectory();
                var legacy = Path.Combine(dir, LegacySettingsFile);
                var current = Path.Combine(dir, Settings.FileName);
                if (File.Exists(legacy) && !File.Exists(current))
                    File.Copy(legacy, current);
            }
            catch
            {
            }
        }

        /// <summary>
        /// First run of the Aerostar build: Aerostar theme, feet / mph and ArduCopter where not already set. Later runs
        /// change nothing, so a user who picks another theme or units keeps them.
        /// </summary>
        public static void ApplyFirstRunDefaults()
        {
            if (Settings.Instance[DefaultsAppliedKey] != null)
                return;

            if (File.Exists(Settings.GetRunningDirectory() + ThemeFile))
                Settings.Instance["theme"] = ThemeFile;

            for (int i = 0; i < Defaults.GetLength(0); i++)
            {
                if (Settings.Instance[Defaults[i, 0]] == null)
                    Settings.Instance[Defaults[i, 0]] = Defaults[i, 1];
            }

            Settings.Instance[DefaultsAppliedKey] = "1";

            // MainV2 re-reads the settings file at start-up (LoadConfig), which would put back the old theme value.
            Settings.Instance.Save();
        }
    }
}
