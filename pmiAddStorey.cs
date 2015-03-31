// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;


using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Commands;
using Rhino.Input;

using System.Collections.Specialized;

namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("59f0cc43-3019-41ae-b65b-87e1fbadfba3")]
    public class pmiAddStorey : Command
    {
        public pmiAddStorey() { }
        public override string EnglishName { get { return "pmiAddStorey"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

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
            obj.Select(false);
            RhinoApp.Write("<DEBUG(AddStorey)  ");
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
            }

            NurbsSurface sur = extr.ToNurbsSurface();
            double height = sur.Points.GetControlPoint(0, 0).Location.Z - sur.Points.GetControlPoint(0, 1).Location.Z;
            double elevation = sur.Points.GetControlPoint(0, 1).Location.Z;
            Polyline polygon = new Polyline();
            for (int u = 0; u < sur.Points.CountU; u++) polygon.Add(sur.Points.GetControlPoint(u, height < 0.0011 / (myp.degree / myp.cancel) ? 1 : 0).Location);

            ObjectAttributes attributes = new ObjectAttributes();
            attributes.Name = obj.Attributes.Name + "-Storey";
            while (myp.names.Contains(attributes.Name)) { attributes.Name += "-Renamed"; obj.CommitChanges(); }
            attributes.WireDensity = obj.Attributes.WireDensity;
            attributes.ObjectColor = obj.Attributes.ObjectColor;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;
            if (null != kud)
            {
                NameValueCollection exdata = new NameValueCollection();
                exdata.Add("Name", attributes.Name);
                if (null == kud.exda["Description"]) exdata.Add("Description", "StoreyCreatedOn" + DateTime.Now.ToString("ddMMMyyyy", myp.cult) + "From" + obj.Attributes.Name);
                foreach (String s in kud.exda.AllKeys) if (s != "Name") exdata.Add(s, kud.exda[s]);
                attributes.UserData.Add(new kmlUserData(kud.fill, exdata));
            }
            doc.Objects.AddExtrusion(Extrusion.Create(polygon.ToNurbsCurve(), -height, true), attributes);
            RhinoApp.WriteLine(", building \"{0}\" is created  (AddStorey)DEBUG>", attributes.Name);

            RhinoObject objstorey = doc.Objects.MostRecentObject();
            objstorey.Select(true);
            //myp.knr.Add(-1);
            //myp.rsn.Add(objstorey.RuntimeSerialNumber);
            //myp.ids.Add(objstorey.Id);

            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
