using System;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Dynamics;


namespace FarseerPhysics.Controllers
{
    public abstract class Controller : FilterData
    {
        public ControllerCategory ControllerCategory = ControllerCategory.Cat01;

        public bool Enabled = true;
        public World World { get; internal set; }

        public Controller()
        {
        }

        public override bool IsActiveOn(Body body)
        {
            if (body.ControllerFilter.IsControllerIgnored(ControllerCategory))
                return false;

            return base.IsActiveOn(body);
        }

        public abstract void Update(float dt);
    }
}