using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

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
            if ( assetName == "bullet" )
            {
                return new MemoryStream( Jypeli.Content.Bullet.rawData );
            }

            throw new ContentLoadException( "Resource not found" );
        }
    }
}
