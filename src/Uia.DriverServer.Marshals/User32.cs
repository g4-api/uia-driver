﻿/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-hardwareinput
 * https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput
 * https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
 */
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Uia.DriverServer.Marshals.Models;

namespace Uia.DriverServer.Marshals
{
    public static partial class User32
    {
        // P/Invoke declaration for the GetDeviceCaps function from gdi32.dll.
        // Retrieves device-specific information for the specified device.
        [LibraryImport("gdi32.dll")]
        private static partial int GetDeviceCaps(IntPtr hdc, int nIndex);

        // P/Invoke declaration for the GetMessageExtraInfo function from user32.dll.
        // Retrieves extra message information for the current thread.
        [LibraryImport("user32.dll")]
        private static partial IntPtr GetMessageExtraInfo();

        // P/Invoke declaration for the GetPhysicalCursorPos function from user32.dll.
        // Retrieves the position of the mouse cursor.
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetPhysicalCursorPos(out Point lpPoint);

        // Imports the mouse_event function from user32.dll.
        [LibraryImport("user32.dll")]
        private static partial void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        // P/Invoke declaration for the SendInput function from user32.dll.
        // Sends an array of input events (mouse, keyboard, or hardware) to the system.
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U4)]
        private static partial uint SendInput(
            uint nInputs,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] in Input[] pInputs,
            int cbSize);

        // P/Invoke declaration for the SetPhysicalCursorPos function from user32.dll.
        // Sets the position of the mouse cursor.
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetPhysicalCursorPos(int x, int y);

        /// <summary>
        /// Wrapper method for the GetDeviceCaps function.
        /// Retrieves device-specific information for the specified device.
        /// </summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <param name="index">The item to be retrieved. For a list of device capability indexes, see the documentation for GetDeviceCaps.</param>
        /// <returns>The value of the specified item for the specified device context.</returns>
        public static int GetDeviceCapabilities(nint hdc, int index)
        {
            // Call the P/Invoke GetDeviceCaps function with the provided device context handle and index.
            return GetDeviceCaps(hdc: new IntPtr(hdc), index);
        }

        /// <summary>
        /// Wrapper method for the GetMessageExtraInfo function.
        /// Retrieves extra message information for the current thread.
        /// </summary>
        /// <returns>An IntPtr that contains the extra message information.</returns>
        public static IntPtr GetMessageExtraInformation()
        {
            // Call the P/Invoke GetMessageExtraInfo function and return its result.
            return GetMessageExtraInfo();
        }

        /// <summary>
        /// Retrieves the position of the mouse cursor.
        /// </summary>
        /// <returns>A <see cref="Point"/> structure that contains the screen coordinates of the cursor.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the cursor position could not be retrieved.</exception>
        public static Point GetPhysicalCursorPosition()
        {
            // Call the P/Invoke GetPhysicalCursorPos function and check if it succeeded.
            if (GetPhysicalCursorPos(out Point point))
            {
                // Return the cursor position if the function succeeded.
                return point;
            }
            else
            {
                // Throw an exception if the function failed to retrieve the cursor position.
                throw new InvalidOperationException("Failed to get cursor position.");
            }
        }

        /// <summary>
        /// Wrapper method for the SendInput function.
        /// Sends an array of input events (mouse, keyboard, or hardware) to the system.
        /// </summary>
        /// <param name="inputs">An array of Input structures that represent the input events to be inserted into the input stream.</param>
        /// <returns>The number of events that were successfully inserted into the input stream.</returns>
        public static uint SendInput(in Input[] inputs)
        {
            // Call the P/Invoke SendInput function with the number of inputs, the array of inputs, and the size of an Input structure.
            return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        }

        /// <summary>
        /// Wrapper method for the mouse_event function.
        /// Sends a mouse event with the specified event code at the specified coordinates.
        /// </summary>
        /// <param name="eventCode">The event code for the mouse event (e.g., mouse down, mouse up).</param>
        /// <param name="xpos">The x-coordinate for the mouse event.</param>
        /// <param name="ypos">The y-coordinate for the mouse event.</param>
        public static void SendMouseEvent(int eventCode, int xpos, int ypos)
        {
            // Define the valid mouse event codes.
            var mouseEvents = new int[]
            {
                0x0001, // MOUSEEVENTF_MOVE
                0x0002, // MOUSEEVENTF_LEFTDOWN
                0x0004, // MOUSEEVENTF_LEFTUP
                0x0008, // MOUSEEVENTF_RIGHTDOWN
                0x0010, // MOUSEEVENTF_RIGHTUP
                0x0020, // MOUSEEVENTF_MIDDLEDOWN
                0x0040, // MOUSEEVENTF_MIDDLEUP
                0x0080, // MOUSEEVENTF_XDOWN
                0x0100, // MOUSEEVENTF_XUP
                0x0800, // MOUSEEVENTF_WHEEL
                0x8000  // MOUSEEVENTF_ABSOLUTE
            };

            // Check if the provided event code is valid.
            if (!Array.Exists(mouseEvents, element => element == eventCode))
            {
                // Throw an exception if the event code is invalid.
                throw new ArgumentException("Invalid mouse event code.", nameof(eventCode));
            }

            // Call the mouse_event function with the provided parameters.
            mouse_event(eventCode, xpos, ypos, cButtons: 0, dwExtraInfo: 0);
        }

        /// <summary>
        /// Wrapper method for the SetPhysicalCursorPos function.
        /// Sets the position of the mouse cursor.
        /// </summary>
        /// <param name="x">The new x-coordinate of the cursor.</param>
        /// <param name="y">The new y-coordinate of the cursor.</param>
        /// <exception cref="InvalidOperationException">Thrown when the cursor position could not be set.</exception>
        public static void SetPhysicalCursorPosition(int x, int y)
        {
            // Call the P/Invoke SetPhysicalCursorPos function and check if it succeeded.
            if (!SetPhysicalCursorPos(x, y))
            {
                // Throw an exception if the function failed to set the cursor position.
                throw new InvalidOperationException("Failed to set cursor position.");
            }
        }

        /// <summary>
        /// Wrapper method for the SetPhysicalCursorPos function.
        /// Sets the position of the mouse cursor.
        /// </summary>
        /// <param name="point">A <see cref="Point"/> structure that contains the new screen coordinates of the cursor.</param>
        /// <exception cref="InvalidOperationException">Thrown when the cursor position could not be set.</exception>
        public static void SetPhysicalCursorPosition(Point point)
        {
            // Call the P/Invoke SetPhysicalCursorPos function with the coordinates from the Point structure and check if it succeeded.
            if (!SetPhysicalCursorPos(point.x, point.y))
            {
                // Throw an exception if the function failed to set the cursor position.
                throw new InvalidOperationException("Failed to set cursor position.");
            }
        }
    }
}

