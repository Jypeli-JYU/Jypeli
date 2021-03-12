using Jypeli.Tests.Common;
using Jypeli.Tests.Physics;
using NUnit.Framework;

namespace Jypeli.Tests.Physics
{
    [TestFixture]
    public class GravityTest : TestClass
    {
        [Test]
        public void YGravityTest()
        {
            // Tän pitäisi mennä läpi, mutta ei mene :D
            game.TestFunction = delegate
            {
                game.Gravity = new Vector(0, -1000);

                PhysicsObject obj = new PhysicsObject(10, 10);
                obj.Position = new Vector(0, 0);
                obj.Color = Color.Red;

                game.Add(obj);

                Vector v1 = new Vector(10, 10);

                double expected = game.Gravity.Magnitude / 2;

                Timer.SingleShot(1, () =>
                Assert.AreEqual(expected, obj.Position.Magnitude, 2));
            };

            game.Run();
        }

        [Test]
        public void XGravityTest()
        {
            // Tän pitäisi mennä läpi, mutta ei mene :D
            game.TestFunction = delegate
            {
                game.Gravity = new Vector(1000, 0);

                PhysicsObject obj = new PhysicsObject(10, 10);
                obj.Position = new Vector(0, 0);
                obj.Color = Color.Red;

                game.Add(obj);

                Vector v1 = new Vector(10, 10);
                double delay = 0.5;
                double expected = game.Gravity.Magnitude / 2 * System.Math.Pow(delay, 2);

                Timer.SingleShot(delay, () =>
                Assert.AreEqual(expected, obj.Position.Magnitude, 2));
            };

            game.Run();
        }
    }
}