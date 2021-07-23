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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen, Rami Pasanen.
 */

using System;
using System.ComponentModel;
using System.Linq;

using System.Reflection;
using Jypeli.Physics2d;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Kuvio.
    /// </summary>
    public abstract class Shape
    {
        /// <summary>
        /// If true, the shape must be scaled by the size of the object
        /// that has the shape. Typically, an unit-sized object has width
        /// and height of 1.0.
        /// </summary>
        public abstract bool IsUnitSize { get; }

        /// <summary>
        /// Muodon verteksit sisällään pitävä olio.
        /// </summary>
        public abstract ShapeCache Cache { get; }

        /// <summary>
        /// Ympyrä tai ellipsi.
        /// </summary>
        public static readonly Ellipse Circle = new Ellipse();

        /// <summary>
        /// Ellipsi tai ympyrä.
        /// </summary>
        public static readonly Ellipse Ellipse = new Ellipse();

        /// <summary>
        /// Suorakulmio.
        /// </summary>
        public static readonly Rectangle Rectangle = new Rectangle();

        /// <summary>
        /// Tasasivuinen kolmio.
        /// </summary>
        public static readonly Triangle Triangle = new Triangle();

        /// <summary>
        /// Sydän.
        /// </summary>
        public static readonly Heart Heart = new Heart();

        /// <summary>
        /// Tähti.
        /// </summary>
        public static readonly Star Star = new Star();

        /// <summary>
        /// Timantti- / salmiakkikuvio
        /// </summary>
        public static readonly Shape Diamond = new RegularPolygon(4);

        /// <summary>
        /// Pentagoni eli viisikulmio.
        /// </summary>
        public static readonly Shape Pentagon = new RegularPolygon( 5 );

        /// <summary>
        /// Heksagoni eli kuusikulmio.
        /// </summary>
        public static readonly Shape Hexagon = new RegularPolygon( 6 );

        /// <summary>
        /// Oktagoni eli kahdeksankulmio.
        /// </summary>
        public static readonly Shape Octagon = new RegularPolygon( 8 );

        /// <summary>
        /// Luo kuvion annetusta kuvasta. Kuvassa tulee olla vain yksi
        /// yhtenäinen muoto (toisin sanoen kuvio ei voi koostua monesta osasta).
        /// </summary>
        /// <remarks>
        /// Kuvion luominen voi olla melko hidasta. Kannattaa luoda kuvio heti pelin alussa
        /// ja käyttää kerran luotua kuviota kaikille olioille.
        /// </remarks>
        /// <param name="image">Kuva, josta muoto luetaan.</param>
        public static Shape FromImage(Image image)
        {
            List<Vector> vertices = TextureToShapeConverter.DetectVertices(image.GetDataUInt().Cast<uint>().ToArray(), image.Width);

            for (int i = 0; i < vertices.Count; i++)
            {
                // Nämä voisi yhdistää, mutta pidetään erillisenä luettavuuden takia.
                vertices[i] = new Vector(vertices[i].X / image.Width, vertices[i].Y / image.Height);
                vertices[i] -= new Vector((float)0.5, (float)0.5);
                vertices[i] = new Vector(vertices[i].X , -vertices[i].Y); // Muoto tulee muuten ylösalaisin
            }

            Vector[] polygonVertices = new Vector[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                polygonVertices[i] = new Vector(vertices[i].X, vertices[i].Y);

            ShapeCache cache = new ShapeCache(polygonVertices);
            return new Polygon(cache);
        }

        /// <summary>
        /// Luo muodon merkkijonosta, esim. "Circle"
        /// </summary>
        /// <returns></returns>
        public static Shape FromString( string shapeStr )
        {
#if WINDOWS_STOREAPP
            return typeof( Shape ).GetTypeInfo().GetDeclaredField( shapeStr ).GetValue( null ) as Shape;
#else
            Type shapeClass = typeof( Shape );
            BindingFlags flags = BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static;
            FieldInfo selectedShape = shapeClass.GetField( shapeStr, flags );
            return (Shape)selectedShape.GetValue( null );
#endif
        }

        /// <summary>
        /// Luo säännöllisen monikulmion (polygonin)
        /// </summary>
        /// <param name="vertexCount">Kulmapisteiden määrä (3=kolmio, 4=neliö jne.)</param>
        /// <returns>Monikulmio</returns>
        public static Shape CreateRegularPolygon( int vertexCount )
        {
            if ( vertexCount < 3 ) throw new ArgumentException( "You need at least 3 vertices to create a polygon!" );
            return new RegularPolygon( vertexCount );
        }

        internal static ShapeCache CreateRegularPolygonCache( int vertexCount )
        {
            double angleStep = 2 * Math.PI / vertexCount;
            Int16 centerIndex = (Int16)vertexCount;

            Vector[] vertices = new Vector[vertexCount + 1];
            IndexTriangle[] triangles = new IndexTriangle[vertexCount];
            Int16[] outlineIndices = new Int16[vertexCount];

            for ( int i = 0; i < vertexCount; i++ )
            {
                double a = i * angleStep;
                vertices[i] = new Vector( 0.5 * Math.Cos( a ), 0.5 * Math.Sin( a ) );
                outlineIndices[i] = (Int16)i;
            }
            vertices[centerIndex] = Vector.Zero;

            for ( int i = 0; i < vertexCount - 1; i++ )
            {
                triangles[i] = new IndexTriangle( (Int16)i, centerIndex, (Int16)( i + 1 ) );
            }
            triangles[vertexCount - 1] = new IndexTriangle( (Int16)( vertexCount - 1 ), centerIndex, (Int16)0 );

            return new ShapeCache( vertices, triangles, outlineIndices );
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected static bool SameSide( Vector a, Vector b, Vector p1, Vector p2 )
        {
            double cp1 = Vector.CrossProduct( b - a, p1 - a );
            double cp2 = Vector.CrossProduct( b - a, p2 - a );
            return cp1 * cp2 >= 0;
        }

        protected static bool IsInsideTriangle( Vector p, Vector a, Vector b, Vector c )
        {
            Vector v0 = c - a;
            Vector v1 = b - a;
            Vector v2 = p - a;

            // Dot products between each side
            double dot00 = Vector.DotProduct( v0, v0 );
            double dot01 = Vector.DotProduct( v0, v1 );
            double dot02 = Vector.DotProduct( v0, v2 );
            double dot11 = Vector.DotProduct( v1, v1 );
            double dot12 = Vector.DotProduct( v1, v2 );

            // Barycentric coordinates
            double invDenom = 1 / ( dot00 * dot11 - dot01 * dot01 );
            double u = ( dot11 * dot02 - dot01 * dot12 ) * invDenom;
            double v = ( dot00 * dot12 - dot01 * dot02 ) * invDenom;

            return ( u >= 0 ) && ( v >= 0 ) && ( u + v < 1 );
        }

        // TODO: Benchmark these two methods and use as default that which is faster

        protected bool IsInsideTriangles( Vector p )
        {
            for ( int i = 0; i < Cache.Triangles.Length; i++ )
            {
                Vector t1 = Cache.Vertices[Cache.Triangles[i].i1];
                Vector t2 = Cache.Vertices[Cache.Triangles[i].i2];
                Vector t3 = Cache.Vertices[Cache.Triangles[i].i3];

                if ( IsInsideTriangle( p, t1, t2, t3 ) )
                    return true;
            }

            return false;
        }

        protected bool IsInsideOutlines( Vector p )
        {
            Vector a, b;

            for ( int i = 0; i < Cache.OutlineVertices.Length - 1; i++ )
            {
                a = Cache.OutlineVertices[i];
                b = Cache.OutlineVertices[i + 1];

                if ( !SameSide( a, b, p, Vector.Zero ) )
                    return false;
            }

            a = Cache.OutlineVertices[Cache.OutlineVertices.Length - 1];
            b = Cache.OutlineVertices[0];
            if ( !SameSide( a, b, p, Vector.Zero ) )
                return false;

            return true;
        }

        protected bool IsInsideCircle( double x, double y, double r )
        {
            return x * x + y * y <= r;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Onko piste muodon sisällä.
        /// Pisteen koordinaatiston origo on muodon keskellä.
        /// Muoto on kokoa 1x1 jos IsUnitSize, muuten saman kokoinen kuin olio.
        /// </summary>
        /// <param name="x">X-koordinaatti</param>
        /// <param name="y">Y-koordinaatti</param>
        /// <returns>Onko piste muodon sisällä</returns>
        public virtual bool IsInside( double x, double y )
        {
            if ( Cache != null && Cache.Triangles != null )
            {
                // Use the shape cache triangles
                return IsInsideTriangles( new Vector( x, y ) );
            }
            else if ( Cache != null && Cache.OutlineVertices != null )
            {
                // Use the shape cache outlines
                return IsInsideOutlines( new Vector( x, y ) );
            }
            else
            {
                // Default: use the equation for a circle
                return IsInsideCircle( x, y, 1 );
            }
        }
    }

    /// <summary>
    /// Ympyrä.
    /// </summary>
    public class Ellipse : Shape
    {
        private static ShapeCache _cache = CreateRegularPolygonCache( 64 );

        /// <inheritdoc/>
        public override ShapeCache Cache
        {
            get { return _cache; }
        }

        /// <inheritdoc/>
        public override bool IsUnitSize { get { return true; } }

        internal Ellipse() { }

        /// <inheritdoc/>
        public override bool IsInside( double x, double y )
        {
            return IsInsideCircle( x, y, 1 );
        }
    }

    /// <summary>
    /// Suorakulmio.
    /// </summary>
    public class Rectangle : Shape
    {
        private static readonly Vector[] vertices = new Vector[]
        {
            new Vector( -0.5, -0.5 ),
            new Vector( -0.5, 0.5 ),
            new Vector( 0.5, -0.5 ),
            new Vector( 0.5, 0.5 ),
        };

        private static readonly IndexTriangle[] triangles = new IndexTriangle[]
        {
            new IndexTriangle( 0, 1, 2 ),
            new IndexTriangle( 2, 1, 3 ),
        };

        private static readonly Int16[] outlineIndices = new Int16[]
        {
            2, 3, 1, 0
        };

        private static readonly ShapeCache _cache = new ShapeCache( vertices, triangles, outlineIndices );

        /// <inheritdoc/>
        public override ShapeCache Cache { get { return _cache; } }

        /// <inheritdoc/>
        public override bool IsUnitSize { get { return true; } }

        internal Rectangle() { }

        /// <inheritdoc/>
        public override bool IsInside( double x, double y )
        {
            return ( Math.Abs( x ) <= 1 && Math.Abs( y ) <= 1 );
        }
    }

    /// <summary>
    /// Sydän.
    /// </summary>
    public class Heart : Shape
    {
        private static readonly Vector[] vertices = new Vector[]
        {
            new Vector( 0, -0.5 ),
            new Vector( 0.5, 0.2 ),
            new Vector( 0.4, 0.4 ),
            new Vector( 0.25, 0.5 ),
            new Vector( 0.1, 0.4 ),
            new Vector( 0, 0.2 ),
            new Vector( -0.1, 0.4 ),
            new Vector( -0.25, 0.5 ),
            new Vector( -0.4, 0.4 ),
            new Vector( -0.5, 0.2 )
       };

        private static readonly IndexTriangle[] triangles = new IndexTriangle[]
        {
            new IndexTriangle( 0, 9, 5 ),
            new IndexTriangle( 8, 7, 6 ),
            new IndexTriangle( 8, 6, 5 ),
            new IndexTriangle( 9, 8, 5 ),
            new IndexTriangle( 3, 2, 4 ),
            new IndexTriangle( 4, 2, 5 ),
            new IndexTriangle( 2, 1, 5 ),
            new IndexTriangle( 5, 1, 0 ),
        };

        private static readonly ShapeCache _cache = new ShapeCache( vertices, triangles );

        /// <inheritdoc/>
        public override ShapeCache Cache { get { return _cache; } }

        /// <inheritdoc/>
        public override bool IsUnitSize { get { return true; } }

        internal Heart() { }
    }

    /// <summary>
    /// Tähti.
    /// </summary>
    public class Star : Shape
    {
        private static readonly Vector[] vertices = new Vector[]
        {
            new Vector( -0.5, -0.5 ),
            new Vector( -0.25, 0 ),
            new Vector( 0, -0.16 ),
            new Vector( -0.5, 0.2 ),
            new Vector( -0.15, 0.2 ),
            new Vector( 0, 0.5 ),
            new Vector( 0.15, 0.2 ),
            new Vector( 0.5, 0.2 ),
            new Vector( 0.25, 0 ),
            new Vector( 0.5, -0.5 ),
        };

        private static readonly IndexTriangle[] triangles = new IndexTriangle[]
        {
            new IndexTriangle( 0, 1, 2 ),
            new IndexTriangle( 1, 3, 4 ),
            new IndexTriangle( 1, 5, 8 ),
            new IndexTriangle( 8, 6, 7 ),
            new IndexTriangle( 9, 2, 8 ),
            new IndexTriangle( 1, 8, 2 ),
        };

        private static readonly Int16[] outlineIndices = new Int16[]
        {
            2, 9, 8, 7, 6, 5, 4, 3, 1, 0
        };

        private static readonly ShapeCache _cache = new ShapeCache( vertices, triangles, outlineIndices );

        /// <inheritdoc/>
        public override ShapeCache Cache { get { return _cache; } }

        /// <inheritdoc/>
        public override bool IsUnitSize { get { return true; } }

        internal Star() { }
    }

    /// <summary>
    /// Tasasivuinen kolmio.
    /// </summary>
    public class Triangle : Shape
    {
        private static readonly Vector[] vertices = new Vector[]
        {
            new Vector( 0, 0.5 ),
            new Vector( 0.5, -0.5 ),
            new Vector( -0.5, -0.5 ),
        };

        private static readonly IndexTriangle[] triangles = new IndexTriangle[]
        {
            new IndexTriangle( 0, 1, 2 ),
        };

        private static readonly Int16[] outlineIndices = new Int16[]
        {
            2, 1, 0
        };

        private static readonly ShapeCache _cache = new ShapeCache( vertices, triangles, outlineIndices );

        /// <inheritdoc/>
        public override ShapeCache Cache { get { return _cache; } }

        /// <inheritdoc/>
        public override bool IsUnitSize { get { return true; } }

        internal Triangle() { }

        /// <inheritdoc/>
        public override bool IsInside( double x, double y )
        {
            return IsInsideTriangle( new Vector( x, y ), vertices[0], vertices[1], vertices[2] );
        }
    }

    /// <summary>
    /// Jana.
    /// </summary>
    public class RaySegment : Shape
    {
        /// <inheritdoc/>
        public override ShapeCache Cache
        {
            get { 
                // throw new Exception( "Cache is not defined for RaySegment" ); what is the point of crashing the game?
                return null;
            }
        }

        /// <inheritdoc/>
        public override bool IsUnitSize { get { return false; } }

        /// <summary>
        /// Lähtöpiste
        /// </summary>
        public Vector Origin;

        /// <summary>
        /// Suunta
        /// </summary>
        public Vector Direction;

        /// <summary>
        /// Pituus
        /// </summary>
        public double Length;

        /// <summary>
        /// Säde
        /// </summary>
        /// <param name="origin">Lähtöpiste</param>
        /// <param name="direction">Suunta</param>
        /// <param name="length">Pituus</param>
        public RaySegment( Vector origin, Vector direction, double length )
        {
            this.Origin = origin;
            this.Direction = direction;
            this.Length = length;
        }
    }

    /// <summary>
    /// Monikulmio.
    /// </summary>
    public class Polygon : Shape
    {
        private bool isUnitSize = true;
        private ShapeCache _cache;

        /// <summary>
        /// If true, the shape must be scaled by the size of the object
        /// that has the shape. Typically, an unit-sized object has width
        /// and height of 1.0.
        /// </summary>
        public override bool IsUnitSize 
        {
            get { return isUnitSize; }
        }

        /// <inheritdoc/>
        public override ShapeCache Cache
        {
            get { return _cache; }
        }

        /// <summary>
        /// Monikulmio.
        /// Muodostamiseen kannattaa mielummin käyttää <c>Shape.CreateRegularPolygon</c> -metodia.
        /// </summary>
        /// <param name="cache"></param>
        public Polygon( ShapeCache cache )
            : this( cache, true )
        {
        }

        /// <summary>
        /// Monikulmio.
        /// Muodostamiseen kannattaa mielummin käyttää <c>Shape.CreateRegularPolygon</c> -metodia.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="isUnitSize"></param>
        public Polygon( ShapeCache cache, bool isUnitSize )
        {
            this._cache = cache;
            this.isUnitSize = isUnitSize;
        }
    }

    /// <summary>
    /// Säännöllinen monikulmio.
    /// </summary>
    internal class RegularPolygon : Polygon
    {
        public int CornerCount { get; internal set; }
        public override bool IsUnitSize { get { return true; } }

        public RegularPolygon( int vertexCount )
            : base( CreateRegularPolygonCache( vertexCount ) )
        {
            CornerCount = vertexCount;
        }
    }

    /// <summary>
    /// Muotojen määrityksessä käytettävä kolmio.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public struct IndexTriangle
    {
        /// <summary>
        /// Kulmapisteet.
        /// </summary>
        public Int16 i1, i2, i3;

        /// <summary>
        /// Luo uuden kolmion. Parametreina kulmapisteiden indeksit lueteltuna myötäpäivään.
        /// </summary>
        public IndexTriangle( Int16 i1, Int16 i2, Int16 i3 )
        {
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
        }

        /// <summary>
        /// Luo uuden kolmion. Parametreina kulmapisteiden indeksit lueteltuna myötäpäivään.
        /// </summary>
        public IndexTriangle( int i1, int i2, int i3 )
            : this( (Int16)i1, (Int16)i2, (Int16)i3 )
        {
        }
    }

    /// <summary>
    /// Sisältää valmiiksi lasketut kolmiot, joiden avulla piirtäminen on suoraviivaista.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public class ShapeCache
    {
        // The vertices are in counter-clockwise order
        // because that is what the polygon shape of the physics
        // engine expects.

        /// <summary>
        /// Ulkoreunan verteksit, lueteltuna vastapäivään.
        /// </summary>
        public readonly Vector[] OutlineVertices;

        /// <summary>
        /// Kaikki verteksit, ml. kolmioiden kulmapisteet.
        /// </summary>
        public readonly Vector[] Vertices;

        /// <summary>
        /// Kolmiot, joiden avulla kuvio voidaan täyttää värillä.
        /// </summary>
        public readonly IndexTriangle[] Triangles;

        /// <summary>
        /// Luo kuvion kolmioilla, joiden avulla kuvio voidaan täyttää värillä. Kaikkien
        /// verteksien tulee olla kuvion ulkoreunalla.
        /// </summary>
        /// <param name="outlineVertices">Ulkoreunan verteksit, lueteltuna vastapäivään.</param>
        /// <param name="triangles">Kolmiot.</param>
        public ShapeCache( Vector[] outlineVertices, IndexTriangle[] triangles )
        {
            this.Vertices = this.OutlineVertices = outlineVertices;
            Triangles = triangles;
        }

        /// <summary>
        /// Luo kuvion pelkillä reuna-vertekseillä. Kuviolle ei tule tietoa kolmioista,
        /// näin ollen sitä ei voi täyttää värillä.
        /// </summary>
        /// <param name="outlineVertices">Ulkoreunan verteksit, lueteltuna vastapäivään.</param>
        public ShapeCache( Vector[] outlineVertices )
        {
            this.Vertices = this.OutlineVertices = outlineVertices;
            this.Triangles = null;
        }

        /// <summary>
        /// Luo kuvion, joka voidaan piirtää täytettynä värillä.
        /// </summary>
        /// <param name="vertices">Kaikki verteksit (ml. reunan verteksit sekä kolmioiden verteksit).</param>
        /// <param name="triangles">Kolmiot, joista kuvio koostuu.</param>
        /// <param name="outlineIndices">Ulkoreunan verteksit lueteltuna vastapäivään, indekseinä <c>vertices</c>-taulukkoon.</param>
        public ShapeCache( Vector[] vertices, IndexTriangle[] triangles, Int16[] outlineIndices )
        {
            Vertices = vertices;
            Triangles = triangles;

            OutlineVertices = new Vector[outlineIndices.Length];
            for ( int i = 0; i < outlineIndices.Length; i++ )
            {
                OutlineVertices[i] = vertices[outlineIndices[i]];
            }
        }
    }

    /// <summary>
    /// Tekstuuribittikartta muotojen luomiseen tekstuureista.
    /// Sisältää tekstuurin tiedot väritaulukkona.
    /// </summary>
    internal class TextureBitmap : IBitmap
    {
        /// <summary>
        /// Bittikarttadata.
        /// </summary>
        protected bool[,] bitmap;

        /// <summary>
        /// Luo uuden bittikartan tekstuurin pohjalta.
        /// </summary>
        /// <param name="texture">Tekstuuri.</param>
        /// <param name="isOpaque">Predikaatti, joka määrää, onko annettu väri läpinäkyvä.</param>
        public TextureBitmap( Image texture, Predicate<Color> isOpaque )
        {
            /*Color[] scanline = new Color[texture.Width];
            XnaRectangle srcRect = new XnaRectangle( 0, 0, texture.Width, 1 );

            bitmap = new bool[texture.Width, texture.Height];

            for ( int i = 0; i < texture.Height; i++ )
            {
                // Scan a line from the texture
                srcRect.Y = i;
                texture.GetData<Color>( 0, srcRect, scanline, 0, texture.Width );

                for ( int j = 0; j < texture.Width; j++ )
                {
                    // Flip the y-coordinates because the y-coordinates of the texture
                    // increase downwards.
                    bitmap[j, texture.Height - i - 1] = isOpaque( scanline[j] );
                }
            }*/
        }

        /// <summary>
        /// Luo uuden bittikartan tekstuurin pohjalta oletusläpinäkyvyysehdoilla.
        /// Ks. <see cref="IsOpaqueColor"/>
        /// </summary>
        /// <param name="texture">Tekstuuri.</param>
        public TextureBitmap( Image texture )
            : this( texture, IsOpaqueColor )
        {
        }

        /// <summary>
        /// Bittikartan leveys pikseleinä.
        /// </summary>
        public int Width
        {
            get { return bitmap.GetLength( 0 ); }
        }

        /// <summary>
        /// Bittikartan korkeus pikseleinä.
        /// </summary>
        public int Height
        {
            get { return bitmap.GetLength( 1 ); }
        }

        /// <summary>
        /// Palauttaa yksittäisen pikselin värin annetusta koordinaattipisteestä (ensin x, sitten y).
        /// </summary>
        public bool this[int x, int y]
        {
            get
            {
                if ( x < 0 || y < 0 || x >= Width || y >= Height ) { return false; }
                return bitmap[x, y];
            }
        }

        /// <summary>
        /// Päättelee pikselin läpinäkyvyyden sen värin perusteella.
        /// Tässä tapauksessa alfa-arvon tulee olla suurempi tai yhtäsuuri kuin 127.
        /// </summary>
        /// <param name="c">Pikselin väri.</param>        
        /// <returns>Läpinäkyvyys.</returns>
        public static bool IsOpaqueColor( Color c )
        {
            return c.AlphaComponent >= 127;
        }
    }
}
