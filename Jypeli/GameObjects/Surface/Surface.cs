using System;

namespace Jypeli
{
    /// <summary>
    /// Tasainen tai epätasainen pinta.
    /// </summary>
    public class Surface : PhysicsObject
    {
        double[] heights;
        double scale = 1.0;

        #region Constructors

        /// <summary>
        /// Helppo tapa lisätä kenttään epätasainen pinta.
        /// Pinta kuvataan luettelemalla Y-koordinaatteja vasemmalta oikealle lukien. Kahden Y-koordinaatin
        /// väli on aina sama.
        /// </summary>
        /// <param name="width">Pinnan leveys</param>
        /// <param name="heights">Y-koordinaatit lueteltuna vasemmalta oikealle.</param>
        /// <param name="scale">Vakio, jolla jokainen Y-koordinaatti kerrotaan. Hyödyllinen,
        /// jos halutaan muuttaa koko pinnan korkeutta muuttamatta jokaista pistettä yksitellen.
        /// Tavallisesti arvoksi kelpaa 1.0.</param>
        /// <remarks>
        /// Huomaa, että pinnassa ei voi olla kahta pistettä päällekkäin.
        /// </remarks>
        public Surface( double width, double[] heights, double scale )
            : base( width, CalculateHeight( heights, scale ), CreateRuggedShape( width, heights, scale ) )
        {
            InitializeRugged( heights, scale );
        }

        /// <summary>
        /// Luo tasaisen pinnan.
        /// </summary>
        /// <param name="width">Pinnan leveys</param>
        /// <param name="height">Pinnan korkeus</param>
        public Surface( double width, double height )
            : base( width, height, Shape.Rectangle )
        {
            InitializeFlat( height );
        }

        private void InitializeFlat( double height )
        {
            heights = new double[] { height };
            scale = 1.0;

            Color = Color.ForestGreen;
            TextureFillsShape = true;
            MakeStatic();
        }

        private void InitializeRugged( double[] heights, double scale )
        {
            this.heights = heights;
            this.scale = scale;
            Color = Color.ForestGreen;
            TextureFillsShape = true;
            MakeStatic();
        }

        /// <summary>
        /// Luo satunnaisen pinnan.
        /// </summary>
        /// <param name="width">Pinnan leveys</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <returns></returns>
        public Surface( double width, double min, double max, int points )
            : this( width, RandomGen.NextDoubleArray( Math.Max( min, 1.0f ), max, points ), 1.0 )
        {
        }

        /// <summary>
        /// Luo satunnaisen pinnan.
        /// </summary>
        /// <param name="width">Pinnan leveys</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="maxchange">Suurin sallittu erotus kahden pisteen välillä.</param>
        /// <returns></returns>
        public Surface( double width, double min, double max, int points, int maxchange )
            : this( width, RandomGen.NextDoubleArray( Math.Max( min, 1.0f ), max, points, maxchange ), 1.0 )
        {
        }
        
        #endregion

        #region Static private methods for constructors

        private static double CalculateHeight( double[] heights, double scale )
        {
            if ( heights.Length < 2 )
                throw new Exception( "At least two Y-points needed in order to create ground" );
            if ( heights.Min() < 1 )
                throw new Exception( "The heights must be positive and at least 1" ); // JN: doesn't work well with 0 values (slow and memory usage problems)

            return heights.Max() * scale;
        }

        private static Vector[] CalculateVertexes( double width, double[] heights, double scale )
        {
            int n = heights.Length;
            double step = width / ( n - 1 );
            double maxHeight = heights.Max() * scale;

            // Each point should be adjusted by this amount in order to get the center of the shape at (0, 0)
            // This way, drawing a single texture on top of it covers the whole shape.
            double halfHeight = maxHeight / 2;

            Vector[] vertexes = new Vector[n * 2];

            // Let's start from the bottom right corner and head clockwise from there.
            Vector bottomRight = new Vector( width / 2, -halfHeight );

            // Bottom vertexes, right to left
            for ( int i = 0; i < n; i++ )
            {
                vertexes[i] = new Vector( bottomRight.X - ( i * step ), bottomRight.Y );
            }

            double left = -width / 2;

            // Top vertexes, left to right
            for ( int i = 0; i < n; i++ )
            {
                vertexes[n + i] = new Vector( left + ( i * step ), heights[i] * scale - halfHeight );
            }

            return vertexes;
        }

        private static Polygon CreateRuggedShape( double width, double[] heights, double scale )
        {
            Vector[] vertexes = CalculateVertexes( width, heights, scale );
            return CreateShape( width, heights, vertexes );
        }

