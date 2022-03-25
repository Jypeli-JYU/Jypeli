namespace Jypeli
{
    public class AccelerometerAnalogState : AnalogState
    {
        public double State { get; private set; }
        public double AnalogChange { get; private set; }
        public Vector StateVector { get; private set; }
        public Vector MouseMovement { get; private set; }

        internal AccelerometerAnalogState(Vector prev, Vector curr)
        {
            StateVector = curr;
            State = curr.Magnitude;
            MouseMovement = curr - prev;
            AnalogChange = MouseMovement.Magnitude;
        }
    }
}
