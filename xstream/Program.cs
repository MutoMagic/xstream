using SmartGlass;
using SmartGlass.Common;
using SmartGlass.Nano;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;

namespace Xstream
{
    static class Program
    {
        public static NanoClient Nano { get; private set; }

        public static AudioFormat AudioFormat { get; private set; }
        public static VideoFormat VideoFormat { get; private set; }
        public static AudioFormat ChatAudioFormat { get; private set; }

        static string _userHash;
        static string _xToken;
        static string _tokenFilePath;

        static void Authenticate()
        {
            if (string.IsNullOrEmpty(_tokenFilePath))
            {
                throw Shell.Abort("The tokenFilePath IsNullOrEmpty!", null);
            }

            AuthenticationService auth;
            try
            {
                using (FileStream fs = new FileStream(_tokenFilePath, FileMode.Open))
                    (auth = AuthenticationService.LoadFromFile(fs)).Authenticate();
            }
            catch (Exception e)
            {
                throw Shell.Abort($"Failed to refresh XBL tokens, error: {e.Message}", e);
            }
            _userHash = auth.XToken.UserInformation.Userhash;
            _xToken = auth.XToken.Jwt;
        }

        static void Authenticate(string url)
        {
            AuthenticationService auth;
            try
            {
                // Call requestUrl via WebWidget or manually and authenticate
                WindowsLiveResponse rep = AuthenticationService.ParseWindowsLiveResponse(url);
                (auth = new AuthenticationService(rep)).Authenticate();
            }
            catch (Exception e)
            {
                throw Shell.Abort($"Authentication failed, error: {e.Message}", e);
            }

            FileStream tokenOutputFile;
            try
            {
                using (tokenOutputFile = new FileStream(_tokenFilePath, FileMode.Create))
                    auth.DumpToFile(tokenOutputFile);// Save token to JSON
            }
            catch (Exception e)
            {
                throw Shell.Abort("Failed to open token outputfile \'{0}\', error: {1}", e
                    , _tokenFilePath, e.Message);
            }
            _userHash = auth.XToken.UserInformation.Userhash;
            _xToken = auth.XToken.Jwt;

            Shell.WriteLine(auth.XToken);
            Shell.WriteLine(auth.UserInformation);
            Shell.WriteLine("Storing tokens to file \'{0}\' on successful auth"
                , tokenOutputFile.Name);
        }

