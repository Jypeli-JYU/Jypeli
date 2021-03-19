module PhysicsGame

open Jypeli
open SimplePhysics

type public PhysicsGame() =
    inherit Game()

    let mutable _gravity = Vector.Zero
    let mutable objects : PhysicsObject list = []
    let maxdt = 0.01

    member this.Gravity
        with get() = _gravity
        and set(a) = _gravity <- a

    override this.Add(obj, layer) =
        match obj with
            | :? PhysicsObject as physObj ->
                objects <- physObj :: objects
                base.Add(obj, layer)
            | _ -> base.Add(obj, layer)

        base.Add(obj, layer)

    override this.Update(t : Time) =
        let mutable dt = t.SinceLastUpdate.TotalSeconds
        while (dt < maxdt) do
            this.Integrate(maxdt)
            dt <- dt - maxdt
        this.Integrate(dt);
        this.Cleanup();
        base.Update(t)

    member this.Integrate(dt) =
        match dt with
            | 0.0 -> ()
            | _ ->
                List.map (fun o -> this.UpdateBody(dt, o.Body) objects) |> ignore
                this.SolveCollisions()

(*
PhysicsBody body = (PhysicsBody)physObjects[i].Body;
if ( !body.IgnoresGravity && !body.IgnoresPhysicsLogics )
    body.Velocity += Gravity * body.MassInv * dt;
body.Velocity += body.Acceleration * dt;
body.Position += body.Velocity * dt;
*)

    member this.UpdateBody(dt, b : PhysicsBody) =
        b.Velocity <- b.Velocity + b.Acceleration * dt

    member this.Cleanup() =
        ()

    member this.SolveCollisions() =
        ()

