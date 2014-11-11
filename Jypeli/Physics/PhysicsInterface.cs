using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

#if WINRT
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Jypeli.Physics
{
    internal static class PhysicsInterface
    {
        private static IPhysicsClient _client = null;

        public static IPhysicsClient GetClient()
        {
            if ( _client != null )
                return _client;

            foreach ( var assembly in GetAssemblies() )
            {
                Type clientClass = assembly.GetType( "Jypeli.Physics.PhysicsClient" );
                if ( clientClass != null )
                {
                    _client = (IPhysicsClient)Activator.CreateInstance( clientClass );
                    return _client;
                }
            }

            return null;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
#if WINRT
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assemblies = new List<Assembly>();

            IEnumerable<Windows.Storage.StorageFile> files = null;

            var operation = folder.GetFilesAsync();
            operation.Completed = async ( r, s ) =>
            {
                var result = await r;
                files = result;
            };

            while ( files == null ) ;

            foreach ( var file in files )
            {
                if ( file.FileType == ".dll" )
                {
                    var name = new AssemblyName() { Name = System.IO.Path.GetFileNameWithoutExtension( file.Name ) };
                    try
                    {
                        Assembly asm = Assembly.Load( name );
                        assemblies.Add( asm );
                    }
                    catch { }
                }
            }

            return assemblies;
#else
            return AppDomain.CurrentDomain.GetAssemblies();
#endif
        }
    }
}
