// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;


using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Commands;
using Rhino.Input;
using System.Windows;
using System.Collections.Specialized;

namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("22222222-1111-2222-1111-222222222222")]
    public class pmiExtWrite : Command
    {
        public pmiExtWrite() { }
        public override string EnglishName { get { return "pmiExtWrite"; } }
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

            RhinoApp.Write("<DEBUG(ExtWrite)  ");
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

            string pyfill = "";
            NameValueCollection exdata = new NameValueCollection();

            kmlUserData kud = obj.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
            if (null == kud)
            {
                RhinoApp.Write(" without kmlUserData selected");
                pyfill = "NoValue";
            }
            else
            {
                if (kud.exda.Count != 0) RhinoApp.Write(" with kmlUserData inclusive of ExtendedData selected");
                else RhinoApp.Write(" with kmlUserData exclusive of ExtendedData selected");
                pyfill = kud.fill;
                obj.Attributes.UserData.Remove(kud);
            }

            foreach (myp.gridrow gr in myp.gridrows)
            {
                myp.gridrow dummy = new myp.gridrow();
                if (null != gr.name) dummy.name = gr.name; else dummy.name = "";
                if (null != gr.value) dummy.value = gr.value; else dummy.value = "";
                exdata.Add(dummy.name, dummy.value);
            }
            obj.Attributes.UserData.Add(new kmlUserData(pyfill, exdata));
            obj.CommitChanges();

            RhinoApp.WriteLine("  (ExtWrite)DEBUG>");
            return Result.Success;
        }
    }
}
