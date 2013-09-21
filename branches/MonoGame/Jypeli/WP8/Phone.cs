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
 * Authors: Tero Jäntti, Tomi Karppinen
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Devices;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    /// <summary>
    /// Aliohjelmia ja ominaisuuksia, jotka toimivat vain puhelimessa. Voidaan kutsua myös muilla alustoilla,
    /// mutta tällöin mitään ei yksinkertaisesti tapahdu.
    /// </summary>
    public class Phone
    {
        private DisplayOrientation _displayOrientation = DisplayOrientation.Landscape;
        private DisplayResolution _displayResolution = DisplayResolution.Large;

        /// <summary>
        /// Puhelimen näytön tarkkuus.
        /// </summary>
        public DisplayResolution DisplayResolution
        {
            get { return _displayResolution; }
            set
            {
                if ( _displayResolution == value )
                    return;

                _displayResolution = value;
                if ( Game.Instance != null && Game.Screen != null ) ResetScreen();
            }
        }

        /// <summary>
        /// Puhelimen näytön asemointi.
        /// </summary>
        public DisplayOrientation DisplayOrientation
        {
            get { return _displayOrientation; }
            set
            {
                if ( _displayOrientation == value )
                    return;

                _displayOrientation = value;
                //Game.Instance.Accelerometer.DisplayOrientation = value;
                if ( Game.Instance != null && Game.Screen != null ) UpdateOrientation();
            }
        }

        /// <summary>
        /// Värisyttää puhelinta.
        /// </summary>
        /// <param name="milliSeconds">Värinän kesto millisekunteina.</param>
        public void Vibrate( int milliSeconds )
        {
            VibrateController.Default.Start( TimeSpan.FromMilliseconds( milliSeconds ) );
        }

        /// <summary>
        /// Lopettaa puhelimen värinän.
        /// </summary>
        public void StopVibrating()
        {
            VibrateController.Default.Stop();
        }

        private static void GetScreenSize( DisplayResolution resolution, out int width, out int height )
        {
            switch ( resolution )
            {
                case DisplayResolution.HD720:
                    width = 720;
                    height = 1280;
                    break;
                default:
                    width = 480;
                    height = 800;
                    break;
            }
        }

        private void UpdateOrientation()
        {
            Vector defaultSize = Game.Screen.ViewportSize;
            Vector defaultScale = Vector.Diagonal;

            if ( _displayResolution == DisplayResolution.Small )
            {
                defaultSize = new Vector( 240, 400 );
                defaultScale = new Vector( Game.Screen.ViewportSize.Y / defaultSize.X, Game.Screen.ViewportSize.X / defaultSize.Y );
            }

            if ( _displayOrientation == DisplayOrientation.Portrait || _displayOrientation == DisplayOrientation.PortraitInverse )
            {
                Game.Screen.Size = defaultSize;
                Game.Screen.Angle = _displayOrientation == DisplayOrientation.PortraitInverse ? Angle.StraightAngle : Angle.Zero;
            }
            else
            {
                Game.Screen.Size = defaultSize.Transpose();
                Game.Screen.Angle = _displayOrientation == DisplayOrientation.LandscapeRight ? Angle.RightAngle : -Angle.RightAngle;
            }
        }

        internal void ResetScreen()
        {
#if WINDOWS_PHONE
            int screenWidth, screenHeight;
            GraphicsDeviceManager graphics = Game.GraphicsDeviceManager;
            GetScreenSize( _displayResolution, out screenWidth, out screenHeight );
            graphics.ApplyChanges();
            UpdateOrientation();
#endif
        }
    }
}