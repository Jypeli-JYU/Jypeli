// Learn more about F# at http://fsharp.net

module SimplePhysics

open Jypeli
open Jypeli.Physics

type public PhysicsBody(w, h, s) =
    let mutable _shape : Shape = s
    let mutable _mass  = 1.0
    let mutable _massInv = 1.0;
    let mutable _restitution = 1.0

    let mutable _size = new Vector(w, h)
    let mutable _pos = Vector.Zero
    let mutable _vel = Vector.Zero
    let mutable _acc = Vector.Zero

    let mutable _iCollResp = false
    let mutable _iGrav = false
    let mutable _iPhysLog = false

    member this.Shape
        with set(a) = _shape <- a

    member this.Size
        with get() = _size
        and set(a) = _size <- a

    interface IPhysicsBody with
        member this.Shape with get() = _shape
        member this.Mass
            with get() = _mass
            and set(a) =
                _mass <- a
                _massInv <- 1.0 / a
        member this.MassInv
            with get() = _massInv
            and set(a) =
                _massInv <- a
                _mass <- 1.0 / a
        member this.Restitution
            with get() = _restitution
            and set(a) = _restitution <- a

        member this.MomentOfInertia
            with get() = infinity
            and set(a) = ()
        member this.KineticFriction
            with get() = 0.0
            and set(a) = ()
        member this.AngularDamping
            with get() = 1.0
            and set(a) = ()
        member this.LinearDamping
            with get() = 1.0
            and set(a) = ()

        member this.Position
            with get() = _pos
            and set(a) = _pos <- a
        member this.Velocity
            with get() = _vel
            and set(a) = _vel <- a
        member this.Acceleration
            with get() = _acc
            and set(a) = _acc <- a

        member this.Angle
            with get() = 0.0
            and set(a) = ()
        member this.AngularVelocity
            with get() = 0.0
            and set(a) = ()
        member this.AngularAcceleration
            with get() = 0.0
            and set(a) = ()

        member this.IgnoresCollisionResponse
            with get() = _iCollResp
            and set(a) = _iCollResp <- a
        member this.IgnoresGravity
            with get() = _iGrav
            and set(a) = _iGrav <- a
        member this.IgnoresPhysicsLogics
            with get() = _iPhysLog
            and set(a) = _iPhysLog <- a

        member this.MakeStatic() =
            _mass <- infinity
            _massInv <- 0.0
            _iGrav <- true

        member this.ApplyImpulse(impulse) =
            _vel <- _vel + impulse * _massInv

