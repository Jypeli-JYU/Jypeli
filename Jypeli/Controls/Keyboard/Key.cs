#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using Keys = Silk.NET.Input.Key;

namespace Jypeli
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Näppäimistön näppäin.
    /// </summary>
    public enum Key
    {
        None = Keys.Unknown,
        Back = Keys.Backspace,
        Tab = Keys.Tab,
        Enter = Keys.Enter,
        Pause = Keys.Pause,
        CapsLock = Keys.CapsLock,
        Escape = Keys.Escape,
        Space = Keys.Space,
        PageUp = Keys.PageUp,
        PageDown = Keys.PageDown,
        End = Keys.End,
        Home = Keys.Home,
        Left = Keys.Left,
        Up = Keys.Up,
        Right = Keys.Right,
        Down = Keys.Down,
        PrintScreen = Keys.PrintScreen,
        Insert = Keys.Insert,
        Delete = Keys.Delete,
        D0 = Keys.Number0,
        D1 = Keys.Number1,
        D2 = Keys.Number2,
        D3 = Keys.Number3,
        D4 = Keys.Number4,
        D5 = Keys.Number5,
        D6 = Keys.Number6,
        D7 = Keys.Number7,
        D8 = Keys.Number8,
        D9 = Keys.Number9,
        A = Keys.A,
        B = Keys.B,
        C = Keys.C,
        D = Keys.D,
        E = Keys.E,
        F = Keys.F,
        G = Keys.G,
        H = Keys.H,
        I = Keys.I,
        J = Keys.J,
        K = Keys.K,
        L = Keys.L,
        M = Keys.M,
        N = Keys.N,
        O = Keys.O,
        P = Keys.P,
        Q = Keys.Q,
        R = Keys.R,
        S = Keys.S,
        T = Keys.T,
        U = Keys.U,
        V = Keys.V,
        W = Keys.W,
        X = Keys.X,
        Y = Keys.Y,
        Z = Keys.Z,
        Å = Keys.LeftBracket,
        Ä = Keys.Apostrophe,
        Ö = Keys.Semicolon,
        NumPad0 = Keys.Keypad0,
        NumPad1 = Keys.Keypad1,
        NumPad2 = Keys.Keypad2,
        NumPad3 = Keys.Keypad3,
        NumPad4 = Keys.Keypad4,
        NumPad5 = Keys.Keypad5,
        NumPad6 = Keys.Keypad6,
        NumPad7 = Keys.Keypad7,
        NumPad8 = Keys.Keypad8,
        NumPad9 = Keys.Keypad9,
        Multiply = Keys.KeypadMultiply,
        Add = Keys.KeypadAdd,
        //Separator = Keys.Separator, TODO: Mikä tää on?
        Subtract = Keys.KeypadSubtract,
        Decimal = Keys.KeypadDecimal,
        Divide = Keys.KeypadDivide,
        F1 = Keys.F1,
        F2 = Keys.F2,
        F3 = Keys.F3,
        F4 = Keys.F4,
        F5 = Keys.F5,
        F6 = Keys.F6,
        F7 = Keys.F7,
        F8 = Keys.F8,
        F9 = Keys.F9,
        F10 = Keys.F10,
        F11 = Keys.F11,
        F12 = Keys.F12,
        F13 = Keys.F13,
        F14 = Keys.F14,
        F15 = Keys.F15,
        F16 = Keys.F16,
        F17 = Keys.F17,
        F18 = Keys.F18,
        F19 = Keys.F19,
        F20 = Keys.F20,
        F21 = Keys.F21,
        F22 = Keys.F22,
        F23 = Keys.F23,
        F24 = Keys.F24,
        NumLock = Keys.NumLock,
        Scroll = Keys.ScrollLock,
        LeftShift = Keys.ShiftLeft,
        RightShift = Keys.ShiftRight,
        LeftControl = Keys.ControlLeft,
        RightControl = Keys.ControlRight,
        LeftAlt = Keys.AltLeft,
        RightAlt = Keys.AltRight,
        //Ouml = Keys.OemTilde,
        //Auml = Keys.OemQuotes, //TODO: Entä nää?
        LessOrGreater = Keys.World2,
        Period = Keys.Period,
        Comma = Keys.Comma,
    }
}
