#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen, Rami Pasanen.
 */

using System;
using System.ComponentModel;


namespace Jypeli.Assets
{
    /// <summary>
    /// Yksinkertainen tykki, joka ampuu kuulia tai muuta ammuksia.
    /// </summary>
    public class Cannon : Weapon
    {
        /// <summary>
        /// Alustaa uuden tykin.
        /// </summary>
        public Cannon(double width, double height)
            : base(width, height)
        {
            Power.DefaultValue = 15000;
            TimeBetweenUse = TimeSpan.FromSeconds(1);

            Image = Game.LoadImageFromResources("Cannon.png");
            AttackSound = Game.LoadSoundEffectFromResources("CannonFire.wav");
        }

        /// <summary>
        /// Luo uuden ammuksen
        /// </summary>
        /// <returns>Ammus</returns>
        protected override PhysicsObject CreateProjectile()
        {
            return new CannonBall(5);
        }
    }

    /// <summary>
    /// Rynnäkkökivääri.
    /// </summary>
    public class AssaultRifle : Weapon
    {
        /// <summary>
        /// Alustaa uuden rynnäkkökiväärin.
        /// </summary>
        public AssaultRifle(double width, double height)
            : base(width, height)
        {
            AmmoIgnoresGravity = true;
            Power.DefaultValue = 200;
            TimeBetweenUse = TimeSpan.FromSeconds(0.2);

            Image = Game.LoadImageFromResources("AsRifle.png");
            AttackSound = Game.LoadSoundEffectFromResources("MGAttack.wav");
        }

        /// <summary>
        /// Luo uuden ammuksen
        /// </summary>
        /// <returns>Ammus</returns>
        protected override PhysicsObject CreateProjectile()
        {
            return new Bullet(5);
        }
    }

    /// <summary>
    /// Plasmatykki.
    /// </summary>
    public class PlasmaCannon : Weapon
    {
        /// <summary>
        /// Alustaa uuden plasmakiväärin.
        /// </summary>
        public PlasmaCannon(double width, double height)
            : base(width, height)
        {
            Power.DefaultValue = 6000;
            AmmoIgnoresGravity = true;
            AmmoIgnoresExplosions = true;
            MaxAmmoLifetime = TimeSpan.FromSeconds(0.7);

            Image = Game.LoadImageFromResources("PlasmaCannon.png");
            AttackSound = Game.LoadSoundEffectFromResources("PlasmaAttack.wav");
            TimeBetweenUse = TimeSpan.FromSeconds(0.1);
        }

        /// <summary>
        /// Luo uuden ammuksen
        /// </summary>
        /// <returns>Ammus</returns>
        protected override PhysicsObject CreateProjectile()
        {
            return new Projectile(3, 5, Color.Green);
        }
    }

    /// <summary>
    /// Lasertykki
    /// </summary>
    public class LaserGun : Weapon
    {
        /// <summary>
        /// Alustaa uuden laser-tykin.
        /// </summary>
        public LaserGun(double width, double height)
            : base(width, height)
        {
            Power.DefaultValue = 500;
            Ammo.DefaultValue = Int32.MaxValue;
            AmmoIgnoresGravity = true;
            AmmoIgnoresExplosions = true;
            //MaxAmmoLifetime = TimeSpan.FromSeconds( 0.7 );

            Image = Game.LoadImageFromResources("PlasmaCannon.png");
            AttackSound = Game.LoadSoundEffectFromResources("Laser.wav");
            TimeBetweenUse = TimeSpan.FromSeconds(0);
        }

        /// <summary>
        /// Luo uuden ammuksen
        /// </summary>
        /// <returns>Ammus</returns>
        protected override PhysicsObject CreateProjectile()
        {
            //return new Projectile( 10, 1, 5, Color.Red );
            PhysicsObject beam = new PhysicsObject(new RaySegment(Vector.Zero, Vector.UnitX, 10));
            beam.Color = Color.Red;
            beam.Mass = 0.3;
            beam.IgnoresGravity = true;
            return beam;
        }
    }

    /// <summary>
    /// Tykinkuula.
    /// </summary>
    public class CannonBall : Projectile
    {
        /// <summary>
        /// Alustaa uuden tykinkuulan.
        /// </summary>
        public CannonBall(double radius)
            : base(radius, 20, "CannonBall.png")
        {
        }
    }

    /// <summary>
    /// Luoti.
    /// </summary>
    public class Bullet : Projectile
    {
        /// <summary>
        /// Alustaa uuden luodin.
        /// </summary>
        public Bullet(double radius)
            : base(radius, 0.2, "Bullet.png")
        {
        }
    }

    /// <summary>
    /// Kranaatti.
    /// </summary>
    public class Grenade : Projectile
    {
        /// <summary>
        /// Räjähdys, joka kranaatista syntyy.
        /// </summary>
        public Explosion Explosion { get; set; }

        /// <summary>
        /// Onko kranaatti räjähtänyt
        /// </summary>
        public bool Exploded { get; set; }

