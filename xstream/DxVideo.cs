using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Windows.Forms;

namespace Xstream
{
    unsafe class DxVideo
    {
        const int D3DADAPTER_DEFAULT = 0;// Used to specify the primary display adapter.
        const int D3DTEXF_FORCE_DWORD = 0x7fffffff;

        Xstream _window;// The window associated with the renderer
        IntPtr _hwnd;

        Rectangle _rectOrigin;
        string _fontSourceRegular;
        string _fontSourceBold;

        Direct3D _d3d;
        Device _d3dDevice;
        bool _beginScene;
        TextureFilter _scaleMode;
        PresentParameters _pparams;
        Surface _defaultRenderTarget;
        Surface _currentRenderTarget;
        string _magic;// renderer_magic
        FPoint _scale;
        bool _hidden;

        RendererInfo _info;// The current renderer info

        public DxVideo(int width, int height)
        {
            _rectOrigin = new Rectangle(0, 0, width, height);
            _fontSourceRegular = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Regular.ttf";
            _fontSourceBold = $"{AppDomain.CurrentDomain.BaseDirectory}Fonts/Xolonium-Bold.ttf";
        }

        public void Initialize(Xstream f)
        {
            _window = f;
            _hwnd = f.GetHandle();

            Initialize(SDL_RendererFlags.SDL_RENDERER_ACCELERATED
                | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
        }

        private void Initialize(SDL_RendererFlags flags)
        {
            _info = new RendererInfo();
            _info.Flags = SDL_RendererFlags.SDL_RENDERER_ACCELERATED;

            _d3d = new Direct3D();

            _pparams = new PresentParameters();
            _pparams.DeviceWindowHandle = _hwnd;
            _pparams.BackBufferWidth = _window.ClientSize.Width;
            _pparams.BackBufferHeight = _window.ClientSize.Height;
            if (Config.Fullscreen)
            {
            }
            else
            {
                _pparams.BackBufferFormat = Format.Unknown;
            }
            _pparams.BackBufferCount = 1;// 后备缓冲区的数量。通常设为“1”，即只有一个后备表面。
            /*
             * 指定系统如何将后台缓冲区的内容复制到前台缓冲区，从而在屏幕上显示。它的值有：
             * D3DSWAPEFFECT_DISCARD: 清除后台缓存的内容。
             * D3DSWAPEEFECT_FLIP: 保留后台缓存的内容，当缓存区>1时。
             * D3DSWAPEFFECT_COPY: 保留后台缓存的内容，缓冲区=1时。
             * 一般情况下使用D3DSWAPEFFECT_DISCARD
             */
            _pparams.SwapEffect = SwapEffect.Discard;

            if (Config.Fullscreen)
            {
                if (Config.Borderless)
                {
                    _pparams.Windowed = true;
                    _pparams.FullScreenRefreshRateInHz = 0;
                }
                else
                {
                    _pparams.Windowed = false;
                    _pparams.FullScreenRefreshRateInHz = 0;
                }
            }
            else
            {
                _pparams.Windowed = true;// 指定窗口模式。True = 窗口模式；False = 全屏模式
                /*
                 * 显示适配器刷新屏幕的速率。该值取决于应用程序运行的模式：
                 * 对于窗口模式，刷新率必须为0。
                 * 对于全屏模式，刷新率是EnumAdapterModes返回的刷新率之一。
                 */
                _pparams.FullScreenRefreshRateInHz = 0;
            }
            if ((flags & SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC) != 0)
            {
                /*
                 * 交换链的后缓冲区可以提供给前缓冲区的最大速率。可以用以下方式：
                 * D3DPRESENT_INTERVAL_DEFAULT: 这几乎等同于D3DPRESENT_INTERVAL_ONE。
                 * D3DPRESENT_INTERVAL_ONE: 垂直同步。当前的操作不会比刷新屏幕更频繁地受到影响。
                 * D3DPRESENT_INTERVAL_IMMEDIATE: 以实时的方式来显示渲染画面。
                 */
                _pparams.PresentationInterval = PresentInterval.One;
            }
            else
            {
                _pparams.PresentationInterval = PresentInterval.Immediate;
            }

            CreateFlags device_flags = CreateFlags.FpuPreserve;// 将Direct3D浮点计算的精度设置为调用线程使用的精度
            //device_flags |= CreateFlags.Multithreaded;// 瓶颈主要在IO上面，且SDL中尚未设置，此处预留以备不时之需
            Capabilities caps = _d3d.GetDeviceCaps(D3DADAPTER_DEFAULT, DeviceType.Hardware);
            if ((caps.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
            {
                device_flags |= CreateFlags.HardwareVertexProcessing;
            }
            else
            {
                device_flags |= CreateFlags.SoftwareVertexProcessing;
            }

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
            _d3dDevice = new Device(_d3d
                , D3DADAPTER_DEFAULT
                , DeviceType.Hardware
                , _hwnd
                , device_flags
                , _pparams);
            _beginScene = true;
            _scaleMode = (TextureFilter)D3DTEXF_FORCE_DWORD;

            // Get presentation parameters to fill info
            SwapChain chain = _d3dDevice.GetSwapChain(0);
            _pparams = chain.PresentParameters;
            chain.Dispose();// FIXME: IDirect3DSwapChain9::Release?
            if (_pparams.PresentationInterval == PresentInterval.One)
            {
                _info.Flags |= SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
            }

            caps = _d3dDevice.Capabilities;
            _info.MaxTextureWidth = caps.MaxTextureWidth;
            _info.MaxTextureHeight = caps.MaxTextureHeight;
            if (caps.SimultaneousRTCount >= 2)
            {
                _info.Flags |= SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE;
            }

            // Set up parameters for rendering
            _d3dDevice.VertexShader = null;
            // IDirect3DDevice9::SetFVF(D3DFVF_XYZ | D3DFVF_DIFFUSE | D3DFVF_TEX1)
            _d3dDevice.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;
            // IDirect3DDevice9::SetRenderState(D3DRS_ZENABLE, D3DZB_FALSE)
            _d3dDevice.SetRenderState(RenderState.ZEnable, ZBufferType.DontUseZBuffer);
            _d3dDevice.SetRenderState(RenderState.CullMode, Cull.None);
            _d3dDevice.SetRenderState(RenderState.Lighting, false);
            // Enable color modulation by diffuse color
            _d3dDevice.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
            _d3dDevice.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
            _d3dDevice.SetTextureStageState(0, TextureStage.ColorArg2, TextureArgument.Diffuse);
            // Enable alpha modulation by diffuse alpha
            _d3dDevice.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
            _d3dDevice.SetTextureStageState(0, TextureStage.AlphaArg1, TextureArgument.Texture);
            _d3dDevice.SetTextureStageState(0, TextureStage.AlphaArg2, TextureArgument.Diffuse);
            // Disable second texture stage, since we're done
            _d3dDevice.SetTextureStageState(1, TextureStage.ColorOperation, TextureOperation.Disable);
            _d3dDevice.SetTextureStageState(1, TextureStage.AlphaOperation, TextureOperation.Disable);

            // Store the default render target
            _defaultRenderTarget = _d3dDevice.GetRenderTarget(0);
            _currentRenderTarget = null;

            // Set an identity world and view matrix
            RawMatrix matrix;
            matrix.M11 = 1.0f;
            matrix.M12 = 0.0f;
            matrix.M13 = 0.0f;
            matrix.M14 = 0.0f;
            matrix.M21 = 0.0f;
            matrix.M22 = 1.0f;
            matrix.M23 = 0.0f;
            matrix.M24 = 0.0f;
            matrix.M31 = 0.0f;
            matrix.M32 = 0.0f;
            matrix.M33 = 1.0f;
            matrix.M34 = 0.0f;
            matrix.M41 = 0.0f;
            matrix.M42 = 0.0f;
            matrix.M43 = 0.0f;
            matrix.M44 = 1.0f;
            _d3dDevice.SetTransform(TransformState.World, matrix);
            _d3dDevice.SetTransform(TransformState.View, matrix);

            _scale.x = 1.0f;
            _scale.y = 1.0f;

            if (!_window.Visible || _window.WindowState == FormWindowState.Minimized)
            {
                _hidden = true;
            }
            else
            {
                _hidden = false;
            }


        }

        public void Close()
        {
            
        }

        struct FPoint
        {
            public float x;
            public float y;
        }
    }

    class RendererInfo
    {
        public SDL_RendererFlags Flags;
        public uint NumTextureFormats;// The number of available texture formats
        public Format[] TextureFormats;// The available texture formats
        public int MaxTextureWidth;// The maximimum texture width
        public int MaxTextureHeight;// The maximimum texture height

        public RendererInfo()
        {
            Flags = SDL_RendererFlags.SDL_RENDERER_ACCELERATED
                | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
                | SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE;
            NumTextureFormats = 1;

            TextureFormats = new Format[16];
            TextureFormats[0] = Format.A8R8G8B8;

            MaxTextureWidth = 0;
            MaxTextureHeight = 0;
        }
    }

    enum SDL_RendererFlags : uint
    {
        SDL_RENDERER_SOFTWARE = 0x00000001,// The renderer is a software fallback
        SDL_RENDERER_ACCELERATED = 0x00000002,// The renderer uses hardware acceleration
        SDL_RENDERER_PRESENTVSYNC = 0x00000004,// Present is synchronized with the refresh rate
        SDL_RENDERER_TARGETTEXTURE = 0x00000008// The renderer supports rendering to texture
    }
}
