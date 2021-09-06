
#if DEBUG
//#define VISUALIZE
// TODO: Tää voisi olla ihan ajonaikainen asetus.
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Jypeli;
using Jypeli.Assets;


/// <summary>
/// Tasohyppelypelin hahmo. Voi liikkua ja hyppiä. Lisäksi sillä voi olla ase.
/// </summary>
public class PlatformCharacter : PhysicsObject
{
    private class CollisionHelper
    {
#if VISUALIZE
        private struct Appearance
        {
            public Color Color;
            public Animation Animation;
        }

        private static Dictionary<PhysicsObject, Appearance> originalAppearances = new Dictionary<PhysicsObject, Appearance>();

        private static void ResetAppearance(PhysicsObject o)
        {
            if (!originalAppearances.ContainsKey(o))
                return;
            Appearance original = originalAppearances[o];
            o.Color = original.Color;
            o.Animation = original.Animation;
            originalAppearances.Remove(o);
        }
#endif

        private PhysicsObject parent;
        public PhysicsObject Object;
        public PhysicsObject LastHitObject;

        public CollisionHelper( PhysicsObject parent )
        {
            this.parent = parent;
        }

        public void SetObjectBeingHit(PhysicsObject collisionHelper, PhysicsObject target)
        {
            if (target != parent && IsPlatform(target))
            {
#if VISUALIZE
                if (! originalAppearances.ContainsKey(target))
                {
                    originalAppearances.Add(target, new Appearance { Animation = target.Animation, Color = target.Color });
                }

                target.Color = Color.Red;
                target.Animation = null;
                Timer.SingleShot(1.0, delegate() { ResetAppearance(target); });
#endif

                LastHitObject = target;
            }
        }
    }

    private enum PlatformCharacterState { Idle, Falling, Jumping }
    private PlatformCharacterState state = PlatformCharacterState.Idle;
    
    
    private Weapon weapon = null;
    private CollisionHelper[] collisionHelpers = new CollisionHelper[3];
    private Direction _facingDirection = Direction.Right;
    private Vector? lastPlatformPosition = null;
    private PhysicsObject lastPlatform = null;
    private bool isWalking = false;
    private bool _turnsWhenWalking = true;
    private double lastDt = 0;
    private GameObject stateIndicator;

    private double lowTolerance { get { return Height * 0.1; } }
    private double highTolerance { get { return Height * 0.2; } }

    private bool customAnimPlaying = false;
    Action customAnimAction;

    /// <summary>
    /// Hahmon rintamasuunta (vasen tai oikea).
    /// </summary>
    public Direction FacingDirection
    {
        get { return _facingDirection; }
        set { Turn( value ); }
    }

    /// <summary>
    /// Kääntyykö hahmo automaattisesti kun se kävelee.
    /// </summary>
    public bool TurnsWhenWalking
    {
        get { return _turnsWhenWalking; }
        set { _turnsWhenWalking = value; }
    }

    ///<inheritdoc/>
    public override Vector Size
    {
        get
        {
            return base.Size;
        }
        set
        {
            base.Size = value;
            for (int i = 0; i < collisionHelpers.Length; i++)
                collisionHelpers[i].Object.Size = new Vector(value.X / 3, value.Y);
        }
    }

    /// <summary>
    /// Voiko hahmo kävellä kun sen edessä on seinä.
    /// Oletus false.
    /// </summary>
    public bool CanWalkAgainstWalls { get; set; }

    /// <inheritdoc/>
    public override Jypeli.Ignorer CollisionIgnorer
    {
        get
        {
            return base.CollisionIgnorer;
        }
        set
        {
            base.CollisionIgnorer = value;
            for (int i = 0; i < collisionHelpers.Length; i++)
                collisionHelpers[i].Object.CollisionIgnorer = value;
        }
    }

    /// <inheritdoc/>
    public override int CollisionIgnoreGroup
    {
        get { return base.CollisionIgnoreGroup; }
        set
        {
            base.CollisionIgnoreGroup = value;
            for (int i = 0; i < collisionHelpers.Length; i++)
                collisionHelpers[i].Object.CollisionIgnoreGroup = value;
        }
    }

