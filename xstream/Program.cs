using SmartGlass;
using SmartGlass.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;
using XboxWebApi.Services;
using XboxWebApi.Services.Api;

namespace xstream
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AllocConsole();

            Console.Write("tokenFilePath: ");
            string tokenFilePath = Console.ReadLine();

            AuthenticationService auth = null;

            if (!File.Exists(tokenFilePath))
            {
                Shell.WriteLine("Warning: '{0}' file not found.\n", tokenFilePath);

                string reqURL = AuthenticationService.GetWindowsLiveAuthenticationUrl();

                Console.WriteLine("1) Open following URL in your WebBrowser:\n\n{0}\n\n" +
                                        "2) Authenticate with your Microsoft Account\n" +
                                        "3) Paste returned URL from addressbar: \n", reqURL);

                // Call requestUrl via WebWidget or manually and authenticate

                string url = Console.ReadLine();
                WindowsLiveResponse rep = AuthenticationService.ParseWindowsLiveResponse(url);
                auth = new AuthenticationService(rep);

                if (!auth.Authenticate())
                {
                    Shell.WriteLine("Error: Authentication failed!");
                    Shell.PressAnyKeyToContinue();
                    return;
                }

                Console.WriteLine(auth.XToken);
                Console.WriteLine(auth.UserInformation);

                // Save token to JSON

                FileStream tokenOutputFile = null;
                try
                {
                    tokenOutputFile = new FileStream(tokenFilePath, FileMode.Create);
                }
                catch (Exception e)
                {
                    Shell.WriteLine("Error: Failed to open token outputfile \'{0}\', error: {1}",
                        tokenOutputFile, e.Message);
                    Shell.PressAnyKeyToContinue();
                    return;
                }
                auth.DumpToFile(tokenOutputFile);
                tokenOutputFile.Close();

                Console.WriteLine("Storing tokens to file \'{0}\' on successful auth",
                        tokenOutputFile.Name);
            }
            else
            {
                // Load token from JSON

                FileStream fs = new FileStream(tokenFilePath, FileMode.Open);
                auth = AuthenticationService.LoadFromFile(fs);
                if (!auth.Authenticate())
                {
                    Shell.WriteLine("Error: 令牌已过期，需要重新认证！");
                    Shell.PressAnyKeyToContinue();
                    return;
                }
                fs.Close();
            }

            Discover().Wait();

            Console.Write("Input IP Address or hostname: ");
            string addressOrHostname = Console.ReadLine();
            Console.WriteLine($"Connecting to {addressOrHostname}...");
            SmartGlassClient client = null;
            try
            {
                Task<SmartGlassClient> connect = SmartGlassClient.ConnectAsync(
                        addressOrHostname, auth.XToken.UserInformation.Userhash, auth.XToken.Jwt);
                try
                {
                    connect.Wait();
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
                // 如果Task失败了GetResult()会直接抛出异常，而Task.Result会抛出AggregateException
                client = connect.GetAwaiter().GetResult();
            }
            catch (SmartGlassException e)
            {
                Shell.WriteLine($"Error: Failed to connect: {e.Message}");
            }
            catch (TimeoutException)
            {
                Shell.WriteLine("Error: Timeout while connecting");
            }

            Shell.PressAnyKeyToContinue();
            FreeConsole();

            if (client != null)
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Xstream(auth, client));
            }
        }

        async static Task<int> Discover()
        {
            Console.WriteLine("{0,-15} {1,-36} {2,-15} {3,-16}", "Name", "HardwareId", "Address", "LiveId");

            IEnumerable<SmartGlass.Device> devices = await SmartGlass.Device.DiscoverAsync();
            foreach (SmartGlass.Device device in devices)
            {
                Console.WriteLine("{0,-15} {1,-36} {2,-15} {3,-16}",
                    device.Name, device.HardwareId, device.Address, device.LiveId);
            }

            return 0;
        }

        [DllImport("kernel32")]
        static extern bool AllocConsole();
        [DllImport("kernel32")]
        static extern bool FreeConsole();
    }
}
