using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.Controls
{
    using XnaState = Microsoft.Xna.Framework.Input.MouseState;

    public struct MouseState
    {
        static MouseButton[] buttons =
            { MouseButton.Left, MouseButton.Right, MouseButton.Middle,
              MouseButton.XButton1, MouseButton.XButton2 };

        static ButtonState[] states =
            { ButtonState.Up, ButtonState.Pressed,
                ButtonState.Released, ButtonState.Down };

        public double X;
        public double Y;
        public int ButtonMask;
        public int Wheel;

        public Vector Position
        {
            get { return new Vector( X, Y ); }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public bool LeftDown
        {
            get { return ( ButtonMask & 1 ) > 0; }
            set { if ( value ) ButtonMask |= 1; else ButtonMask &= ~1; }
        }

        public bool RightDown
        {
            get { return ( ButtonMask & 2 ) > 0; }
            set { if ( value ) ButtonMask |= 2; else ButtonMask &= ~2; }
        }

        public bool MiddleDown
        {
            get { return ( ButtonMask & 4 ) > 0; }
            set { if ( value ) ButtonMask |= 4; else ButtonMask &= ~4; }
        }

        public bool X1Down
        {
            get { return ( ButtonMask & 8 ) > 0; }
            set { if ( value ) ButtonMask |= 8; else ButtonMask &= ~8; }
        }

        public bool X2Down
        {
            get { return ( ButtonMask & 16 ) > 0; }
            set { if ( value ) ButtonMask |= 16; else ButtonMask &= ~16; }
        }

        public bool AnyButtonDown
        {
            get { return ButtonMask > 0; }
        }

        internal bool IsButtonDown( MouseButton button )
        {
            int bi = 1;

            for ( int i = 0; i < 5; i++ )
            {
                if ( button == buttons[i] )
                    return ( ButtonMask & bi ) > 0;

                bi *= 2;
            }

            return false;
        }

        internal static ButtonState GetButtonState( MouseState oldState, MouseState newState, MouseButton button )
        {
            int oldDown = oldState.IsButtonDown( button ) ? 2 : 0;
            int newDown = newState.IsButtonDown( button ) ? 1 : 0;
            return states[oldDown + newDown];
        }

        public MouseState( MouseState oldState, Vector position )
        {
            this.X = position.X;
            this.Y = position.Y;
            this.Wheel = oldState.Wheel;
            this.ButtonMask = oldState.ButtonMask;
        }
    }
}
