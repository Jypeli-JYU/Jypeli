
using System;

namespace Jypeli
{
    public class Gesture : Touch
    {
        Vector _position2;
        Vector _movement2;

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
                return ScreenView.FromDisplayCoords(_position).Transform(Game.Screen.GetScreenTransform());
            }
        }

        /// <summary>
        /// Kosketuksen paikka pelimaailmassa.
        /// </summary>
        public Vector PositionOnWorld2
        {
            get
            {
                return Game.Instance.Camera.ScreenToWorld(PositionOnScreen2);
            }
        }

        /// <summary>
        /// Kosketuksen liike ruudulla.
        /// </summary>
        public Vector MovementOnScreen2
        {
            get
            {
                return new Vector((double)_movement2.X, -(double)_movement2.Y);
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
                return (PositionOnWorld2 + MovementOnWorld2) - (PositionOnWorld + MovementOnWorld);
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
                return (PositionOnScreen2 + MovementOnScreen2) - (PositionOnScreen + MovementOnScreen);
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

        internal Gesture(GestureType type, Vector position, Vector movement, Vector position2, Vector movement2)
            : base(position, movement)
        {
            this.GestureType = type;
            this._position2 = position2;
            this._movement2 = movement2;
        }

        public Gesture(GestureSample sample)
            : this(sample.GestureType, sample.Position, sample.Delta, sample.Position2, sample.Delta2)
        {
        }
    }

    public enum GestureType
    {
        /// <summary>
        /// No gestures.
        /// </summary>
        None = 0,
        /// <summary>
        /// The user touched a single point.
        /// </summary>
        Tap = 1,
        /// <summary>
        /// States completion of a drag gesture(VerticalDrag, HorizontalDrag, or FreeDrag).
        /// </summary>
        /// <remarks>No position or delta information is available for this sample.</remarks>
		DragComplete = 2,
        /// <summary>
        /// States that a touch was combined with a quick swipe.
        /// </summary>    
        /// <remarks>Flicks does not contain position information. The velocity of it can be read from <see cref="GestureSample.Delta"/></remarks>
        Flick = 4,
        /// <summary>
        /// The use touched a point and then performed a free-form drag.
        /// </summary>
        FreeDrag = 8,
        /// <summary>        
        /// The use touched a single point for approximately one second.
        /// </summary>
        /// <remarks>As this is a single event, it will not be contionusly fired while the user is holding the touch-point.</remarks>
        Hold = 16,
        /// <summary>
        /// The user touched the screen and performed either left to right or right to left drag gesture.
        /// </summary>
        HorizontalDrag = 32,
        /// <summary>
        /// The user either converged or diverged two touch-points on the screen which is like a two-finger drag.
        /// </summary>
        /// <remarks>When this gesture-type is enabled and two fingers are down, it takes precedence over drag gestures.</remarks>
        Pinch = 64,
        /// <summary>
        /// An in-progress pinch operation was completed.
        /// </summary>
        /// <remarks>No position or delta information is available for this sample.</remarks>
        PinchComplete = 128,
        /// <summary>
        /// The user tapped the device twice which is always preceded by a Tap gesture.
        /// </summary>
        /// <remarks>If the time between two touchs are long enough, insted two seperate single Tap gestures will be generated.</remarks>
        DoubleTap = 256,
        /// <summary>
        /// The user touched the screen and performed either top to bottom or bottom to top drag gesture.
        /// </summary>
        VerticalDrag = 512,
    }

    /// <summary>
    /// Represents data from a multi-touch gesture over a span of time.
    /// </summary>
    public struct GestureSample
    {
        // attributes
        private GestureType _gestureType;
        private TimeSpan _timestamp;
        private Vector _position;
        private Vector _position2;
        private Vector _delta;
        private Vector _delta2;

        #region Properties

        /// <summary>
        /// Gets the type of the gesture.
        /// </summary>
        public GestureType GestureType
        {
            get
            {
                return this._gestureType;
            }
        }

        /// <summary>
        /// Gets the starting time for this multi-touch gesture sample.
        /// </summary>
        public TimeSpan Timestamp
        {
            get
            {
                return this._timestamp;
            }
        }

        /// <summary>
        /// Gets the position of the first touch-point in the gesture sample.
        /// </summary>
        public Vector Position
        {
            get
            {
                return this._position;
            }
        }

        /// <summary>
        /// Gets the position of the second touch-point in the gesture sample.
        /// </summary>
        public Vector Position2
        {
            get
            {
                return this._position2;
            }
        }

        /// <summary>
        /// Gets the delta information for the first touch-point in the gesture sample.
        /// </summary>
        public Vector Delta
        {
            get
            {
                return this._delta;
            }
        }

        /// <summary>
        /// Gets the delta information for the second touch-point in the gesture sample.
        /// </summary>
        public Vector Delta2
        {
            get
            {
                return this._delta2;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new <see cref="GestureSample"/>.
        /// </summary>
        /// <param name="gestureType"><see cref="GestureType"/></param>
        /// <param name="timestamp"></param>
        /// <param name="position"></param>
        /// <param name="position2"></param>
        /// <param name="delta"></param>
        /// <param name="delta2"></param>
        public GestureSample(GestureType gestureType, TimeSpan timestamp, Vector position, Vector position2, Vector delta, Vector delta2)
        {
            this._gestureType = gestureType;
            this._timestamp = timestamp;
            this._position = position;
            this._position2 = position2;
            this._delta = delta;
            this._delta2 = delta2;
        }
    }
}
