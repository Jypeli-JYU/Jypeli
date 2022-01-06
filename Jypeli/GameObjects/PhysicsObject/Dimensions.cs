using Jypeli.Physics;

namespace Jypeli
{
    public partial class PhysicsObject
    {
        private BoundingRectangle bRect = new BoundingRectangle();

        /// <summary>
        /// Fysiikkamoottorin käsittelemä fysiikkakappale.
        /// </summary>
        public IPhysicsBody Body { get; private set; }

        /// <summary>
        /// Olion sisältävä laatikko törmäyskäsittelyä varten.
        /// </summary>
        /// <value>The bounding rectangle.</value>
        public BoundingRectangle BoundingRectangle
        {
            get
            {
                bRect.Position = Position;
                bRect.Size = Size;
                return bRect;
            }
        }

        /// <inheritdoc/>
        public override Vector Position
        {
            get { return Body.Position; }
            set 
            {
                Body.Position = value;

                Objects?.ForEach(o => {
                    o.RelativePositionToMainParent = o.InitialRelativePosition;
                    o.RelativeAngleToMainParent = o.InitialRelativeAngle;
                });

                // TODO: Purkkapallokorjaus, SynchronousListin kappalaiden lisäys pitäisi saada hieman yksinkertaisemmaksi.
                foreach (var o in Objects?.GetObjectsAboutToBeAdded())
                {
                    o.RelativePositionToMainParent = o.InitialRelativePosition;
                    o.RelativeAngleToMainParent = o.InitialRelativeAngle;
                }

                if (Parent != null)
                    InitialRelativePosition = RelativePositionToMainParent;
            }
        }

        /// <inheritdoc/>
        public override Angle Angle
        {
            get { return Angle.FromRadians(Body.Angle); }
            set 
            {
                Body.Angle = value.Radians;

                Objects?.ForEach(o => {
                    o.RelativePositionToMainParent = o.InitialRelativePosition;
                    o.RelativeAngleToMainParent = o.InitialRelativeAngle;
                });

                // TODO: Purkkapallokorjaus, SynchronousListin kappalaiden lisäys pitäisi saada hieman yksinkertaisemmaksi.
                foreach (var o in Objects?.GetObjectsAboutToBeAdded())
                {
                    o.RelativePositionToMainParent = o.InitialRelativePosition;
                    o.RelativeAngleToMainParent = o.InitialRelativeAngle;
                }

                if (Parent != null)
                    InitialRelativeAngle = RelativeAngleToMainParent;
            }
        }

        /// <inheritdoc/>
        public override Vector Size
        {
            get { return Body.Size; }
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
