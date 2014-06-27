using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public abstract partial class FileManager : Updatable
    {
        public bool IsUpdated
        {
            get { return false; }
        }

        public void Update( Time time )
        {
        }
    }
}
