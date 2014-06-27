﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class FileManager
    {
        public abstract IList<string> GetFileList();
        public abstract bool Exists( string fileName );

        public StorageFile Create( string fileName )
        {
            return Open( fileName, true );
        }
        
        public abstract StorageFile Open( string fileName, bool write );
        public abstract void Delete( string fileName );
    }
}
