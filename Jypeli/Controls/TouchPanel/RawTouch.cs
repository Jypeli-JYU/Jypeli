using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Controls
{
    internal struct RawTouch
    {
        public Vector Position;
        public int Id;
        public TouchAction Action;
    }

    internal enum TouchAction
    {
        Down,
        Move,
        Up
    }
}
