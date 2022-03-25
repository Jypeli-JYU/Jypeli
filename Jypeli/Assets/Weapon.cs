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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;

namespace Jypeli.Assets
{
    /// <summary>
    /// Ase.
    /// </summary>
    public abstract class Weapon : GameObject
    {
        private TimeSpan timeOfLastUse;

        /// <summary>
        /// Voiko asetta valmiina käytettäväksi.
        /// Ottaa huomioon tulinopeuden, ammukset jne.
        /// </summary>
        public virtual bool IsReady
        {
            get
            {
                return (Ammo > 0) && ((Game.Time.SinceStartOfGame - timeOfLastUse) > TimeBetweenUse);
            }
        }

        /// <summary>
        /// Voi ammus osua aseen omistajaan?
        /// </summary>
        public bool CanHitOwner { get; set; }

        /// <summary>
        /// Voima, jolla panos ammutaan. Nollautuu ammuttaessa.
        /// </summary>
        public DoubleMeter Power { get; protected set; }

        /// <summary>
        /// Panosten määrä.
        /// </summary>
        public IntMeter Ammo { get; protected set; }

        /// <summary>
        /// Loputtomat ammukset.
        /// </summary>
        public bool InfiniteAmmo
        {
            get { return Ammo.Value == Int32.MaxValue; }
            set
            {
                if (!value && Ammo.Value == Int32.MaxValue)
                    Ammo.Value = 100;
                else if (value)
                    Ammo.Value = Int32.MaxValue;
            }
        }

        /// <summary>
        /// Jättävätkö panokset painovoiman huomiotta.
        /// </summary>
        public bool AmmoIgnoresGravity { get; set; }

        /// <summary>
        /// Jättävätkö panokset räjähdyksen paineaallot huomiotta.
        /// </summary>
        public bool AmmoIgnoresExplosions { get; set; }

        /// <summary>
        /// Ammuksen elinikä.
        /// TimeSpan.MaxValue jos ikuinen, TimeSpan.FromSeconds( 5 ) jos 5 sekuntia jne.
        /// </summary>
        public TimeSpan MaxAmmoLifetime { get; set; }

        /// <summary>
        /// Hyökkäysääni (pistoolin pamaus jne.)
        /// </summary>
        public SoundEffect AttackSound { get; set; }

        /// <summary>
        /// Kuinka kauan kestää, että asetta voidaan käyttää uudestaan.
        /// </summary>
        public TimeSpan TimeBetweenUse { get; set; }

        /// <summary>
        /// Aseen laukauksen voimakkuus väliltä 0-1.0.
        /// Oletuksena 0.5.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Tulinopeus (ammusta sekunnissa)
        /// </summary>
        public double FireRate
        {
            get { return 1 / TimeBetweenUse.TotalSeconds; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Fire rate cannot be negative!");
                if (value == 0)
                    TimeBetweenUse = TimeSpan.MaxValue;
                else
                    TimeBetweenUse = TimeSpan.FromSeconds(1 / value);
            }
        }

        /// <summary>
        /// Tapahtumankäsittelijä ampumiselle, parametrinä ammus.
        /// Voit antaa kaikille aseesta lähteville ammuksille ominaisuuksia
        /// tällä tapahtumankäsittelijällä.
        /// </summary>
        public event Action<PhysicsObject> Shooting;

        /// <summary>
        /// Suoritetaan kun ase ampuu
        /// </summary>
        /// <param name="projectile">Ammus</param>
        protected void OnShooting(PhysicsObject projectile)
        {
            if (Shooting != null)
                Shooting(projectile);
        }

        /// <summary>
        /// Tapahtumankäsittelijä ammuksen törmäykselle.
        /// </summary>
        public CollisionHandler<PhysicsObject, PhysicsObject> ProjectileCollision { get; set; }

        /// <summary>
        /// Luo uuden aseen.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public Weapon(double width, double height)
            : base(width, height)
        {
            Shape = Shape.Rectangle;
            Power = new DoubleMeter(0);
            Power.MinValue = 0;
            Ammo = new IntMeter(Int32.MaxValue, 0, Int32.MaxValue);
            Ammo.MinValue = 0;
            MaxAmmoLifetime = TimeSpan.MaxValue;
            Volume = 0.5;
        }

