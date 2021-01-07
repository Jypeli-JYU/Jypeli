using FarseerPhysics.Dynamics.Joints;
using Jypeli.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli
{
    /// <summary>
    /// Sisäinen luokka liitosten käsittelyyn.
    /// </summary>
    public abstract class AbstractJoint : IAxleJoint
    {
        public virtual Joint innerJoint { get; set; }

        /// <summary>
        /// Ensimmäinen olio.
        /// </summary>
        public virtual PhysicsObject Object1 { get; internal set; }

        /// <summary>
        /// Toinen olio (null jos ensimmäinen olio on sidottu pisteeseen)
        /// </summary>
        public virtual PhysicsObject Object2 { get; internal set; }

        /// <summary>
        /// Pyörimisakselin (tämänhetkiset) koordinaatit.
        /// </summary>
        public abstract Vector AxlePoint { get;}

        /// <summary>
        /// Liitoksen pehmeys eli kuinka paljon sillä on liikkumavaraa.
        /// </summary>
        public abstract double Softness { get;  set; }

        Physics.IPhysicsEngine engine = null;

        public void SetEngine(Physics.IPhysicsEngine engine)
        {
            this.engine = engine;
        }

        public void AddToEngine()
        {
            if (this.engine == null) throw new InvalidOperationException("AddToEngine: physics engine not set");
            if (this.Object1 == null) throw new InvalidOperationException("AddToEngine: joint.Object1 == null");
            if (!this.Object1.IsAddedToGame) throw new InvalidOperationException("AddToEngine: object 1 not added to game");
            if (this.Object2 == null && !this.Object2.IsAddedToGame) throw new InvalidOperationException("AddToEngine: object 2 not added to game");

            engine.AddJoint(this);
        }

        #region Destroyable

        /// <summary>
        /// Onko liitos tuhottu.
        /// Farseerilla kertoo onko liitos enabloitu
        /// </summary>
        public bool IsDestroyed
        {
            get { return innerJoint.Enabled; }
        }

        /// <summary>
        /// Tapahtuu kun liitos on tuhottu.
        /// </summary>
        public event Action Destroyed;

        /// <summary>
        /// Tuhoaa liitoksen.
        /// </summary>
        public void Destroy()
        {
            if (Destroyed != null)
                Destroyed();
            innerJoint.Enabled = false;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Destroy();
        }

        #endregion
    }
}
