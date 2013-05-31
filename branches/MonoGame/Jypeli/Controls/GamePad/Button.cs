﻿#region MIT License
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

using Microsoft.Xna.Framework.Input;

namespace Jypeli
{
    public enum Button
    {
        DPadUp = Buttons.DPadUp,
        DPadDown = Buttons.DPadDown,
        DPadLeft = Buttons.DPadLeft,
        DPadRight = Buttons.DPadRight,

        /// <summary>
        /// Start.
        /// </summary>
        Start = Buttons.Start,

        /// <summary>
        /// Back.
        /// </summary>
        Back = Buttons.Back,

        /// <summary>
        /// Oikea tikku.
        /// </summary>
        LeftStick = Buttons.LeftStick,

        /// <summary>
        /// Vasen tikku.
        /// </summary>
        RightStick = Buttons.RightStick,

        /// <summary>
        /// Vasen olkanappi.
        /// </summary>
        LeftShoulder = Buttons.LeftShoulder,

        /// <summary>
        /// Oikea olkanappi.
        /// </summary>
        RightShoulder = Buttons.RightShoulder,

        BigButton = Buttons.BigButton,
        A = Buttons.A,
        B = Buttons.B,
        X = Buttons.X,
        Y = Buttons.Y,

        /// <summary>
        /// Oikea liipasin.
        /// </summary>
        RightTrigger = Buttons.RightTrigger,

        /// <summary>
        /// Vasen liipasin.
        /// </summary>
        LeftTrigger = Buttons.LeftTrigger
    }
}
