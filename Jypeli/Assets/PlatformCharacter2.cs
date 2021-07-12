
#if DEBUG
// #define VISUALIZE
#endif

using System;
using System.ComponentModel;
using System.Linq;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Physics;


/// <summary>
/// Tasohyppelypelin hahmo. Voi liikkua ja hyppiä. Lisäksi sillä voi olla ase.
/// </summary>
public class PlatformCharacter2 : PhysicsObject
{
    private int _tolerance = 5;
    private double _accel = 1000;
    private double _maxVel = 500;

    private enum PlatformCharacterState { Idle, Falling, Jumping }
    private PlatformCharacterState state = PlatformCharacterState.Idle;

    private Weapon weapon = null;
    private Vector _cachedGravity = Vector.Zero;
    private double _cachedGravityMagnitude = 0;
    private Vector _cachedGravityNormal = Vector.Zero;

    private bool collisionDetected = false;
    private int noCollisionCount = 0;
    private int walkSteps = 0;
    private Direction _curDirection = Direction.Right;
    private Timer detachTimer = new Timer();

    private bool customAnimPlaying = false;
    private Action customAnimAction;

    private double gravityMagnitude
    {
        get
        {
            updateGravity();
            return _cachedGravityMagnitude;
        }
    }

    private Vector gravityNormal
    {
        get
        {
            updateGravity();
            return _cachedGravityNormal;
        }
    }

    /// <summary>
    /// Toleranssiarvo, joka määrittää kuinka monta pelin päivitysjaksoa hahmo voi
    /// olla irti tasosta ennen kuin se lasketaan ilmassa olevaksi.
    /// </summary>
    public int PlatformTolerance
    {
        get { return _tolerance; }
        set { _tolerance = value; }
    }

    /// <summary>
    /// Hahmon rintamasuunta (vasen tai oikea).
    /// </summary>
    public Direction FacingDirection
    {
        get { return _curDirection; }
        set { Turn( value ); }
    }

    /// <summary>
    /// Hahmon kiihtyvyys.
    /// </summary>
    public new double Acceleration
    {
        get { return _accel; }
        set { _accel = value; }
    }

    /// <summary>
    /// Hahmon maksiminopeus.
    /// </summary>
    public new double MaxVelocity
    {
        get { return _maxVel; }
        set { _maxVel = value; }
    }

#if VISUALIZE
    private GameObject ssurface = new GameObject( 1, 2, Shape.Rectangle ) { Color = Color.Blue };
    private PhysicsObject _platform = null;

    /// <summary>
    /// Fysiikkaolio jonka päällä seisotaan.
    /// </summary>
    public PhysicsObject Platform
    {
        get { return _platform; }
        set
        {
            if ( value != null )
            {
                Vector distNormal = this.Position - value.Position;
                double sideWidth = Math.Sqrt( Width * Width + Height * Height );
                ssurface.Position = this.Position - Vector.FromLengthAndAngle( sideWidth, distNormal.Angle );
                ssurface.Color = Color.Red;
            }
            else
            {
                ssurface.Color = Color.Blue;
                PlatformNormal = Vector.Zero;
            }

            _platform = value;
        }
    }
#else
    /// <summary>
    /// Fysiikkaolio jonka päällä seisotaan.
    /// </summary>
    public PhysicsObject Platform { get; set; }
#endif