    /// <summary>
    /// Hahmolla oleva ase.
    /// </summary>
    public Weapon Weapon
    {
        get { return weapon; }
        set
        {
            // First check: same weapon
            if ( weapon == value ) return;

            // Remove the previous weapon if any
            if ( weapon != null )
            {
                // Reset the weapon when removing

                if ( !IsWeaponFacingRight() )
                {
                    weapon.X *= -1;
                    weapon.TextureWrapSize = new Vector( 1, 1 );
                }

                weapon.Angle = Angle.Zero;
                this.Remove( weapon );
            }
            this.weapon = value;
            if ( value == null )
                return;

            this.Add( value );

            if ( FacingDirection == Direction.Left )
            {
                // If facing left, set the weapon to match the direction
                //weapon.X *= -1;
                weapon.Angle = Angle.StraightAngle;
                weapon.TextureWrapSize = new Vector( 1, -1 );
            }

            weapon.Position = this.Position;
        }
    }

    /// <summary>
    /// Jos <c>false</c>, hahmoa ei voi liikuttaa kun se on ilmassa.
    /// </summary>
    public bool CanMoveOnAir { get; set; }

    /// <summary>
    /// Jos <c>true</c>, hahmon liike jatkuu hidastuen vaikka kävelemisen lopettaa.
    /// </summary>
    public bool MaintainMomentum { get; set; }

    /// <summary>
    /// Kävelyanimaatio (oikealle)
    /// </summary>
    public Animation AnimWalk { get; set; }

    /// <summary>
    /// Hyppyanimaatio (oikealle)
    /// </summary>
    public Animation AnimJump { get; set; }

    /// <summary>
    /// Putoamisanimaatio (oikealle)
    /// </summary>
    public Animation AnimFall { get; set; }

    /// <summary>
    /// Toistetaanko hyppyanimaatiota useammin kuin kerran.
    /// </summary>
    public bool LoopJumpAnim { get; set; }

    /// <summary>
    /// Toistetaanko putoamisanimaatiota useammin kuin kerran.
    /// </summary>
    public bool LoopFallAnim { get; set; }

    /// <summary>
    /// Animaatio, jota käytetään kun hahmo on paikallaan (kääntyneenä oikealle)
    /// </summary>
    public Animation AnimIdle { get; set; }

    /// <summary>
    /// Toistetaanko kävelyanimaatiota ilmassa liikuttaessa?
    /// </summary>
    public bool WalkOnAir { get; set; }

    /// <summary>
    /// Hahmon suunnan muutos.
    /// </summary>
    public event Action<Direction> DirectionChanged;

    /// <summary>
    /// Luo uuden tasohyppelyhahmon.
    /// </summary>
    /// <param name="width">Hahmon leveys</param>
    /// <param name="height">Hahmon korkeus</param>
    public PlatformCharacter(double width, double height)
        : this(width, height, Shape.Rectangle)
    {
    }

    /// <summary>
    /// Luo uuden tasohyppelyhahmon.
    /// </summary>
    /// <param name="width">Hahmon leveys</param>
    /// <param name="height">Hahmon korkeus</param>
    /// <param name="shape">Hahmon muoto</param>
    public PlatformCharacter(double width, double height, Shape shape)
        : base(width, height, shape/*, CollisionShapeQuality.FromValue(0.7)*/)
    {
        KineticFriction = 0.0;
        Restitution = 0.0;
        CanRotate = false;
        CanMoveOnAir = true;

        // This avoids high speeds, particularly when falling. This then avoids
        // going through objects.
        LinearDamping = 0.96;

        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            collisionHelpers[i] = new CollisionHelper( this );
            collisionHelpers[i].Object = new PhysicsObject(width / 3, height)
            {
                IgnoresGravity = true,
                IgnoresCollisionResponse = true,
                IgnoresExplosions = true,
#if VISUALIZE
                IsVisible = true,
#else
                IsVisible = false,
#endif
            };
        }

#if VISUALIZE
        collisionHelpers[0].Object.Color = new Color(150, 150, 0, 100);
        collisionHelpers[1].Object.Color = new Color(150, 180, 0, 100);
        collisionHelpers[2].Object.Color = new Color(150, 210, 0, 100);
#endif
        
        AddedToGame += AddCollisionHelpers;
        AddedToGame += SetIdleAnim;
        Removed += RemoveCollisionHelpers;

