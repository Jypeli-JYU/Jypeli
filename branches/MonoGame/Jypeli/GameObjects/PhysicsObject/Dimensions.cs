using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Physics;

namespace Jypeli
{
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        [Save]
        public IPhysicsBody Body { get; private set; }

        public override Vector Position
        {
            get { return Body.Position; }
            set { Body.Position = value; }
        }

        [Save]
        public override Angle Angle
        {
            get { return Angle.FromRadians( Body.Angle ); }
            set { Body.Angle = value.Radians; }
        }

        [Save]
        public override Vector Size
        {
            get { return (Vector)Body.Size; }
            set { Body.Size = value; }
        }

        [Save]
        public override Shape Shape
        {
            get { return Body.Shape; }
            set { Body.Shape = value; }
        }
    }
}
