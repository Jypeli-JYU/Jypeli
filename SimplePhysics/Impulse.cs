using System;
using Jypeli;

namespace Jypeli.SimplePhysics
{
	public interface Impulse
    {
        bool IsExpired { get; }
        Vector GetNextForce();
    }
}

