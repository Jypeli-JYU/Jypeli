using System;
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
                Vector diff = value - Position;
                Body.Position = value;

                Objects?.ForEach(o => {
                    o.Position += diff;
                });

                // TODO: Purkkapallokorjaus, SynchronousListin kappalaiden lisäys pitäisi saada hieman yksinkertaisemmaksi.
                foreach (var o in Objects?.GetObjectsAboutToBeAdded())
                {
                    o.Position += diff;
                }
                prevPos += diff;
            }
        }

        /// <inheritdoc/>
        public override Angle Angle
        {
            get { return Angle.FromRadians(Body.Angle); }
            set 
            {
                UpdateChildrenPos();
                Angle diff = value - Angle;
                Body.Angle = value.Radians;

                Objects?.ForEach(o => {
                    o.Angle += diff;
                    Vector vdiff = o.Position - Position;
                    o.Position += -vdiff + Vector.FromLengthAndAngle(vdiff.Magnitude, diff + vdiff.Angle);
                });

                // TODO: Purkkapallokorjaus, SynchronousListin kappalaiden lisäys pitäisi saada hieman yksinkertaisemmaksi.
                foreach (var o in Objects?.GetObjectsAboutToBeAdded())
                {
                    o.Angle += diff;
                    Vector vdiff = o.Position - Position;
                    o.Position += -vdiff + Vector.FromLengthAndAngle(vdiff.Magnitude, diff + vdiff.Angle);
                }
                prevAngle += diff;
            }
        }

        /// <inheritdoc/>
        public override Vector Size
        {
            get { return Body.Size; }
            set 
            {
                if (IsDestroyed)
                    throw new InvalidOperationException("Object is already destroyed");
                if (value.X < 0 || value.Y < 0)
                    throw new ArgumentException("The size must be positive!");
                if(IsAddedToGame)
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
                if (IsDestroyed)
                    throw new InvalidOperationException("Object is already destroyed");
                Body.Shape = value;
                if (Parent != null)
                    Body.RegenerateConnectedFixtures();
            }
        }
    }
}
