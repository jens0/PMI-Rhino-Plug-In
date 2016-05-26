// PMI Rhino Plug-In, Copyright (c) 2015-2016 QUT




using Rhino.Geometry;
using Rhino.DocObjects;





namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("a16ffefb-d9d4-439a-b241-c08e3f0522b2")]
    public class ob
    {
        public static RhinoObject ject = null;
        public static kmlUserData kud;
        public static Polyline polygon = new Polyline();
        public static Point3d center;
        public static double area, elevation, height;
        public static int vector;
        public static string verify(RhinoObject obj, bool top)
        {
            ob.ject = null;
            polygon.Clear();

            if (null == obj)
                return "invalid object";

            Extrusion extr = obj.Geometry as Extrusion;
            if (null == extr)
                return "object no extrusion";

            if (!extr.IsClosed(0))
                return "object not closed";

            NurbsSurface sur = extr.ToNurbsSurface();
            if ((sur.Points.CountU < 1) || (sur.Points.CountV != 2))
                return "invalid building (u/v)";

            Polyline polygon0 = new Polyline();
            Polyline polygon1 = new Polyline();
            for (int u = 0; u < sur.Points.CountU; u++)
            {
                polygon0.Add(sur.Points.GetControlPoint(u, 0).Location);
                polygon1.Add(sur.Points.GetControlPoint(u, 1).Location);
                if ((polygon0.Last.X != polygon1.Last.X) ||
                    (polygon0.Last.Y != polygon1.Last.Y))
                    return "building not planar (x/y)";
            }

            center.X = 0;
            center.Y = 0;
            area = 0;
            double z0 = polygon0.Last.Z;
            double z1 = polygon1.Last.Z;
            for (int p = 0; p < polygon0.Count - 1; p++)
            {
                double tmp = polygon0[p].X * polygon0[p + 1].Y - polygon0[p + 1].X * polygon0[p].Y;
                area += tmp;
                center.X += (polygon0[p].X + polygon0[p + 1].X) * tmp;
                center.Y += (polygon0[p].Y + polygon0[p + 1].Y) * tmp;
                if ((polygon0[p].Z != z0) ||
                    (polygon1[p].Z != z1))
                    return "building not planar (z)";
            }

            if (area >= 0)
            { // counterclockwise rhino polygons
                vector = 1;
                elevation = z0;
                height = z1 - z0;
                polygon = top && height > 0.0101 ? polygon1 : polygon0;
            }
            else
            { // clockwise tonsley polygons
                vector = -1;
                elevation = z1;
                height = z0 - z1;
                polygon = top && height > 0.0101 ? polygon0 : polygon1;
            }

            center.X /= area * 3;
            center.Y /= area * 3;
            center.Z = elevation + height;
            area /= vector * 2; //area *= my.meter * my.meter * vector / 2;
            ob.ject = obj;
            ob.kud = ob.ject.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
            return string.Empty;
        }

        public static bool hasexda;
        public static string collectinfo()
        {
            string s = "";
            int pos = my.ids.IndexOf(ob.ject.Id);
            if (pos != -1)
            {
                if (my.rsn[pos] == ob.ject.RuntimeSerialNumber)
                    s = "unmodified";
                else
                    s = "modified";
            }
            else
                s = "new";
            s += " building with";
            if (null == ob.kud)
            {
                hasexda = false;
                s += "out kmlUserData";
            }
            else
            {
                hasexda = null != ob.kud.exda;
                if (hasexda)
                    s += " kmlUserData inclusive of ExtendedData";
                else
                    s += " kmlUserData exclusive of ExtendedData";
            }
            return s;
        }
    }
}
