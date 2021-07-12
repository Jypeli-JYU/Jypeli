using System;

namespace Jypeli
{
    /// <summary>
    /// Tehtävä mittarin arvon kasvattamiselle.
    /// </summary>
    public class IntMeterAddOperation : Operation
    {
        IntMeter meter;
        Timer timer;
        int dx;

        /// <inheritdoc/>
        public bool Active
        {
            get { return timer.Enabled; }
        }

        internal IntMeterAddOperation( IntMeter meter, int change, double seconds )
        {
            this.meter = meter;
            this.dx = Math.Sign( change );
            int times = (int)Math.Abs( change );

            timer = new Timer();
            timer.Times.LowerLimit += OnFinished;
            timer.Interval = Math.Abs( seconds / change );
            timer.Timeout += Tick;
            timer.Start( times );
        }

        private void Tick()
        {
            meter.Value += dx;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            timer.Stop();
            if ( Stopped != null ) Stopped();
        }

        /// <inheritdoc/>
        public event Action Finished;

        /// <inheritdoc/>
        public event Action Stopped;

        private void OnFinished()
        {
            if ( Finished != null )
                Finished();
        }
    }

    /// <summary>
    /// Tehtävä mittarin arvon kasvattamiselle.
    /// </summary>
    public class DoubleMeterAddOperation : Operation
    {
        DoubleMeter meter;
        Timer timer;
        double dx;

        /// <inheritdoc/>
        public bool Active
        {
            get { return timer.Enabled; }
        }

        internal DoubleMeterAddOperation( DoubleMeter meter, double change, double seconds )
        {
            this.meter = meter;

            double dt = findDt( seconds );
            this.dx = dt * change / seconds;
            int times = (int)( seconds / dt );

            timer = new Timer();
            timer.Times.LowerLimit += OnFinished;
            timer.Interval = dt;
            timer.Timeout += Tick;
            timer.Start( times );
        }

        private double findDt( double seconds )
        {
            double dt = seconds;
            while ( dt > 0.05 ) dt /= 2;
            return dt;
        }

        private void Tick()
        {
            meter.Value += dx;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            timer.Stop();
            if ( Stopped != null ) Stopped();
        }

        /// <inheritdoc/>
        public event Action Finished;

        /// <inheritdoc/>
        public event Action Stopped;

        private void OnFinished()
        {
            if ( Finished != null )
                Finished();
        }
    }
}
