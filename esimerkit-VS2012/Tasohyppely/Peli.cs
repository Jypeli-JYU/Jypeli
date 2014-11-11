using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;

namespace Tasohyppely
{
    public class Peli : PhysicsGame
    {
        public override void Begin()
        {
            IsMouseVisible = true;
            Gravity = new Vector( 0, -1000 );

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

            AssaultRifle ase = new AssaultRifle( 60, 15 );
            hahmo.Weapon = ase;

            //PlatformCharacter apu = new PlatformCharacter( 20, 20, Shape.Circle );
            PhysicsObject apu = new PhysicsObject( 20, 20, Shape.Circle );
            FollowerBrain aivot = new FollowerBrain( hahmo );
            aivot.DistanceClose = 80;
            aivot.StopWhenTargetClose = true;
            apu.Brain = aivot;
            apu.IgnoresGravity = true;
            Add( apu );

            AddCollisionHandler( apu, "ammus", delegate( PhysicsObject a, PhysicsObject b ) { a.Destroy(); b.Destroy(); } );

            Label hahmonNimi = new Label( RandomGen.NextLetter(true).ToString() + RandomGen.NextLetter(false).ToString() );
            hahmonNimi.Bottom = hahmo.Height;
            hahmo.Add( hahmonNimi );

            Keyboard.Listen( Key.Space, ButtonState.Down, delegate { var ammus = ase.Shoot(); if (ammus != null) ammus.Tag = "ammus"; }, null );
            Keyboard.Listen( Key.Right, ButtonState.Down, () => hahmo.Walk( 200 ), null );
            Keyboard.Listen( Key.Left, ButtonState.Down, () => hahmo.Walk( -200 ), null );
            Keyboard.Listen( Key.Up, ButtonState.Down, () => hahmo.Jump( 600 ), null );
            Keyboard.Listen( Key.Enter, ButtonState.Pressed, delegate { if ( !apu.IsDestroyed ) MessageDisplay.Add( "boop!" ); }, null );
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
