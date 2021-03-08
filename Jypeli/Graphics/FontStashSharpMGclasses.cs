/*
 * Original files by Roman Shapiro: https://github.com/rds1983/FontStashSharp/tree/7ec7dbff73eb0826fb1a830fcb2c5bb671095c08/samples/FontStashSharp.Samples.MonoGameBackend
 * Renderer.cs, Texture2DManager.cs & Utility.cs
 * Copied here with minor modifications
 */

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		SpriteBatch _batch;

		public Renderer(SpriteBatch batch)
		{
			if (batch == null)
			{
				throw new ArgumentNullException(nameof(batch));
			}

			_batch = batch;
		}

		public void Draw(object texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
		{
			var textureWrapper = (Texture2D)texture;

			_batch.Draw(textureWrapper,
				position.ToXNA(),
				sourceRectangle == null ? default(Microsoft.Xna.Framework.Rectangle?) : sourceRectangle.Value.ToXNA(),
				color.ToXNA(),
				rotation,
				origin.ToXNA(),
				scale.ToXNA(),
				SpriteEffects.None,
				depth);
		}
	}
	class Texture2DManager : ITexture2DManager
	{
		readonly GraphicsDevice _device;

		public Texture2DManager(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			_device = device;
		}

		public object CreateTexture(int width, int height)
		{
			return new Texture2D(_device, width, height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{
			var mgTexture = (Texture2D)texture;
			mgTexture.SetData(0, 0, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height),
				data, 0, bounds.Width * bounds.Height * 4);
		}
	}

	static class Utility
	{
		public static Microsoft.Xna.Framework.Vector2 ToXNA(this System.Numerics.Vector2 r)
		{
			return new Microsoft.Xna.Framework.Vector2(r.X, r.Y);
		}

		public static System.Numerics.Vector2 ToSystemNumerics(this Microsoft.Xna.Framework.Vector2 r)
		{
			return new System.Numerics.Vector2(r.X, r.Y);
		}

		public static Microsoft.Xna.Framework.Rectangle ToXNA(this System.Drawing.Rectangle r)
		{
			return new Microsoft.Xna.Framework.Rectangle(r.Left, r.Top, r.Width, r.Height);
		}

		public static System.Drawing.Rectangle ToSystemDrawing(this Microsoft.Xna.Framework.Rectangle r)
		{
			return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
		}


		public static Microsoft.Xna.Framework.Color ToXNA(this System.Drawing.Color c)
		{
			return new Microsoft.Xna.Framework.Color(c.R, c.G, c.B, c.A);
		}

		public static System.Drawing.Color ToSystemDrawing(this Microsoft.Xna.Framework.Color c)
		{
			return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
		}
	}
}
