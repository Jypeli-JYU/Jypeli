using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Jypeli
{
    public class Gesture : Touch
    {
        Vector2 _position2;
        Vector2 _movement2;

        /// <summary>
        /// Kosketuseleen tyyppi.
        /// </summary>
        public GestureType GestureType { get; private set; }

        /// <summary>
        /// Kosketuksen paikka ruudulla.
        /// </summary>
        public Vector PositionOnScreen2
        {
            get
            {
                return ScreenView.FromXnaCoords( _position, Game.Screen.ViewportSize, Vector.Zero ).Transform( Game.Screen.GetScreenTransform() );
            }
        }

        /// <summary>
        /// Kosketuksen paikka pelimaailmassa.
        /// </summary>
        public Vector PositionOnWorld2
        {
            get
            {
                return Game.Instance.Camera.ScreenToWorld( PositionOnScreen2 );
            }
        }

        /// <summary>
        /// Kosketuksen liike ruudulla.
        /// </summary>
        public Vector MovementOnScreen2
        {
            get
            {
                return new Vector( (double)_movement2.X, -(double)_movement2.Y );
            }
        }

        /// <summary>
        /// Kosketuksen liike pelimaailmassa.
        /// </summary>
        public Vector MovementOnWorld2
        {
            get
            {
                return MovementOnScreen2 / Game.Instance.Camera.ZoomFactor;
            }
        }

        /// <summary>
        /// Sormien etäisyysvektori maailmassa eleen alussa.
        /// </summary>
        public Vector WorldDistanceBefore
        {
            get
            {
                return PositionOnWorld2 - PositionOnWorld;
            }
        }

        /// <summary>
        /// Sormien etäisyysvektori maailmassa eleen lopussa.
        /// </summary>
        public Vector WorldDistanceAfter
        {
            get
            {
                return ( PositionOnWorld2 + MovementOnWorld2 ) - ( PositionOnWorld + MovementOnWorld );
            }
        }

        /// <summary>
        /// Sormien etäisyysvektori ruudulla eleen alussa.
        /// </summary>
        public Vector ScreenDistanceBefore
        {
            get
            {
                return PositionOnScreen2 - PositionOnScreen;
            }
        }

        /// <summary>
        /// Sormien etäisyysvektori ruudulla eleen lopussa.
        /// </summary>
        public Vector ScreenDistanceAfter
        {
            get
            {
                return ( PositionOnScreen2 + MovementOnScreen2 ) - ( PositionOnScreen + MovementOnScreen );
            }
        }

        /// <summary>
        /// Kiertokulma.
        /// </summary>
        public Angle Rotation
        {
            get
            {
                return ScreenDistanceAfter.Angle - ScreenDistanceBefore.Angle;
            }
        }

        internal Gesture( GestureType type, Vector2 position, Vector2 movement, Vector2 position2, Vector2 movement2 )
            : base( position, movement )
        {
            this.GestureType = type;
            this._position2 = position2;
            this._movement2 = movement2;
        }

        public Gesture( GestureSample sample )
            : this( sample.GestureType, sample.Position, sample.Delta, sample.Position2, sample.Delta2 )
        {
        }
    }
}
