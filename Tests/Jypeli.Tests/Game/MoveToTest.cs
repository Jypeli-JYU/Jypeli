using Jypeli.Tests.Common;
using Jypeli.Tests.Game;
using NUnit.Framework;

namespace Jypeli.Tests.Game
{
    [TestFixture]
    public class MoveToTest : TestClass
    {
        [Test]
        public void MoveToSimpleTest()
        {
            game.TestFunction = delegate
            {
                GameObject g = new GameObject(10, 10);
                game.Add(g);

                Vector pos = new Vector(50, 50);

                g.MoveTo(pos, 40, () => { Assert.AreEqual(g.Position, pos); Assert.Pass(); });
            };
            game.Run();
        }
    }
}