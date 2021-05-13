using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xstream
{
    public static partial class Native
    {
        public static readonly Version WINVER = Environment.OSVersion.Version;
        public static readonly int _WIN32_WINNT = WINVER.Major << 8 | WINVER.Minor;
        public const int _WIN32_WINNT_NT4 = 0x0400;             // Windows NT 4.0
        public const int _WIN32_WINNT_WIN2K = 0x0500;           // Windows 2000
        public const int _WIN32_WINNT_WINXP = 0x0501;           // Windows XP
        public const int _WIN32_WINNT_WS03 = 0x0502;            // Windows Server 2003
        public const int _WIN32_WINNT_WIN6 = 0x0600;            // Windows Vista
        public const int _WIN32_WINNT_VISTA = 0x0600;           // Windows Vista
        public const int _WIN32_WINNT_WS08 = 0x0600;            // Windows Server 2008
        public const int _WIN32_WINNT_LONGHORN = 0x0600;        // Windows Vista
        public const int _WIN32_WINNT_WIN7 = 0x0601;            // Windows 7
        public const int _WIN32_WINNT_WIN8 = 0x0602;            // Windows 8
        public const int _WIN32_WINNT_WINBLUE = 0x0603;         // Windows 8.1
        public const int _WIN32_WINNT_WINTHRESHOLD = 0x0A00;    // Windows 10
        public const int _WIN32_WINNT_WIN10 = 0x0A00;           // Windows 10

        public const long STYLE_BASIC = WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        public const long STYLE_FULLSCREEN = WS_POPUP;
        public const long STYLE_BORDERLESS = WS_POPUP;
        public const long STYLE_NORMAL = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
        public const long STYLE_RESIZABLE = WS_THICKFRAME | WS_MAXIMIZEBOX;
        public const long STYLE_MASK = STYLE_FULLSCREEN | STYLE_BORDERLESS | STYLE_NORMAL | STYLE_RESIZABLE;

        static Dictionary<Type, Delegate> _cachedILShallow = new Dictionary<Type, Delegate>();
        static Dictionary<Type, Delegate> _cachedILDeep = new Dictionary<Type, Delegate>();

        public static uint SizeOf(object structure) => (uint)Marshal.SizeOf(structure);
        public static uint SizeOf(Type t) => (uint)Marshal.SizeOf(t);
        public static uint SizeOf<T>() => (uint)Marshal.SizeOf<T>();
        public static uint SizeOf<T>(T structure) => (uint)Marshal.SizeOf(structure);

        #region CBool

        public static bool CBool(sbyte val) => val != 0;
        public static bool CBool(byte val) => val != 0;
        public static bool CBool(short val) => val != 0;
        public static bool CBool(ushort val) => val != 0;
        public static bool CBool(int val) => val != 0;
        public static bool CBool(uint val) => val != 0U;
        public static bool CBool(long val) => val != 0L;
        public static bool CBool(ulong val) => val != 0UL;
        public static bool CBool(double val) => val != 0D;
        public static bool CBool(float val) => val != 0F;
        public static bool CBool(char val) => val != 0;
        public static bool CBool(IntPtr val) => val != IntPtr.Zero;
        public static unsafe bool CBool(void* val) => val != (void*)0;
        public static unsafe bool CBool(Enum val)
        {
            fixed (void* pVal = &Unsafe.As<RawData>(val).data)
            {
                return val.GetTypeCode() switch
                {
                    TypeCode.SByte => CBool(*(sbyte*)pVal),
                    TypeCode.Byte => CBool(*(byte*)pVal),
                    TypeCode.Int16 => CBool(*(short*)pVal),
                    TypeCode.UInt16 => CBool(*(ushort*)pVal),
                    TypeCode.Int32 => CBool(*(int*)pVal),
                    TypeCode.UInt32 => CBool(*(uint*)pVal),
                    TypeCode.Int64 => CBool(*(long*)pVal),
                    TypeCode.UInt64 => CBool(*(ulong*)pVal),
                    _ => throw new InvalidOperationException($"Invalid primitive type {val.GetTypeCode()}")
                };
            }
        }

        //public static T CBool<T>(bool val) where T : IConvertible => (T)Convert.ChangeType(val, typeof(T));
        public static T CBool<T>(bool val) => Unsafe.As<bool, T>(ref val);
        public static byte CBool(bool val) => Unsafe.As<bool, byte>(ref val);

        #endregion

        public static ref T GetValue<T>(object val) => ref Unsafe.As<byte, T>(ref Unsafe.As<RawData>(val).data);
        public static T GetValue<T>(Enum val) => Unsafe.As<byte, T>(ref Unsafe.As<RawData>(val).data);
        public static uint GetValue(Enum val) => Unsafe.As<byte, uint>(ref Unsafe.As<RawData>(val).data);

        public static T[] Resize<T>(T[] array, int newSize)
        {
            T[] newArray = new T[newSize];
            if (newSize < array.Length)
            {
                for (int i = newSize; i-- > 0;)
                {
                    newArray[i] = array[i];
                }
            }
            else
            {
                array.CopyTo(newArray, 0);// 浅拷贝
            }
            return newArray;
        }

        // 数组组合
        public static T[][] Combine<T>(T[][] array)
        {
            int arrLength = array.Length;
            int[] lengths = new int[arrLength];
            int[] product = new int[arrLength];

            int itemLength = 1;
            for (int i = 0; i < arrLength; i++)
            {
                Debug.Assert(array[i].Rank == 1);

                lengths[i] = array[i].Length;
                product[i] = itemLength;    // i > 0 ? lengths[i - 1] * product[i - 1] : 1
                itemLength *= lengths[i];   // 锯齿数组可以长度不一
            }

            T[][] items = new T[itemLength][];
            for (int i = 0; i < itemLength; i++)
            {
                items[i] = new T[arrLength];
                for (int j = 0; j < arrLength; j++)
                {
                    // 比较规范的写法 (int)(Math.Floor((double)i / product[j]))
                    items[i][j] = array[j][i / product[j] % lengths[j]];
                }
            }
            return items;
        }

        // 区间集[from,to)
        public static int[] IntervalClass(int from, int to)
        {
            int[] set = new int[to - from];
            int index = 0;

            for (int i = from; i < to; i++)
            {
                set[index++] = i;
            }

            Debug.Assert(++index == set.Length);
            return set;
        }

        #region Clone

        public static Array Clone(Array array, bool deep = false)
        {
            if (array.Rank > 1)
            {
                throw new NotSupportedException("Jagged arrays should be used instead of multidimensional " +
                    "when you use it to analyse your projects.");
            }

            Array newArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length);
            for (int i = array.Length; i-- > 0;)
            {
                object element = array.GetValue(i);
                object newElement = Clone(element, deep);
                newArray.SetValue(newElement, i);
            }
            return newArray;
        }

        public static bool IsSimpleType(Type t)
        {
            /*
             * 以下所有判断均为true！！！蜜汁逻辑，相当致命
             * 
             * enum CBool : int { False, True }
             * typeof(CBool).IsEnum == true
             * typeof(CBool).BaseType == System.Enum
             * typeof(Enum).IsEnum == false
             * 
             * typeof(byte).IsValueType == true
             * typeof(byte).BaseType == System.ValueType
             * typeof(Enum).BaseType == System.ValueType
             * typeof(Enum).IsValueType == false
             * typeof(ValueType).IsValueType == false
             */
            return t.IsValueType // every primitive type has IsValueType set to true,
                                 // so checking for IsPrimitive is not need.
                || t == typeof(string)
                || typeof(ValueType).IsAssignableFrom(t)
                || typeof(Delegate).IsAssignableFrom(t);
        }

        public static bool IsArrayType(Type t)
        {
            /*
             * 数组也相当蜜汁，你甚至可以 Array variable = new int[length];
             * 
             * typeof(int[]).IsArray == true
             * typeof(int[]).BaseType == System.Array
             * typeof(Array).IsArray == false
             */
            return t.IsArray || typeof(Array).IsAssignableFrom(t);
        }

        public static T Clone<T>(T obj, bool deep = false)
        {
            Dictionary<Type, Delegate> cachedIL = deep ? _cachedILDeep : _cachedILShallow;
            if (!cachedIL.TryGetValue(typeof(T), out Delegate exec))
            {
                ConstructorInfo constructor = obj.GetType().GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException("Requires no-argument constructor");
                }

                // Create ILGenerator
                DynamicMethod clone = new DynamicMethod("DoClone", typeof(T), new Type[] { typeof(T) }, true);
                ILGenerator generator = clone.GetILGenerator();
                LocalBuilder newobj = generator.DeclareLocal(typeof(T));

                generator.Emit(OpCodes.Newobj, constructor);
                //generator.Emit(OpCodes.Stloc, newobj);
                generator.Emit(OpCodes.Stloc_0);// 节省空间，至于效率不明
                foreach (FieldInfo field in obj.GetType().GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic))
                {
                    if (deep && !IsSimpleType(field.FieldType))
                    {
                        LocalBuilder value = generator.DeclareLocal(field.FieldType);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldfld, field);
                        generator.Emit(OpCodes.Stloc, value);
                        Clone(generator, field, value, newobj);
                        continue;
                    }

                    // Load the new object on the eval stack...     (currently 1 item on eval stack)
                    generator.Emit(OpCodes.Ldloc_0);
                    // Load initial object (parameter)              (currently 2 items on eval stack)
                    generator.Emit(OpCodes.Ldarg_0);
                    // Replace value by field value                 (still currently 2 items on eval stack)
                    generator.Emit(OpCodes.Ldfld, field);
                    // Store the value of the top on the eval stack into the object
                    // underneath that value on the value stack.    (0 items on eval stack)
                    generator.Emit(OpCodes.Stfld, field);
                }
                // Load new constructed obj on eval stack           --> 1 item on stack
                generator.Emit(OpCodes.Ldloc_0);
                // Return constructed object.                       --> 0 items on stack
                generator.Emit(OpCodes.Ret);

                exec = clone.CreateDelegate(typeof(DoClone<T>));
                cachedIL.Add(typeof(T), exec);
            }
            return ((DoClone<T>)exec)(obj);
        }

        static void Clone(ILGenerator generator, FieldInfo field, LocalBuilder value, LocalBuilder newobj)
        {
            Label pass = generator.DefineLabel();
            MethodInfo clone;
            if (field.FieldType.GetCustomAttribute<StructInterfaceAttribute>() != null)
            {
                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Brfalse, pass);// fieldValue从堆栈中弹出

                generator.Emit(OpCodes.Ldloc, newobj);
                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Unbox, field.FieldType);// 对null拆箱会引发NullReferenceException
                generator.Emit(OpCodes.Stfld, field);

                generator.MarkLabel(pass);// 直接跳过，使用缺省值
            }
            else if (IsArrayType(field.FieldType))
            {
                clone = typeof(Native).GetMethod("Clone", new Type[] { typeof(Array), typeof(bool) });
                Debug.Assert(clone != null);

                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Brfalse, pass);

                generator.Emit(OpCodes.Ldloc, newobj);
                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Ldc_I4_1);// true
                generator.Emit(OpCodes.Call, clone);
                generator.Emit(OpCodes.Stfld, field);

                generator.MarkLabel(pass);
            }
            else if (field.FieldType.GetInterface("IPrototype`1") != null)
            {
                clone = field.FieldType.GetMethod("DeepCopy");
                Debug.Assert(clone != null);

                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Brfalse, pass);

                generator.Emit(OpCodes.Ldloc, newobj);
                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Callvirt, clone);
                generator.Emit(OpCodes.Stfld, field);

                generator.MarkLabel(pass);
            }
            else if (field.FieldType.IsClass && !field.FieldType.IsAbstract)
            {
                Type T = null;
                foreach (MethodInfo method in typeof(Program).GetMethods())
                {
                    Type[] args = method.GetGenericArguments();
                    if ("Clone".Equals(method.Name)
                        && method.IsGenericMethodDefinition
                        && args.Length == 1
                        && "T".Equals(args[0].Name))
                    {
                        T = args[0];
                        break;
                    }
                }
                Debug.Assert(T != null);
                clone = typeof(Program)
                    .GetMethod("Clone", new Type[] { T, typeof(bool) })
                    .MakeGenericMethod(field.FieldType);
                Debug.Assert(clone.IsGenericMethod);

                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Brfalse, pass);

                generator.Emit(OpCodes.Ldloc, newobj);
                generator.Emit(OpCodes.Ldloc, value);
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Call, clone);
                generator.Emit(OpCodes.Stfld, field);

                generator.MarkLabel(pass);
            }
            else
            {
                throw new NotSupportedException("Unable to control object content");
            }
        }

        #endregion

        static unsafe bool SDL_EnclosePoints(Point* points, int count, Rectangle* clip, Rectangle* result)
        {
            int minx = 0;
            int miny = 0;
            int maxx = 0;
            int maxy = 0;
            int x, y, i;

            if (!CBool(points))
            {
                // TODO: error message
                return false;
            }

            if (count < 1)
            {
                // TODO: error message
                return false;
            }

            if (CBool(clip))
            {
                bool added = false;
                int clip_minx = clip->X;
                int clip_miny = clip->Y;
                int clip_maxx = clip->X + clip->Width - 1;
                int clip_maxy = clip->Y + clip->Height - 1;

                // Special case for empty rectangle
                if (clip->IsEmpty)
                {
                    return false;
                }

                for (i = 0; i < count; ++i)
                {
                    x = points[i].X;
                    y = points[i].Y;

                    if (x < clip_minx || x > clip_maxx ||
                        y < clip_miny || y > clip_maxy)
                    {
                        continue;
                    }

                    if (!added)
                    {
                        // Special case: if no result was requested, we are done
                        if (result == null)
                        {
                            return true;
                        }

                        // First point added
                        minx = maxx = x;
                        miny = maxy = y;
                        added = true;
                        continue;
                    }
                    if (x < minx)
                    {
                        minx = x;
                    }
                    else if (x > maxx)
                    {
                        maxx = x;
                    }
                    if (y < miny)
                    {
                        miny = y;
                    }
                    else if (y > maxy)
                    {
                        maxy = y;
                    }
                }
                if (!added)
                {
                    return false;
                }
            }
            else
            {
                // Special case: if no result was requested, we are done
                if (result == null)
                {
                    return true;
                }

                // No clipping, always add the first point
                minx = maxx = points[0].X;
                miny = maxy = points[0].Y;

                for (i = 1; i < count; ++i)
                {
                    x = points[i].X;
                    y = points[i].Y;

                    if (x < minx)
                    {
                        minx = x;
                    }
                    else if (x > maxx)
                    {
                        maxx = x;
                    }
                    if (y < miny)
                    {
                        miny = y;
                    }
                    else if (y > maxy)
                    {
                        maxy = y;
                    }
                }
            }

            if (result != null)
            {
                result->X = minx;
                result->Y = miny;
                result->Width = (maxx - minx) + 1;
                result->Height = (maxy - miny) + 1;
            }
            return true;
        }

        #region windows modes

        static unsafe bool WIN_GetDisplayMode(string deviceName, uint index, SDL_DisplayMode mode)
        {
            WIN_DisplayModeData data;
            DEVMODE devmode = new DEVMODE();
            IntPtr hdc;

            devmode.dmSize = (ushort)Marshal.SizeOf(devmode);
            devmode.dmDriverExtra = 0;
            if (!EnumDisplaySettings(deviceName, index, ref devmode))
            {
                return false;
            }

            data = new WIN_DisplayModeData();
            data.DeviceMode = devmode;
            data.DeviceMode.dmFields =
                DM_BITSPERPEL |
                DM_PELSWIDTH |
                DM_PELSHEIGHT |
                DM_DISPLAYFREQUENCY |
                DM_DISPLAYFLAGS;

            // Fill in the mode information
            mode.format = SDL_PIXELFORMAT_UNKNOWN;
            mode.w = (int)devmode.dmPelsWidth;
            mode.h = (int)devmode.dmPelsHeight;
            mode.refresh_rate = (int)devmode.dmDisplayFrequency;
            mode.driverdata = data;

            if (index == ENUM_CURRENT_SETTINGS
                && (hdc = CreateDC(deviceName, null, null, IntPtr.Zero)) != IntPtr.Zero)
            {
                BITMAPINFO bmi;
                IntPtr hbm;

                bmi = new BITMAPINFO();
                bmi.bmiHeader.Init();

                hbm = CreateCompatibleBitmap(hdc, 1, 1);
                GetDIBits(hdc, hbm, 0, 1, null, ref bmi, DIB_RGB_COLORS);
                GetDIBits(hdc, hbm, 0, 1, null, ref bmi, DIB_RGB_COLORS);
                DeleteObject(hbm);
                DeleteDC(hdc);
                if (bmi.bmiHeader.biCompression == BI_BITFIELDS)
                {
                    fixed (RGBQUAD* bmiColors = bmi.bmiColors)
                    {
                        switch (*(uint*)bmiColors)
                        {
                            case 0x00FF0000:
                                mode.format = SDL_PIXELFORMAT_RGB888;
                                break;
                            case 0x000000FF:
                                mode.format = SDL_PIXELFORMAT_BGR888;
                                break;
                            case 0xF800:
                                mode.format = SDL_PIXELFORMAT_RGB565;
                                break;
                            case 0x7C00:
                                mode.format = SDL_PIXELFORMAT_RGB555;
                                break;
                        }
                    }
                }
                else if (bmi.bmiHeader.biBitCount == 8)
                {
                    mode.format = SDL_PIXELFORMAT_INDEX8;
                }
                else if (bmi.bmiHeader.biBitCount == 4)
                {
                    mode.format = SDL_PIXELFORMAT_INDEX4LSB;
                }
            }
            else
            {
                // FIXME: Can we tell what this will be?
                if ((devmode.dmFields & DM_BITSPERPEL) == DM_BITSPERPEL)
                {
                    switch (devmode.dmBitsPerPel)
                    {
                        case 32:
                            mode.format = SDL_PIXELFORMAT_RGB888;
                            break;
                        case 24:
                            mode.format = SDL_PIXELFORMAT_RGB24;
                            break;
                        case 16:
                            mode.format = SDL_PIXELFORMAT_RGB565;
                            break;
                        case 15:
                            mode.format = SDL_PIXELFORMAT_RGB555;
                            break;
                        case 8:
                            mode.format = SDL_PIXELFORMAT_INDEX8;
                            break;
                        case 4:
                            mode.format = SDL_PIXELFORMAT_INDEX4LSB;
                            break;
                    }
                }
            }
            return true;
        }

        static bool WIN_AddDisplay(string deviceName)
        {
            SDL_VideoDisplay display = new SDL_VideoDisplay();
            WIN_DisplayData displaydata;
            SDL_DisplayMode mod = new SDL_DisplayMode();
            DISPLAY_DEVICE device = new DISPLAY_DEVICE();

            Debug.WriteLine("Display: {0}", deviceName);
            if (!WIN_GetDisplayMode(deviceName, ENUM_CURRENT_SETTINGS, mod))
            {
                return false;
            }

            displaydata = new WIN_DisplayData();
            displaydata.DeviceName = deviceName;

            device.cb = SizeOf(device);
            if (EnumDisplayDevices(deviceName, 0, ref device, 0))
            {
                display.name = device.DeviceString;// 监视器上下文
            }
            display.desktop_mode = mod;
            display.current_mode = mod.DeepCopy();
            display.driverdata = displaydata;
            SDL_AddVideoDisplay(display);
            return true;
        }

        static void WIN_InitModes(SDL_VideoDevice _this)
        {
            int pass;
            uint i, j, count;
            DISPLAY_DEVICE device;

            device = new DISPLAY_DEVICE();
            device.cb = SizeOf(device);

            // Get the primary display in the first pass
            for (pass = 0; pass < 2; ++pass)
            {
                for (i = 0; ; ++i)
                {
                    string deviceName;

                    if (!EnumDisplayDevices(null, i, ref device, 0)) break;
                    if (!CBool(device.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP)) continue;
                    if (pass == 0)
                    {
                        if (!CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                    }
                    else
                    {
                        if (CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                    }
                    deviceName = device.DeviceName;// 适配器名称
                    Debug.WriteLine("Device: {0}", deviceName);
                    count = 0;
                    for (j = 0; ; ++j)
                    {
                        if (!EnumDisplayDevices(deviceName, j, ref device, 0)) break;
                        if (!CBool(device.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP)) continue;
                        if (pass == 0)
                        {
                            if (!CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                        }
                        else
                        {
                            if (CBool(device.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE)) continue;
                        }
                        count += CBool(WIN_AddDisplay(device.DeviceName));// 监视器设备（显示器）
                    }
                    if (count == 0)
                    {
                        WIN_AddDisplay(deviceName);// 适配器设备（显卡）
                    }
                }
            }
            if (_this.num_displays == 0)
            {
                throw new NotSupportedException("No displays available");
            }
        }

        static Rectangle WIN_GetDisplayBounds(SDL_VideoDisplay display)
        {
            ref WIN_DisplayModeData data = ref GetValue<WIN_DisplayModeData>(display.current_mode.driverdata);

            return new Rectangle(
                data.DeviceMode.DUMMYUNIONNAME.dmPosition.X,
                data.DeviceMode.DUMMYUNIONNAME.dmPosition.Y,
                (int)data.DeviceMode.dmPelsWidth,
                (int)data.DeviceMode.dmPelsHeight);
        }

        static void WIN_GetDisplayModes(SDL_VideoDisplay display)
        {
            ref WIN_DisplayData data = ref GetValue<WIN_DisplayData>(display.driverdata);
            uint i;
            SDL_DisplayMode mode = new SDL_DisplayMode();

            for (i = 0; ; ++i)
            {
                if (!WIN_GetDisplayMode(data.DeviceName, i, mode))
                {
                    break;
                }

                if (SDL_IsPixelFormat_Indexed(mode.format))
                {
                    // We don't support palettized modes now
                    continue;
                }

                if (mode.format != SDL_PIXELFORMAT_UNKNOWN)
                {
                    SDL_AddDisplayMode(display, mode);
                }
            }

            //if (display.display_modes == null)
            //{
            //    display.display_modes = new SDL_DisplayMode[display.max_display_modes];
            //}
        }

        static void WIN_SetDisplayMode(SDL_VideoDisplay display, SDL_DisplayMode mode)
        {
            ref WIN_DisplayData displaydata = ref GetValue<WIN_DisplayData>(display.driverdata);
            ref WIN_DisplayModeData data = ref GetValue<WIN_DisplayModeData>(mode.driverdata);
            int status;

            status = ChangeDisplaySettingsEx(displaydata.DeviceName
                , ref data.DeviceMode
                , IntPtr.Zero
                , CDS_FULLSCREEN
                , IntPtr.Zero);
            if (status != DISP_CHANGE_SUCCESSFUL)
            {
                string reason = "Unknown reason";
                switch (status)
                {
                    case DISP_CHANGE_BADFLAGS:
                        reason = "DISP_CHANGE_BADFLAGS";
                        break;
                    case DISP_CHANGE_BADMODE:
                        reason = "DISP_CHANGE_BADMODE";
                        break;
                    case DISP_CHANGE_BADPARAM:
                        reason = "DISP_CHANGE_BADPARAM";
                        break;
                    case DISP_CHANGE_FAILED:
                        reason = "DISP_CHANGE_FAILED";
                        break;
                }
                throw new InvalidOperationException($"ChangeDisplaySettingsEx() failed: {reason}");
            }
            EnumDisplaySettings(displaydata.DeviceName, ENUM_CURRENT_SETTINGS, ref data.DeviceMode);
        }

        #endregion

        static int WIN_CompareMemory(SDL_DisplayModeData m1, SDL_DisplayModeData m2)
        {
            WIN_DisplayModeData d1 = (WIN_DisplayModeData)m1;
            WIN_DisplayModeData d2 = (WIN_DisplayModeData)m2;
            return CompareMemory(d1, d2, Marshal.SizeOf(d1));
        }

        class RawData
        {
            public byte data;
        }
    }

    public delegate T DoClone<T>(T src);

    [AttributeUsage(AttributeTargets.Interface)]
    public class StructInterfaceAttribute : Attribute { }

    public interface IPrototype<T>
    {
        T DeepCopy();       // 深层复制
        T ShallowCopy();    // 浅表复制
    }

    public abstract class Prototype<T> : IPrototype<T> where T : class
    {
        public virtual T DeepCopy() => Native.Clone(Unsafe.As<T>(this), true);
        public virtual T ShallowCopy() => (T)MemberwiseClone();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WIN_DisplayData : SDL_DisplayData
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WIN_DisplayModeData : SDL_DisplayModeData
    {
        public DEVMODE DeviceMode;
    }
}
