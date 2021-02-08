using SmartGlass;
using SmartGlass.Common;
using SmartGlass.Nano;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        /// <summary>
        /// Authenticate with Xbox Live / refresh tokens
        /// </summary>
        /// <param name="tokenFilePath">File to json tokenfile</param>
        /// <param name="ae">AuthenticateException</param>
        /// <returns>True if successful, otherwise false</returns>
        static bool Authenticate(string tokenFilePath, out Exception ae)
        {
            ae = null;

            if (string.IsNullOrEmpty(tokenFilePath))
            {
                ae = new ArgumentNullException("tokenFilePath IsNullOrEmpty");
                return false;
            }

            FileStream fs = new FileStream(tokenFilePath, FileMode.Open);
            AuthenticationService auth = AuthenticationService.LoadFromFile(fs);
            try
            {
                auth.Authenticate();
            }
            catch (Exception e)
            {
                ae = e;
                return false;
            }
            fs.Close();

            _userHash = auth.XToken.UserInformation.Userhash;
            _xToken = auth.XToken.Jwt;
            return true;
        }

        static bool Authenticate(string url, string tokenFilePath
            , out AuthenticationService auth, out Exception ae
            , out FileStream tokenOutputFile, out Exception fe)
        {
            tokenOutputFile = null;
            ae = null;
            fe = null;

            // Call requestUrl via WebWidget or manually and authenticate
            WindowsLiveResponse rep = AuthenticationService.ParseWindowsLiveResponse(url);
            auth = new AuthenticationService(rep);
            try
            {
                auth.Authenticate();
            }
            catch (Exception e)
            {
                ae = e;
                return false;
            }

            // Save token to JSON
            try
            {
                tokenOutputFile = new FileStream(tokenFilePath, FileMode.Create);
            }
            catch (Exception e)
            {
                fe = e;
                return false;
            }
            auth.DumpToFile(tokenOutputFile);
            tokenOutputFile.Close();

            _userHash = auth.XToken.UserInformation.Userhash;
            _xToken = auth.XToken.Jwt;
            return true;
        }

        /// <summary>
        /// Connect to console, request Broadcast Channel and start gamestream
        /// </summary>
        /// <param name="ipAddress">IP address of console</param>
        /// <param name="gamestreamConfig">Desired gamestream configuration</param>
        /// <param name="ce">ConnectException</param>
        /// <returns>Null if failed</returns>
        public static GamestreamSession ConnectToConsole(string ipAddress
            , GamestreamConfiguration gamestreamConfig, out Exception ce)
        {
            ce = null;

            try
            {
                // 如果Task失败了GetAwaiter()会直接抛出异常，而Task.Wait()会抛出AggregateException
                SmartGlassClient client = SmartGlassClient.ConnectAsync(ipAddress, _userHash, _xToken)
                    .GetAwaiter().GetResult();
                return client.BroadcastChannel.StartGamestreamAsync(gamestreamConfig)
                    .GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ce = e;
                return null;
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Native.AllocConsole();

            Console.Write("tokenFilePath: ");
            string tokenFilePath = Console.ReadLine();

            if (File.Exists(tokenFilePath))
            {
                if (!Authenticate(tokenFilePath, out Exception ae))
                {
                    Shell.WriteLine($"Error: Failed to refresh XBL tokens, error: {ae.Message}");
                    Shell.PressAnyKeyToContinue();
                    return;
                }
            }
            else
            {
                Shell.WriteLine("Warning: '{0}' file not found.\n", tokenFilePath);
                Shell.WriteLine("1) Open following URL in your WebBrowser:\n\n{0}\n\n"
                    + "2) Authenticate with your Microsoft Account\n"
                    + "3) Paste returned URL from addressbar: \n"
                    , AuthenticationService.GetWindowsLiveAuthenticationUrl());

                if (Authenticate(Console.ReadLine(), tokenFilePath
                    , out AuthenticationService auth, out Exception ae
                    , out FileStream tokenOutputFile, out Exception fe))
                {
                    Console.WriteLine(auth.XToken);
                    Console.WriteLine(auth.UserInformation);

                    Shell.WriteLine("Storing tokens to file \'{0}\' on successful auth",
                            tokenOutputFile.Name);
                }
                else
                {
                    if (ae != null)
                    {
                        Shell.WriteLine($"Error: Authentication failed, error: {ae.Message}");
                    }

                    if (fe != null)
                    {
                        Shell.WriteLine("Error: Failed to open token outputfile \'{0}\', error: {1}",
                            tokenOutputFile, fe.Message);
                    }

                    Shell.PressAnyKeyToContinue();
                    return;
                }
            }

            Config.CurrentMapping.TokenFilePath = tokenFilePath;
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

                Console.Write("Input IP Address or hostname: ");
                Config.CurrentMapping.IP = Console.ReadLine();
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
            if (Config.CurrentMapping.Quality != null)
            {
                config.UrcpMaximumRate = Config.CurrentMapping.Quality.Unknown1;
                config.VideoMaximumHeight = Config.CurrentMapping.Quality.Unknown2;
                config.VideoMaximumFrameRate = Config.CurrentMapping.Quality.Unknown3;
                //config.Unknown4 = Config.CurrentMapping.Quality.Unknown4;
                //config.Unknown5 = Config.CurrentMapping.Quality.Unknown5;
                config.AudioSyncMinLatency = Config.CurrentMapping.Quality.Unknown6;
                config.AudioSyncDesiredLatency = Config.CurrentMapping.Quality.Unknown7;
                config.AudioSyncMaxLatency = Config.CurrentMapping.Quality.Unknown8;

                if (XboxTVResolution.SupportList.ContainsKey(config.VideoMaximumHeight))
                {
                    config.VideoMaximumWidth = XboxTVResolution.SupportList[config.VideoMaximumHeight].W;
                }
                else
                {

                }
            }

            Shell.WriteLine($"Connecting to {Config.CurrentMapping.IP}...");
            GamestreamSession session = ConnectToConsole(Config.CurrentMapping.IP, config, out Exception ce);
            if (session == null)
            {
                if (ce is SmartGlassException)
                    Shell.WriteLine($"Error: Failed to connect: {ce.Message}");
                else if (ce is TimeoutException)
                    Shell.WriteLine($"Error: Timeout while connecting: {ce.Message}");
                else
                    Shell.WriteLine($"Error: {ce}");

                Shell.PressAnyKeyToContinue();
                return;
            }

            Shell.WriteLine($"Connecting to NANO // TCP: {session.TcpPort}, UDP: {session.UdpPort}");
            Nano = new NanoClient(Config.CurrentMapping.IP, session);
            try
            {
                // General Handshaking & Opening channels
                Shell.WriteLine($"Running protocol init...");
                Nano.InitializeProtocolAsync().Wait();

                // Start Controller input channel
                Nano.OpenInputChannelAsync((uint)config.VideoMaximumWidth, (uint)config.VideoMaximumHeight).Wait();

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
                Shell.WriteLine("Note: Stream is running");
            }
            catch (Exception e)
            {
                Shell.WriteLine($"Error: Failed to init Nano, error: {e}");
                Shell.PressAnyKeyToContinue();
                return;
            }

            Native.FreeConsole();

            // Run a mainloop, to gather controller input events or similar
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Xstream(config.VideoMaximumWidth, config.VideoMaximumHeight));

            // finally (dirty)
            Process.GetCurrentProcess().Kill();
        }
    }

    /// <summary>
    /// 关于电视分辨率和 Xbox One
    /// https://support.xbox.com/zh-CN/help/hardware-network/display-sound/tv-resolutions
    /// </summary>
    public class XboxTVResolution
    {
        public readonly int H4V3 = Per(4, 3);
        public readonly int H16V9 = Per(16, 9);

        public static XboxTVResolution EDTV_480P = new XboxTVResolution(640, 480, "480p");
        public static XboxTVResolution HDTV_720p = new XboxTVResolution(1280, 720, "720p");
        public static XboxTVResolution HDTV_1080i = new XboxTVResolution(1920, 1080, "1080i");
        public static XboxTVResolution HDTV_1080p = new XboxTVResolution(1920, 1080, "1080p");
        public static XboxTVResolution HDTV_1440p = new XboxTVResolution(2560, 1440, "1440p");
        public static XboxTVResolution UHDTV_4K = new XboxTVResolution(3840, 2160, "2160p");

        public static Dictionary<int, XboxTVResolution> SupportList =
            new Dictionary<int, XboxTVResolution>()
        {
            {EDTV_480P.H, EDTV_480P},
            {HDTV_720p.H, HDTV_720p},
            {HDTV_1080p.H, HDTV_1080p},
            {HDTV_1440p.H, HDTV_1440p},
            {UHDTV_4K.H, UHDTV_4K}
        };

        static int Per(int h, int v) => h << 16 | v;

        public int W { get; private set; }
        public int H { get; private set; }
        public int P { get; private set; }
        public string Scan { get; private set; }
        public string AspectRatio { get { return $"{P >> 16}:{P & 0xFF}"; } }

        XboxTVResolution(int h, int v, string s)
        {
            W = h;// horizon
            H = v;// vertical
            Scan = s;

            if (0 != v) while (0 != (h %= v) && 0 != (v %= h)) ;
            P = Per(v += h, v);
        }
    }
}
