using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Jypeli
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true )]
    [ComVisible( true )]
    public class SaveAttribute : Attribute
    {
        public SaveAttribute()
        {
        }
    }

    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true )]
    [ComVisible( true )]
    public class SaveAllFieldsAttribute : SaveAttribute
    {
        public SaveAllFieldsAttribute()
        {
        }
    }
}
