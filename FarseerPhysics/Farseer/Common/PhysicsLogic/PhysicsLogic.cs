using System;
using FarseerPhysics.Dynamics;


namespace FarseerPhysics.Common.PhysicsLogic
{
    public abstract class PhysicsLogic : FilterData
    {
        public ControllerCategory ControllerCategory = ControllerCategory.Cat01;

        public World World { get; internal set; }

        public PhysicsLogic(World world)
        {
            World = world;
        }
        public override bool IsActiveOn(Body body)
        {
            if (body.ControllerFilter.IsControllerIgnored(ControllerCategory))
                return false;

            return base.IsActiveOn(body);
        }

    }
}