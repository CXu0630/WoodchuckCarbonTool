using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator.src
{
    internal class GeometryProcessor
    {
        public static double GetDimensionalInfo(ObjRef obj, int dimension)
        {
            switch (dimension)
            {
                case 3:
                    return GetVolume(obj);
                case 2:
                    return GetArea(obj);
                case 1:
                    return GetLength(obj);
                default:
                    return -1;
            }
        }

        public static double GetVolume(ObjRef obj)
        {
            Brep brep = obj.Brep();
            if (brep == null)
            {
                return -1;
            }
            return brep.GetVolume();
        }

        public static double GetArea(ObjRef obj)
        {
            Brep brep = obj.Brep();
            if (brep != null) { return brep.GetArea(); }
            BrepFace face = obj.Face();
            if (face != null)
            {
                double h; double w;
                face.GetSurfaceSize(out h, out w);
                return h * w;
            }
            Surface srf = obj.Surface();
            if (srf != null)
            {
                double h; double w;
                srf.GetSurfaceSize(out h, out w);
                return h * w;
            }
            return -1;
        }

        public static double GetLength(ObjRef obj)
        {
            Brep brep = obj.Brep();
            if (brep != null)
            {
                double len = 0;
                Curve[] edges = brep.DuplicateEdgeCurves();
                foreach (Curve brepEdge in edges)
                {
                    len += brepEdge.GetLength();
                }
                return len;
            }
            Curve crv = obj.Curve();
            if (crv != null) { return crv.GetLength(); }
            BrepEdge edge = obj.Edge();
            if (edge != null) { return edge.GetLength(); }
            BrepFace face = obj.Face();
            if (face != null) { return face.OuterLoop.To3dCurve().GetLength(); }
            return -1;
        }
    }
}
