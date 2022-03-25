using Jypeli.Effects;

namespace Jypeli.Assets
{
    /// <summary>
    /// Kokoelma valmiita törmäyksenkäsittelijöitä.
    /// </summary>
    public static class CollisionHandler
    {
        /// <summary>
        /// Tuhoaa törmäävän olion.
        /// </summary>
        /// <param name="collidingObject">Törmäävä olio</param>
        /// <param name="targetObject">Kohde johon törmätään</param>
        public static void DestroyObject(PhysicsObject collidingObject, PhysicsObject targetObject)
        {
            collidingObject.Destroy();
        }

        /// <summary>
        /// Tuhoaa olion johon törmätään.
        /// </summary>
        /// <param name="collidingObject">Törmäävä olio</param>
        /// <param name="targetObject">Kohde johon törmätään</param>
        public static void DestroyTarget(PhysicsObject collidingObject, PhysicsObject targetObject)
        {
            targetObject.Destroy();
        }

        /// <summary>
        /// Tuhoaa molemmat törmäävät oliot.
        /// </summary>
        /// <param name="collidingObject">Törmäävä olio</param>
        /// <param name="targetObject">Kohde johon törmätään</param>
        public static void DestroyBoth(PhysicsObject collidingObject, PhysicsObject targetObject)
        {
            collidingObject.Destroy();
            targetObject.Destroy();
        }

        /// <summary>
        /// Räjäyttää törmäävän olion.
        /// </summary>
        /// <param name="radius">Räjähdyksen säde</param>
        /// <param name="destroyObject">Tuhotaanko törmäävä olio</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> ExplodeObject(double radius, bool destroyObject)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                Explosion e = new Explosion(radius);
                e.Position = collidingObject.Position;
                Game.Instance.Add(e);

