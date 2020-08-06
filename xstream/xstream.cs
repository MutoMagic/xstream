using SmartGlass;
using SmartGlass.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;

namespace xstream
{
    public partial class Xstream : Form
    {
        AuthenticationService auth;
        SmartGlassClient client;

        public Xstream(AuthenticationService auth, SmartGlassClient client)
        {
            InitializeComponent();

            this.auth = auth;
            this.client = client;
            StartNano();
        }

        async Task<int> StartNano()
        {
            // Get general gamestream configuration
            GamestreamConfiguration config = GamestreamConfiguration.GetStandardConfig();
            // Modify standard config, if desired

            GamestreamSession session = await client.BroadcastChannel.StartGamestreamAsync(config);
            Console.WriteLine($"Connecting to NANO // TCP: {session.TcpPort}, UDP: {session.UdpPort}");

            return 0;
        }
    }
}
