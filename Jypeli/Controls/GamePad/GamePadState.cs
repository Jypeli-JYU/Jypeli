using System;
using System.Numerics;
using Silk.NET.Input;

namespace Jypeli.Controls
{

    /// <summary>
    /// Peliohjaimen tilan ylläpito
    /// </summary>
    public unsafe struct GamePadState
    {
        private fixed bool buttons[20];

        /// <summary>
        /// Onko ohjain kytketty
        /// </summary>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Vasen tatti
        /// </summary>
        public Vector LeftThumbStick { get; internal set; }

        /// <summary>
        /// Oikea tatti
        /// </summary>
        public Vector RightThumbStick { get; internal set; }

        /// <summary>
        /// Vasen liipaisin
        /// </summary>
        public double LeftTrigger { get; internal set; }

        /// <summary>
        /// Oikea liipaisin
        /// </summary>
        public double RightTrigger { get; internal set; }

        /// <summary>
        /// Onko nappi alhaalla
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsButtonDown(Button button)
        {
            return buttons[(int)button];
        }

        /// <summary>
        /// Onko nappi ylhäällä
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsButtonUp(Button button)
        {
            return !buttons[(int)button];
        }

        internal void SetButtonUp(Silk.NET.Input.Button arg2)
        {
            buttons[(int)arg2.Name] = false;
        }

        internal void SetButtonDown(Silk.NET.Input.Button arg2)
        {
            buttons[(int)arg2.Name] = true;
        }

        internal void SetTrigger(Trigger trigger)
        {
            switch (trigger.Index)
            {
                case 0:
                    LeftTrigger = trigger.Position;
                    break;
                case 1:
                    RightTrigger = trigger.Position;
                    break;
            }
        }

        internal void SetThumbstick(Thumbstick stick)
        {
            switch (stick.Index)
            {
                case 0:
                    LeftThumbStick = new Vector(stick.X, -stick.Y); // Y-akseli on väärinpäin?
                    break;
                case 1:
                    RightThumbStick = new Vector(stick.X, -stick.Y);
                    break;
            }
        }
    }
}
