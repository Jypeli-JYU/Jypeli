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
 * Authors: Tero Jäntti, Tomi Karppinen
 */


namespace Jypeli.Devices
{
    /// <summary>
    /// Fyysinen laite.
    /// </summary>
    public class Device
    {
        private DisplayOrientation _displayOrientation = DisplayOrientation.Landscape;
        private DisplayResolution _displayResolution = DisplayResolution.Large;

        /// <summary>
        /// Laitteen kiihtyvyysanturi.
        /// </summary>
        public Accelerometer Accelerometer { get; protected set; }

        /// <summary>
        /// Tiedostojen säilytyspaikka.
        /// </summary>
        public FileManager Storage { get; protected set; }

        /// <summary>
        /// Onko laite mobiililaite.
        /// </summary>
        public virtual bool IsMobileDevice
        {
            get { return false; }
        }

        /// <summary>
        /// Onko laite puhelin.
        /// </summary>
        public virtual bool IsPhone
        {
            get { return false; }
        }

        /// <summary>
        /// Näytön tarkkuus.
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
        /// Näytön asemointi.
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
                if ( Game.Instance != null && Game.Screen != null ) UpdateScreen();
            }
        }

        /// <summary>
        /// Fyysinen laite
        /// </summary>
        protected Device()
        {
            // Initialize components as dummy ones if they're not initialized otherwise
            if (this.Accelerometer == null)
                this.Accelerometer = new DummyAccelerometer();
        }

        internal static Device Create()
        {
#if DESKTOP
            return new ComputerDevice();
#elif ANDROID
            return new Jypeli.Android.AndroidDevice();
#elif WINDOWS_PHONE81
            return new WindowsPhone81Device();
#elif WINDOWS_UAP
            return new WindowsUniversalDevice();
#elif WINRT
            return new Windows8Device();
#else
            return new Device();
#endif
        }

        /// <summary>
        /// Värisyttää puhelinta.
        /// </summary>
        /// <param name="milliSeconds">Värinän kesto millisekunteina.</param>
        public virtual void Vibrate( int milliSeconds )
        {
        }

        /// <summary>
        /// Lopettaa puhelimen värinän.
        /// </summary>
        public virtual void StopVibrating()
        {
        }
        
        /// <summary>
        /// Päivittää näytön koon ja asemoinnin
        /// </summary>
        protected virtual void UpdateScreen()
        {
            Vector defaultSize = Game.Screen.ViewportSize;
            Vector defaultScale = Vector.Diagonal;

            if ( _displayResolution == DisplayResolution.Small )
            {
                defaultSize = new Vector( 400, 240 );
                defaultScale = new Vector( Game.Screen.ViewportSize.X / defaultSize.X, Game.Screen.ViewportSize.Y / defaultSize.Y );
            }

            if ( _displayOrientation == DisplayOrientation.Portrait || _displayOrientation == DisplayOrientation.PortraitInverse )
            {
                Game.Screen.Size = defaultSize.Transpose();
                Game.Screen.Scale = defaultScale.Transpose();
                Game.Screen.Angle = _displayOrientation == DisplayOrientation.PortraitInverse ? -Angle.RightAngle : Angle.RightAngle;
            }
            else
            {
                Game.Screen.Size = defaultSize;
                Game.Screen.Scale = defaultScale;
                Game.Screen.Angle = _displayOrientation == DisplayOrientation.LandscapeRight ?  Angle.StraightAngle : Angle.Zero;
            }
        }

        internal virtual void ResetScreen()
        {
        }
    }
}
