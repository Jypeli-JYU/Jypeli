namespace Jypeli.Physics

open SimplePhysics

type public PhysicsClient =
    interface IPhysicsClient with
        member this.CreateBody(w, h, s) = new PhysicsBody(w, h, s) :> IPhysicsBody
