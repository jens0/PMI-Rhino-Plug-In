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
    [System.Runtime.InteropServices.Guid("5ef9f03c-3472-4769-a8d8-4b4e857f2ad1")]
    public class pmiExtRead : Command
    {
        public pmiExtRead() { }
        public override string EnglishName { get { return "pmiExtRead"; } }
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
            my.gridrows.Clear();
            string error = ob.verify(objref.Object(), false);
            if (error != "")
            {
                RhinoApp.WriteLine("pmi: " + error);
                return Result.Failure;
            }
            my.db(String.Format("<{0}<  ", EnglishName));
            my.db(ob.collectinfo());
            if (!my.ismeterinit) my.initmeter(doc);
            my.removeasterisk(doc); my.drawasterisk(doc); if (my.showasterisk || my.storeymode) doc.Views.Redraw();
            extdataread();

            my.dbln(String.Format("  >{0}>", EnglishName));
            return Result.Success;
        }

        public static void extdataread()
        {
            foreach (String name in my.extnames)
            {
                string value = "";
                if (ob.hasexda && null != ob.kud.exda[name]) value = ob.kud.exda[name];
                my.gridrows.Add(new my.gridrow { name = name, value = value });
            }
            if (ob.hasexda)
                foreach (String name in ob.kud.exda.AllKeys)
                    if (!my.extnames.Contains(name))
                        my.gridrows.Add(new my.gridrow { name = name, value = ob.kud.exda[name] });
#if showbox
#warning showbox@pmiExtRead
            if (hasexda)
            {
                string allkeys = "";
                foreach (String s in ob.kud.exda.AllKeys)
                {
                    string _s = String.Format("{0} = {1}", s, ob.kud.exda[s]);
                    if (_s.Length > 56) _s = _s.Substring(0, 55) + "...";
                    allkeys += _s + "\n";
                }
                MessageBox.Show(allkeys, "Extended Data");
            }
#endif
        }
    }
}
