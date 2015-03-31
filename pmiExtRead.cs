// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;


using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Commands;
using Rhino.Input;
using System.Windows;


namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("00000000-1111-0000-1111-000000000000")]
    public class pmiExtRead : Command
    {
        public pmiExtRead() { }
        public override string EnglishName { get { return "pmiExtRead"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            myp.gridrows.Clear();
            const ObjectType filter = Rhino.DocObjects.ObjectType.AnyObject;
            ObjRef objref = null;
            Result rc = RhinoGet.GetOneObject("choose building", false, filter, out objref);
            if (rc != Result.Success || null == objref)
            {
                RhinoApp.WriteLine("error: no building chosen");
                return rc;
            }

            RhinoObject obj = objref.Object();
            if (null == obj)
            {
                RhinoApp.WriteLine("error: invalid building");
                return Result.Failure;
            }

            GeometryBase geom = obj.Geometry;
            Extrusion extr = geom as Extrusion;
            if (null == extr)
            {
                RhinoApp.WriteLine("error: building is no extrusion");
                return Result.Failure;
            }

#if testing
#warning testing@pmiExtRead
            if (!extr.IsClosed(0))
            {
                RhinoApp.WriteLine("error: extrusion is not closed in U-direction");
                return Result.Failure;
            }

            RhinoApp.Write(" Top:"    + extr.IsCappedAtTop);
            RhinoApp.Write(" Bottom:" + extr.IsCappedAtBottom);
            RhinoApp.Write(" Torus:" + extr.IsTorus(0.1));
            RhinoApp.Write(" Planar:" + extr.IsPlanar(0.1));
            RhinoApp.Write(" Singu1:" + extr.IsAtSingularity(0, 1, false));
            RhinoApp.Write(" Singu2:"  + extr.IsAtSingularity(1, 0, false));
            RhinoApp.Write(" ");

            NurbsSurface sur = extr.ToNurbsSurface();
            double height = sur.Points.GetControlPoint(0, 0).Location.Z - sur.Points.GetControlPoint(0, 1).Location.Z;
            double elevation = sur.Points.GetControlPoint(0, 1).Location.Z;
            Polyline polygon = new Polyline();
            for (int u = 0; u < sur.Points.CountU; u++) polygon.Add(sur.Points.GetControlPoint(u, 1).Location);
            RhinoApp.Write("  h:" + height.ToString("F3", myp.cult));
            RhinoApp.Write("  e:" + elevation.ToString("F3", myp.cult));
            if (height < 0) { height *= -1; elevation -= height; }
            RhinoApp.Write("   h:" + height.ToString("F3", myp.cult));
            RhinoApp.Write("  e:" + elevation.ToString("F3", myp.cult));

            RhinoApp.Write("   ");

            //if (!extr.IsPlanar(0.1))
            //{
            //    RhinoApp.WriteLine("error: extrusion is not planar");
            //    return Result.Failure;
            //}
#endif

            RhinoApp.Write("<DEBUG(ExtRead)  ");
            int pos = myp.ids.IndexOf(obj.Id);
            if (pos != -1)
            {
                if (myp.rsn[pos] == obj.RuntimeSerialNumber)
                {
                    RhinoApp.Write("unmodified building");
                }
                else
                {
                    RhinoApp.Write("modified building");
                }
            }
            else
            {
                RhinoApp.Write("new building");
            }




            kmlUserData kud = obj.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
            if (null == kud)
            {
                RhinoApp.Write(" without kmlUserData selected");

            }
            else
            {
                if (kud.exda.Count != 0) RhinoApp.Write(" with kmlUserData inclusive of ExtendedData selected");
                else RhinoApp.Write(" with kmlUserData exclusive of ExtendedData selected");

                foreach (String s in myp.exnames)
                {
                    string calc = kud.exda[s] != null ? kud.exda[s] : "";
#if calcvalues
#warning calcvalues@pmiExtRead
                    if (myp.calculate)
                    {
                        NurbsSurface sur = extr.ToNurbsSurface();
                        double height = sur.Points.GetControlPoint(0, 0).Location.Z - sur.Points.GetControlPoint(0, 1).Location.Z;
                        double elevation = sur.Points.GetControlPoint(0, 1).Location.Z;
                        Polyline polygon = new Polyline();
                        for (int u = 0; u < sur.Points.CountU; u++) polygon.Add(sur.Points.GetControlPoint(u, 1).Location);
                        if (s == "Perimeter")
                            calc = (polygon.Length * 1000 * (myp.degree / myp.cancel)).ToString("F0", myp.cult);
                        if (s == "GSASpaceAr")
                            calc = Rhino.Geometry.AreaMassProperties.Compute(polygon.ToNurbsCurve()).Area.ToString("F2", myp.cult);
                    }
#endif
                    myp.gridrows.Add(new myp.gridrow { name = s, value = calc });
                }
                foreach (String s in kud.exda.AllKeys)
                    if (!myp.exnames.Contains(s)) myp.gridrows.Add(new myp.gridrow { name = s, value = kud.exda[s] });
#if showbox
#warning showbox@pmiExtRead
                string allkeys = "";
                foreach (String s in kud.exda.AllKeys)
                {
                    if (!myp.exnames.Contains(s)) myp.gridrows.Add(new myp.gridrow { name = s, value = kud.exda[s] });
                    string _s = String.Format("{0} = {1}", s, kud.exda[s]);
                    if (_s.Length > 56) _s = _s.Substring(0, 55) + "...";
                    allkeys += _s + "\n";
                }
                MessageBox.Show(allkeys, "Extended Data");
#endif
            }
            RhinoApp.WriteLine("  (ExtRead)DEBUG>");
            return Result.Success;
        }
    }
}