        private static Polygon CreateShape( double width, double[] heights, Vector[] vertexes )
        {
            int n = heights.Length;
            IndexTriangle[] triangles = new IndexTriangle[( n - 1 ) * 2];
            Int16[] outlineIndices = new Int16[n * 2];

            for ( int i = 0; i < n * 2; i++ )
                outlineIndices[i] = (Int16)i;
            for ( int i = 0; i < n - 1; i++ )
            {
                triangles[2 * i] = new IndexTriangle( i, 2 * n - i - 2, 2 * n - i - 1 );
                triangles[2 * i + 1] = new IndexTriangle( i, i + 1, 2 * n - i - 2 );
            }

            Vector[] outlineVertices = new Vector[outlineIndices.Length];
            for ( int i = 0; i < outlineIndices.Length; i++ )
                outlineVertices[i] = vertexes[outlineIndices[i]];

            return new Polygon( new ShapeCache( vertexes, triangles, outlineIndices ), false );
        }

        private static double GetMinHeightDifference( double[] heights )
        {
            int n = heights.Length;
            double minHeightDifference = double.PositiveInfinity;

            for ( int i = 0; i < n - 1; i++ )
            {
                double diff = Math.Abs( heights[i + 1] - heights[i] );
                if ( diff > double.Epsilon && diff < minHeightDifference ) minHeightDifference = diff;
            }

            return minHeightDifference;
        }
                
        #endregion

        #region Factory methods

        /// <summary>
        /// Luo kentälle tasaisen reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="direction">Mikä reuna luodaan.</param>
        /// <returns>Reunaolio</returns>
        public static Surface Create( Level level, Direction direction )
        {
            if ( direction == Direction.Left ) return CreateLeft( level );
            if ( direction == Direction.Right ) return CreateRight( level );
            if ( direction == Direction.Up ) return CreateTop( level );
            if ( direction == Direction.Down ) return CreateBottom( level );

            return null;
        }

        /// <summary>
        /// Luo kentälle epätasaisen reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="direction">Mikä reuna luodaan.</param>
        /// <returns>Reunaolio</returns>
        public static Surface Create( Level level, Direction direction, double min, double max, int points )
        {
            if ( direction == Direction.Left ) return CreateLeft( level, min, max, points );
            if ( direction == Direction.Right ) return CreateRight( level, min, max, points );
            if ( direction == Direction.Up ) return CreateTop( level, min, max, points );
            if ( direction == Direction.Down ) return CreateBottom( level, min, max, points );

            return null;
        }

        /// <summary>
        /// Luo kentälle epätasaisen reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="maxchange">Suurin sallittu erotus kahden pisteen välillä.</param>
        /// <param name="direction">Mikä reuna luodaan.</param>
        /// <returns>Reunaolio</returns>
        public static Surface Create( Level level, Direction direction, double min, double max, int points, int maxchange )
        {
            if ( direction == Direction.Left ) return CreateLeft( level, min, max, points, maxchange );
            if ( direction == Direction.Right ) return CreateRight( level, min, max, points, maxchange );
            if ( direction == Direction.Up ) return CreateTop( level, min, max, points, maxchange );
            if ( direction == Direction.Down ) return CreateBottom( level, min, max, points, maxchange );

            return null;
        }

