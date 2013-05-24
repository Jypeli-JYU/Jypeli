﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Törmäyskuvion laatuun vaikuttavat parametrit.
    /// </summary>
    public struct CollisionShapeParameters
    {
        public double DistanceGridSpacing;
        public double MaxVertexDistance;

        internal CollisionShapeParameters( double distanceGridSpacing, double maxVertexDistance )
        {
            this.DistanceGridSpacing = distanceGridSpacing;
            this.MaxVertexDistance = maxVertexDistance;
        }
    }
}
