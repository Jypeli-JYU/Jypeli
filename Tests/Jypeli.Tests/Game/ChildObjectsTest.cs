using Jypeli.Tests.Common;
using Jypeli.Tests.Common.Comparers;
using Jypeli.Tests.Game;
using NUnit.Framework;

namespace Jypeli.Tests.Game
{
    [TestFixture]
    public class ChildObjectsTest : TestClass
    {

        GameObject parent;

        public override void Setup()
        {
            base.Setup();
            parent = new GameObject(10, 10);
            parent.Position = new Vector(0, 0);
            parent.Color = Color.Red;
        }

        [Test]
        public void MoveTo()
        {
            game.TestFunction = delegate
            {
                Vector delta = new Vector(10, 10);
                Vector posToMove = new Vector(20, 20);

                GameObject g = new GameObject(10, 10);
                g.Position = delta;
                parent.Add(g);

                game.Add(parent);

                parent.MoveTo(posToMove, 20, () =>
                {
                    Assert.That(parent.Position, Is.EqualTo(posToMove).Using(new VectorComparer(0.1)));
                    Assert.That(g.Position, Is.EqualTo(posToMove + delta).Using(new VectorComparer(0.1)));
                    Assert.Pass();
                });
            };
            game.Run();
        }

        [Test]
        public void MoveToWithRotate()
        {
            game.TestFunction = delegate
            {
                Vector delta = new Vector(10, 10);
                Vector posToMove = new Vector(20, 20);

                GameObject g = new GameObject(10, 10);
                g.Position = delta;
                parent.Add(g);

                game.Add(parent);

                parent.Angle = Angle.FromDegrees(90);

                parent.MoveTo(posToMove, 20, () =>
                {
                    Assert.That(parent.Position, Is.EqualTo(posToMove).Using(new VectorComparer(0.1)));
                    Assert.That(g.Position, Is.EqualTo(posToMove + new Vector(-delta.X, delta.Y)).Using(new VectorComparer(0.1)));
                    Assert.Pass();
                });
            };
            game.Run();
        }

        [TestCase(0, 0, 0)]
        [TestCase(-10, 0, 90)]
        [TestCase(10, 10, -180)]
        public void SetRelativeposition(double parentX, double parentY, double parentAngle)
        {
            game.TestFunction = delegate
            {
                Vector delta = new Vector(10, 10);
                Vector parentPos = new Vector(parentX, parentY);
                parent.Position = parentPos;
                parent.Angle = Angle.FromDegrees(parentAngle);

                GameObject g = new GameObject(10, 10);
                parent.Add(g);
                g.RelativePosition = delta;

                game.Add(parent);

                Assert.That(parent.Position, Is.EqualTo(parentPos).Using(new VectorComparer(0.001)));
                Assert.That(g.Position, Is.EqualTo(parentPos + (new Vector(delta.X, delta.Y)).Transform(System.Numerics.Matrix4x4.CreateRotationZ((float)parent.Angle.Radians))).Using(new VectorComparer(0.001)));
                Assert.That(g.RelativePosition, Is.EqualTo(delta).Using(new VectorComparer(0.001)));
                Assert.Pass();
            };
            game.Run();
        }

        [TestCase(0, 0, 0)]
        [TestCase(-10, 0, 90)]
        [TestCase(10, 10, -180)]
        public void SetRelativepositionToMainParent(double parentX, double parentY, double parentAngle)
        {
            game.TestFunction = delegate
            {
                Vector delta = new Vector(10, 10);
                Vector parentPos = new Vector(parentX, parentY);
                parent.Position = parentPos;
                parent.Angle = Angle.FromDegrees(parentAngle);

                GameObject g1 = new GameObject(10, 10);
                parent.Add(g1);

                GameObject g2 = new GameObject(10, 10);
                g2.Position = new Vector(-100, 100); // T‰ll‰ ei tulisi olla mit‰‰n merkityst‰
                g1.Add(g2);

                g2.RelativePositionToMainParent = delta;

                game.Add(parent);

                Assert.That(parent.Position, Is.EqualTo(parentPos).Using(new VectorComparer(0.001)));
                Assert.That(g2.Position, Is.EqualTo(parentPos + (new Vector(delta.X, delta.Y)).Transform(System.Numerics.Matrix4x4.CreateRotationZ((float)parent.Angle.Radians))).Using(new VectorComparer(0.001)));
                Assert.That(g2.RelativePositionToMainParent, Is.EqualTo(delta).Using(new VectorComparer(0.001)));
                Assert.Pass();
            };
            game.Run();
        }
    }
}