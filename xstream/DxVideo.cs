using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Windows.Forms;

namespace Xstream
{
    class DxVideo
    {
        Device d3dDevice;

        string _fontSourceRegular;
        string _fontSourceBold;

        public DxVideo(int width, int height, Form f)
        {
            try
            {
                PresentParameters pp = new PresentParameters();
                pp.Windowed = true;
                pp.SwapEffect = SwapEffect.Discard;

                d3dDevice = new Device(0, DeviceType.Hardware, f, CreateFlags.SoftwareVertexProcessing, pp);
            }
            catch(DirectXException e)
            {
                MessageBox.Show(e.ToString(), e.ErrorString, MessageBoxButtons.OK);
                f.Close();
            }

            _fontSourceRegular = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Regular.ttf";
            _fontSourceBold = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Bold.ttf";
        }
    }
}
