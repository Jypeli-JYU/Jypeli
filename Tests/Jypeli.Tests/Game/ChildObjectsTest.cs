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
                    Assert.That(parent.Position, Is.EqualTo(posToMove).Using(new VectorComparer(0.001)));
                    Assert.That(g.Position, Is.EqualTo(posToMove + delta).Using(new VectorComparer(0.001)));
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
                    Assert.That(parent.Position, Is.EqualTo(posToMove).Using(new VectorComparer(0.001)));
                    Assert.That(g.Position, Is.EqualTo(posToMove + new Vector(-delta.X, delta.Y)).Using(new VectorComparer(0.001)));
                    Assert.Pass();
                });
            };
            game.Run();
        }
    }
}