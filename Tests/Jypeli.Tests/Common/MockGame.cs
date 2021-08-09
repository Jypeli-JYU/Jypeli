using Jypeli;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Jypeli.Tests.Game
{
    public class MockGame : PhysicsGame
    {
        public Action TestFunction { get; set; }
        public int MinUpdateCount { get; set; }
        public int MaxUpdateCount { get; set; }
        public int UpdateCount { get; set; }

        public MockGame()
        {
            MaxUpdateCount = 600;
            MinUpdateCount = int.MaxValue;
            FixedTimeStep = true; // Aikaan liittyvät testit toimivat epävarmasti vaihtelevalla aika-askeleella.
                                  // TODO: Vai kuinka isoa virhemarginaalia niissä pitäisi käyttää?
        }

        public override void Begin()
        {
            base.Begin();

            TestFunction.Invoke();
        }

        private void EvaluateExitCriteria()
        {
            if (UpdateCount >= MaxUpdateCount)
                throw new NotFinishedException();
        }

        protected override void Update(Time time)
        {
            base.Update(time);
            UpdateCount++;
            EvaluateExitCriteria();
        }
    }

    internal class NotFinishedException : Exception
    {
        public NotFinishedException()
        {
        }
    }
}