using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class PhysicsGameBase
    {
        /// <summary>
        /// Lisää liitoksen peliin.
        /// </summary>
        public void Add( Physics2DDotNet.Joints.Joint j )
        {
            Joints.Add( j );
        }

        /// <summary>
        /// Poistaa liitoksen pelistä.
        /// </summary>
        /// <param name="j"></param>
        internal void Remove( Physics2DDotNet.Joints.Joint j )
        {
            Joints.Remove( j );
        }

        /// <summary>
        /// Poistaa liitoksen pelistä.
        /// </summary>
        /// <param name="j"></param>
        internal void Remove( AxleJoint j )
        {
            Joints.Remove( j.innerJoint );
        }

        /// <summary>
        /// Lisää liitoksen peliin.
        /// </summary>
        public void Add( AxleJoint j )
        {
            bool obj1ok = j.Object1.IsAddedToGame;
            bool obj2ok = j.Object2 == null || j.Object2.IsAddedToGame;

            if ( obj1ok && obj2ok )
            {
                Add( j.innerJoint );
            }
            else
            {
                if ( !obj1ok ) j.Object1.AddedToGame += j.DelayedAddJoint;
                if ( !obj2ok ) j.Object2.AddedToGame += j.DelayedAddJoint;
            }
        }
    }
}