        /// <summary>
        /// Räjähdyksen säde.
        /// </summary>
        [Obsolete("Use Explosion.MaxRadius")]
        public double ExplosionRadius
        {
            get { return Explosion.MaxRadius; }
            set { Explosion.MaxRadius = value; }
        }

        /// <summary>
        /// Räjähdyksen nopeus.
        /// </summary>
        [Obsolete("Use Explosion.Speed")]
        public double ExplosionSpeed
        {
            get { return Explosion.Speed; }
            set { Explosion.Speed = value; }
        }

        /// <summary>
        /// Räjähdyksen voima.
        /// </summary>
        [Obsolete("Use Explosion.Force")]
        public double ExplosionForce
        {
            get { return Explosion.Force; }
            set { Explosion.Force = value; }
        }

        /// <summary>
        /// Aika, jonka päästä ammus räjähtää itsestään.
        /// </summary>
        public TimeSpan FuseTime { get; set; }

        /// <summary>
        /// Luo uuden kranaatin, joka räjähtää kolmen sekunnin päästä.
        /// </summary>
        /// <param name="radius"></param>
        public Grenade(double radius)
            : this(radius, TimeSpan.FromSeconds(3))
        {
        }

        /// <summary>
        /// Luo uuden kranaatin.
        /// </summary>
        /// <param name="radius">Säde.</param>
        /// <param name="fuseTime">Kuinka kauan kestää ennen räjähdystä.</param>
        public Grenade(double radius, TimeSpan fuseTime)
            : base(radius, 20, "Grenade.png")
        {
            FuseTime = fuseTime;
            IsUpdated = true;
            Explosion = new Assets.Explosion(150) { Speed = 150, Force = 1000 };
        }

        /// <summary>
        /// Räjäytä kranaatti.
        /// </summary>
        public virtual void Explode()
        {
            if (!IsAddedToGame)
                return;

            this.Destroy();
            Explosion.Position = this.Position;
            Game.Instance.Add(Explosion);
            Exploded = true;
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Update(Time time)
        {
            if ((time.SinceStartOfGame - CreationTime) > FuseTime && IsAddedToGame && !Exploded)
            {
                // Boom!
                Explode();
            }

            base.Update(time);
        }
    }

    /// <summary>
    /// Rypälepommi. Hajoaa räjähtäessään pienempiin osiin, jotka
    /// voivat edelleen räjähtää pienempiin osiin.
    /// </summary>
    public class ClusterGrenade : Grenade
    {
        private int clusterlevel = 0;

        /// <summary>
        /// Sirpaleiden määrä, joka räjähdyksestä syntyy.
        /// </summary>
        public int NumberOfClusters { get; set; }

        /// <summary>
        /// Sirpaleiden hajontasuunta.
        /// </summary>
        public Angle ClusterDirection { get; set; }

        /// <summary>
        /// Sirpaleiden hajontakaari.
        /// </summary>
        public double ClusterArc { get; set; }

        /// <summary>
        /// Alustaa uuden rypälepommin.
        /// </summary>
        /// <param name="radius">Säde.</param>
        /// <param name="cl">Kuinka monta kertaa rypäleet hajoavat edelleen. Kuitenkin vähintään yhden kerran.</param>
        public ClusterGrenade(double radius, int cl)
            : base(radius)
        {
            Mass = 20;
            CollisionIgnorer = new ObjectIgnorer();
            NumberOfClusters = 3;
            ClusterDirection = Angle.FromRadians(Math.PI / 2);
            ClusterArc = Math.PI;
            clusterlevel = cl - 1;
        }

        /// <summary>
        /// Räjäyttää kranaatin sirpaleiksi.
        /// </summary>
        public override void Explode()
        {
            if (!IsAddedToGame) return;

            Vector posOffset;
            double direction;

            base.Explode();

            for (int i = 0; i < NumberOfClusters; i++)
            {
                double currentRadius = Width / 2;
                double r = currentRadius * 0.6;
                // TODO: The mass could be evenly distributed to the clusters and the main projectile.
                Grenade g = (clusterlevel > 0) ?
                    new ClusterGrenade(r, clusterlevel - 1) : new Grenade(r);

                direction = ClusterDirection.Radians - ((i - NumberOfClusters / 2) * ClusterArc / NumberOfClusters);
                posOffset = Vector.FromLengthAndAngle(2 + this.Size.Magnitude, Angle.FromRadians(direction));

                g.Position = this.Position + posOffset;
                g.Mass = this.Mass / NumberOfClusters;
                g.Image = this.Image;
                g.Color = this.Color;
                g.FuseTime = this.FuseTime;
                g.Explosion.Force = this.Explosion.Force / 2;
                g.Explosion.MaxRadius = this.Explosion.MaxRadius / 2;
                g.Explosion.Speed = this.Explosion.Speed / 2;
                g.CollisionIgnorer = this.CollisionIgnorer;
                Game.Instance.Add(g);
            }
        }
    }
}
