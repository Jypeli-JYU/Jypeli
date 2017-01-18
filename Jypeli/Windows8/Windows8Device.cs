using System;
using Windows.Foundation.Metadata;

namespace Jypeli.Devices
{
    public class Windows8Device : Device
    {
        public override bool IsMobileDevice
        {
            get { return true; }
        }

        public override bool IsPhone
        {
            get { return false; }
        }

        public Windows8Device()
        {
            this.Storage = new RTFileManager();
        }
    }
}
