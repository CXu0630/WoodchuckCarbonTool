using Rhino.DocObjects;
using Rhino.Geometry;

namespace WoodchuckCarbonTool.src
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
            if (brep != null)
            {
                return brep.GetVolume();
            }
            Mesh mesh = obj.Mesh();
            if (mesh != null)
            {
                return mesh.Volume();
            }
            return -1;
        }

        public static double GetArea(ObjRef obj)
        {
            Brep brep = obj.Brep();
            
            if (brep != null) 
            {
                var faces = brep.Faces;
                BrepFace largestFace;
                double largestFaceArea = -1;
                foreach (var brepFace in faces)
                {
                    double faceArea = 0;
                    brepFace.GetSurfaceSize(out double h, out double w);
                    faceArea = h * w;
                    if (faceArea > largestFaceArea)
                    {
                        largestFace = brepFace;
                        largestFaceArea = faceArea;
                    }
                }
                return largestFaceArea;
            }
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
                double maxLen = -1;
                Curve[] edges = brep.DuplicateEdgeCurves();
                foreach (Curve brepEdge in edges)
                {
                    if(brepEdge.GetLength() > maxLen)
                    {
                        maxLen = brepEdge.GetLength();
                    }
                }
                return maxLen;
            }
            Curve crv = obj.Curve();
            if (crv != null) { return crv.GetLength(); }
            BrepEdge edge = obj.Edge();
            if (edge != null) { return edge.GetLength(); }
            BrepFace face = obj.Face();
            if (face != null) 
            {
                double maxLen = -1;
                Line[] lines = face.GetBoundingBox(true).GetEdges();
                foreach (Line line in lines)
                {
                    if (line.Length > maxLen)
                    {
                        maxLen = line.Length;
                    }
                }
                return maxLen;
            }
            return -1;
        }
    }
}
