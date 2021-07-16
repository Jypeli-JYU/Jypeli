/*
 * Original files by Roman Shapiro: https://github.com/rds1983/FontStashSharp/tree/7ec7dbff73eb0826fb1a830fcb2c5bb671095c08/samples/FontStashSharp.Samples.MonoGameBackend
 * Renderer.cs, Texture2DManager.cs & Utility.cs
 * Copied here with minor modifications
 */

using FontStashSharp.Interfaces;
using System;
using System.Drawing;
using System.Numerics;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using Vector2 = System.Numerics.Vector2;

namespace FontStashSharp
{
	class Renderer : IFontStashRenderer
	{
		//SpriteBatch _batch;

		/*public Renderer(SpriteBatch batch)
		{
			if (batch == null)
			{
				throw new ArgumentNullException(nameof(batch));
			}

			_batch = batch;
		}*/

		public void Draw(object texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
		{
			/*var textureWrapper = (Texture2D)texture;

			_batch.Draw(textureWrapper,
				position.ToXNA(),
				sourceRectangle == null ? default(Microsoft.Xna.Framework.Rectangle?) : sourceRectangle.Value.ToXNA(),
				color.ToXNA(),
				rotation,
				origin.ToXNA(),
				scale.ToXNA(),
				SpriteEffects.None,
				depth);*/
		}
	}
	class Texture2DManager : ITexture2DManager
	{
		//readonly GraphicsDevice _device;

		/*public Texture2DManager(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			_device = device;
		}*/

		public object CreateTexture(int width, int height)
		{
            return null;// new Texture2D(_device, width, height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{/*
			var mgTexture = (Texture2D)texture;
			mgTexture.SetData(0, 0, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height),
				data, 0, bounds.Width * bounds.Height * 4);*/
		}
	}
}
