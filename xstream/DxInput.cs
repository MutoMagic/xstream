using SharpDX.DirectInput;
using SmartGlass.Nano;
using SmartGlass.Nano.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Xstream
{
    public unsafe class DxInput
    {
        public bool Initialized { get; private set; }
        public string ControllerMappingFilepath { get; private set; }

        public uint Timestamp { get; private set; }
        public InputButtons Buttons { get; private set; }
        public InputAnalogue Analog { get; private set; }
        public InputExtension Extension { get; private set; }

        IntPtr _hwnd;// 顶级窗口句柄

        public static Dictionary<string, NanoGamepadButton> ButtonMap =
                new Dictionary<string, NanoGamepadButton>()
                {
                    {"a", NanoGamepadButton.A},
                    {"b", NanoGamepadButton.B},
                    {"x", NanoGamepadButton.X},
                    {"y", NanoGamepadButton.Y},
                    {"dpleft", NanoGamepadButton.DPadLeft},
                    {"dpright", NanoGamepadButton.DPadRight},
                    {"dpup", NanoGamepadButton.DPadUp},
                    {"dpdown", NanoGamepadButton.DPadDown},
                    {"start", NanoGamepadButton.Start},
                    {"back", NanoGamepadButton.Back},
                    {"leftshoulder", NanoGamepadButton.LeftShoulder},
                    {"rightshoulder", NanoGamepadButton.RightShoulder},
                    {"leftstick", NanoGamepadButton.LeftThumbstick},
                    {"rightstick", NanoGamepadButton.RightThumbstick},
                    {"guide", NanoGamepadButton.Guide}
                };

        public static Dictionary<string, NanoGamepadAxis> AxisMap =
            new Dictionary<string, NanoGamepadAxis>()
            {
                {"leftx", NanoGamepadAxis.LeftX},
                {"lefty", NanoGamepadAxis.LeftY},
                {"rightx", NanoGamepadAxis.RightX},
                {"righty", NanoGamepadAxis.RightY},
                {"lefttrigger", NanoGamepadAxis.TriggerLeft},
                {"righttrigger", NanoGamepadAxis.TriggerRight}
            };

        DirectInput _directInput;
        SortedList _joystickGuidList = new SortedList();// Dictionary<Guid, string[]>
        Joystick _controller = null;

        Dictionary<string, string> _controllerMapping = new Dictionary<string, string>();

        public DxInput(string controllerMappingFilepath)
        {
            ControllerMappingFilepath = controllerMappingFilepath;

            Timestamp = 0;
            Buttons = new InputButtons();
            Analog = new InputAnalogue();
            Extension = new InputExtension();

            // Set "controller byte"
            Extension.Unknown1 = 1;
        }

        public bool Initialize(Form f)
        {
            _hwnd = f.Handle;

            Dictionary<string, string[]> controllerMappings = new Dictionary<string, string[]>();

            if (ControllerMappingFilepath != null)
            {
                string[] lines = File.ReadAllLines(ControllerMappingFilepath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith('#') || lines[i].StartsWith(';'))
                        continue;// 仅处理整行注释，行内注释一律当作实际值处理。

                    string[] columns = lines[i].Split(',');
                    if (columns.Length < 2)
                        continue;// 前两个必须是ProductGuid及ProductName，顺序都不能错。

                    // 别忘了中括号，因为ProductGuid会重。
                    controllerMappings.Add($"{columns[0]}[{columns[1]}]", columns);
                }
            }

            // Initialize DirectInput
            _directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance
                in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                _joystickGuidList.Add(joystickGuid, null);
            }

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance
                    in _directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    _joystickGuidList.Add(joystickGuid, null);
                }

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Debug.WriteLine("No joystick/Gamepad found.");
                return false;
            }

            Initialized = true;

            int numJoysticks = _joystickGuidList.Count;
            Debug.WriteLine("Found {0} joysticks", numJoysticks);
            for (int i = 0; i < numJoysticks; i++)
            {
                joystickGuid = (Guid)_joystickGuidList.GetKey(i);
                var joystick = new Joystick(_directInput, joystickGuid);
                string key = $"{joystick.Information.ProductGuid:N}[{joystick.Information.ProductName}]";
                joystick.Dispose();

                if (controllerMappings.ContainsKey(key))
                {
                    _joystickGuidList.SetByIndex(i, controllerMappings[key]);

                    Debug.WriteLine("Found a gamecontroller (Index: {0})", i);
                    OpenController(i);
                }
            }

            return true;
        }

        public int OpenController(int joystickIndex)
        {
            if (!Initialized)
            {
                Debug.WriteLine("DirectInput not initialized yet...");
                return -1;
            }

            string[] controllerMapping = (string[])_joystickGuidList.GetByIndex(joystickIndex);

            if (controllerMapping == null)
            {
                Debug.WriteLine("Joystick device does not support controllermode");
                return -1;
            }

            if (_controller != null)
            {
                Debug.WriteLine("There is an active controller already.");
                Debug.WriteLine("Closing the old one...");
                CloseController();
            }

            var joystickGuid = (Guid)_joystickGuidList.GetKey(joystickIndex);

            // Instantiate the joystick
            var joystick = new Joystick(_directInput, joystickGuid);

            Debug.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            /*
             * BUG?：XBOX Controller 只能 Foreground，即便将其设为 Background 也是无效的。
             *       详见“SharpDX.DirectInput监听不到手柄按键”https://bbs.csdn.net/topics/392460595
             *       为了便于多开，这里统一设定为Foreground。
             * 
             * Exclusive    该应用程序需要互斥访问。
             *              如果授予独占访问权限，则在获取设备时，设备的其他任何实例都无法获得对该设备的独占访问权限。
             *              但是，即使另一个应用程序获得了独占访问，也始终允许对设备进行非独占访问。
             *              在独占模式下获取鼠标或键盘设备的应用程序在收到WM_ENTERSIZEMOVE和WM_ENTERMENULOOP消息时应始终取消获取设备。
             *              否则，用户将无法操作菜单或移动窗口并调整窗口大小。
             * NonExclusive 应用程序需要非独占访问。对设备的访问不会干扰正在访问同一设备的其他应用程序。
             * Foreground   该应用程序需要前台访问。
             *              如果授予了前台访问权限，则当关联的窗口移至后台时，将自动取消获取设备。
             * Background   该应用程序需要后台访问。
             *              如果授予了后台访问权限，则即使关联的窗口不是活动窗口，也可以随时获取设备。
             * NoWinKey     禁用Windows徽标键。设置此标志可确保用户不会意外退出应用程序。
             *              但是请注意，当显示默认操作映射用户界面（UI）时，DISCL_NOWINKEY无效，并且只要存在该UI，Windows徽标键就可以正常运行。
             */
            joystick.SetCooperativeLevel(_hwnd, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);

            // Query all suported ForceFeedback effects
            //var allEffects = joystick.GetEffects();
            //foreach (var effectInfo in allEffects)
            //    Debug.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            _controller = joystick;
            Debug.WriteLine("Opened Controller {0} {1}", joystickIndex, _controller.Information.ProductName);

            for (int i = 2; i < controllerMapping.Length; i++)
            {
                string[] mapping = controllerMapping[i].Split(':');
                if (mapping.Length == 2)
                    _controllerMapping.Add(mapping[0], mapping[1]);
            }

            return 0;
        }

        public void GetData()
        {
            // Poll events from joystick
            _controller.Poll();
            var datas = _controller.GetBufferedData();
            foreach (var state in datas)
                Debug.WriteLine(state);
        }

        public void CloseController()
        {
            if (_controller == null)
            {
                Debug.WriteLine("Controller is not initialized, cannot remove");
                return;
            }
            Debug.WriteLine("Removing Controller...");
            if (!_controller.IsDisposed)
                _controller.Dispose();
            _controller = null;// .NET GC
        }

        private void HandleControllerButtonChange(NanoGamepadButton button, bool pressed)
        {
            Buttons.ToggleButton(button, pressed);
        }

        private void HandleControllerAxisChange(NanoGamepadAxis axis, float axisValue)
        {
            Analog.SetValue(axis, axisValue);
        }

        internal void HandleInput(object sender, InputEventArgs e)
        {
            Timestamp = e.Timestamp;

            switch (e.EventType)
            {
                case InputEventType.ControllerAdded:
                    OpenController(e.ControllerIndex);
                    break;
                case InputEventType.ControllerRemoved:
                    CloseController();
                    break;
                case InputEventType.ButtonPressed:
                case InputEventType.ButtonReleased:
                    HandleControllerButtonChange(e.Button, e.EventType == InputEventType.ButtonPressed);
                    break;
                case InputEventType.AxisMoved:
                    HandleControllerAxisChange(e.Axis, e.AxisValue);
                    break;
                default:
                    throw new NotSupportedException($"Invalid InputEventType: {e.EventType}");
            }
        }
    }
}