        /// <summary>
        /// Luo kentälle tasaisen vasemman reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateLeft( Level level )
        {
            double thickness = level.GetBorderThickness();
            Surface ground = new Surface( level.Height, thickness );
            ground.Angle = -Angle.RightAngle;
            ground.Position = new Vector( level.Left - ( thickness / 2 ), level.Center.Y );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen vasemman reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateLeft( Level level, double min, double max, int points )
        {
            Surface ground = new Surface( level.Height + 2 * max, min, max, points );
            ground.Angle = -Angle.RightAngle;
            ground.Position = new Vector( level.Left - ( max / 2 ), level.Center.Y );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen vasemman reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="maxchange">Suurin sallittu erotus kahden pisteen välillä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateLeft( Level level, double min, double max, int points, int maxchange )
        {
            Surface ground = new Surface( level.Height + 2 * max, min, max, points, maxchange );
            ground.Angle = -Angle.RightAngle;
            ground.Position = new Vector( level.Left - ( max / 2 ), level.Center.Y );
            return ground;
        }

        /// <summary>
        /// Luo kentälle tasaisen oikean reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateRight( Level level )
        {
            double thickness = level.GetBorderThickness();
            Surface ground = new Surface( level.Height, thickness );
            ground.Angle = Angle.RightAngle;
            ground.Position = new Vector( level.Right + ( thickness / 2 ), level.Center.Y );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen oikean reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="level">Kenttä</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateRight( Level level, double min, double max, int points )
        {
            Surface ground = new Surface( level.Height + 2 * max, min, max, points );
            ground.Angle = Angle.RightAngle;
            ground.Position = new Vector( level.Right + ( max / 2 ), level.Center.Y );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen oikean reunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="level">Kenttä</param>
        /// <param name="maxchange">Suurin sallittu erotus kahden pisteen välillä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateRight( Level level, double min, double max, int points, int maxchange )
        {
            Surface ground = new Surface( level.Height + 2 * max, min, max, points, maxchange );
            ground.Angle = Angle.RightAngle;
            ground.Position = new Vector( level.Right + ( max / 2 ), level.Center.Y );
            return ground;
        }

        /// <summary>
        /// Luo kentälle tasaisen yläreunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateTop( Level level )
        {
            double thickness = level.GetBorderThickness();
            Surface ground = new Surface( level.Width + ( 2 * thickness ), thickness );
            ground.Angle = Angle.StraightAngle;
            ground.Position = new Vector( level.Center.X, level.Top + ( thickness / 2 ) );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen yläreunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateTop( Level level, double min, double max, int points )
        {
            Surface ground = new Surface( level.Width + 2 * max, min, max, points );
            ground.Angle = Angle.StraightAngle;
            ground.Position = new Vector( level.Center.X, level.Top + ( max / 2 ) );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen yläreunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="maxchange">Suurin sallittu erotus kahden pisteen välillä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateTop( Level level, double min, double max, int points, int maxchange )
        {
            Surface ground = new Surface( level.Width + 2 * max, min, max, points, maxchange );
            ground.Angle = Angle.StraightAngle;
            ground.Position = new Vector( level.Center.X, level.Top + ( max / 2 ) );
            return ground;
        }

        /// <summary>
        /// Luo kentälle tasaisen alareunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateBottom( Level level )
        {
            double thickness = level.GetBorderThickness();
            Surface ground = new Surface( level.Width + ( 2 * thickness ), thickness );
            ground.Position = new Vector( level.Center.X, level.Bottom - ( thickness / 2 ) );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen alareunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateBottom( Level level, double min, double max, int points )
        {
            Surface ground = new Surface( level.Width + 2 * max, min, max, points );
            ground.Position = new Vector( level.Center.X, level.Bottom - ( max / 2 ) );
            return ground;
        }

        /// <summary>
        /// Luo kentälle epätasaisen alareunan.
        /// Ei lisää reunaa automaattisesti kenttään.
        /// </summary>
        /// <param name="level">Kenttä</param>
        /// <param name="min">Matalin kohta.</param>
        /// <param name="max">Korkein kohta.</param>
        /// <param name="points">Pisteiden määrä.</param>
        /// <param name="maxchange">Suurin sallittu erotus kahden pisteen välillä.</param>
        /// <returns>Reunaolio</returns>
        public static Surface CreateBottom( Level level, double min, double max, int points, int maxchange )
        {
            Surface ground = new Surface( level.Width + 2 * max, min, max, points, maxchange );
            ground.Position = new Vector( level.Center.X, level.Bottom - ( max / 2 ) );
            return ground;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Maanpinnan korkeus annetussa x-koordinaatissa
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetGroundHeight( double x )
        {
            if ( heights == null || x < Left || x > Right ) return Top;

            int n = heights.Length;
            double step = Width / ( n - 1 );

            double indexX = ( Width / 2 + x ) / step;
            int lowerIndex = (int)Math.Floor( indexX );
            int upperIndex = (int)Math.Ceiling( indexX );

            if ( upperIndex >= n ) return Top; // DEBUG
            if ( lowerIndex == upperIndex ) return Bottom + scale * heights[lowerIndex];

            double k = ( heights[upperIndex] - heights[lowerIndex] ) / step;
            double relX = ( Width / 2 + x ) % step;

            return Bottom + heights[lowerIndex] + relX * k;
        }

        /// <summary>
        /// Maanpinnan normaalivektori annetulla x-koordinaatilla
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public Vector GetGroundNormal( double x )
        {
            if ( heights == null || x < Left || x > Right ) return Vector.UnitY;

            int n = heights.Length;
            double step = Width / ( n - 1 );

            double indexX = ( Width / 2 + x ) / step;
            int lowerIndex = (int)Math.Floor( indexX );
            int upperIndex = (int)Math.Ceiling( indexX );

            if ( upperIndex >= n ) return Vector.UnitY; // DEBUG
            if ( lowerIndex == upperIndex )
            {
                return ( GetGroundNormal( x - step / 2 ) + GetGroundNormal( x + step / 2 ) ) / 2;
            }

            double k = ( heights[upperIndex] - heights[lowerIndex] ) / step;
            return Vector.FromLengthAndAngle( 1, Angle.ArcTan( k ) + Angle.RightAngle );
        }

        #endregion
    }
}
