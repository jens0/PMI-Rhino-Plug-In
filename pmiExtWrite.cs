// PMI Rhino Plug-In, Copyright (c) 2015-2016 QUT
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
    [System.Runtime.InteropServices.Guid("bc4c437e-b519-4fc8-b10d-2c61d6920b7d")]
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
                RhinoApp.WriteLine("no building chosen");
                return rc;
            }

            string error = ob.verify(objref.Object(), false);
            if (error != "")
            {
                RhinoApp.WriteLine("pmi: " + error);
                return Result.Failure;
            }
            my.db(String.Format("<{0}<  ", EnglishName));
            my.db(ob.collectinfo());


            extdatawrite();

            my.dbln(String.Format("  >{0}>", EnglishName));
            return Result.Success;
        }

        public static void extdatawrite()
        {
            string pyfill = "";
            if (null == ob.kud)
                pyfill = null;
            else
            {
                pyfill = ob.kud.fill; //if (null==pyfill) my.db("NOFILL");
                ob.ject.Attributes.UserData.Remove(ob.kud);
            }
            NameValueCollection exdata = new NameValueCollection();
            foreach (my.gridrow gr in my.gridrows)
            {
                my.gridrow i = new my.gridrow();
                if (null != gr.name) i.name = gr.name; else i.name = "";
                if (null != gr.value) i.value = gr.value; else i.value = "";
                if ((i.name != "") && (!my.extnames.Contains(i.name)) || i.value != "")
                    exdata.Add(i.name, i.value);
            }

            if (exdata.Count == 0) exdata = null;
            if (null != pyfill || null != exdata)
                ob.ject.Attributes.UserData.Add(new kmlUserData(pyfill, exdata));
            //ob.ject.CommitChanges();
            ob.kud = ob.ject.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
        }
    }
}
