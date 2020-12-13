using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;


namespace Jypeli.Farseer
{
    internal class FSFixtureDef
    {
        public FarseerPhysics.Collision.Shapes.Shape Shape;
        public float Friction = 0.2f;
        public float Restitution;
        public float Density = 1f;
        public bool IsSensor;
        public Category CollidesWith = Settings.DefaultFixtureCollidesWith;
        public Category CollisionCategories = Settings.DefaultFixtureCollisionCategories;
        public Category IgnoreCCDWith = Settings.DefaultFixtureIgnoreCCDWith;
        public short CollisionGroup;
    }
}