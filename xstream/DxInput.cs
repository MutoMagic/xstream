using SharpDX.DirectInput;
using SharpDX.XInput;
using SmartGlass.Nano;
using SmartGlass.Nano.Packets;
using System;
using System.Diagnostics;
using System.Threading;

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

        IntPtr _controller;

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

        public bool Initialize()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance
                in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance
                    in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Debug.WriteLine("No joystick/Gamepad found.");
                return false;
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Debug.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Debug.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            while (true)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                    Debug.WriteLine(state);
            }

            //int ret = SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER);
            //if (ret < 0)
            //{
            //    Debug.WriteLine("SDL_Init GAMECONTROLLER failed: {0}", SDL.SDL_GetError());
            //    return false;
            //}
            //if (ControllerMappingFilepath != null)
            //{
            //    ret = SDL.SDL_GameControllerAddMappingsFromFile(ControllerMappingFilepath);
            //    if (ret < 0)
            //    {
            //        Debug.WriteLine(String.Format("Failed to load GameControllerDB, {0}", ControllerMappingFilepath));
            //        return false;
            //    }
            //}
            Initialized = true;

            //int numJoysticks = SDL.SDL_NumJoysticks();
            //Debug.WriteLine("Found {0} joysticks", numJoysticks);
            //for (int i = 0; i < numJoysticks; i++)
            //{
            //    if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
            //    {
            //        Debug.WriteLine("Found a gamecontroller (Index: {0})", i);
            //        OpenController(i);
            //    }
            //}

            return true;
        }

        public int OpenController(int joystickIndex)
        {
            // Initialize XInput
            var controllers = new[] {
                new Controller(UserIndex.One), 
                new Controller(UserIndex.Two), 
                new Controller(UserIndex.Three), 
                new Controller(UserIndex.Four) };

            // Get 1st controller available
            Controller controller = null;
            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    controller = selectControler;
                    break;
                }
            }

            if (controller == null)
            {
                Console.WriteLine("No XInput controller installed");
                return 0;
            }

            Console.WriteLine("Found a XInput controller available");
            Console.WriteLine("Press buttons on the controller to display events or escape key to exit... ");

            // Poll events from joystick
            var previousState = controller.GetState();

            while (controller.IsConnected)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Escape");
                    return 0;
                }
                var state = controller.GetState();
                if (previousState.PacketNumber != state.PacketNumber)
                    Console.WriteLine(state.Gamepad);
                Thread.Sleep(10);
                previousState = state;
            }

            if (!Initialized)
            {
                Debug.WriteLine("SDL Input not initialized yet...");
                return -1;
            }

            //if (SDL.SDL_IsGameController(joystickIndex) == SDL.SDL_bool.SDL_FALSE)
            //{
            //    Debug.WriteLine("Joystick device does not support controllermode");
            //    return -1;
            //}
            //if (_controller != IntPtr.Zero)
            //{
            //    Debug.WriteLine("There is an active controller already.");
            //    Debug.WriteLine("Closing the old one...");
            //    CloseController();
            //}
            //
            //_controller = SDL.SDL_GameControllerOpen(joystickIndex);
            //if (_controller == IntPtr.Zero)
            //{
            //    Debug.WriteLine("Failed to open controller: {0}", joystickIndex);
            //    return -1;
            //}
            //string name = SDL.SDL_GameControllerNameForIndex(joystickIndex);
            //Debug.WriteLine("Opened Controller {0} {1}", joystickIndex, name);
            return 0;
        }

        public void CloseController()
        {
            //if (_controller == IntPtr.Zero)
            //{
            //    Debug.WriteLine("Controller is not initialized, cannot remove");
            //    return;
            //}
            //Debug.WriteLine("Removing Controller...");
            //SDL.SDL_GameControllerClose(_controller);
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
