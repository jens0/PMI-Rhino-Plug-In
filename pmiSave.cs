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
    [System.Runtime.InteropServices.Guid("35c5b659-0325-446f-a57c-6615287fbe74")]
    public class pmiSave : Command
    {
        public pmiSave() { }
        public override string EnglishName { get { return "pmiSave"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            my.db(String.Format("<{0}<", EnglishName));

            my.initmeter(doc);
            my.initeapzero(doc);
            if (my.eap.Description != "") my.kmldocname = my.eap.Description;
            if (my.kmldocname != "") my.db("__'" + my.kmldocname + "'");

            my.db("__oldCount:" + my.rsn.Count + "/k" + my.kml.GetElementsByTagName("Placemark").Count);

            Rhino.DocObjects.ObjectEnumeratorSettings settings = new Rhino.DocObjects.ObjectEnumeratorSettings();
            IEnumerable<RhinoObject> rhinoobjects = doc.Objects.GetObjectList(settings);

            List<uint> _rsn = new List<uint>();
            List<Guid> _ids = new List<Guid>();
            List<int> _knr = new List<int>();

            int current = 0;
            int added = 0;
            foreach (RhinoObject obj in rhinoobjects)
            //if (obj.RuntimeSerialNumber != my.asteriskrsn)
            {
                int pos = my.ids.IndexOf(obj.Id);
                if (pos == -1)
                    added++;
                else
                    pos = my.knr[pos];
                _rsn.Add(obj.RuntimeSerialNumber);
                _ids.Add(obj.Id);
                _knr.Add(pos);
                current++;
            }
            my.db("__AKT/ADD");
            my.db("__newCount:" + _rsn.Count + "/k" + my.kml.GetElementsByTagName("Placemark").Count);
            my.db("_Current:" + current);
            my.db("_Added:" + added);


            int del = 0;
            int mod = 0;
            for (int i = my.ids.Count - 1; i >= 0; i--)
            {
                int _i = _ids.IndexOf(my.ids[i]);
                if (_i == -1)
                {
                    string objname;
                    RhinoObject obj = doc.Objects.Find(my.rsn[i]);
                    if (null == obj)
                        objname = "traceless";
                    else
                    {
                        if (obj.IsDeleted == false) my.db("__!!should be deleted!!");
                        objname = obj.Attributes.Name;
                    }

                    my.rsn.RemoveAt(i);
                    my.ids.RemoveAt(i);

                    if (my.knr[i] != -1)
                    {
                        for (int k = 0; k < _knr.Count; k++) { if (_knr[k] > i) _knr[k]--; }

                        XmlNode placemark = my.kml.GetElementsByTagName("Placemark")[i];
                        placemark.ParentNode.RemoveChild(placemark);

                        const string deletedobjects = "PurgeArea";
                        XmlNodeList delobjlists = my.kml.GetElementsByTagName(deletedobjects);

                        if (delobjlists.Count == 0)
                            my.kml.GetElementsByTagName("kml")[0].AppendChild(my.kml.CreateElement(deletedobjects, xmlns));

                        XmlElement newdelobj = my.kml.CreateElement("Name", xmlns);
                        XmlAttribute attr = my.kml.CreateAttribute("PurgeDate");
                        attr.InnerText = DateTime.Now.ToString("ddMMMyyyy", my.cult);
                        newdelobj.InnerText = objname;
                        newdelobj.Attributes.Append(attr);
                        my.kml.GetElementsByTagName(deletedobjects)[my.kml.GetElementsByTagName(deletedobjects).Count - 1].AppendChild(newdelobj);
                    }
                    if (del + mod == 0) my.db("__"); else my.db(",");
                    my.db(String.Format("del({0})\"{1}\"", i, objname));
                    del++;
                }
                else
                {
                    if (my.rsn[i] != _rsn[_i])
                    {
                        RhinoObject obj = doc.Objects.Find(_rsn[_i]);
                        //kmlUserData ud = obj.Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData;
                        //if (null != ud)
                        //{
                        //    obj.Attributes.UserData.Remove(ud);
                        //    obj.Attributes.UserData.Add(doc.Objects.Find(my.rsn[i]).Attributes.UserData.Find(typeof(kmlUserData)) as kmlUserData);
                        //}
                        if (del + mod == 0) my.db("__"); else my.db(",");
                        my.db(String.Format("mod({0}-{1})\"{2}\"", i, _i, obj.Attributes.Name));
                        mod++;
                    }
                }
            }

            my.db("__DEL/MOD");
            my.db("__Deleted:" + del + "_Modified:" + mod
                + "_oldCount:" + my.rsn.Count + "/k" + my.kml.GetElementsByTagName("Placemark").Count + "__");


            if (!my.finekmlfile)
            {
                if (my.kmldocname == "") my.kmldocname = "Untitled";
                my.kml.LoadXml(
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                    "<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:kml=\"http://www.opengis.net/kml/2.2\" xmlns:xal=\"urn:oasis:names:tc:ciq:xsdschema:xAL:2.0\">\n" +
                    "<Document><Folder><name>" + my.kmldocname + "</name></Folder></Document></kml>");
                my.finekmlfile = true;
            }


            for (int i = 0; i < _ids.Count; i++)
            {
                if ("" != ob.verify(doc.Objects.Find(_ids[i]), false))
                {
                }
                else
                {
                    if (_knr[i] == -1)
                    {
                        string schema = "";
                        if (my.kmldocname != "") schema = " schemaUrl=#" + my.kmldocname;
                        string[] placemarkstructure = {
                            "name",
                            "Style",
                            "Style/LineStyle",
                            "Style/LineStyle/color",
                            "Style/PolyStyle",
                            "Style/PolyStyle/fill",
                            "ExtendedData",
                            "ExtendedData/SchemaData" + schema,
                            "Polygon",
                            "Polygon/outerBoundaryIs",
                            "Polygon/outerBoundaryIs/LinearRing",
                            "Polygon/outerBoundaryIs/LinearRing/coordinates" };
                        _knr[i] = my.kml.GetElementsByTagName("Placemark").Count;
                        XmlElement pm = my.kml.CreateElement("Placemark", xmlns);
                        foreach (string s in placemarkstructure)
                            if (!s.Contains("PolyStyle") || (null != ob.kud && null != ob.kud.fill))
                            {
                                string elem = "/" + s;
                                string attr = "";
                                if (s.Contains(' '))
                                {
                                    elem = elem.Split(' ')[0];
                                    attr = s.Remove(0, elem.Length);
                                }
                                XmlElement child = my.kml.CreateElement(elem.Split('/').Last(), xmlns);
                                if (attr != "")
                                {
                                    XmlAttribute a = my.kml.CreateAttribute(attr.Split('=')[0]);
                                    a.InnerText = attr.Split('=')[1];
                                    child.Attributes.Append(a);
                                }
                                string directory = elem.Split('/')[elem.Split('/').Count() - 2];
                                if (directory == "")
                                    pm.AppendChild(child);
                                else
                                    pm.GetElementsByTagName(directory)[0].AppendChild(child);
                            }
                        if (_knr[i] != 0)
                            my.kml.GetElementsByTagName("Placemark")[_knr[i] - 1].ParentNode.AppendChild(pm);
                        else
                        {
                            string[] where = { "Folder", "Document", "kml" };
                            int there = 0;
                            foreach (string s in where)
                            {
                                if (there == 0)
                                {
                                    there = my.kml.GetElementsByTagName(s).Count;
                                    if (there != 0)
                                        my.kml.GetElementsByTagName(s)[there - 1].AppendChild(pm);
                                }
                            }
                        }

                        if (ob.ject.Attributes.Name == "") { ob.ject.Attributes.Name = "Unnamed"; ob.ject.CommitChanges(); }
                        while (my.names.Contains(ob.ject.Attributes.Name)) { ob.ject.Attributes.Name += "-Renamed"; ob.ject.CommitChanges(); }
                        my.names.Add(ob.ject.Attributes.Name);
                    }

                    int element_count = 0;
                    int poly_pos = -1;
                    int exda_pos = -1;
                    int schema_childs = -1;
                    XmlNode placemark = my.kml.GetElementsByTagName("Placemark")[_knr[i]];
                    my.note("__" + ob.ject.Name);
                    foreach (XmlNode element in placemark)
                    {
                        switch (element.Name)
                        {
                            case "name":
                                element.InnerText = ob.ject.Attributes.Name;
                                break;

                            case "Style":
                                element.FirstChild.FirstChild.InnerText = ob.ject.Attributes.ObjectColor.ToArgb().ToString("x8");
                                if (null != ob.kud && null != ob.kud.fill)
                                    element.LastChild.FirstChild.InnerText = ob.kud.fill;
                                break;

                            case "ExtendedData":
                                exda_pos = element_count;
                                if (element.HasChildNodes && element.FirstChild.Name == "SchemaData")
                                    schema_childs = element.FirstChild.ChildNodes.Count;
                                break;

                            case "Polygon":
                                poly_pos = element_count;
                                string s = "";
                                ob.polygon.ForEach(pt => s += (s.Length > 0 ? " " : "") +
                                    (my.zero.X + pt.X * my.meter / my.degreex / Math.Cos(my.zero.Y * (Math.PI / 180.0))).ToString("G17", my.cult) + "," +
                                    (my.zero.Y + pt.Y * my.meter / my.degreey).ToString("G17", my.cult));
                                element.LastChild.LastChild.LastChild.InnerText = s;
                                break;
                        }
                        element_count++;
                    }

                    my.note("[" + exda_pos + "/" + schema_childs);
                    XmlNode extendeddata = placemark.ChildNodes[exda_pos];
                    if (null == extendeddata) my.note("/null");
                    if (schema_childs > 0)
                        while (extendeddata.FirstChild.HasChildNodes)
                            extendeddata.FirstChild.RemoveChild(extendeddata.FirstChild.FirstChild);
                    ob.collectinfo();
                    if (ob.hasexda || my.createname || my.creategeom
                        || ob.elevation * my.meter + my.zero.Z != 0 || ob.height * my.meter >= 0.0011)
                    {
                        if (schema_childs == -1)
                        {
                            XmlElement schemadata = createelement("SchemaData", "schemaUrl", '#' + (my.kmldocname != "" ? my.kmldocname : "unknown"), "");
                            if (exda_pos == -1)
                            {
                                XmlElement extendeddatanew = my.kml.CreateElement("ExtendedData", xmlns);
                                extendeddatanew.AppendChild(schemadata);
                                placemark.InsertBefore(extendeddatanew, placemark.ChildNodes[poly_pos]);
                                exda_pos = poly_pos;
                                extendeddata = placemark.ChildNodes[exda_pos];
                                element_count++; // not needed anymore
                                if (poly_pos == -1) my.note("/PolyNotFound");
                                my.note("/new");
                            }
                            else
                            {
                                extendeddata.PrependChild(schemadata);
                                my.note("/fixschema");
                            }
                        }

                        string s = "";
                        double levels = 1;
                        if (ob.hasexda && null != (s = ob.kud.exda["Levels"]))
                            levels = double.Parse(s, my.cult);
                        double area = ob.area * my.meter * my.meter;
                        foreach (String name in my.extnames)
                        {
                            string value = "";
                            if (ob.hasexda && null != ob.kud.exda[name]) value = ob.kud.exda[name];
                            switch (name)
                            {
                                case "Name": if (value != "" && my.updatename || my.createname)
                                        value = ob.ject.Attributes.Name; break;
                                case "FPA": if (value != "" && my.updategeom || my.creategeom)
                                        value = area.ToString("F2", my.cult); break;
                                case "GFA": if (value != "" && my.updategeom || my.creategeom)
                                        value = (area * levels).ToString("F0", my.cult) + " m2"; break;
                                case "Height": if (value != "" || ob.height * my.meter >= 0.0011)
                                        value = ob.height * my.meter < 0.0011 ? "0" : (ob.height * my.meter).ToString("G14", my.cult); break;
                                case "Elevation": if (value != "" || ob.elevation * my.meter + my.zero.Z != 0)
                                        value = (ob.elevation * my.meter + my.zero.Z).ToString("G13", my.cult); break;
                                case "Perimeter": if (value != "" && my.updategeom || my.creategeom)
                                        value = (ob.polygon.Length * my.meter * 1000).ToString("F0"); break;
                                case "GSASpaceAr": if (value != "" && my.updategeom || my.creategeom)
                                        value = area.ToString("F2", my.cult); break;
                            }
                            if (value != "")
                                extendeddata.FirstChild.AppendChild(createelement("SimpleData", "name", name, value));
                        }
                        if (ob.hasexda)
                            foreach (String name in ob.kud.exda.AllKeys)
                                if (!my.extnames.Contains(name))
                                    extendeddata.FirstChild.AppendChild(createelement("SimpleData", "name", name, ob.kud.exda[name]));
                    }
                    else
                    {
                        if (exda_pos != -1)
                            my.note("/kill:" + extendeddata.ChildNodes.Count + "<=" + (schema_childs != -1 ? 1 : 0) + " ");
                        else
                            my.note("/nokill");
                        if (exda_pos != -1 && extendeddata.ChildNodes.Count <= (schema_childs != -1 ? 1 : 0))
                            placemark.RemoveChild(extendeddata);
                    }
                    my.note("]");

                    //ObjectAttributes attributes = new ObjectAttributes();
                    //attributes = ob.ject.Attributes;
                    //doc.Objects.Delete(ob.ject, false);
                    //doc.Objects.AddExtrusion(Extrusion.Create(ob.polygon.ToNurbsCurve(), ob.height * ob.vector, true), attributes);
                    ////doc.Objects.AddPolyline(ob.polygon, attributes);
                }
            }
            doc.Views.Redraw();

            my.db("__oldCount:" + my.rsn.Count + "/k" + my.kml.GetElementsByTagName("Placemark").Count);
            my.db("__newCount:" + _rsn.Count + "/k" + my.kml.GetElementsByTagName("Placemark").Count);
            my.rsn = _rsn;
            my.ids = _ids;
            my.knr = _knr;

            Rhino.UI.SaveFileDialog fd = new Rhino.UI.SaveFileDialog();
            fd.Filter = "Keyhole Markup Language (*.kml)|*.kml";
            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                my.dbln("__Cancel");
                return Result.Cancel;
            }

            try
            {
                my.kml.Save(fd.FileName);
            }
            catch (XmlException e)
            {
                my.dbln("__XmlException");
                RhinoApp.WriteLine("pmi: " + e.Message);
                return Result.Failure;
            }
            catch (System.IO.IOException io)
            {
                my.dbln("__IOException");
                RhinoApp.WriteLine("pmi: " + io.Message);
                return Result.Failure;
            }

            my.dbln(String.Format("__>{0}>", EnglishName));

            return Result.Success;
        }

        XmlElement createelement(string elementname, string attributename, string name, string value)
        {
            XmlElement element = my.kml.CreateElement(elementname, xmlns);
            XmlAttribute attribute = my.kml.CreateAttribute(attributename);
            attribute.InnerText = name;
            element.InnerText = value;
            element.Attributes.Append(attribute);
            return element;
        }
        const string xmlns = "http://www.opengis.net/kml/2.2";
    }
}
