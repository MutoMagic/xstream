using SharpDX.DirectInput;
using SmartGlass.Nano;
using SmartGlass.Nano.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        IntPtr _hwnd;

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
        List<Guid> _joystickGuidList = new List<Guid>();
        Joystick _controller = null;

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

            // Initialize DirectInput
            _directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance
                in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                _joystickGuidList.Add(joystickGuid);
            }

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance
                    in _directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                    _joystickGuidList.Add(joystickGuid);
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
                if (IsGameController(i))
                {
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

            if (!IsGameController(joystickIndex))
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

            var joystickGuid = _joystickGuidList[joystickIndex];

            // Instantiate the joystick
            var joystick = new Joystick(_directInput, joystickGuid);
            joystick.SetCooperativeLevel(_hwnd, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);

            Debug.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Debug.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            _controller = joystick;
            Debug.WriteLine("Opened Controller {0} {1}", joystickIndex, _controller.Information.ProductName);

            // Poll events from joystick
            while (true)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                    Debug.WriteLine(state);
            }

            return 0;
        }

        public void CloseController()
        {
            if (_controller == null || _controller.IsDisposed)
            {
                Debug.WriteLine("Controller is not initialized, cannot remove");
                return;
            }
            Debug.WriteLine("Removing Controller...");
            _controller.Dispose();
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

        private bool IsGameController(int joystickIndex)
        {
            return true;
        }
    }
}
