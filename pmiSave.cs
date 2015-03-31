// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;
using System.Xml;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Commands;
using System.Linq;



namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("11111111-1111-1111-1111-111111111111")]
    [CommandStyle(Rhino.Commands.Style.ScriptRunner)]
    public class pmiOpen : Command
    {
        public pmiOpen() { }
        public override string EnglishName { get { return "pmiOpen"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.RunScript("_Open", false);
            RhinoApp.WriteLine("ScriptOpen");
            return Result.Success;
        }
    }

    [System.Runtime.InteropServices.Guid("22222222-2222-2222-2222-222222222222")]
    [CommandStyle(Rhino.Commands.Style.ScriptRunner)]
    public class pmiImport : Command
    {
        public pmiImport() { }
        public override string EnglishName { get { return "pmiImport"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.RunScript("_Import", false);
            return Result.Success;
        }
    }

    [System.Runtime.InteropServices.Guid("33333333-3333-3333-3333-333333333333")]
    public class pmiSave : Command
    {
        public pmiSave() { }
        public override string EnglishName { get { return "pmiSave"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            string[] tab = 
                        {
                            "name",
                            "Style",
                            "Style/LineStyle",
                            "Style/LineStyle/color",
                            "Style/PolyStyle",
                            "Style/PolyStyle/fill",
                            "ExtendedData",
                            "ExtendedData/SchemaData schemaUrl=#Tonsley___140013_Masterplan_A13_new_shp_v6",
                            "ExtendedData/SchemaData/SimpleData name=Name",
                            "ExtendedData/SchemaData/SimpleData name=Description",
                            "ExtendedData/SchemaData/SimpleData name=WKT",
                            "ExtendedData/SchemaData/SimpleData name=Type",
                            "ExtendedData/SchemaData/SimpleData name=FPA",
                            "ExtendedData/SchemaData/SimpleData name=GFA",
                            "ExtendedData/SchemaData/SimpleData name=Levels",
                            "ExtendedData/SchemaData/SimpleData name=Height",
                            "ExtendedData/SchemaData/SimpleData name=Area Type",
                            "ExtendedData/SchemaData/SimpleData name=UDP_Saleab",
                            "ExtendedData/SchemaData/SimpleData name=CPBuilding",
                            "ExtendedData/SchemaData/SimpleData name=AreaScheme",
                            "ExtendedData/SchemaData/SimpleData name=CP_RATE",
                            "ExtendedData/SchemaData/SimpleData name=Persons",
                            "ExtendedData/SchemaData/SimpleData name=JobTarget",
                            "ExtendedData/SchemaData/SimpleData name=Perimeter",
                            "ExtendedData/SchemaData/SimpleData name=Elevation",
                            "ExtendedData/SchemaData/SimpleData name=Reference",
                            "ExtendedData/SchemaData/SimpleData name=Level",
                            "ExtendedData/SchemaData/SimpleData name=GSASpaceAr",
                            "ExtendedData/SchemaData/SimpleData name=Computatio",
                            "Polygon",
                            "Polygon/outerBoundaryIs",
                            "Polygon/outerBoundaryIs/LinearRing",
                            "Polygon/outerBoundaryIs/LinearRing/coordinates"
                        };

            RhinoApp.Write("<DEBUG(Save)");
            RhinoApp.Write("__oldCount:" + myp.rsn.Count + "/" + myp.ids.Count + "/k" + myp.kml.GetElementsByTagName("Placemark").Count);

            Rhino.DocObjects.ObjectEnumeratorSettings settings = new Rhino.DocObjects.ObjectEnumeratorSettings();
            IEnumerable<RhinoObject> rhinoobjects = doc.Objects.GetObjectList(settings);

            List<uint> _rsn = new List<uint>();
            List<Guid> _ids = new List<Guid>();
            List<int> _knr = new List<int>();
            const string xmlns = "http://www.opengis.net/kml/2.2";


            int akt = 0;
            int add = 0;
            foreach (RhinoObject obj in rhinoobjects)
            {
                int pos = myp.ids.IndexOf(obj.Id);
                if (pos == -1)
                    add++;
                else
                    pos = myp.knr[pos];
                _rsn.Add(obj.RuntimeSerialNumber);
                _ids.Add(obj.Id);
                _knr.Add(pos);
                akt++;
            }
            RhinoApp.Write("__AKT/ADD");
            RhinoApp.Write("__newCount:" + _rsn.Count + "/" + _ids.Count + "/k" + myp.kml.GetElementsByTagName("Placemark").Count);
            RhinoApp.Write("_Aktuell:" + akt);
            RhinoApp.Write("_Added:" + add);


            int del = 0;
            int mod = 0;
            for (int i = myp.ids.Count - 1; i >= 0; i--)
            {
                int _i = _ids.IndexOf(myp.ids[i]);
                if (_i == -1)
                {
                    string objname = "";
                    XmlNode placemark = myp.kml.GetElementsByTagName("Placemark")[i];
                    RhinoObject obj = doc.Objects.Find(myp.rsn[i]);
                    if (null == obj)
                        objname = "KILL";
                    else
                    {
                        if (obj.IsDeleted == false) RhinoApp.Write("__!!NotDeleted!!");     //should be deleted
                        objname = obj.Attributes.Name;
                    }

                    for (int k = 0; k < _knr.Count; k++) { if (_knr[k] > i) _knr[k]--; }
                    myp.rsn.RemoveAt(i);
                    myp.ids.RemoveAt(i);
                    placemark.ParentNode.RemoveChild(placemark);

                    const string deletedobjects = "PurgeArea";
                    XmlNodeList delobjlists = myp.kml.GetElementsByTagName(deletedobjects);

                    if (delobjlists.Count == 0)
                        myp.kml.GetElementsByTagName("kml")[0].AppendChild(myp.kml.CreateElement(deletedobjects, xmlns));

                    XmlElement newdelobj = myp.kml.CreateElement("Name", xmlns);
                    XmlAttribute attr = myp.kml.CreateAttribute("PurgeDate");
                    attr.InnerText = DateTime.Now.ToString("ddMMMyyyy", myp.cult);
                    newdelobj.InnerText = objname;
                    newdelobj.Attributes.Append(attr);
                    myp.kml.GetElementsByTagName(deletedobjects)[myp.kml.GetElementsByTagName(deletedobjects).Count - 1].AppendChild(newdelobj);

                    if (del + mod == 0) RhinoApp.Write("__"); else RhinoApp.Write(",");
                    RhinoApp.Write("del({0})\"{1}\"", i, objname);
                    del++;
                }
                else
                {
                    if (myp.rsn[i] != _rsn[_i])
                    {
                        RhinoObject obj = doc.Objects.Find(_rsn[_i]);
                        kmlUserData ud = obj.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
                        if (null != ud)
                        {
                            obj.Attributes.UserData.Remove(ud);
                            obj.Attributes.UserData.Add(doc.Objects.Find(myp.rsn[i]).Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData);
                        }
                        if (del + mod == 0) RhinoApp.Write("__"); else RhinoApp.Write(",");
                        RhinoApp.Write("mod({0}-{1})\"{2}\"", i, _i, obj.Attributes.Name);
                        mod++;
                    }
                }
            }

            RhinoApp.Write("__DEL/MOD");
            RhinoApp.Write("__Deleted:" + del + "_Modified:" + mod
                + "_oldCount:" + myp.rsn.Count + "/" + myp.ids.Count + "/k" + myp.kml.GetElementsByTagName("Placemark").Count + "__");


            if (!myp.finekmlfile)
            {
                myp.kml.LoadXml(
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                    "<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:kml=\"http://www.opengis.net/kml/2.2\" xmlns:xal=\"urn:oasis:names:tc:ciq:xsdschema:xAL:2.0\">\n" +
                    "<Document><Folder><name>Unnamed</name></Folder></Document></kml>");
                myp.finekmlfile = true;
            }


            for (int i = 0; i < _ids.Count; i++)
            {
                RhinoObject obj = doc.Objects.Find(_ids[i]);
                kmlUserData kud = obj.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
                GeometryBase geom = obj.Geometry;
                Extrusion extr = geom as Extrusion;
                if (null == extr)
                {
                }
                else
                {
                    if (_knr[i] == -1)
                    {
                        _knr[i] = myp.kml.GetElementsByTagName("Placemark").Count;
                        XmlElement pm = myp.kml.CreateElement("Placemark", xmlns);
                        foreach (string s in tab)
                        {
                            string selem = "/" + s;
                            string sattr = "";
                            if (s.Contains(' '))
                            {
                                selem = selem.Split(' ')[0];
                                sattr = s.Remove(0, selem.Length);
                            }
                            XmlElement child = myp.kml.CreateElement(selem.Split('/').Last(), xmlns);
                            if (sattr != "")
                            {
                                XmlAttribute a = myp.kml.CreateAttribute(sattr.Split('=')[0]);
                                a.InnerText = sattr.Split('=')[1];
                                child.Attributes.Append(a);
                            }
                            string directory = selem.Split('/')[selem.Split('/').Count() - 2];
                            if (directory == "")
                                pm.AppendChild(child);
                            else
                                pm.GetElementsByTagName(directory)[0].AppendChild(child);
                        }
                        if (_knr[i] != 0)
                            myp.kml.GetElementsByTagName("Placemark")[_knr[i] - 1].ParentNode.AppendChild(pm);
                        else
                        {
                            string[] where = { "Folder", "Document", "kml" };
                            int there = 0;
                            foreach (string s in where)
                            {
                                if (there == 0)
                                {
                                    there = myp.kml.GetElementsByTagName(s).Count;
                                    if (there != 0)
                                        myp.kml.GetElementsByTagName(s)[there - 1].AppendChild(pm);
                                }
                            }
                        }

                        if (obj.Attributes.Name == "") { obj.Attributes.Name = "Unnamed"; obj.CommitChanges(); }
                        while (myp.names.Contains(obj.Attributes.Name)) { obj.Attributes.Name += "-Renamed"; obj.CommitChanges(); }
                        myp.names.Add(obj.Attributes.Name);
                    }

                    NurbsSurface sur = extr.ToNurbsSurface();
                    double height = sur.Points.GetControlPoint(0, 0).Location.Z - sur.Points.GetControlPoint(0, 1).Location.Z;
                    double elevation = sur.Points.GetControlPoint(0, 1).Location.Z;
                    Polyline polygon = new Polyline();
                    for (int u = 0; u < sur.Points.CountU; u++) polygon.Add(sur.Points.GetControlPoint(u, 1).Location);
                    if (height < 0) { height *= -1; elevation -= height; }

                    //XmlNode extendeddata = myp.kml.GetElementsByTagName("ExtendedData")[_knr[i]];
                    //extendeddata.ParentNode.RemoveChild(extendeddata);
                    //XmlElement ed = myp.kml.CreateElement("ExtendedData", xmlns);

                    foreach (XmlNode element in myp.kml.GetElementsByTagName("Placemark")[_knr[i]])
                    {
                        switch (element.Name)
                        {
                            case "name":
                                element.InnerText = obj.Attributes.Name;
                                break;

                            case "Style":
                                element.FirstChild.FirstChild.InnerText = obj.Attributes.ObjectColor.ToArgb().ToString("x");
                                if (null != kud)
                                    element.LastChild.FirstChild.InnerText = kud.fill;
                                else
                                    element.LastChild.FirstChild.InnerText = "NoValue";
                                //if (element.LastChild.FirstChild.InnerText == "")
                                //    element.LastChild.FirstChild.InnerText = "0";
                                break;

                            case "ExtendedData":
                                double x = 0, y = 0, area = 0, tmp;
                                for (int i0 = 0; i0 < polygon.Count - 1; i0++)
                                {
                                    int i1 = (i0 + 1) % (polygon.Count - 1);
                                    tmp = polygon[i0].X * polygon[i1].Y -
                                          polygon[i1].X * polygon[i0].Y;
                                    x += (polygon[i0].X + polygon[i1].X) * tmp;
                                    y += (polygon[i0].Y + polygon[i1].Y) * tmp;
                                    area += tmp;
                                }
                                x /= area * 3;
                                y /= area * 3;
                                area /= 2;

                                if ((element.HasChildNodes) && (element.FirstChild.Name == "SchemaData"))
                                    foreach (XmlNode simpledata in element.FirstChild.ChildNodes)
                                    {
                                        if (null != kud)
                                            simpledata.InnerText = kud.exda[simpledata.Attributes.Item(0).InnerText];
                                        else
                                            simpledata.InnerText = "";
                                        switch (simpledata.Attributes.Item(0).InnerText)
                                        {
                                            case "Name":
                                                simpledata.InnerText = obj.Attributes.Name;
                                                break;
                                            //case "Description":
                                            //    if (simpledata.InnerText == "") simpledata.InnerText = "Created" + DateTime.Now.ToString("ddMMMyyyy", myp.cult);
                                            //    break;
                                            case "Height":
                                                simpledata.InnerText = height < 0.0011 / (myp.degree / myp.cancel) ? "0" : (height * (myp.degree / myp.cancel)).ToString("G14", myp.cult);
                                                //if (simpledata.InnerText.Length > 9) RhinoApp.Write("ALERT!:'{0}'_", obj.Attributes.Name);
                                                break;
                                            case "Elevation":
                                                simpledata.InnerText = (elevation * (myp.degree / myp.cancel)).ToString("G13", myp.cult);//max.G14(modified)/G16(original)
                                                //if (simpledata.InnerText.Length > 9) RhinoApp.Write("ALERT!:'{0}'_", obj.Attributes.Name);
                                                break;
#if calcvalues
#warning calcvalues@pmiSave
                                            case "Perimeter":
                                                //if (simpledata.InnerText == "")
                                                if (myp.calculate)
                                                    simpledata.InnerText = (polygon.Length * 1000 * (myp.degree / myp.cancel)).ToString("F0", myp.cult);
                                                //simpledata.InnerText = "~~" + (polygon.Length * 1000 * (myp.degree / myp.cancel)).ToString("F2", myp.cult);
                                                break;
                                            case "GSASpaceAr":
                                                if (myp.calculate)
                                                    simpledata.InnerText = Rhino.Geometry.AreaMassProperties.Compute(polygon.ToNurbsCurve()).Area.ToString("F2", myp.cult);
                                                //simpledata.InnerText += "~~" + (Rhino.Geometry.AreaMassProperties.Compute(
                                                //    polygon.ToNurbsCurve()).Area).ToString("G0", myp.cult)
                                                //          + "~~" + Math.Abs(area).ToString("G0", myp.cult);
                                                break;
                                            // * Math.Cos((y / myp.cancely) * (Math.PI / 180.0))
                                            //msdn.microsoft.com/de-de/library/kfsatb94%28v=vs.110%29.aspx
                                            //==>> s.IndexOf
                                            //simpledata.InnerText += "(" + (Rhino.Geometry.AreaMassProperties.Compute(polygon.ToNurbsCurve()).Area * 10000000000).ToString("G" + (simpledata.InnerText.Replace(".","").Length+1).ToString(), myp.cult) + ")";
                                            //simpledata.InnerText += "(" + (polygon.Length * 100000000).ToString("G" + (simpledata.InnerText.Replace(".", "").Length + 1).ToString(), myp.cult) + ")";
#endif
                                        }
                                    }
                                break;

                            case "Polygon":
                                string coordis = "";
                                polygon.ForEach(pt =>
                                    coordis += (coordis.Length > 0 ? " " : "") + (pt.X / (myp.cancelx * Math.Cos(myp.zero.Y * (Math.PI / 180.0))) + myp.zero.X).ToString("G17", myp.cult)
                                                                         + "," + (pt.Y / myp.cancely + myp.zero.Y).ToString("G17", myp.cult));
                                //if (element.FirstChild.FirstChild.FirstChild.InnerText!=coordis) coordis+="!!!";
                                element.FirstChild.FirstChild.LastChild.InnerText = coordis;
                                break;
                        }
                    }
                    //var attributes = new ObjectAttributes();
                    //attributes = obj.Attributes;
                    //doc.Objects.Delete(obj, false);
                    //doc.Objects.AddExtrusion(Extrusion.Create(polygon.ToNurbsCurve(), -height, true), attributes);
                }
            }
            doc.Views.Redraw();

            RhinoApp.Write("__oldCount:" + myp.rsn.Count + "/" + myp.ids.Count + "/k" + myp.kml.GetElementsByTagName("Placemark").Count);
            RhinoApp.Write("__newCount:" + _rsn.Count + "/" + _ids.Count + "/k" + myp.kml.GetElementsByTagName("Placemark").Count);
            myp.rsn = _rsn;
            myp.ids = _ids;
            myp.knr = _knr;

            Rhino.UI.SaveFileDialog fd = new Rhino.UI.SaveFileDialog();
            fd.Filter = "Keyhole Markup Language (*.kml)|*.kml";
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return Result.Cancel;

            try
            {
                myp.kml.Save(fd.FileName);
            }
            catch (XmlException e)
            {
                RhinoApp.WriteLine("__XmlSaveError: " + e.Message);
                return Result.Failure;
            }
            catch (System.IO.IOException io)
            {
                RhinoApp.WriteLine("__IOSaveError: " + io.Message);
                return Result.Failure;
            }

            RhinoApp.WriteLine("__(Save)DEBUG>");
            return Result.Success;
        }
    }
}