namespace Uia.DriverServer.Marshals.Models
{
    /// <summary>
    /// Represents hardware input data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        /// <summary>
        /// The message generated by the hardware.
        /// </summary>
        public uint uMsg;

        /// <summary>
        /// The high-order word of the additional message-specific information.
        /// </summary>
        public ushort wParamH;

        /// <summary>
        /// The low-order word of the additional message-specific information.
        /// </summary>
        public ushort wParamL;
    }

    /// <summary>
    /// Represents an input event, which can be a mouse, keyboard, or hardware event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        /// <summary>
        /// The type of input event.
        /// 0 indicates mouse input, 1 indicates keyboard input, 2 indicates hardware input.
        /// </summary>
        public int type;

        /// <summary>
        /// The union containing the specific input data, based on the type.
        /// </summary>
        public InputUnion union;
    }

    /// <summary>
    /// Represents a union of input types: mouse, keyboard, and hardware.
    /// This struct allows a single field to hold multiple types of input data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        /// <summary>
        /// Specifies the mouse input event data.
        /// </summary>
        [FieldOffset(0)]
        public MouseInput mi;

        /// <summary>
        /// Specifies the keyboard input event data.
        /// </summary>
        [FieldOffset(0)]
        public KeyboardInput ki;

        /// <summary>
        /// Specifies the hardware input event data.
        /// </summary>
        [FieldOffset(0)]
        public HardwareInput hi;
    }

    /// <summary>
    /// Represents keyboard input data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        /// <summary>
        /// A virtual-key code. The code must be a value in the range 1 to 254.
        /// </summary>
        public ushort wVk;

        /// <summary>
        /// A hardware scan code for the key.
        /// </summary>
        public ushort wScan;

        /// <summary>
        /// Flags specifying various aspects of the keystroke.
        /// </summary>
        public uint dwFlags;

        /// <summary>
        /// Additional information associated with the event.
        /// </summary>
        public IntPtr dwExtraInfo;

        /// <summary>
        /// The time stamp for the event, in milliseconds.
        /// </summary>
        public uint time;
    }

    /// <summary>
    /// Represents mouse input data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        /// <summary>
        /// A set of flags that specify various aspects of mouse motion and button clicks.
        /// </summary>
        public uint dwFlags;

        /// <summary>
        /// Additional information associated with the event.
        /// </summary>
        public IntPtr dwExtraInfo;

        /// <summary>
        /// The x-coordinate of the mouse movement or the absolute position of the mouse, depending on the value of the dwFlags member.
        /// </summary>
        public int dx;

        /// <summary>
        /// The y-coordinate of the mouse movement or the absolute position of the mouse, depending on the value of the dwFlags member.
        /// </summary>
        public int dy;

        /// <summary>
        /// Additional data associated with the mouse event.
        /// For example, this can specify the amount of wheel movement for mouse wheel events.
        /// </summary>
        public uint mouseData;

        /// <summary>
        /// The time stamp for the event, in milliseconds.
        /// </summary>
        public uint time;
    }

    /// <summary>
    /// Represents a point in a two-dimensional coordinate system.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        /// <summary>
        /// The x-coordinate of the point.
        /// </summary>
        public int x;

        /// <summary>
        /// The y-coordinate of the point.
        /// </summary>
        public int y;
    }

    /// <summary>
    /// Enumerates the types of input events that can be sent.
    /// </summary>
    [Flags]
    [SuppressMessage(
        category: "Critical Code Smell",
        "S2346:Flags enumerations zero-value members should be named \"None\"",
        Justification = "Using 0 value for Mouse as the default event type instead of None for simplicity."
    )]
    public enum SendInputEventType
    {
        /// <summary>
        /// Represents a mouse input event.
        /// </summary>
        Mouse = 0,

        /// <summary>
        /// Represents a keyboard input event.
        /// </summary>
        Keyboard = 1,
        /// <summary>
        /// Represents a hardware input event.
        /// </summary>
        Hardware = 2
    }

    /// <summary>
    /// Enumerates the types of mouse events that can be sent.
    /// </summary>
    [Flags]
    public enum MouseEvent : uint
    {
        /// <summary>
        /// No mouse event.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// The mouse moved.
        /// </summary>
        Move = 0x0001,

        /// <summary>
        /// The left mouse button is pressed.
        /// </summary>
        LeftDown = 0x0002,

        /// <summary>
        /// The left mouse button is released.
        /// </summary>
        LeftUp = 0x0004,

        /// <summary>
        /// The right mouse button is pressed.
        /// </summary>
        RightDown = 0x0008,

        /// <summary>
        /// The right mouse button is released.
        /// </summary>
        RightUp = 0x0010,

        /// <summary>
        /// The middle mouse button is pressed.
        /// </summary>
        MiddleDown = 0x0020,

        /// <summary>
        /// The middle mouse button is released.
        /// </summary>
        MiddleUp = 0x0040,

        /// <summary>
        /// An X button is pressed.
        /// </summary>
        XDown = 0x0080,

        /// <summary>
        /// An X button is released.
        /// </summary>
        XUp = 0x0100,

        /// <summary>
        /// The wheel was moved.
        /// </summary>
        Wheel = 0x0800,

        /// <summary>
        /// The absolute position of the mouse.
        /// </summary>
        Absolute = 0x8000,
    }

    /// <summary>
    /// Enumerates the types of keyboard events that can be sent.
    /// </summary>
    [Flags]
    [SuppressMessage(
        category: "Critical Code Smell",
        "S2346:Flags enumerations zero-value members should be named \"None\"",
        Justification = "Using 0x0000 for KeyDown as the default event type instead of None for simplicity and consistency with native key event definitions."
    )]
    public enum KeyEvent : uint
    {
        /// <summary>
        /// Represents a key down event.
        /// </summary>
        KeyDown = 0x0000,

        /// <summary>
        /// Indicates that an extended key is pressed.
        /// </summary>
        ExtendedKey = 0x0001,

        /// <summary>
        /// Represents a key up event.
        /// </summary>
        KeyUp = 0x0002,

        /// <summary>
        /// Indicates that a unicode character is sent.
        /// </summary>
        Unicode = 0x0004,

        /// <summary>
        /// Indicates that a scancode is sent.
        /// </summary>
        Scancode = 0x0008
    }

    /// <summary>
    /// Enumerates device capabilities used for retrieving display settings.
    /// </summary>
    public enum DeviceCapabilities
    {
        /// <summary>
        /// Represents the horizontal resolution of the entire virtual screen.
        /// </summary>
        Desktophorzres = 118,

        /// <summary>
        /// Represents the vertical resolution of the entire virtual screen.
        /// </summary>
        Desktopvertres = 117
    }
}
