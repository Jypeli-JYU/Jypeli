using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

namespace Tasohyppely
{
    public class Peli : PhysicsGame
    {
        public override void Begin()
        {
            IsMouseVisible = true;
            Gravity = new Vector( 0, -1000 );

            //PhysicsObject lattia = PhysicsObject.CreateStaticObject( 1000, 60 );
            //lattia.Top = -200;
            //lattia.Color = Color.Green;
            //Add( lattia );

            TileMap kentta = TileMap.FromLevelAsset( "kentta" );
            kentta.SetTileMethod( 'p', LuoPelaaja );
            kentta.SetTileMethod( 'x', LuoTaso );
            kentta.Execute( 40, 40 );
        }

        private void LuoPelaaja( Vector position, double width, double height )
        {
            PlatformCharacter hahmo = new PlatformCharacter( 40, 80 );
            hahmo.Position = position;
            Add( hahmo );

            Keyboard.Listen( Key.Right, ButtonState.Down, () => hahmo.Walk( 200 ), null );
            Keyboard.Listen( Key.Left, ButtonState.Down, () => hahmo.Walk( -200 ), null );
            Keyboard.Listen( Key.Up, ButtonState.Down, () => hahmo.Jump( 600 ), null );
        }

        private void LuoTaso( Vector position, double width, double height )
        {
            PhysicsObject taso = PhysicsObject.CreateStaticObject( width, height );
            taso.Position = position;
            taso.Color = Color.Green;
            Add( taso );
        }
    }
}
