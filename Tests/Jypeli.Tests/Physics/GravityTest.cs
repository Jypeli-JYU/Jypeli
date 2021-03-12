using Jypeli.Tests.Common;
using Jypeli.Tests.Common.Comparers;
using Jypeli.Tests.Physics;
using NUnit.Framework;

namespace Jypeli.Tests.Physics
{
    [TestFixture]
    public class GravityTest : TestClass
    {
        PhysicsObject obj;
        
        public override void Setup()
        {
            base.Setup();
            obj = new PhysicsObject(10, 10);
            obj.Position = new Vector(0, 0);
            obj.Color = Color.Red;
        }

        [TestCase(0, -1000, 2)]
        [TestCase(1000, -1000, 1)]
        [TestCase(1000, 0, 1)]
        [TestCase(0, 0, 2, float.Epsilon)]
        public void PositionTest(double gx, double gy, double delay, float tolerance = 5)
        {
            // Tän pitäisi mennä läpi, mutta ei mene :D
            game.TestFunction = delegate
            {
                game.Gravity = new Vector(gx, gy);
                game.Add(obj);

                // x = 1/2*a*t^2
                double ex = game.Gravity.X / 2 * System.Math.Pow(delay, 2);
                double ey = game.Gravity.Y / 2 * System.Math.Pow(delay, 2);

                Vector expectedPos = new Vector(ex, ey);

                Timer.SingleShot(delay, () =>
                {
                    Assert.That(obj.Position, Is.EqualTo(expectedPos).Using(new VectorComparer(tolerance)));
                    Assert.Pass(); // TODO: Tän vaatiminen on vähän huonoa.
                });
            };

            game.Run();
        }
    }
}