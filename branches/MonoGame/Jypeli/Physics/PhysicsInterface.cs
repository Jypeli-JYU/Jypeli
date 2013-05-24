using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Jypeli.Physics
{
    internal static class PhysicsInterface
    {
        public static IPhysicsClient GetClient()
        {
            foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                Type clientClass = assembly.GetType( "Jypeli.Physics.PhysicsClient", false );
                if ( clientClass != null )
                    return (IPhysicsClient)Activator.CreateInstance( clientClass );
            }

            return null;
        }
    }
}
