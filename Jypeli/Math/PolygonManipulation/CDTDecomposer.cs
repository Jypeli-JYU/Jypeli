using System.Collections.Generic;
using Jypeli.PolygonManipulation.Decomposition.CDT;
using Jypeli.PolygonManipulation.Decomposition.CDT.Delaunay;
using Jypeli.PolygonManipulation.Decomposition.CDT.Delaunay.Sweep;


namespace Jypeli.PolygonManipulation
{
    /// <summary>
    /// 2D constrained Delaunay triangulation algorithm.
    /// Based on the paper "Sweep-line algorithm for constrained Delaunay triangulation" by V. Domiter and and B. Zalik
    /// 
    /// Properties:
    /// - Creates triangles with a large interior angle.
    /// - Supports holes
    /// - Generate a lot of garbage due to incapsulation of the Poly2Tri library.
    /// - Running time is O(n^2), n = number of vertices.
    /// - Does not care about winding order.
    /// 
    /// Source: http://code.google.com/p/poly2tri/
    /// </summary>
    internal static class CDTDecomposer
    {
        /// <summary>
        /// Decompose the polygon into several smaller non-concave polygon.
        /// </summary>
        public static List<List<Vector>> ConvexPartition(List<Vector> vertices)
        {
            Decomposition.CDT.Polygon.Polygon poly = new Decomposition.CDT.Polygon.Polygon();

            foreach (Vector vertex in vertices)
                poly.Points.Add(new TriangulationPoint(vertex.X, vertex.Y));

            DTSweepContext tcx = new DTSweepContext();
            tcx.PrepareTriangulation(poly);
            DTSweep.Triangulate(tcx);

            List<List<Vector>> results = new List<List<Vector>>();

            foreach (DelaunayTriangle triangle in poly.Triangles)
            {
                List<Vector> v = new List<Vector>();
                foreach (TriangulationPoint p in triangle.Points)
                {
                    v.Add(new Vector(p.X, p.Y));
                }

                results.Add(v);
            }

            return results;
        }
    }
}