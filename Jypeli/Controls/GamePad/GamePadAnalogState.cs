using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Controls.GamePad
{
    internal class GamePadAnalogState : AnalogState
    {
        public GamePadAnalogState(double state, double analogChange, Vector stateVector, Vector mouseMovement)
        {
            State = state;
            AnalogChange = analogChange;
            StateVector = stateVector;
            MouseMovement = mouseMovement;
        }

        public double State { get; private set; }

        public double AnalogChange { get; private set; }

        public Vector StateVector { get; private set; }

        public Vector MouseMovement { get; private set; }
    }
}
