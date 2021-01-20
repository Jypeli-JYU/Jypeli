#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using Jypeli.Widgets;

namespace Jypeli
{
    public class ObjectLoadMethods
    {
        internal static readonly ObjectLoadMethods Empty = new ObjectLoadMethods();

        internal Dictionary<string, Func<GameObject, GameObject>> MethodsByTag = new Dictionary<string, Func<GameObject, GameObject>>();
        internal Dictionary<string, Func<GameObject, GameObject>> MethodsByTemplate = new Dictionary<string, Func<GameObject, GameObject>>();

        /// <summary>
        /// Lisää metodin ladatun olion muokkaamiseksi.
        /// Metodin tulee palauttaa muokkaamansa olio. Metodi voi myös palauttaa uuden olion, jos
        /// haluttu tyyppi ei ole tuettu editorissa.
        /// </summary>
        /// <param name="tag">Tagi, joka oliolla tulee olla.</param>
        /// <param name="method">Metodi, joka muokkaa oliota tai palauttaa uuden.</param>
        public void AddByTag( string tag, Func<GameObject, GameObject> method )
        {
            MethodsByTag.Add( tag, method );
        }

        /// <summary>
        /// Lisää metodin ladatun olion muokkaamiseksi.
        /// Metodin tulee palauttaa muokkaamansa olio. Metodi voi myös palauttaa uuden olion, jos
        /// haluttu olion tyyppi ei ole tuettu editorissa.
        /// </summary>
        /// <param name="template">Template, jonka pohjalta olio on luotu.</param>
        /// <param name="method">Metodi, joka muokkaa oliota tai palauttaa uuden.</param>
        public void AddByTemplate( string template, Func<GameObject, GameObject> method )
        {
            MethodsByTemplate.Add( template, method );
        }
    }


    /// <summary>
    /// Pelikenttä, johon voi lisätä olioita. Kentällä voi myös olla reunat ja taustaväri tai taustakuva.
    /// </summary>
    [Save]
    public partial class Level : Dimensional
    {
        [Save] double _width = 1000;
        [Save] double _height = 800;

        private Game game;
        public double AmbientLight { get; set; }

        /// <summary>
        /// Kentän keskipiste.
        /// </summary>
        public readonly Vector Center = Vector.Zero;

        /// <summary>
        /// Kentän taustaväri.
        /// </summary>
        public Color BackgroundColor
        {
            get { return Background.Color; }
            set { Background.Color = value; }
        }

        /// <summary>
        /// Kentän taustakuva.
        /// </summary>
        public Background Background { get; set; }

        /// <summary>
        /// Kentän leveys.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Kentän korkeus.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Kentän koko (leveys ja korkeus).
        /// </summary>
        public Vector Size
        {
            get { return new Vector( _width, _height ); }
            set { _width = value.X; _height = value.Y; }
        }

        /// <summary>
        /// Kentän vasemman reunan x-koordinaatti.
        /// </summary>
        public double Left
        {
            get { return -Width / 2; }
        }

        /// <summary>
        /// Kentän oikean reunan x-koordinaatti.
        /// </summary>
        public double Right
        {
            get { return Width / 2; }
        }

        /// <summary>
        /// Kentän yläreunan y-koordinaatti.
        /// </summary>
        public double Top
        {
            get { return Height / 2; }
        }

        /// <summary>
        /// Kentän alareunan y-koordinaatti.
        /// </summary>
        public double Bottom
        {
            get { return -Height / 2; }
        }

        /// <summary>
        /// Kentän rajaama alue
        /// </summary>
        public BoundingRectangle BoundingRect
        {
            get { return new BoundingRectangle( Center.X, Center.Y, Width, Height ); }
        }

        internal Level( Game game )
        {
            this.game = game;
            AmbientLight = 1.0;

            // creates a null background
            this.Background = new Jypeli.Widgets.Background( Vector.Zero );
            this.Background.Color = Color.LightBlue; // default color
        }

        internal void Clear()
        {
            Background.Image = null;
        }

        /// <summary>
        /// Laskee pienimmän alueen, jonka sisälle kaikki kentän oliot mahtuvat.
        /// </summary>
        public BoundingRectangle FindObjectLimits()
        {
            var objectsAboutToBeAdded = Game.GetObjectsAboutToBeAdded();

            if ( ( Game.Instance.ObjectCount + objectsAboutToBeAdded.Count ) == 0 )
            {
                throw new InvalidOperationException( "There must be at least one object" );
            }

            double left = double.PositiveInfinity;
            double right = double.NegativeInfinity;
            double top = double.NegativeInfinity;
            double bottom = double.PositiveInfinity;

            foreach ( var layer in Game.Instance.Layers )
            {
                if (layer.IgnoresZoom)
                    continue;

                foreach ( var o in layer.Objects )
                {
                    if ( o.Left < left )
                        left = o.Left * layer.RelativeTransition.X;
                    if ( o.Right > right )
                        right = o.Right * layer.RelativeTransition.X;
                    if ( o.Top > top )
                        top = o.Top * layer.RelativeTransition.Y;
                    if ( o.Bottom < bottom )
                        bottom = o.Bottom * layer.RelativeTransition.Y;
                }
            }

            foreach ( var o in objectsAboutToBeAdded )
            {
                if ( o.Left < left )
                    left = o.Left;
                if ( o.Right > right )
                    right = o.Right;
                if ( o.Top > top )
                    top = o.Top;
                if ( o.Bottom < bottom )
                    bottom = o.Bottom;
            }

            // Jos kentällä ei ollut ainuttakaan objektia kerroksilla jotka eivät ignooraa kameran zoomia.
            if(left == double.PositiveInfinity)
            {
                left = 1;
                right = -1;
                top = 1;
                bottom = -1;
            }

            return new BoundingRectangle( new Vector( left, top ), new Vector( right, bottom ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen kohdan kentän reunojen sisältä.
        /// </summary>
        /// <returns>Vektori.</returns>
        public Vector GetRandomPosition()
        {
            return new Vector( RandomGen.NextDouble( Left, Right ), RandomGen.NextDouble( Bottom, Top ) );
        }

        /// <summary>
        /// Palauttaa satunnaisen vapaan kohdan kentän reunojen sisältä.
        /// </summary>
        /// <param name="radius">Säde jonka sisällä ei saa olla olioita</param>
        /// <returns></returns>
        public Vector GetRandomFreePosition(double radius)
        {
            if (radius < 0) throw new ArgumentException("Radius cannot be negative!");
            if (radius == 0) return GetRandomPosition();

            Vector position;

            do
            {
                position = GetRandomPosition();
            }
            while (Game.Instance.GetObjectAt(position, radius) != null);

            return position;
        }
    }
}