    /// <summary>
    /// Suuntavektori, joka osoittaa tason suuntaan.
    /// Nollavektori, jos ilmassa.
    /// </summary>
    public Vector PlatformNormal { get; private set; }

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
                weapon.X *= -1;
                weapon.Angle = Angle.StraightAngle;
                weapon.TextureWrapSize = new Vector( 1, -1 );
            }
        }
    }

    /// <summary>
    /// Jos <c>false</c>, hahmoa ei voi liikuttaa kun se on ilmassa.
    /// </summary>
    public bool CanMoveOnAir { get; set; }

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
    /// Tapahtuu kun suunta vaihtuu.
    /// </summary>
    public event Action<Direction> DirectionChanged;

    /// <summary>
    /// Luo uuden tasohyppelyhahmon.
    /// </summary>
    public PlatformCharacter2(double width, double height)
        : this(width, height, Shape.Circle)
    {
    }

    /// <summary>
    /// Luo uuden tasohyppelyhahmon.
    /// </summary>
    public PlatformCharacter2( double width, double height, Shape shape )
        : base( width, height, shape /*, CollisionShapeQuality.FromValue(0.7)*/ )
    {
        KineticFriction = 0.0;
        Restitution = 0.2;
        CanRotate = false;
        CanMoveOnAir = true;

        // This avoids high speeds, particularly when falling. This then avoids
        // going through objects.
        LinearDamping = 0.96;

        AddedToGame += AddCollisionHandler;
        IsUpdated = true;
    }

    private void SetAnimation( Animation anim, bool loop = true )
    {
        if ( customAnimPlaying || anim == null || Animation == anim || anim.FrameCount == 0 )
            return;

        Animation = anim;

        if ( loop )
            Animation.Start();
        else
            AnimJump.Start( 1 );
    }

    /// <summary>
    /// Toistaa animaation
    /// </summary>
    /// <param name="anim">Animaatio</param>
    /// <param name="onPlayed">Mitä tehdään kun toistettu (oletuksena null)</param>
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

        if ( customAnimAction != null )
            customAnimAction();
    }

    private void updateGravity()
    {
        PhysicsGameBase physGame = Game.Instance as PhysicsGameBase;
        if ( physGame == null ) return;

        if ( physGame.Gravity != _cachedGravity )
        {
            _cachedGravity = physGame.Gravity;
            _cachedGravityMagnitude = _cachedGravity.Magnitude;
            _cachedGravityNormal = _cachedGravity / _cachedGravityMagnitude;

#if VISUALIZE
            ssurface.Angle = gravityNormal.LeftNormal.Angle;
#endif
        }
    }

    private void AddCollisionHandler()
    {
        PhysicsGameBase physicsGame = Game.Instance as PhysicsGameBase;
        if ( physicsGame == null ) throw new InvalidOperationException( "Cannot have a platform character in non-physics game" );

        this.Body.Colliding += delegate( IPhysicsBody o1, IPhysicsBody o2, Collision c )
        {
            PhysicsObject other = (PhysicsObject)o2.Owner;
            Vector normal = c.Contacts.First().Normal;
            OnColliding( this, other, normal );
        };

        #if VISUALIZE
        ssurface.Width = 2 * Math.Sqrt( Game.Level.Width * Game.Level.Width + Game.Level.Height * Game.Level.Height );
        Game.Instance.Add( ssurface );
        #endif
    }

    /// <summary>
    /// Kävelee.
    /// </summary>
    public void Walk()
    {
        walkSteps++;
        
        if ( state == PlatformCharacterState.Idle || WalkOnAir )
            SetAnimation( AnimWalk );
    }

    /// <summary>
    /// Kävelee tiettyyn suuntaan.
    /// </summary>
    /// <param name="direction">Rintamasuunta. Direction.Left tai Direction.Right</param>
    public void Walk( Direction direction )
    {
        Turn( direction );
        walkSteps++;

        if ( state == PlatformCharacterState.Idle || WalkOnAir )
            SetAnimation( AnimWalk );
    }
    
    /// <summary>
    /// Kääntyy.
    /// </summary>
    /// <param name="direction">Suunta</param>
    public void Turn( Direction direction )
    {
        if ( direction == FacingDirection || ( direction != Direction.Left && direction != Direction.Right ) )
            return;

        walkSteps = 0;
        TextureWrapSize = new Vector( -TextureWrapSize.X, TextureWrapSize.Y );

        if ( Weapon != null )
        {
            Weapon.X *= -1;
            Weapon.TextureWrapSize = new Vector( 1, -Weapon.TextureWrapSize.Y );
            Weapon.Angle = Angle.Supplement( Weapon.Angle );
        }

        _curDirection = direction;

        if ( DirectionChanged != null )
        {
            DirectionChanged( direction );
        }
    }

    /// <summary>
    /// Pysähtyy.
    /// </summary>
    public void StopWalking()
    {
        walkSteps = 0;
        if ( state == PlatformCharacterState.Idle )
            SetAnimation( AnimIdle );
    }

    private bool IsWeaponFacingRight()
    {
        return (-Math.PI / 2) < Weapon.Angle.Radians
            && Weapon.Angle.Radians < (Math.PI / 2);
    }

    /// <summary>
    /// Hyppää tietyllä nopeudella, jos hahmo seisoo tukevalla pohjalla.
    /// </summary>
    /// <param name="speed">Lähtönopeus.</param>
    public void Jump( double speed )
    {
        if ( Platform == null || state == PlatformCharacterState.Jumping ) return;
        ForceJump( speed );
    }

    /// <summary>
    /// Hyppää vaikka olio ei olisikaan toisen päällä.
    /// </summary>
    /// <param name="speed">Lähtönopeus maasta.</param>
    public void ForceJump( double speed )
    {
        IgnoresGravity = false;
        this.Hit( Mass * speed * -gravityNormal );
        Platform = null;
        
        state = PlatformCharacterState.Jumping;
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
        obj.Position = this.Position + this.FacingDirection.GetVector() * d + gravityNormal * axialDelta;
        obj.Hit( Vector.FromLengthAndAngle( force, throwAngle ) );
    }
    
    private void OnColliding( PhysicsObject collisionHelperObject, PhysicsObject target, Vector normal )
    {
        if ( target.IgnoresCollisionResponse )
            return;

        double dot = Vector.DotProduct( normal, gravityNormal );
        if ( Math.Abs( dot ) < 0.5 )
            return;

        if ( Platform != null && Platform != target )
            return;

        if ( Platform == null )
            Platform = target;
            
        collisionDetected = true;
        noCollisionCount = 0;
        PlatformNormal = normal;

        if ( state == PlatformCharacterState.Falling )
            state = PlatformCharacterState.Idle;
    }

    /// <summary>
    /// Ajetaan kun pelitilannetta päivitetään. Päivityksen voi toteuttaa omassa luokassa toteuttamalla tämän
    /// metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
    /// </summary>
    /// <param name="time">Peliaika.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Update(Time time)
    {
        if ( Platform != null && !collisionDetected )
        {
            if ( ++noCollisionCount > PlatformTolerance )
            {
                Platform = null;

                if ( state != PlatformCharacterState.Jumping )
                {
                    state = PlatformCharacterState.Falling;
                    SetAnimation( AnimFall, LoopFallAnim );
                }
            }
        }

        collisionDetected = false;

        if ( walkSteps > 0 )
        {
            // Walking
            double impulse = Mass * Acceleration * time.SinceLastUpdate.TotalSeconds;
            Vector unitX = gravityMagnitude > 0 ? gravityNormal.LeftNormal : Vector.UnitX;

            if ( _curDirection == Direction.Left && -Velocity.X < MaxVelocity )
                this.Push( Mass * Acceleration * -unitX );
            else if ( _curDirection == Direction.Right && Velocity.X < MaxVelocity )
                this.Push( Mass * Acceleration * unitX );

            walkSteps--;

            if ( state == PlatformCharacterState.Idle )
                SetAnimation( AnimWalk );
        }
        else
        {
            // Not walking
            if ( state == PlatformCharacterState.Idle )
                SetAnimation( AnimIdle );
            /*switch ( state )
            {
                case PlatformCharacterState.Idle: SetAnimation( AnimIdle ); break;
                case PlatformCharacterState.Falling: SetAnimation( AnimFall ); break;
                case PlatformCharacterState.Jumping: SetAnimation( AnimJump ); break;
            }*/
        }

        base.Update(time);
    }

    /// <summary>
    /// Siirtää oliota.
    /// </summary>
    /// <param name="movement">Vektori, joka määrittää kuinka paljon siirretään.</param>
    public override void Move( Vector movement )
    {
        if ( movement.X > 0 ) Walk( Direction.Right );
        else if ( movement.X < 0 ) Walk( Direction.Left );
        if ( movement.Y > 0 ) Jump( movement.Y );
    }
}