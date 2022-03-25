namespace Jypeli.Physics
{
    /// <summary>
    /// Rajapinta fysiikkamoottorille.
    /// </summary>
    public interface IPhysicsEngine
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Vector Gravity { get; set; }

        IPhysicsBody CreateBody(IPhysicsObject owner, double width, double height, Shape shape);
        IAxleJoint CreateJoint(IPhysicsObject obj1, IPhysicsObject obj2, Vector pivot);
        IAxleJoint CreateJoint(IPhysicsObject obj1, Vector pivot);
        IAxleJoint CreateJoint(IPhysicsObject obj1, IPhysicsObject obj2, JointTypes type);

        void AddBody(IPhysicsBody body);
        void RemoveBody(IPhysicsBody body);

        void ConnectBodies(PhysicsObject physObj1, PhysicsObject physObj2);

        void AddJoint(IAxleJoint joint);
        void RemoveJoint(IAxleJoint joint);

        void Clear();

        void Update(double dt);
    }
}
