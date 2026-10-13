using Jypeli.Tests.Common;
using NUnit.Framework;

namespace Jypeli.Tests.Game
{
    [TestFixture]
    public class WindowTest : TestClass
    {
        [Test]
        public void MaximizeChangesStateAndScreenSize()
        {
            game.TestFunction = delegate
            {
                Vector before = Jypeli.Game.Screen.Size;
                Assert.IsFalse(game.IsMaximized);

                game.IsMaximized = true;

                Timer.SingleShot(0.5, () =>
                {
                    Assert.IsTrue(game.IsMaximized);
                    Assert.IsFalse(game.IsFullScreen);
                    Assert.AreNotEqual(before, Jypeli.Game.Screen.Size);
                    Assert.Pass();
                });
            };
            game.Run();
        }

        [Test]
        public void MaximizeCanBeUndone()
        {
            game.TestFunction = delegate
            {
                game.IsMaximized = true;

                Timer.SingleShot(0.5, () =>
                {
                    Assert.IsTrue(game.IsMaximized);
                    game.IsMaximized = false;

                    Timer.SingleShot(0.5, () =>
                    {
                        Assert.IsFalse(game.IsMaximized);
                        Assert.Pass();
                    });
                });
            };
            game.Run();
        }
    }
}
