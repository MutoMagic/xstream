namespace Xstream
{
    public static class Config
    {
        public static CFG_Mapping CurrentMapping { get; private set; }
        public static CFG_Quality DefaultQuality { get; private set; }

        public static bool Fullscreen { get; private set; }
        public static bool Borderless { get; private set; }
        public static bool UseController { get; private set; }
        public static bool KeyPreview { get; private set; }

        public static int LeftThumbstickOffset { get; private set; }
        public static int RightThumbstickOffset { get; private set; }
        public static int LeftXOffset { get; private set; }
        public static int LeftYOffset { get; private set; }
        public static int RightXOffset { get; private set; }
        public static int RightYOffset { get; private set; }

        public static int A { get; private set; }
        public static int B { get; private set; }
        public static int X { get; private set; }
        public static int Y { get; private set; }
        public static int DPadLeft { get; private set; }
        public static int DPadRight { get; private set; }
        public static int DPadUp { get; private set; }
        public static int DPadDown { get; private set; }
        public static int Start { get; private set; }
        public static int Back { get; private set; }
        public static int LeftShoulder { get; private set; }
        public static int RightShoulder { get; private set; }
        public static int LeftThumbstick { get; private set; }
        public static int RightThumbstick { get; private set; }
        public static int Guide { get; private set; }
        public static int LeftX { get; private set; }
        public static int LeftY { get; private set; }
        public static int RightX { get; private set; }
        public static int RightY { get; private set; }
        public static int TriggerLeft { get; private set; }
        public static int TriggerRight { get; private set; }

        static Config()
        {
            CurrentMapping = new CFG_Mapping();
            DefaultQuality = new CFG_Quality();
            DefaultQuality = GetQuality(GetSettingString("GAME_STREAMING_AVAILABLE_QUALITY_SETTINGS"));

            switch (GetSettingString("fullscreen"))
            {
                case "borderless":
                    Borderless = true;
                    goto case "exclusive";
                case "exclusive":
                    Fullscreen = true;
                    break;
                case "windowed":
                    goto default;
                default:
                    Fullscreen = false;
                    Borderless = false;
                    break;
            }
            UseController = GetSettingBool("useController");
            KeyPreview = GetSettingBool("KeyPreview");

            if (KeyPreview)
            {
                LeftThumbstickOffset = GetSettingInt("KeyPreview.LeftThumbstick.Offset");
                RightThumbstickOffset = GetSettingInt("KeyPreview.RightThumbstick.Offset");
                LeftXOffset = GetSettingInt("KeyPreview.LeftX.Offset");
                LeftYOffset = GetSettingInt("KeyPreview.LeftY.Offset");
                RightXOffset = GetSettingInt("KeyPreview.RightX.Offset");
                RightYOffset = GetSettingInt("KeyPreview.RightY.Offset");

                A = GetSettingInt("KeyPreview.A");
                B = GetSettingInt("KeyPreview.B");
                X = GetSettingInt("KeyPreview.X");
                Y = GetSettingInt("KeyPreview.Y");
                DPadLeft = GetSettingInt("KeyPreview.DPadLeft");
                DPadRight = GetSettingInt("KeyPreview.DPadRight");
                DPadUp = GetSettingInt("KeyPreview.DPadUp");
                DPadDown = GetSettingInt("KeyPreview.DPadDown");
                Start = GetSettingInt("KeyPreview.Start");
                Back = GetSettingInt("KeyPreview.Back");
                LeftShoulder = GetSettingInt("KeyPreview.LeftShoulder");
                RightShoulder = GetSettingInt("KeyPreview.RightShoulder");
                LeftThumbstick = GetSettingInt("KeyPreview.LeftThumbstick");
                RightThumbstick = GetSettingInt("KeyPreview.RightThumbstick");
                Guide = GetSettingInt("KeyPreview.Guide");
                LeftX = GetSettingInt("KeyPreview.LeftX");
                LeftY = GetSettingInt("KeyPreview.LeftY");
                RightX = GetSettingInt("KeyPreview.RightX");
                RightY = GetSettingInt("KeyPreview.RightY");
                TriggerLeft = GetSettingInt("KeyPreview.TriggerLeft");
                TriggerRight = GetSettingInt("KeyPreview.TriggerRight");
            }
        }

        public static CFG_Quality GetQuality(string quality)
        {
            if (string.IsNullOrEmpty(quality))
            {
                return DefaultQuality;
            }

            if (DefaultQuality.Name != null && DefaultQuality.Name.Equals(quality))
            {
                return DefaultQuality;
            }

            return new CFG_Quality(quality);
        }

        public static bool GetSettingBool(string key) => GetConfigurationBool("SETTINGS", key);

        public static int GetSettingInt(string key) => GetConfigurationInt("SETTINGS", key);

        public static string GetSettingString(string key) => GetConfigurationString("SETTINGS", key);

        public static string GetMappingString(string key) => GetConfigurationString("MAPPING", key);

        public static bool GetConfigurationBool(string section, string key)
        {
            bool.TryParse(GetConfigurationString(section, key), out bool result);
            return result;// When TryParse is false, the result is false
        }

        public static int GetConfigurationInt(string section, string key)
        {
            int.TryParse(GetConfigurationString(section, key), out int result);
            return result;// When TryParse is false, the result is 0
        }

        public static string GetConfigurationString(string section, string key)
            => Native.GetPrivateProfileString(section, key, "", "./cfg.ini");
    }

    public class CFG_Quality
    {
        public string Name { get; private set; }

        public int Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }
        public int Unknown3 { get; private set; }
        public int Unknown4 { get; private set; }
        public int Unknown5 { get; private set; }
        public int Unknown6 { get; private set; }
        public int Unknown7 { get; private set; }
        public int Unknown8 { get; private set; }

        public CFG_Quality()
        {
            Name = null;

            Unknown1 = 6000002;
            Unknown2 = 720;
            Unknown3 = 60;
            Unknown4 = 3600;
            Unknown5 = 0;
            Unknown6 = 40;
            Unknown7 = 70;
            Unknown8 = 200;
        }

        public CFG_Quality(string quality)
        {
            Name = quality;

            Unknown1 = Config.GetConfigurationInt(quality, "Unknown1");
            Unknown2 = Config.GetConfigurationInt(quality, "Unknown2");
            Unknown3 = Config.GetConfigurationInt(quality, "Unknown3");
            Unknown4 = Config.GetConfigurationInt(quality, "Unknown4");
            Unknown5 = Config.GetConfigurationInt(quality, "Unknown5");
            Unknown6 = Config.GetConfigurationInt(quality, "Unknown6");
            Unknown7 = Config.GetConfigurationInt(quality, "Unknown7");
            Unknown8 = Config.GetConfigurationInt(quality, "Unknown8");
        }
    }

    public class CFG_Mapping
    {
        public string TokenFilePath
        {
            get => _tokenFilePath;
            set
            {
                _tokenFilePath = value;

                string[] mapping = Config.GetMappingString(_tokenFilePath).Split(',');
                IP = mapping[0];
                if (mapping.Length == 2)
                {
                    Quality = Config.GetQuality(mapping[1]);
                }
                else
                {
                    Quality = Config.DefaultQuality;
                }
            }
        }
        public string IP { get; set; }
        public CFG_Quality Quality { get; set; }

        string _tokenFilePath;
    }
}
