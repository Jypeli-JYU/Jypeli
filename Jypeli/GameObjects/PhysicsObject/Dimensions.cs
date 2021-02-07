﻿using Jypeli.Physics;

namespace Jypeli
{
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        private BoundingRectangle _bRect = new BoundingRectangle();

        [Save]
        public IPhysicsBody Body { get; private set; }

        /// <summary>
        /// Olion sisältävä laatikko törmäyskäsittelyä varten.
        /// </summary>
        /// <value>The bounding rectangle.</value>
        public BoundingRectangle BoundingRectangle
        {
            get
            {
                _bRect.Position = this.Position;
                _bRect.Size = this.Size;
                return _bRect;
            }
        }

        public override Vector Position
        {
            get { return Body.Position; }
            set 
            {
                if (Parent != null)
                {
                    _prevRelPos += value - Position;
                    Body.Position = value;
                    Body.RegenerateConnectedFixtures();
                }
                else
                {
                    Body.Position = value;
                }
            }
        }

        [Save]
        public override Angle Angle
        {
            get { return Angle.FromRadians( Body.Angle ); }
            set 
            {
                if (Parent != null)
                {
                    _prevRelAngle += value - Angle;
                    Body.Angle = value.Radians;
                    Body.RegenerateConnectedFixtures();
                }
                else
                {
                    Body.Angle = value.Radians;
                }
            }
        }

        [Save]
        public override Vector Size
        {
            get { return (Vector)Body.Size; }
            set 
            { 
                Body.Size = value;
                /*if (Parent != null) // TODO: tästä tulee stackoverflow
                    Body.RegenerateConnectedFixtures();*/
            }
        }

        [Save]
        public override Shape Shape
        {
            get { return Body.Shape; }
            set 
            {
                Body.Shape = value;
                if (Parent != null)
                    Body.RegenerateConnectedFixtures();
            }
        }
    }
}