        static GamestreamSession ConnectToConsole(string ipAddress, GamestreamConfiguration gamestreamConfig)
        {
            try
            {
                // 如果Task失败了GetAwaiter()会直接抛出异常，而Task.Wait()会抛出AggregateException
                SmartGlassClient client = SmartGlassClient.ConnectAsync(ipAddress, _userHash, _xToken)
                    .GetAwaiter().GetResult();
                return client.BroadcastChannel.StartGamestreamAsync(gamestreamConfig)
                    .GetAwaiter().GetResult();
            }
            catch (SmartGlassException e)
            {
                throw Shell.Abort($"Failed to connect: {e.Message}", e);
            }
            catch (TimeoutException e)
            {
                throw Shell.Abort($"Timeout while connecting: {e.Message}", e);
            }
            catch (Exception e)
            {
                throw Shell.Abort(e.Message, e);
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Native.AllocConsole();

            if (args.Length == 0)
            {
                _tokenFilePath = Shell.WriteReadLine("tokenFilePath: ");
            }
            else
            {
                _tokenFilePath = args[0];
            }

            Config.CurrentMapping.TokenFilePath = _tokenFilePath;
            switch (args.Length)
            {
                case 3:
                    Config.CurrentMapping.Quality = Config.GetQuality(args[2]);
                    goto case 2;
                case 2:
                    Config.CurrentMapping.IP = args[1];
                    break;
            }

            if (File.Exists(_tokenFilePath))
            {
                Authenticate();
            }
            else
            {
                Shell.Warning("\'{0}\' file not found.\n", _tokenFilePath);
                Shell.WriteLine("1) Open following URL in your WebBrowser:\n\n{0}\n\n"
                    + "2) Authenticate with your Microsoft Account\n"
                    + "3) Paste returned URL from addressbar: \n"
                    , AuthenticationService.GetWindowsLiveAuthenticationUrl());

                Authenticate(Shell.WriteReadLine());
            }

            if (Config.CurrentMapping.IP.Length == 0)
            {
                Shell.WriteLine("{0,-15} {1,-36} {2,-15} {3,-16}", "Name", "HardwareId", "Address", "LiveId");
                IEnumerable<Device> devices = Device.DiscoverAsync().GetAwaiter().GetResult();
                foreach (Device device in devices)
                {
                    Shell.WriteLine("{0,-15} {1,-36} {2,-15} {3,-16}"
                        , device.Name
                        , device.HardwareId
                        , device.Address
                        , device.LiveId);
                }

                Config.CurrentMapping.IP = Shell.WriteReadLine("Input IP Address or hostname: ");
            }

            // Get general gamestream configuration
            GamestreamConfiguration config = GamestreamConfiguration.GetStandardConfig();
            // Modify standard config, if desired
            /*
             * GAME_STREAMING_VERY_HIGH_QUALITY_SETTINGS: 12000000,1080,60,59,0,10,40,170
             * GAME_STREAMING_HIGH_QUALITY_SETTINGS: 8000000,720,60,59,0,10,40,170
             * GAME_STREAMING_MEDIUM_QUALITY_SETTINGS: 6000002,720,60,3600,0,40,70,200
             * GAME_STREAMING_LOW_QUALITY_SETTINGS: 3000001,480,30,3600,0,40,70,200
             * 
             * 12000000 = 12Mbit/s = 12Mbps
             * 
             * SETTINGS:
             * Unknown1,Unknown2,Unknown3,Unknown4,Unknown5,Unknown6,Unknown7,Unknown8
             * Unknown1 UrcpMaximumRate         FIXME: Or AudioBufferLengthHns, both??
             * Unknown2 VideoMaximumHeight
             * Unknown3 VideoMaximumFrameRate
             * Unknown4                         FIXME: Which is Unknown4?
             * Unknown5                         FIXME: Which is Unknown5?
             * Unknown6 AudioSyncMinLatency
             * Unknown7 AudioSyncDesiredLatency
             * Unknown8 AudioSyncMaxLatency
             * 
             * refer to: https://github.com/OpenXbox/xbox-smartglass-nano-python/issues/7
             * standard: GAME_STREAMING_MEDIUM_QUALITY_SETTINGS
             */
            //config.UrcpMaximumRate = 12000000;// 2后面6个0
            //config.VideoMaximumHeight = 480;
            //config.VideoMaximumFrameRate = 30;
            //config.Unknown4 = 3600;
            //config.Unknown5 = 0;
            //config.AudioSyncMinLatency = 40;
            //config.AudioSyncDesiredLatency = 70;
            //config.AudioSyncMaxLatency = 200;
            config.UrcpMaximumRate = Config.CurrentMapping.Quality.Unknown1;
            config.VideoMaximumHeight = Config.CurrentMapping.Quality.Unknown2;
            config.VideoMaximumFrameRate = Config.CurrentMapping.Quality.Unknown3;
            //config.Unknown4 = Config.CurrentMapping.Quality.Unknown4;
            //config.Unknown5 = Config.CurrentMapping.Quality.Unknown5;
            config.AudioSyncMinLatency = Config.CurrentMapping.Quality.Unknown6;
            config.AudioSyncDesiredLatency = Config.CurrentMapping.Quality.Unknown7;
            config.AudioSyncMaxLatency = Config.CurrentMapping.Quality.Unknown8;

            if (TVResolution.SupportList.ContainsKey(config.VideoMaximumHeight))
            {
                // XboxTVResolution
                config.VideoMaximumWidth = TVResolution.SupportList[config.VideoMaximumHeight].W;
            }
            else
            {
                bool convert = true;

                // ComputerTVResolution
                DEVMODE vDevMode = new DEVMODE();
                vDevMode.dmSize = (short)Marshal.SizeOf(vDevMode);
                vDevMode.dmDriverExtra = 0;
                for (int i = 0; Native.EnumDisplaySettings(null, i, ref vDevMode); i++)
                {
                    bool n = false;

                    TVResolution r = new TVResolution(vDevMode.dmPelsWidth, vDevMode.dmPelsHeight);
                    if (TVResolution.H16V9 == r.P && config.VideoMaximumHeight == r.H && r.Priority)
                    {
                        if (convert)
                        {
                            convert = false;
                        }
                        config.VideoMaximumWidth = r.W;

                        n = true;
                    }

                    Shell.WriteLine("{0}x{1} Color:{2} Frequency:{3} AspectRatio[{4}]"
                        , n ? Shell.NOTE_COLOR : Shell.DEFAULT_COLOR
                        , true
                        , vDevMode.dmPelsWidth
                        , vDevMode.dmPelsHeight
                        , 1L << vDevMode.dmBitsPerPel
                        , vDevMode.dmDisplayFrequency
                        , r.AspectRatio);
                }

                if (convert)
                {
                    config.VideoMaximumWidth = TVResolution.Width(config.VideoMaximumHeight, TVResolution.H16V9);
                    Shell.Warning("未在XboxTVResolution与ComputerTVResolution中找到对应的分辨率，计算得出{0}x{1}"
                        , config.VideoMaximumWidth, config.VideoMaximumHeight);
                }
            }

            Shell.WriteLine($"Connecting to {Config.CurrentMapping.IP}...");
            GamestreamSession session = ConnectToConsole(Config.CurrentMapping.IP, config);

            Shell.WriteLine($"Connecting to NANO // TCP: {session.TcpPort}, UDP: {session.UdpPort}");
            Nano = new NanoClient(Config.CurrentMapping.IP, session);
            try
            {
                // General Handshaking & Opening channels
                Shell.WriteLine("Running protocol init...");
                Nano.InitializeProtocolAsync().Wait();

                // Start Controller input channel
                Nano.OpenInputChannelAsync(Nano.Video.Width, Nano.Video.Height).Wait();

                // Audio & Video client handshaking
                // Sets desired AV formats
                AudioFormat = Nano.AudioFormats[0];
                VideoFormat = Nano.VideoFormats[0];

                Shell.WriteLine("Initializing AV stream (handshaking)...");
                Nano.InitializeStreamAsync(AudioFormat, VideoFormat).Wait();

                // Start ChatAudio channel
                // TODO: Send opus audio chat samples to console
                ChatAudioFormat = new AudioFormat(1, 24000, AudioCodec.Opus);
                Nano.OpenChatAudioChannelAsync(ChatAudioFormat).Wait();

                // Tell console to start sending AV frames
                Shell.WriteLine("Starting stream...");
                Nano.StartStreamAsync().Wait();
                Shell.Note("Stream is running");
            }
            catch (Exception e)
            {
                throw Shell.Abort($"Failed to init Nano, error: {e.Message}", e);
            }

#if !DEBUG
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new Logger(_tokenFilePath));
            Native.FreeConsole();
#endif

            // Run a mainloop, to gather controller input events or similar
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Xstream());

