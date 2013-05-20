using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using Windows.ApplicationModel.Resources;

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

            /*object obj = this.rl.GetString(assetName);

            if (obj == null)
            {
                throw new ContentLoadException("Resource not found");
            }
            if (!(obj is byte[]))
            {
                throw new ContentLoadException("Resource is not in binary format");
            }
            return new MemoryStream(obj as byte[]);*/
        }
    }
}
