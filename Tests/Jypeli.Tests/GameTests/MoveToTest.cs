using Jypeli.Tests.Game;
using NUnit.Framework;

namespace Jypeli.Tests.Game
{
    [TestFixture]
    public class MoveToTest
    {
        private MockGame game;

        [SetUp]
        public void Setup() 
        {
            game = new MockGame();
        }


        [Test]
        public void MoveToSimpleTest()
        {
            game.TestFunction = delegate
            {
                GameObject g = new GameObject(10, 10);
                game.Add(g);

                Vector pos = new Vector(100, 100);

                g.MoveTo(pos, 40, () => { Assert.AreEqual(g.Position, pos); Assert.Pass(); });
            };
            game.Run();
        }

        [TearDown]
        public virtual void TearDown()
        {
            game.Dispose();
            game = null;
        }
    }
}