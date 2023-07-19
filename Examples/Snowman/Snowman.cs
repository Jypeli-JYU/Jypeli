using System;
using Jypeli;

namespace Snowman
{
    public class Snowman : PhysicsGame
    {
        PlatformCharacter player;
        TiledMap map;
        double moveSpeed = 100;

        IntMeter counter;

        public override void Begin()
        {
            Level.BackgroundColor = Color.LightBlue;
            Gravity = new Vector(0, -1500);

            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, null);

            map = new TiledMap("map.tmj", "tileset.tsj");
            map.SetOverride(67, CreateDiamond);
            map.SetOverride(68, CreateSpikes);
            map.SetOverride(108, CreateJumpBlock);
            map.SetOverride(145, CreateSnowman);
            map.SetOverride(178, CreatePlayer);
            map.Execute();


            counter = new IntMeter(0);
        }

        private void CreateSnowman(Vector position, double width, double height, Image tileImage)
        {
            PhysicsObject snowman = PhysicsObject.CreateStaticObject(width, height);
            snowman.Position = position;
            tileImage.Scaling = ImageScaling.Nearest;
            snowman.Image = tileImage;
            snowman.Tag = "snowman";
            Add(snowman, -1);
        }

        private void CreateDiamond(Vector position, double width, double height, Image tileImage)
        {
            PhysicsObject diamond = PhysicsObject.CreateStaticObject(width, height);
            diamond.Position = position;
            tileImage.Scaling = ImageScaling.Nearest;
            diamond.Image = tileImage;
            diamond.Tag = "diamond";
            diamond.Shape = Shape.Circle;
            diamond.Oscillate(Vector.UnitY, 3, 0.5);
            Add(diamond, 2);
        }

        private void CreateSpikes(Vector position, double width, double height, Image tileImage)
        {
            PhysicsObject spike = PhysicsObject.CreateStaticObject(width, height);
            spike.Position = position;
            tileImage.Scaling = ImageScaling.Nearest;
            spike.Image = tileImage;
            spike.Tag = "spike";
            spike.Shape = Shape.FromImage(tileImage);
            Add(spike, -2);
        }

        private void CreatePlayer(Vector position, double width, double height, Image tileImage)
        {
            player = new PlatformCharacter(16, 16);
            player.Position = position;
            player.CollisionIgnoreGroup = 1;

            Image p = map.GetTileImage(146, map.tilesets[0]);
            p = Image.Mirror(p);
            p.Scaling = ImageScaling.Nearest;
            player.Image = p;

            Add(player, 1);

            Camera.Follow(player);
            Camera.StayInLevel = true;
            Camera.ZoomFactor = 4;

            AddCollisionHandler(player, "jump block", Boost);
            AddCollisionHandler(player, "diamond", CollectDiamond);
            AddCollisionHandler(player, "snowman", (p, o) =>
            {
                Keyboard.Clear();
                Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, null);
                MessageDisplay.Add(String.Format("You win! You collected {0}{1} diamonds. Press ESC to exit.", counter.Value == 6 ? "all " : "", counter.Value));
            });
            AddCollisionHandler(player, "spike", (p, o) => player.Position = position);

            Keyboard.Listen(Key.Left, ButtonState.Down, () => player.Walk(-moveSpeed), null);
            Keyboard.Listen(Key.Right, ButtonState.Down, () => player.Walk(moveSpeed), null);
            Keyboard.Listen(Key.Space, ButtonState.Pressed, () => player.Jump(450), null);
        }

        private void CollectDiamond(IPhysicsObject collidingObject, IPhysicsObject otherObject)
        {
            otherObject.IgnoresCollisionResponse = true;
            otherObject.Destroy();
            counter.Value += 1;
        }

        private void Boost(IPhysicsObject collidingObject, IPhysicsObject otherObject)
        {
            if (player.Y > otherObject.Y)
            {
                Image i1 = map.GetTileImage(108, map.tilesets[0]);
                Image i2 = map.GetTileImage(109, map.tilesets[0]);
                i1.Scaling = ImageScaling.Nearest;
                i2.Scaling = ImageScaling.Nearest;
                otherObject.Image = i1;
                player.ForceJump(1000);
                Timer.SingleShot(1, () => otherObject.Image = i2);
            }
        }

        private void CreateJumpBlock(Vector position, double width, double height, Image tileImage)
        {
            PhysicsObject jumpBlock = PhysicsObject.CreateStaticObject(width, height);
            jumpBlock.Position = position;
            tileImage.Scaling = ImageScaling.Nearest;
            jumpBlock.Image = tileImage;
            jumpBlock.Tag = "jump block";
            Add(jumpBlock, -2);
        }
    }
}