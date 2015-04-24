// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;
using System.Xml;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Commands;
using Rhino.Display;
using System.Drawing;
using System.Collections.Specialized;

namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("dd0e8bbd-e443-42e8-b2ab-a17144adbb0e")]
    [CommandStyle(Rhino.Commands.Style.ScriptRunner)]
    public class pmiOpen : Command
    {
        public pmiOpen() { }
        public override string EnglishName { get { return "pmiOpen"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.RunScript("_Open", false);
            return Result.Success;
        }
    }

    [System.Runtime.InteropServices.Guid("b16f8b8d-2633-47c6-82b8-abb501fb2631")]
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

    [System.Runtime.InteropServices.Guid("0240207b-f76e-4d5e-bc65-622036ed89e6")]
    [CommandStyle(Rhino.Commands.Style.ScriptRunner)]
    public class my : Rhino.PlugIns.FileImportPlugIn
    {
        public my()
        {
            Instance = this;
        }

        public static my Instance
        {
            get;
            private set;
        }

        protected override Rhino.PlugIns.FileTypeList AddFileTypes(Rhino.FileIO.FileReadOptions options)
        {
            var result = new Rhino.PlugIns.FileTypeList();
            result.AddFileType("Keyhole Markup Language (*.kml)", "kml");
            return result;
        }

        public static bool automatic = true;
        public static bool showasterisk = false;
        public static bool showdebug = false;
        public static bool colorbytype = false;
        public static bool setelevation = false;
        public static double newelevation = 0;
        public static bool setdisplaymode = true;
        public static bool setzoom = true;
        public static bool createname = false;
        public static bool updatename = true;
        public static bool creategeom = false;
        public static bool updategeom = false;

        public const double degreex = 111319.4917;
        public const double degreey = 111133.3333;
        public static string about = "";
        public static string kmldocname = "";
        public static bool finekmlfile = false;
        public static XmlDocument kml = new XmlDocument();
        public static List<int> knr = new List<int>();
        public static List<uint> rsn = new List<uint>();
        public static List<Guid> ids = new List<Guid>();
        public static List<string> names = new List<string>();
        public static System.Globalization.CultureInfo cult = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
        public class gridrow
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        public static int gridcount = 0;
        public static List<gridrow> gridrows = new List<gridrow>();
        public static bool fxdrawn = false;
        public static bool storeymode = false;
        public static readonly List<string> extnames = new List<string>() {
            "Name", "Description", "WKT", "Type", "FPA", "GFA", "Levels", "Height", "Area Type",
            "UDP_Saleab", "CPBuilding", "AreaScheme", "CP_RATE", "Persons", "JobTarget",
            "Perimeter", "Elevation", "Reference", "Level", "GSASpaceAr", "Computatio" };

        public static uint asteriskrsn = 0;
        public static bool asteriskdrawn = false;
        public static void removeasterisk(RhinoDoc doc)
        {
            if (asteriskdrawn)
            {
                if (my.asteriskrsn == 0)
                    my.note("marker:none ");
                else
                {
                    RhinoObject obj = doc.Objects.Find(my.asteriskrsn);
                    if (null == obj)
                        my.note("marker:null ");
                    else
                        if (obj.IsDeleted)
                            my.note("marker:del ");
                        else
                            my.note("marker:ok ");
                    if ((null != obj) && (!obj.IsDeleted)) doc.Objects.Delete(obj, false);
                    my.asteriskrsn = 0;
                }
                asteriskdrawn = false;
            }
        }
        public static void drawasterisk(RhinoDoc doc)
        {
            if (my.showasterisk)
            {
                my.asteriskrsn = doc.Objects.Find(doc.Objects.AddPoint(ob.center)).RuntimeSerialNumber;
                asteriskdrawn = true;
            }
        }

        public static void db(String s)
        {
            if (my.showdebug) RhinoApp.Write(s);
        }
        public static void dbln(String s)
        {
            if (my.showdebug) RhinoApp.WriteLine(s);
        }
        public static void note(String s)
        {
            //if (my.showdebug) RhinoApp.Write(s);
        }
        public static void noteln(String s)
        {
            //if (my.showdebug) RhinoApp.WriteLine(s);
        }

        public static bool ismeterinit = false;
        public static double meter = 1;
        private static readonly double[] unitconvert = {
                 1, 1.0e-6, 1.0e-3, 1.0e-2,      1,
            1.0e+3,2.54e-8,2.54e-5, 0.0254, 0.3048,
            1609.344,    1,1.0e-10, 1.0e-9, 1.0e-1,
            1.0e+1, 1.0e+2, 1.0e+6, 1.0e+9, 0.9144,
            0.0254/72, 0.0254/6, 1852, 1.4959787e+11, 9.46073e+15, 3.08567758e+16 };
        public static void initmeter(RhinoDoc doc)
        {
            int unitsystem = (int)doc.ModelUnitSystem;
            if (unitsystem >= 0 && unitsystem < unitconvert.Length)
                my.meter = unitconvert[unitsystem];
            else
                my.meter = 1;
            ismeterinit = true;
        }

        public static EarthAnchorPoint eap = new EarthAnchorPoint();
        public static Point3d zero = Point3d.Origin;
        public static bool initeapzero(RhinoDoc doc)
        {
            eap = doc.EarthAnchorPoint;
            if (eap.EarthBasepointLongitude != 0 || eap.EarthBasepointLatitude != 0 || eap.EarthBasepointElevation != 0)
            {
                zero = new Point3d(eap.EarthBasepointLongitude, eap.EarthBasepointLatitude, eap.EarthBasepointElevation);
                return true;
            }
            return false;
        }

        private readonly List<KeyValuePair<string, int>> buildingtype = new List<KeyValuePair<string, int>>() {
            new KeyValuePair<string, int>(""                                    , 0xfccde5),
            new KeyValuePair<string, int>("ADVANCED MANUFACTURING MAB"          , 0x8dd3c7),
            new KeyValuePair<string, int>("CAR PARK"                            , 0xfdb462),
            new KeyValuePair<string, int>("CARPARK - LONG TERM"                 , 0xbebada),
            new KeyValuePair<string, int>("COMMERCIAL / LIGHT INDUSTRIAL"       , 0xd9d9d9),
            new KeyValuePair<string, int>("COMMERCIAL / LIGHT INDUSTRIAL MAB"   , 0xccebc5),
            new KeyValuePair<string, int>("EDUCATION / TRAINING"                , 0xffffb3),
            new KeyValuePair<string, int>("GREEN SPACE"                         , 0xb3de69),
            new KeyValuePair<string, int>("RESIDENTIAL"                         , 0x80b1d3),
            new KeyValuePair<string, int>("RETAIL"                              , 0xbc80bd),
            new KeyValuePair<string, int>("RETAIL MAB"                          , 0xa6cee3),
            new KeyValuePair<string, int>("SUSTAINABLE LIGHT INDUSTRY"          , 0xfb8072) };

        protected override bool ReadFile(string filename, int index, RhinoDoc doc, Rhino.FileIO.FileReadOptions options)
        {
            if (options.ImportMode) my.db("<Import<"); else my.db("<Open<");
            try
            {
                kml.Load(filename);
            }
            catch (XmlException e)
            {
                my.dbln("__XmlException");
                RhinoApp.WriteLine("pmi: " + e.Message);
                return false;
            }
            catch (System.IO.IOException io)
            {
                my.dbln("__IOException");
                RhinoApp.WriteLine("pmi: " + io.Message);
                return false;
            }

            Rhino.DocObjects.ObjectEnumeratorSettings settings = new Rhino.DocObjects.ObjectEnumeratorSettings();
            IEnumerable<RhinoObject> rhinoobjects = doc.Objects.GetObjectList(settings);
            int current = 0;
            foreach (RhinoObject obj in rhinoobjects) { current++; }
            if (current != 0) my.db("__Current:" + current);

            my.db("__unit:" + doc.ModelUnitSystem);
            if (!options.ImportMode || (current == 0))
            {
                doc.AdjustModelUnitSystem((UnitSystem)4, false);
                my.db(">unit:" + doc.ModelUnitSystem);
            }
            my.initmeter(doc);
            my.db("__1" + doc.GetUnitSystemName(true, true, true, true) + "=" + my.meter.ToString("F4") + "m");

            finekmlfile = false;
            knr.Clear();
            rsn.Clear();
            ids.Clear();
            names.Clear();
            int totalplacemarks = 0;
            Point3d pt1, pt2, ptcentroid;
            pt1 = pt2 = ptcentroid = Point3d.Origin;
            bool zeroset = my.initeapzero(doc);

            if (my.showdebug)
            {
                my.db("__eap: ");
                my.db("x:" + eap.EarthBasepointLongitude + " y:" + eap.EarthBasepointLatitude +
                    " z:" + eap.EarthBasepointElevation + " z0:" + eap.EarthBasepointElevationZero);
                if (my.eap.Description != "") my.db(" / '" + my.eap.Description + "'");
                my.db(" / -x:" + zero.X + " -y:" + zero.Y + " -z:" + zero.Z);
                //my.db(" / x:" + eap.ModelEast.X + " y:" + eap.ModelEast.Y + " z:" + eap.ModelEast.Z);
                //my.db(" / x:" + eap.ModelNorth.X + " y:" + eap.ModelNorth.Y + " z:" + eap.ModelNorth.Z);
                //my.db(" / x:" + eap.ModelBasePoint.X + " y:" + eap.ModelBasePoint.Y + " z:" + eap.ModelBasePoint.Z);
                if (setelevation) my.db("__add " + newelevation + " m to elevation");
            }

            kmldocname = "";
            if (kml.GetElementsByTagName("Placemark").Count > 0)
                if (kml.GetElementsByTagName("Placemark")[0].ParentNode.FirstChild.Name == "name")
                    kmldocname = kml.GetElementsByTagName("Placemark")[0].ParentNode.FirstChild.InnerText;
            if (my.kmldocname != "" && my.eap.Description == "") my.eap.Description = my.kmldocname;

            foreach (XmlNode placemark in kml.GetElementsByTagName("Placemark"))
            {
                string name = "Unnamed";
                Color color = colorbytype ? Color.FromArgb(0xfccde5) : Color.Black;
                string pyfill = null;
                NameValueCollection exdata = new NameValueCollection();
                double height = 0;
                //double elevation = setelevation ? newelevation / my.meter : 0; elevation -= zero.Z / my.meter;
                double elevation = ((setelevation ? newelevation : 0) - zero.Z) / my.meter;
                Polyline polygon = new Polyline();
                bool polyfound = false;

                foreach (XmlNode element in placemark)
                {
                    switch (element.Name)
                    {
                        case "name":
                            if ((name == "") || (name == "Unnamed")) name = element.InnerText;
                            break;

                        case "Style":
                            if (element.FirstChild.Name == "LineStyle")
                                if (!colorbytype)
                                    color = Color.FromArgb(int.Parse(element.FirstChild.FirstChild.InnerText, System.Globalization.NumberStyles.AllowHexSpecifier));
                            if (element.LastChild.Name == "PolyStyle")
                                pyfill = element.LastChild.FirstChild.InnerText;
                            break;

                        case "ExtendedData":
                            if ((element.HasChildNodes) && (element.FirstChild.Name == "SchemaData"))
                                foreach (XmlNode simpledata in element.FirstChild)
                                {
                                    my.gridrow i = new my.gridrow
                                    {
                                        name = simpledata.Attributes.Item(0).InnerText,
                                        value = simpledata.InnerText
                                    };
                                    if ((i.name != "") && (!my.extnames.Contains(i.name)) || i.value != "")
                                        exdata.Add(i.name, i.value);
                                    switch (i.name)
                                    {
                                        case "Name":
                                            name = i.value;
                                            while (names.Contains(name)) name += "-Renamed";
                                            names.Add(name);
                                            break;
                                        case "Type":
                                            if (colorbytype)
                                                foreach (KeyValuePair<string, int> typ in buildingtype)
                                                    if (typ.Key == i.value)
                                                        color = Color.FromArgb(typ.Value);
                                            break;
                                        case "Height":
                                            height = double.Parse(i.value, cult) / my.meter;
                                            break;
                                        case "Elevation":
                                            elevation += double.Parse(i.value, cult) / my.meter;
                                            break;
                                    }
                                }
                            break;

                        case "Polygon":
                            polyfound = element.LastChild.LastChild.LastChild.Name == "coordinates";
                            if (polyfound)
                                foreach (string xy in element.LastChild.LastChild.LastChild.InnerText.Split(' '))
                                {
                                    Point3d pt = Point3d.Origin;
                                    string[] x_and_y = xy.Split(',');
                                    for (int i = 0; i < x_and_y.Length; i++) pt[i] = double.Parse(x_and_y[i], cult);
                                    if (!zeroset)
                                    {
                                        eap.EarthBasepointLongitude = zero.X = pt.X;
                                        eap.EarthBasepointLatitude = zero.Y = pt.Y;
                                        eap.EarthBasepointElevation = zero.Z = 0;
                                    }
                                    pt.X = (pt.X - zero.X) * my.degreex / my.meter * Math.Cos(zero.Y * (Math.PI / 180.0));
                                    pt.Y = (pt.Y - zero.Y) * my.degreey / my.meter;
                                    //if (x_and_y.Length == 2) pt.Z = elevation;
                                    polygon.Add(pt);
                                    if (!zeroset)
                                    {
                                        zeroset = true;
                                        pt1.X = pt2.X = pt.X;
                                        pt1.Y = pt2.Y = pt.Y;
                                    }
                                    if (pt.X < pt1.X) pt1.X = pt.X;
                                    if (pt.X > pt2.X) pt2.X = pt.X;
                                    if (pt.Y < pt1.Y) pt1.Y = pt.Y;
                                    if (pt.Y > pt2.Y) pt2.Y = pt.Y;
                                }
                            break;
                    }
                }

                if (polyfound)
                {
                    if (!finekmlfile) { pt1.Z = pt2.Z = elevation; finekmlfile = true; }
                    if (elevation < pt1.Z) pt1.Z = elevation;
                    if (elevation + height > pt2.Z) pt2.Z = elevation + height;
                    ptcentroid.X += polygon.First.X;
                    ptcentroid.Y += polygon.First.Y;
                    ptcentroid.Z += elevation + height / 2;

                    for (int p = 0; p < polygon.Count; p++) // add elevation only here
                        polygon[p] = new Point3d(polygon[p].X, polygon[p].Y, polygon[p].Z + elevation);

                    if (polygon.First != polygon.Last) polygon.Add(polygon.First); // if not closed, close it

                    double area = 0; // calc area to get vector
                    for (int p = 0; p < polygon.Count - 1; p++)
                        area += polygon[p].X * polygon[p + 1].Y - polygon[p + 1].X * polygon[p].Y;
                    int vector = area >= 0 ? 1 : -1;

                    ObjectAttributes attributes = new ObjectAttributes();
                    if (exdata.Count == 0) exdata = null;
                    if (null != pyfill || null != exdata)
                        attributes.UserData.Add(new kmlUserData(pyfill, exdata));
                    attributes.Name = name;
                    attributes.ObjectColor = color;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    attributes.WireDensity = -1;
                    if (height == 0) height = 0.001 / my.meter;
                    ids.Add(doc.Objects.AddExtrusion(Extrusion.Create(polygon.ToNurbsCurve(), height * vector, true), attributes));
                    rsn.Add(doc.Objects.Find(ids[ids.Count - 1]).RuntimeSerialNumber);
                    knr.Add(totalplacemarks);
                }
                totalplacemarks++;
            }

            ptcentroid.X /= ids.Count; // centroid point not used
            ptcentroid.Y /= ids.Count;
            ptcentroid.Z /= ids.Count;

            Point3d ptmean = new Point3d( // mean point used for zoom
                pt1.X + (pt2.X - pt1.X) / 2,
                pt1.Y + (pt2.Y - pt1.Y) / 2,
                pt1.Z + (pt2.Z - pt1.Z) / 2);
            pt1.X = ptmean.X; pt2.X = ptmean.X;
            pt1.Y = ptmean.Y; pt2.Y = ptmean.Y;
            pt1.Z = pt1.Z - 20 / my.meter; pt2.Z = pt2.Z + 20 / my.meter;
            BoundingBox box = new BoundingBox(pt1, pt2);
            //doc.Objects.AddBrep(box.ToBrep());

            if (finekmlfile)
            {
                DisplayModeDescription dmd;
                foreach (RhinoView view in doc.Views.GetViewList(true, false))
                    switch (view.ActiveViewport.Name)
                    {
                        case "Perspective":
                            if (setdisplaymode)
                            {
                                dmd = DisplayModeDescription.FindByName("Rendered");
                                if (null != dmd) view.ActiveViewport.DisplayMode = dmd;
                            }
                            if (setzoom && !options.ImportMode) view.ActiveViewport.ZoomExtents();
                            break;
                        case "Top":
                            if (setdisplaymode)
                            {
                                dmd = DisplayModeDescription.FindByName("X-Ray");
                                if (null != dmd) view.ActiveViewport.DisplayMode = dmd;
                            }
                            if (setzoom && !options.ImportMode) view.ActiveViewport.ZoomExtents();
                            break;
                        case "Front":
                            if (setdisplaymode)
                            {
                                dmd = DisplayModeDescription.FindByName("Artistic");
                                if (null != dmd) view.ActiveViewport.DisplayMode = dmd;
                            }
                            if (setzoom && !options.ImportMode) view.ActiveViewport.ZoomBoundingBox(box);
                            break;
                        case "Right":
                            if (setdisplaymode)
                            {
                                dmd = DisplayModeDescription.FindByName("Artistic");
                                if (null != dmd) view.ActiveViewport.DisplayMode = dmd;
                            }
                            if (setzoom && !options.ImportMode) view.ActiveViewport.ZoomBoundingBox(box);
                            break;
                    }
                doc.EarthAnchorPoint = my.eap;
            }
            //view.ClientToScreen//view.ScreenRectangle//view.ActiveViewport.Bounds
            //view.ActiveViewport.ScreenPortAspect//view.ActiveViewport.Size

            my.db("__Count:" + my.rsn.Count + "/k" + my.kml.GetElementsByTagName("Placemark").Count);
            if (totalplacemarks != knr.Count) my.db("/invalid:" + (totalplacemarks - knr.Count));
            if (options.ImportMode) my.dbln("__>Import>"); else my.dbln("__>Open>");
            doc.Views.Redraw();
            //doc.WriteFile-..
            //doc.ClearUndoRecords(true);
            //my.db("__note:" + doc.Notes);
            //doc.BeginUndoRecord("pmi");
            //doc.Modified = false;
            //doc.Fonts.Notes.Modified.Linetypes.GroundPlane
            return finekmlfile;
        }

        protected override Rhino.PlugIns.LoadReturnCode OnLoad(ref string errorMessage)
        {
            Rhino.UI.Panels.RegisterPanel(this, typeof(PanelHost), "PMI", Icon.FromHandle(MyProject1.Properties.Resources.icon16.GetHicon()));
            about = String.Format("PMI Rhino Plug-In, Version {0}.{1}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(),
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString());
            RhinoApp.WriteLine(about + " loaded, enter \"pmi\" to toggle panel");
            return Rhino.PlugIns.LoadReturnCode.Success;
        }
    }

    [System.Runtime.InteropServices.Guid("5a8bc436-3541-41d7-b76e-1c2785c537c2")]
    public class PanelHost : RhinoWindows.Controls.WpfElementHost
    {
        public PanelHost() : base(new Panel(), null) { }
        public static System.Guid PanelId { get { return typeof(PanelHost).GUID; } }
    }
}