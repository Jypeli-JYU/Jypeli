﻿
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jypeli
{
    internal class LineBatch
    {
        VertexPositionColor[] vertexBuffer = new VertexPositionColor[512];
        Effect effect;
        Matrix matrix;
        int iVertexBuffer = 0;
        bool beginHasBeenCalled = false;
        public bool LightingEnabled = true;


        public void Begin( ref Matrix matrix )
        {
            Debug.Assert( !beginHasBeenCalled );
            beginHasBeenCalled = true;

            this.matrix = matrix;
            iVertexBuffer = 0;
        }

        public void End()
        {
            Debug.Assert( beginHasBeenCalled );
            Flush();
            beginHasBeenCalled = false;
        }

        private void Flush()
        {
            if ( iVertexBuffer > 0 )
            {
                effect = Graphics.GetColorEffect(ref matrix, LightingEnabled);
                for ( int i = 0; i < effect.CurrentTechnique.Passes.Count; i++ )
                    effect.CurrentTechnique.Passes[i].Apply();

                Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList, vertexBuffer, 0, iVertexBuffer / 2);
            }

            iVertexBuffer = 0;
        }

        public void Draw(Vector startPoint, Vector endPoint, Color color)
        {
            if ((iVertexBuffer + 2) > vertexBuffer.Length)
            {
                Flush();
            }

            vertexBuffer[iVertexBuffer++] = new VertexPositionColor(
                new Vector3((float)startPoint.X, (float)startPoint.Y, 0f),
                color.AsXnaColor());
            vertexBuffer[iVertexBuffer++] = new VertexPositionColor(
                new Vector3((float)endPoint.X, (float)endPoint.Y, 0f),
                color.AsXnaColor());
        }
    }
}
