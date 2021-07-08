using Jypeli.Physics;

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

        /// <inheritdoc/>
        public override Vector Position
        {
            get { return Body.Position; }
            set 
            {
                Body.Position = value;
            }
        }

        /// <inheritdoc/>
        public override Angle Angle
        {
            get { return Angle.FromRadians(Body.Angle); }
            set 
            {
                Body.Angle = value.Radians;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