        /// <summary>
        /// Luo uuden ammuksen.
        /// Ylikirjoitetaan aliluokissa.
        /// </summary>
        /// <returns></returns>
        protected abstract PhysicsObject CreateProjectile();

        /// <summary>
        /// Ampuu aseella, ja palauttaa ammuksen tai <c>null</c>, jos
        /// ampuminen ei onnistu (esimerkiksi jos panokset ovat lopussa).
        /// </summary>
        /// <remarks>
        /// Tätä metodia käyttämällä pääsee muokkaamaan ammusta, esimerkiksi muuttamaan
        /// sen fysiikkaominaisuuksia. Huomaa kuitenkin, että tällöin tulee aina
        /// tarkistaa että ammus ei ole <c>null</c>.
        /// </remarks>
        /// <returns>Ammuttu panos tai <c>null</c>.</returns>
        public PhysicsObject Shoot()
        {
            if (IsReady)
            {
                timeOfLastUse = Game.Time.SinceStartOfGame;

                if (AttackSound != null)
                    AttackSound.Play(Volume, 0, 0);

                PhysicsObject p = CreateProjectile();

                SetCollisionHandler(p, ProjectileCollision);
                p.IgnoresGravity = AmmoIgnoresGravity;
                p.IgnoresExplosions = AmmoIgnoresExplosions;
                p.MaximumLifetime = MaxAmmoLifetime;
                ShootProjectile(p, Power.Value);
                if (!InfiniteAmmo)
                    Ammo.Value--;
                Power.Reset();
                OnShooting(p);

                return p;
            }

            return null;
        }

        /// <summary>
        /// Ampuu ammuksen annetulla voimalla
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="power"></param>
        protected void ShootProjectile(PhysicsObject projectile, double power)
        {
            if (!IsAddedToGame)
                return;

            Vector impulse = Vector.FromLengthAndAngle(power, Angle);
            Vector direction = Vector.FromLengthAndAngle(1.0, Angle);
            Vector position = this.Position;

            if (Parent != null)
            {
                //0.75 * max(width, height) -> projectiles don't hit the PlatformCharacter's collisionHelpers.
                //Would be better to choose the width or height manually based on the direction.
                position = this.Position + direction * 0.75 * (Math.Max(this.Parent.Width, this.Parent.Height));
            }

            projectile.Position = position;
            projectile.Angle = this.Angle;

            if (Parent is PhysicsObject && !CanHitOwner)
            {
                // The projectile can not hit the owner of the weapon.
                // Owner's CollisionIgnorer can be null if no CollisionIgnorer is set for the owner
                // Must set CollisionIgnorer separetly for PlatformCharacter because of it's CollisionHelpers
                PhysicsObject physParent = (PhysicsObject)Parent;
                if (physParent.CollisionIgnorer == null)
                {
                    if (physParent is PlatformCharacter)
                        (physParent as PlatformCharacter).CollisionIgnorer = new Jypeli.ObjectIgnorer();
                    else
                        physParent.CollisionIgnorer = new Jypeli.ObjectIgnorer();
                }
                projectile.CollisionIgnorer = physParent.CollisionIgnorer;
            }

            Game.Instance.Add(projectile);
            projectile.Hit(impulse);
        }

        /// <summary>
        /// Lisää törmäyksenkäsittelijän ammukselle
        /// </summary>
        /// <param name="projectile">Ammus</param>
        /// <param name="handler">Käsittelijä</param>
        protected void SetCollisionHandler(PhysicsObject projectile, CollisionHandler<PhysicsObject, PhysicsObject> handler)
        {
            if (handler == null)
                return;

            if (Game.Instance is PhysicsGameBase)
            {
                PhysicsGameBase pg = (PhysicsGameBase)Game.Instance;
                pg.AddCollisionHandler(projectile, handler);
            }
            else
                throw new InvalidOperationException("Cannot set a collision handler to non-physics game!");
        }

        /// <inheritdoc/>
        public override void Update(Time time)
        {
            base.Update(time);
        }
    }
}
