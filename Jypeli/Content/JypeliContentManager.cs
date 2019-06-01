using System;
using System.IO;
using System.Reflection;

#if WINRT_CORE
using Windows.ApplicationModel.Resources;
#endif

namespace Microsoft.Xna.Framework.Content
{
    public class JypeliContentManager : ContentManager
    {
        public JypeliContentManager(IServiceProvider provider)
            : base(provider)
        {
        }

        protected override System.IO.Stream OpenStream(string assetName)
        {
#if WINDOWS_STOREAPP
            var assembly = typeof( JypeliContentManager ).GetTypeInfo().Assembly;
            var assetType = assembly.GetType( "Jypeli.Content." + assetName );
            var fieldInfo = assetType.GetTypeInfo().GetDeclaredField( "rawData" );
#else
            var assetType = Assembly.GetExecutingAssembly().GetType( "Jypeli.Content." + assetName );
            var bindingFlags = BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var fieldInfo = assetType.GetField( "rawData", bindingFlags );
#endif
            var bytes = fieldInfo.GetValue( null );
            return new MemoryStream( bytes as byte[] );
        }
    }
}
