using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Kappaleen kuvion laatu t�rm�yksentunnistuksessa.
    /// </summary>
    [Obsolete( "Use CollisionShapeParameters or the PhysicsTemplates class." )]
    public struct CollisionShapeQuality
    {
        /// <summary>
        /// Laatuarvo.
        /// </summary>
        public double Value;

        internal CollisionShapeQuality( double value )
        {
            this.Value = value;
        }

        /// <summary>
        /// Alustaa uuden laatuattribuutin.
        /// </summary>
        /// <param name="value">Lukuarvo v�lill� <c>0.0</c> (huonoin) ja <c>1.0</c> (paras).</param>
        public static CollisionShapeQuality FromValue( double value )
        {
            if ( value < 0.0 || 1.0 < value )
                throw new ArgumentException( "The value must be between 0.0 and 1.0." );
            return new CollisionShapeQuality( value );
        }

        /// <summary>
        /// Huonoin mahdollinen laatu, nopea mutta ep�tarkka.
        /// </summary>
        public static readonly CollisionShapeQuality Worst = FromValue( 0.0 );

        /// <summary>
        /// V�ltt�v� laatu.
        /// </summary>
        public static readonly CollisionShapeQuality Tolerable = FromValue( 0.3 );

        /// <summary>
        /// Hyv� laatu.
        /// </summary>
        public static readonly CollisionShapeQuality Good = FromValue( 0.5 );

        /// <summary>
        /// Paras mahdollinen laatu, tarkka mutta hidas.
        /// </summary>
        public static readonly CollisionShapeQuality Best = FromValue( 1.0 );

        internal static readonly CollisionShapeQuality Unspecified = new CollisionShapeQuality( -1.0 );

        internal bool IsUnspecified
        {
            get { return ( Value + double.Epsilon ) < 0; }
        }
    }
}
