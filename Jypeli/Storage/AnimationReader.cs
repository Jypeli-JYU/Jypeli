using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli.Content
{
    public class AnimationReader : ContentTypeReader<Animation>
    {
        protected override Animation Read( ContentReader input, Animation existingInstance )
        {
            int fps = input.ReadObject<int>();
            int frameCount = input.ReadObject<int>();
            var frames = new Image[frameCount];

            for ( int i = 0; i < frameCount; )
            {
                Texture2D xnaFrame = input.ReadExternalReference<Texture2D>();
                if ( xnaFrame == null ) continue;
                frames[i] = new Image( xnaFrame );
                i++;
            }

            return new Animation( frames ) { FPS = fps };
        }
    }
}
