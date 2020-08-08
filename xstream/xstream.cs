using SmartGlass;
using SmartGlass.Common;
using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
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
        public Xstream()
        {
            InitializeComponent();
            StartNano();// gamestreaming
        }

        async Task StartNano()
        {
            // Get general gamestream configuration
            GamestreamConfiguration config = GamestreamConfiguration.GetStandardConfig();
            // Modify standard config, if desired

            GamestreamSession session = await Program.client.BroadcastChannel.StartGamestreamAsync(config);
            Console.WriteLine($"Connecting to NANO // TCP: {session.TcpPort}, UDP: {session.UdpPort}");

            NanoClient nano = new NanoClient(Program.addressOrHostname, session);

            // General Handshaking & Opening channels
            Console.WriteLine($"Running protocol init...");
            await nano.InitializeProtocolAsync();

            // Start Controller input channel
            await nano.OpenInputChannelAsync(1280, 720);

            // Start ChatAudio channel
            //AudioFormat chatAudioFormat = new AudioFormat(1, 24000, AudioCodec.Opus);
            //await nano.OpenChatAudioChannelAsync(chatAudioFormat);

            Console.WriteLine("Adding FileConsumer");
            //IConsumer consumer = /* initialize consumer */;
            //nano.AddConsumer(consumer);

            // Start consumer, if necessary
            //consumer.Start();

            // Audio & Video client handshaking
            // Sets desired AV formats
            Console.WriteLine("Initializing AV stream (handshaking)...");
            AudioFormat audioFormat = nano.AudioFormats[0];
            VideoFormat videoFormat = nano.VideoFormats[0];
            await nano.InitializeStreamAsync(audioFormat, videoFormat);

            // Tell console to start sending AV frames
            Console.WriteLine("Starting stream...");
            await nano.StartStreamAsync();

            /* Run a mainloop, to gather controller input events or similar */
            Console.WriteLine("Stream is running");
        }
    }
}