                if (destroyObject)
                    collidingObject.Destroy();
            };
        }

        /// <summary>
        /// Räjäyttää olion johon törmätään.
        /// </summary>
        /// <param name="radius">Räjähdyksen säde</param>
        /// <param name="destroyObject">Tuhotaanko törmäävä olio</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> ExplodeTarget(double radius, bool destroyObject)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                Explosion e = new Explosion(radius);
                e.Position = targetObject.Position;
                Game.Instance.Add(e);

                if (destroyObject)
                    targetObject.Destroy();
            };
        }

        /// <summary>
        /// Räjäyttää molemmat törmäävät oliot.
        /// Räjähdys tulee olioiden törmäyskohtaan.
        /// </summary>
        /// <param name="radius">Räjähdyksen säde</param>
        /// <param name="destroyObject">Tuhotaanko oliot samalla</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> ExplodeBoth(double radius, bool destroyObject)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                Explosion e = new Explosion(radius);
                e.Position = Vector.Average(collidingObject.Position, targetObject.Position);
                Game.Instance.Add(e);

                if (destroyObject)
                {
                    collidingObject.Destroy();
                    targetObject.Destroy();
                }
            };
        }

        /// <summary>
        /// Lisää mittarin arvoa halutulla määrällä.
        /// Voit myös vähentää käyttämällä negatiivista lukua.
        /// </summary>
        /// <param name="meter">Mittari</param>
        /// <param name="value">Kuinka paljon lisätään</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> AddMeterValue(IntMeter meter, int value)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                meter.Value += value;
            };
        }

        /// <summary>
        /// Lisää mittarin arvoa halutulla määrällä.
        /// Voit myös vähentää käyttämällä negatiivista lukua.
        /// </summary>
        /// <param name="meter">Mittari</param>
        /// <param name="value">Kuinka paljon lisätään</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> AddMeterValue(DoubleMeter meter, double value)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                meter.Value += value;
            };
        }

        /// <summary>
        /// Lyö törmäävää oliota vektorin määräämällä suunnalla ja voimalla.
        /// </summary>
        /// <param name="impulse">Impulssi (massa * nopeus)</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> HitObject(Vector impulse)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                collidingObject.Hit(impulse);
            };
        }

        /// <summary>
        /// Lyö oliota johon törmätään vektorin määräämällä suunnalla ja voimalla.
        /// </summary>
        /// <param name="impulse">Impulssi (massa * nopeus)</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> HitTarget(Vector impulse)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                targetObject.Hit(impulse);
            };
        }

        /// <summary>
        /// Soittaa ääniefektin.
        /// </summary>
        /// <param name="soundEffectName">Ääniefekitin nimi</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> PlaySound(string soundEffectName)
        {
            SoundEffect effect = Game.LoadSoundEffect(soundEffectName);

            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                effect.Play();
            };
        }

        /// <summary>
        /// Lisää efektin törmäävän olion kohdalle.
        /// </summary>
        /// <param name="expSystem">Efektijärjestelmä</param>
        /// <param name="numParticles">Kuinka monta partikkelia laitetaan</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> AddEffectOnObject(ExplosionSystem expSystem, int numParticles)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                expSystem.AddEffect(collidingObject.Position, numParticles);
            };
        }

        /// <summary>
        /// Lisää efektin sen olion kohdalle, johon törmätään.
        /// </summary>
        /// <param name="expSystem">Efektijärjestelmä</param>
        /// <param name="numParticles">Kuinka monta partikkelia laitetaan</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> AddEffectOnTarget(ExplosionSystem expSystem, int numParticles)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                expSystem.AddEffect(targetObject.Position, numParticles);
            };
        }

        /// <summary>
        /// Kasvattaa törmäävän olion kokoa (tai pienentää negatiivisilla arvoilla)
        /// </summary>
        /// <param name="width">Leveyden muutos</param>
        /// <param name="height">Korkeuden muutos</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> IncreaseObjectSize(double width, double height)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                if (collidingObject.Width + width <= 0)
                    return;
                if (collidingObject.Height + height <= 0)
                    return;

                collidingObject.Size += new Vector(width, height);
            };
        }

        /// <summary>
        /// Kasvattaa törmäyskohteen kokoa (tai pienentää negatiivisilla arvoilla)
        /// </summary>
        /// <param name="width">Leveyden muutos</param>
        /// <param name="height">Korkeuden muutos</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> IncreaseTargetSize(double width, double height)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                if (targetObject.Width + width <= 0)
                    return;
                if (targetObject.Height + height <= 0)
                    return;

                targetObject.Size += new Vector(width, height);
            };
        }

        /// <summary>
        /// Vaihtaa törmäävän olion väriä.
        /// </summary>
        /// <param name="color">Väri</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> SetColor(Color color)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                collidingObject.Color = color;
            };
        }

        /// <summary>
        /// Vaihtaa törmäyskohteen väriä.
        /// </summary>
        /// <param name="color">Väri</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> SetTargetColor(Color color)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                targetObject.Color = color;
            };
        }

        /// <summary>
        /// Vaihtaa törmäävän olion värin satunnaiseen.
        /// </summary>
        /// <param name="collidingObject"></param>
        /// <param name="targetObject"></param>
        public static void SetRandomColor(PhysicsObject collidingObject, PhysicsObject targetObject)
        {
            collidingObject.Color = RandomGen.NextColor();
        }

        /// <summary>
        /// Vaihtaa törmäyskohteen värin satunnaiseen.
        /// </summary>
        /// <param name="collidingObject"></param>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        public static void SetRandomTargetColor(PhysicsObject collidingObject, PhysicsObject targetObject)
        {
            targetObject.Color = RandomGen.NextColor();
        }

        /// <summary>
        /// Näyttää viestin MessageDisplayssä.
        /// </summary>
        /// <param name="message">Viesti.</param>
        /// <returns></returns>
        public static CollisionHandler<PhysicsObject, PhysicsObject> ShowMessage(string message)
        {
            return delegate (PhysicsObject collidingObject, PhysicsObject targetObject)
            {
                Game.Instance.MessageDisplay.Add(message);
            };
        }
    }
}
