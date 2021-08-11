using System;
using System.Collections.Generic;
using Silk.NET.Input;

namespace Jypeli.Controls.GamePad
{
    internal class Vibration : Destroyable, Updatable
    {
        public double LifetimeLeft { get; private set; }
        public double LeftMotor { get; private set; }
        public double RightMotor { get; private set; }
        public double LeftAccel { get; set; }
        public double RightAccel { get; set; }
        public IGamepad Gamepad { get; set; }
        public bool IsUpdated { get { return true; } }

        public Vibration(IGamepad gamepad, double lmotor, double rmotor, double laccel, double raccel, double life)
        {
            LeftMotor = lmotor;
            RightMotor = rmotor;
            LeftAccel = laccel;
            RightAccel = raccel;
            LifetimeLeft = life;
            Gamepad = gamepad;
        }

        public void Update(Time time)
        {
            if (LifetimeLeft <= 0)
            {
                Destroy();
                return;
            }

            // Acceleration
            LeftMotor += time.SinceLastUpdate.TotalSeconds * LeftAccel;
            RightMotor += time.SinceLastUpdate.TotalSeconds * RightAccel;

            // Lifetime progression
            LifetimeLeft -= time.SinceLastUpdate.TotalSeconds;

            Execute();
        }

        public void Execute()
        {
            // TODO: Jostain syystä ei havaitse PS3 ohjaimen moottoreita
            if(Gamepad.VibrationMotors.Count == 2)
            {
                Gamepad.VibrationMotors[0].Speed = (float)LeftMotor;
                Gamepad.VibrationMotors[1].Speed = (float)RightMotor;
            }
        }

        #region Destroyable Members

        public bool IsDestroyed { get; private set; }

        public event Action Destroyed;

        public void Destroy()
        {
            if (IsDestroyed)
                return;
            IsDestroyed = true;
            if (Destroyed != null)
                Destroyed();
        }

        #endregion
    }
}