        IsUpdated = true;
    }

    private void SetIdleAnim()
    {
        if ( state == PlatformCharacterState.Idle )
            SetAnimation( AnimIdle );
    }

    private void AddCollisionHelpers()
    {
        if (Game.Instance is not PhysicsGameBase physicsGame)
            throw new InvalidOperationException("Cannot have a platform character in non-physics game");

        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            physicsGame.Add(collisionHelpers[i].Object);
        }

        physicsGame.AddProtectedCollisionHandler<PhysicsObject, PhysicsObject>(this, OnCollision);

        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            physicsGame.AddProtectedCollisionHandler<PhysicsObject, PhysicsObject>( collisionHelpers[i].Object, collisionHelpers[i].SetObjectBeingHit );
        }
    }

    private void RemoveCollisionHelpers()
    {
        if (Game.Instance is not PhysicsGameBase physicsGame)
            throw new InvalidOperationException("Cannot have a platform character in non-physics game");

        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            physicsGame.Remove(collisionHelpers[i].Object);
        }

        physicsGame.RemoveCollisionHandlers(this, null, null, null);

        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            physicsGame.RemoveProtectedCollisionHandlers(collisionHelpers[i].Object, null, null, null);
        }
    }

    /// <summary>
    /// Resetoi hahmon tilan ja pysäyttää animaation
    /// </summary>
    public void Reset()
    {
        state = PlatformCharacterState.Idle;
        customAnimPlaying = false;
    }

    private void SetAnimation( Animation anim, bool loop = true )
    {
        if ( customAnimPlaying || anim == null || Animation == anim || anim.FrameCount == 0 )
            return;

        Animation = anim;

        if ( loop )
            Animation.Start();
        else
            Animation.Start( 1 );
    }

    /// <summary>
    /// Toistaa hahmolle animaation
    /// </summary>
    /// <param name="anim">Animaatio</param>
    /// <param name="onPlayed">Suoritetaan kun animaatio on toistettu loppuun</param>
    public void PlayAnimation( Animation anim, Action onPlayed = null )
    {
        customAnimPlaying = true;
        customAnimAction = onPlayed;
        this.Animation = anim;
        anim.Played += AnimationPlayed;
        anim.Start( 1 );
    }

    private void AnimationPlayed()
    {
        Animation.Played -= AnimationPlayed;
        customAnimPlaying = false;

        customAnimAction?.Invoke();
    }

    private static bool IsPlatform(PhysicsObject o)
    {
        return !o.IgnoresCollisionResponse;
    }

    /// <summary>
    /// Liikuttaa hahmoa.
    /// </summary>
    /// <param name="horizontalVelocity">Nopeus vaakasuunnassa.</param>
    public void Walk(double horizontalVelocity)
    {
        if ( horizontalVelocity > 0 && TurnsWhenWalking )
            Turn( Direction.Right );
        else if ( horizontalVelocity < 0 && TurnsWhenWalking )
            Turn( Direction.Left );

        if ( CanWalk( horizontalVelocity * lastDt ) )
        {
            this.Velocity = new Vector( horizontalVelocity / 2, this.Velocity.Y );
            this.X += horizontalVelocity * lastDt; // Hahmo törmäsi tasaisen pinnan reunoihin farseerilla,
                                                   // clipataan niiden sisälle jolloin moottori "korjaa" sijaintia hieman ylöspäin.
        }

        if ( state == PlatformCharacterState.Idle || WalkOnAir )
            SetAnimation( AnimWalk );

        isWalking = true;
    }

    /// <summary>
    /// Kääntyy.
    /// </summary>
    /// <param name="direction">Suunta</param>
    public void Turn( Direction direction )
    {
        if ( direction == FacingDirection || ( direction != Direction.Left && direction != Direction.Right ) )
            return;

        TextureWrapSize = new Vector( -TextureWrapSize.X, TextureWrapSize.Y );

        if ( Weapon != null )
        {
            Weapon.TextureWrapSize = new Vector( 1, -Weapon.TextureWrapSize.Y );
            Weapon.Angle = Angle.Supplement( Weapon.Angle );
        }

        _facingDirection = direction;

        DirectionChanged?.Invoke(direction);
    }

    private bool IsWeaponFacingRight()
    {
        return (-Math.PI / 2) < Weapon.Angle.Radians
            && Weapon.Angle.Radians < (Math.PI / 2);
    }
    
    /// <summary>
    /// Hyppää, jos hahmo on staattisen olion päällä.
    /// </summary>
    /// <param name="speed">Lähtönopeus maasta.</param>
    /// <returns><code>true</code> jos hyppäys onnistui.</returns>
    public bool Jump(double speed)
    {
        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            if (IsStandingOn(collisionHelpers[i].LastHitObject, highTolerance))
            {
                ForceJump( speed );
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Onko hahmo astumassa tyhjän päälle.
    /// </summary>
    /// <returns><code>true</code> jos on menossa tyhjän päälle.</returns>
    public bool IsAboutToFall()
    {
 /* TODO:
 * E = elephant, # = platform
 * 
 *    E#
 *  ###  <-- E.IsAboutToFall == true, when walking to the right.
 * 
 *    E#
 *  #### <-- seems to work fine  
*/

        double speedDirection = this.FacingDirection.GetVector().X;
        double lowestPoint = this.Bottom;

        for (int i = 0; i < this.collisionHelpers.Length; i++)
        {
            if (collisionHelpers[i].Object.Bottom < lowestPoint) lowestPoint = collisionHelpers[i].Object.Bottom;
        }

        if (lastPlatform != null
            && this.X + speedDirection * (this.Width / 2) < lastPlatform.Right
            && this.X + speedDirection * (this.Width / 2) > lastPlatform.Left
            && lowestPoint < lastPlatform.Top
            && lowestPoint > lastPlatform.Bottom)
        {
            return false;
        }

        GameObject platform = this.Game.GetObjectAt(new Vector(this.X + speedDirection * (this.Width/ 2), lowestPoint - 1));
        if (platform == null) return true;

        if (platform is not PhysicsObject p)
            return true;
        if (p.IgnoresCollisionResponse) return true;
        if (p.CollisionIgnoreGroup == this.CollisionIgnoreGroup && p.CollisionIgnoreGroup != 0) return true;

        return false;
    }

    /// <summary>
    /// Hyppää vaikka olio ei olisikaan toisen päällä.
    /// </summary>
    /// <param name="speed">Lähtönopeus maasta.</param>
    public void ForceJump( double speed )
    {
        state = PlatformCharacterState.Jumping;
        IgnoresGravity = false;
        Hit( new Vector( 0, speed * Mass ) );
        SetAnimation( AnimJump, LoopJumpAnim );

        Timer t = new Timer();
        t.Interval = 0.01;
        t.Timeout += delegate
        {
            if ( this.Velocity.Y < 0 )
            {
                t.Stop();
                state = PlatformCharacterState.Falling;
                SetAnimation( AnimFall, LoopFallAnim );
            }
        };
        t.Start();
    }

    /// <inheritdoc/>
    protected override void PrepareThrowable( PhysicsObject obj, Angle angle, double force, double distanceDelta, double axialDelta )
    {
        double d = ( this.Width + obj.Width ) / 2;
        Angle throwAngle = FacingDirection == Direction.Left ? Angle.StraightAngle - angle : angle;
        obj.Position = this.Position + this.FacingDirection.GetVector() * d + Vector.UnitY * axialDelta;
        obj.Hit( Vector.FromLengthAndAngle( force, throwAngle ) );
    }

    private double GetPlatformTopY( GameObject platform )
    {
        if ( platform.Angle == Angle.Zero )
            return platform.Top;

        Vector uTargetX = Vector.FromAngle( platform.Angle );
        Vector uTargetY = uTargetX.LeftNormal;

        double leftProj = new Vector( this.Left - platform.Position.X, this.Bottom - platform.Position.Y ).ScalarProjection( uTargetX );
        double rightProj = new Vector( this.Right - platform.Position.X, this.Bottom - platform.Position.Y ).ScalarProjection( uTargetX );
        Vector leftTopPoint = leftProj * uTargetX + platform.Height / 2 * uTargetY;
        Vector rightTopPoint = rightProj * uTargetX + platform.Height / 2 * uTargetY;

        return platform.Position.Y + Math.Max( leftTopPoint.Y, rightTopPoint.Y );
    }

    /// <summary>
    /// Checks if the character is located on top of the target.
    /// </summary>
    /// <param name="target">Another object</param>
    /// <param name="yTolerance">How close must the distance on the y-axis must be</param>
    private bool IsStandingOn(PhysicsObject target, double yTolerance)
    {
        if (target == null || target.IsDestroyed || this.IgnoresCollisionWith(target)) return false;

        // This prevents jumping when on the side of the wall, not on top of it.
        double epsilon = Width / 6;

        double targetTop = this.GetPlatformTopY( target );
        return (target.Position.Y <= this.Position.Y
            && (this.Bottom - targetTop) < yTolerance
            && (target.Left + epsilon) < this.Right
            && this.Left < (target.Right - epsilon));
    }

    private void OnCollision(PhysicsObject collisionHelperObject, PhysicsObject target)
    {
        // Velocity <= 0 condition is here to allow jumping through a platform without stopping
        if (IsPlatform(target) && IsStandingOn(target, highTolerance) && Velocity.Y <= 0)
        {
            IgnoresGravity = true;
            StopVertical();
            SetAnimation( AnimIdle );
            state = PlatformCharacterState.Idle;
        }
    }

    /// <inheritdoc/>
    public override void Destroy()
    {
        for (int i = 0; i < collisionHelpers.Length; i++)
        {
            collisionHelpers[i].Object.Destroy();
        }
        base.Destroy();
    }

    /// <summary>
    /// Ajetaan kun pelitilannetta päivitetään. Päivityksen voi toteuttaa omassa luokassa toteuttamalla tämän
    /// metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
    /// </summary>
    /// <param name="time">Peliaika.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Update(Time time)
    {
        Visualize();
        AdjustPosition();

        if (!isWalking)
            StopWalking();
        isWalking = false;
        
        lastDt = time.SinceLastUpdate.TotalSeconds;
        base.Update(time);
    }

    private bool CanWalk(double dx)
    {
        if ( !CanMoveOnAir && !collisionHelpers.Any( c => IsStandingOn( c.LastHitObject, lowTolerance ) ) )
            return false;

        if ( CanWalkAgainstWalls || Game == null || Math.Abs( dx ) < float.Epsilon )
            return true;

        Vector wallPos = this.Position + ( Math.Sign(dx) * this.Width / 2 + dx ) * Vector.UnitX;

        foreach ( var obj in Game.GetObjectsAt( wallPos ) )
        {
            if ( !( obj is PhysicsObject ) )
                continue;

            if ( !this.IgnoresCollisionWith( (PhysicsObject)obj ) )
                return false;
        }

        return true;
    }

    private void Visualize()
    {
#if VISUALIZE
        if ( stateIndicator == null )
        {
            stateIndicator = new GameObject( this.Width, 10 );
            Game.Add( stateIndicator );
        }

        stateIndicator.Position = this.Position + new Vector( 0, this.Height );
        stateIndicator.Color = GetStateColor( state );
#endif
    }

    private void AdjustPosition()
    {
        collisionHelpers[0].Object.Position = this.Position - new Vector( -Width / 4, Height * 1 / 8 );
        collisionHelpers[1].Object.Position = this.Position - new Vector( 0, Height * 1 / 8 );
        collisionHelpers[2].Object.Position = this.Position - new Vector( Width / 4, Height * 1 / 8 );

        if ( state == PlatformCharacterState.Jumping )
            return;

        PhysicsObject platform = FindPlatform();

        if ( platform != null && IsStandingOn(platform, lowTolerance) )
        {
            this.Y = GetPlatformTopY( platform ) + this.Height / 2;

            if ( lastPlatformPosition.HasValue ) this.X += platform.X - lastPlatformPosition.Value.X;
            lastPlatformPosition = platform.Position;
            lastPlatform = platform;
        }
        else
        {
            IgnoresGravity = false;
            lastPlatformPosition = null;
            state = PlatformCharacterState.Falling;
            SetAnimation( AnimFall, LoopFallAnim );
        }
    }

    private static Color GetStateColor( PlatformCharacterState state )
    {
        return state switch
        {
            PlatformCharacterState.Falling => Color.Red,
            PlatformCharacterState.Jumping => Color.Yellow,
            _ => Color.White,
        };
    }

    private PhysicsObject FindPlatform()
    {
        PhysicsObject platform = null;

        for ( int i = 0; i < collisionHelpers.Length; i++ )
        {
            if ( IsStandingOn( collisionHelpers[i].LastHitObject, lowTolerance ) )
            {
                platform = collisionHelpers[i].LastHitObject;
                if ( lastPlatform != platform ) lastPlatformPosition = null;
            }
        }

        return platform;
    }

    private void StopWalking()
    {
        if ( !MaintainMomentum )
        {
            StopHorizontal();
        }

        if ( state == PlatformCharacterState.Idle )
            SetAnimation( AnimIdle );
    }

    /// <summary>
    /// Siirtää oliota.
    /// </summary>
    /// <param name="movement">Vektori, joka määrittää kuinka paljon siirretään.</param>
    public override void Move( Vector movement )
    {
        Vector dv = movement - this.Velocity;
        Walk( dv.X );
    }

    /// <inheritdoc/>
    protected override void MoveToTarget()
    {
        if ( !moveTarget.HasValue )
        {
            Stop();
            moveTimer.Stop();
            return;
        }

        Vector d = moveTarget.Value - Position;
        double vt = moveSpeed * moveTimer.Interval;

        if ( d.Magnitude < vt )
        {
            Vector targetLoc = moveTarget.Value;
            Stop();
            moveTimer.Stop();
            moveTarget = null;

            arrivedAction?.Invoke();
        }
        else
        {
            Vector dv = Vector.FromLengthAndAngle( moveSpeed, d.Angle ) - this.Velocity;
            Walk( dv.X );
        }
    }
}
