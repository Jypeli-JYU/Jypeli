﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Tests.Common;
using NUnit.Framework;

namespace Jypeli.Tests.Game
{
    [TestFixture]
    public class TimerTest : TestClass
    {
        [TestCase(1.5)]
        [TestCase(1)]
        public void SingleShot(double delay)
        {
            game.TestFunction = delegate
            {
                Timer.SingleShot(delay, () => { Assert.AreEqual(delay, game.UpdateCount*1.0/60); Assert.Pass(); } );
            };
            game.Run();
        }

        [TestCase(0.1, 3)]
        [TestCase(0.5,2)]
        [TestCase(0.001, 20)] // hmm...
        [TestCase(0.001, 100)]
        public void RunCount(double interval, int count)
        {
            game.TestFunction = delegate
            {
                int counter = 0;
                Timer t = new Timer(interval);
                t.Timeout += () => counter++;
                t.Start(count);

                Timer.SingleShot(interval*2*count, () => { Assert.AreEqual(count, counter); Assert.Pass(); });

            };
            game.Run();
        }
    }
}
