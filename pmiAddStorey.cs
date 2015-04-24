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
    [System.Runtime.InteropServices.Guid("c9c45afd-d208-447e-9f1a-8807fadd4668")]
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
                RhinoApp.WriteLine("no building chosen");
                return rc;
            }

            string error = ob.verify(objref.Object(), true);
            if (error != "")
            {
                RhinoApp.WriteLine("pmi: " + error);
                return Result.Failure;
            }
            my.db(String.Format("<{0}<  ", EnglishName));
            my.db(ob.collectinfo());
            my.storeymode = true;
            ob.ject.Select(false);
            ob.ject = doc.Objects.Find(doc.Objects.AddExtrusion(Extrusion.Create(ob.polygon.ToNurbsCurve(), ob.height * ob.vector, true), attribstorey()));
            if (!my.automatic) { ob.ject.Select(true); doc.Views.Redraw(); }
            my.dbln(String.Format("  >{0}>", EnglishName));
            return Result.Success;
        }

        public static ObjectAttributes attribstorey()
        {
            ObjectAttributes attributes = new ObjectAttributes();
            attributes.Name = ob.ject.Attributes.Name + "-Storey";
            while (my.names.Contains(attributes.Name)) { attributes.Name += "-Renamed"; ob.ject.CommitChanges(); }
            attributes.WireDensity = ob.ject.Attributes.WireDensity;
            attributes.ObjectColor = ob.ject.Attributes.ObjectColor;
            attributes.ColorSource = ObjectColorSource.ColorFromObject;
            if (null != ob.kud)
            {
                NameValueCollection exdata = new NameValueCollection();
                if (ob.hasexda)
                {
                    exdata.Add("Name", attributes.Name);
                    if (null == ob.kud.exda["Description"]) exdata.Add("Description",
                        "CreatedOn" + DateTime.Now.ToString("ddMMMyyyy", my.cult) + "From" + ob.ject.Attributes.Name);
                    foreach (String s in ob.kud.exda.AllKeys) if (s != "Name") exdata.Add(s, ob.kud.exda[s]);
                }
                else
                    exdata = null;
                attributes.UserData.Add(new kmlUserData(ob.kud.fill, exdata));
            }
            my.db(String.Format(", create \"{0}\"", attributes.Name));
            return attributes;
        }
    }
}