            // finally (dirty)
            Process.GetCurrentProcess().Kill();
        }
    }

    /// <summary>
    /// 关于电视分辨率和 Xbox One
    /// https://support.xbox.com/zh-CN/help/hardware-network/display-sound/tv-resolutions
    /// </summary>
    public class TVResolution
    {
        public static readonly int H4V3 = Per(4, 3);
        public static readonly int H16V9 = Per(16, 9);

        public static TVResolution EDTV_480P = new TVResolution(640, 480, H4V3);
        public static TVResolution HDTV_720p = new TVResolution(1280, 720, H16V9);
        public static TVResolution HDTV_1080i = new TVResolution(1920, 1080, H16V9) { _scan = "1080i" };
        public static TVResolution HDTV_1080p = new TVResolution(1920, 1080, H16V9) { _scan = "1080p" };
        public static TVResolution HDTV_1440p = new TVResolution(2560, 1440, H16V9);
        public static TVResolution UHDTV_4K = new TVResolution(3840, 2160, H16V9);

        public static Dictionary<int, TVResolution> SupportList = new Dictionary<int, TVResolution>();

        static Dictionary<long, TVResolution> SpecialList = new Dictionary<long, TVResolution>();

        static TVResolution()
        {
            SupportList.Add(EDTV_480P);
            SupportList.Add(HDTV_720p);
            SupportList.Add(HDTV_1080p);
            SupportList.Add(HDTV_1440p);
            SupportList.Add(UHDTV_4K);

            SpecialList.Add(new TVResolution(480, 234, H16V9));
            SpecialList.Add(new TVResolution(480, 272, H16V9));
            SpecialList.Add(new TVResolution(848, 480, H16V9) { _scan = string.Empty });
            SpecialList.Add(new TVResolution(854, 480, H16V9));
            SpecialList.Add(new TVResolution(960, 544, H16V9));
            SpecialList.Add(new TVResolution(1024, 600, H16V9));
            SpecialList.Add(new TVResolution(1136, 640, H16V9) { _scan = string.Empty });
            SpecialList.Add(new TVResolution(1138, 640, H16V9));
            SpecialList.Add(new TVResolution(1334, 750, H16V9));
            SpecialList.Add(new TVResolution(1360, 768, H16V9) { _scan = string.Empty });
            SpecialList.Add(new TVResolution(1366, 768, H16V9));
            SpecialList.Add(new TVResolution(1776, 1000, H16V9));
        }

        public static int Per(int horizon, int vertical) => horizon << 16 | vertical;

        public static int Horizon(int p) => p >> 16;

        public static int Vertical(int p) => p & 0xFFFF;

        public static long LPer(int horizon, int vertical) => horizon << 32 | vertical;

        public static int LHorizon(long p) => (int)(p >> 32);

        public static int LVertical(long p) => (int)(p & 0xFFFFFFFF);

        public static int Width(int h, int p) => h / Vertical(p) * Horizon(p);

        public static int Height(int w, int p) => w / Horizon(p) * Vertical(p);

        static int GCD(int a, int b)
        {
            if (0 != b) while (0 != (a %= b) && 0 != (b %= a)) ;
            return a + b;
        }

        public int W { get; private set; }
        public int H { get; private set; }
        public int P { get; private set; }
        public string AspectRatio => $"{Horizon(P)}:{Vertical(P)}";
        public bool Priority => _scan == null || _scan.EndsWith('p');

        string _scan;

        public TVResolution(int w, int h)
            : this(w, h, 0)
        {
            long k = LPer(w, h);
            if (SpecialList.ContainsKey(k))
            {
                P = SpecialList[k].P;
            }
            else
            {
                P = GCD(w, h);// 最大公约数
                P = Per(w / P, h / P);
            }
        }

        TVResolution(int w, int h, int p)
        {
            W = w;
            H = h;
            P = p;
        }
    }

    static class Dictionary_Extensions
    {
        public static void Add(this Dictionary<int, TVResolution> d, TVResolution r) => d.Add(r.H, r);

        public static void Add(this Dictionary<long, TVResolution> d, TVResolution r)
            => d.Add(TVResolution.LPer(r.W, r.H), r);
    }
}
