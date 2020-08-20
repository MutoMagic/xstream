using SmartGlass.Nano;
using System;

namespace Xstream
{
    enum InputEventType
    {
        ControllerAdded,
        ControllerRemoved,
        ButtonPressed,
        ButtonReleased,
        AxisMoved
    }

    class InputEventArgs : EventArgs
    {
        public InputEventType EventType { get; internal set; }
        public int ControllerIndex { get; internal set; }
        public uint Timestamp { get; internal set; }
        public NanoGamepadButton Button { get; internal set; }
        public NanoGamepadAxis Axis { get; internal set; }
        public float AxisValue { get; internal set; }
    }
}
