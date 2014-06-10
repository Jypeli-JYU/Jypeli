﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    public partial class Widget
    {
        /// <summary>
        /// Reunojen väri.
        /// </summary>
        public Color BorderColor { get; set; }

        private void InitAppearance()
        {
            this.BorderColor = Color.Transparent;
        }

        public virtual void Draw( Matrix parentTransformation, Matrix transformation )
        {
        }

        public void Draw( Matrix parentTransformation )
        {
            if (!IsVisible)
                return;

            Matrix transformation =
                Matrix.CreateScale( (float)Size.X, (float)Size.Y, 1f )
                * Matrix.CreateRotationZ( (float)Angle.Radians )
                * Matrix.CreateTranslation( (float)Position.X, (float)Position.Y, 0f )
                * parentTransformation;

            if ( Image != null && ( !TextureFillsShape ) )
            {
                Renderer.DrawImage( Image, ref transformation, TextureWrapSize );
            }
            else if ( Image != null )
            {
                Renderer.DrawShape( Shape, ref transformation, ref transformation, Image, TextureWrapSize, Color );
            }
            else
            {
                Renderer.DrawShape( Shape, ref transformation, Color );
            }

            if ( BorderColor != Color.Transparent )
            {
                Graphics.LineBatch.Begin( ref transformation );
                {
                    Vector[] vertices = Shape.Cache.OutlineVertices;
                    for ( int i = 0; i < vertices.Length - 1; i++ )
                    {
                        Graphics.LineBatch.Draw( vertices[i], vertices[i + 1], BorderColor );
                    }
                    Graphics.LineBatch.Draw( vertices[vertices.Length - 1], vertices[0], BorderColor );
                }
                Graphics.LineBatch.End();
            }

            Draw( parentTransformation, transformation );

            if ( _childObjects != null && _childObjects.Count > 0 )
            {
                Matrix childTransformation =
                    Matrix.CreateRotationZ( (float)Angle.Radians )
                    * Matrix.CreateTranslation( (float)Position.X, (float)Position.Y, 0f )
                    * parentTransformation;

                DrawChildObjects( ref parentTransformation, ref transformation, ref childTransformation );
            }
        }

        internal protected virtual void DrawChildObjects( ref Matrix parentTransformation, ref Matrix transformation, ref Matrix childTransformation )
        {
            foreach ( var child in Objects )
            {
                Widget wc = child as Widget;

                if (wc != null && wc.IsVisible)
                {
                    wc.Draw(childTransformation);
                }
            }
        }
    }
}
