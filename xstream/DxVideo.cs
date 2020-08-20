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
                pp.Windowed = true;// 指定窗口模式。True = 窗口模式；False = 全屏模式
                /*
                 * 指定系统如何将后台缓冲区的内容复制到前台缓冲区，从而在屏幕上显示。它的值有：
                 * D3DSWAPEFFECT_DISCARD: 清除后台缓存的内容。
                 * D3DSWAPEEFECT_FLIP: 保留后台缓存的内容，当缓存区>1时。
                 * D3DSWAPEFFECT_COPY: 保留后台缓存的内容，缓冲区=1时。
                 * 一般情况下使用D3DSWAPEFFECT_DISCARD
                 */
                pp.SwapEffect = SwapEffect.Discard;

                /*
                 * @param adapter       表示显示适配器的序号。D3DADAPTER_DEFAULT(0)始终是主要的显示适配器。
                 * @param renderWindow  窗体或任何其他Control派生类的句柄。此参数指示要绑定到设备的表面。
                 *                      指定的窗口必须是顶级窗口。不支持空值。
                 * @param deviceType    定义设备类型。
                 *                      D3DDEVTYPE_HAL 硬件栅格化。可以使用软件，硬件或混合的变换和照明进行着色。
                 *                      详见：https://docs.microsoft.com/en-us/windows/win32/direct3d9/d3ddevtype
                 * @param behaviorFlags 控制设备创建行为的一个或多个标志的组合。
                 *                      D3DCREATE_HARDWARE_VERTEXPROCESSING 指定硬件顶点处理。
                 *                      D3DCREATE_SOFTWARE_VERTEXPROCESSING 指定软件顶点处理。
                 *                      对于Windows 10版本1607及更高版本，不建议使用此设置。
                 *                      使用D3DCREATE_HARDWARE_VERTEXPROCESSING。
                 *                      [!Note] 除非没有可用的硬件顶点处理，
                 *                      否则在Windows 10版本1607（及更高版本）中不建议使用软件顶点处理，
                 *                      因为在提高实现安全性的同时，软件顶点处理的效率已大大降低。
                 *                      详见：https://docs.microsoft.com/en-us/windows/win32/direct3d9/d3dcreate
                 *
                 * @see https://docs.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3d9-createdevice
                 */
                d3dDevice = new Device(0, DeviceType.Hardware, f, CreateFlags.HardwareVertexProcessing, pp);
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString(), e.ErrorString, MessageBoxButtons.OK);
                f.Close();
            }

            _fontSourceRegular = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Regular.ttf";
            _fontSourceBold = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Bold.ttf";
        }
    }
}
