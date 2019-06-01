namespace Jypeli.Physics
{
    public interface IPhysicsEngine
    {
        Vector Gravity { get; set; }

        IPhysicsBody CreateBody( IPhysicsObject owner, double width, double height, Shape shape );
        IAxleJoint CreateJoint( IPhysicsObject obj1, IPhysicsObject obj2, Vector pivot );
        IAxleJoint CreateJoint( IPhysicsObject obj1, Vector pivot );

        void AddBody( IPhysicsBody body );
        void RemoveBody( IPhysicsBody body );
        
        void AddJoint( IAxleJoint joint );
        void RemoveJoint( IAxleJoint joint );

        void Clear();

        void Update( double dt );
    }
}
