// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;
using System.Xml;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Display;
using System.Drawing;
using System.Globalization;
using System.Collections.Specialized;

namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("00000000-0000-0000-0000-000000000000")]
    public class myp : Rhino.PlugIns.FileImportPlugIn
    {
        public myp()
        {
            Instance = this;
        }

        public static myp Instance
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

        public const double degreex = 111319.4917;
        public const double cancelx = 111319.4917;
        public const double degreey = 111133.3333;
        public const double cancely = 111133.3333;
        public const double degree = (degreex + degreey) / 2;   //111226.4125
        public const double cancel = (cancelx + cancely) / 2;

        static public string about = "";
        static public bool finekmlfile = false;
        static public XmlDocument kml = new XmlDocument();
        static public List<int> knr = new List<int>();
        static public List<uint> rsn = new List<uint>();
        static public List<Guid> ids = new List<Guid>();
        static public List<string> names = new List<string>();
        static public CultureInfo cult = CultureInfo.CreateSpecificCulture("en-US");
        static public Point3d zero = new Point3d(0, 0, 0);
        static public bool zeroset = false;
        public class gridrow
        {
            public string name { get; set; }
            public string value { get; set; }
        }
        static public List<gridrow> gridrows = new List<gridrow>() { };
        static public List<string> exnames = new List<string>()
                        {
                            "Name",
                            "Description",
                            "WKT",
                            "Type",
                            "FPA",
                            "GFA",
                            "Levels",
                            "Height",
                            "Area Type",
                            "UDP_Saleab",
                            "CPBuilding",
                            "AreaScheme",
                            "CP_RATE",
                            "Persons",
                            "JobTarget",
                            "Perimeter",
                            "Elevation",
                            "Reference",
                            "Level",
                            "GSASpaceAr",
                            "Computatio"
                        };
#if calcvalues
#warning calcvalues@PlugIn
        static public bool calculate = false;
#endif




        protected override bool ReadFile(string filename, int index, RhinoDoc doc, Rhino.FileIO.FileReadOptions options)
        {
            //int debug = 0;
            //RhinoApp.Write("_1:" + ++debug + "_");
            RhinoApp.Write("<DEBUG(Open)");

            finekmlfile = false;
            knr.Clear();
            rsn.Clear();
            ids.Clear();
            names.Clear();
            zeroset = false;

            Rhino.DocObjects.EarthAnchorPoint eap = doc.EarthAnchorPoint;
            if ((eap.EarthBasepointLongitude != 0) || (eap.EarthBasepointLatitude != 0))
            {
                zeroset = true;
                zero.X = eap.EarthBasepointLongitude;
                zero.Y = eap.EarthBasepointLatitude;
                zero.Z = 0;
            }

            //Rhino.DocObjects.EarthAnchorPoint eap2 = new Rhino.DocObjects.EarthAnchorPoint();
            //eap = Rhino.DocObjects.EarthAnchorPoint;
            RhinoApp.Write("__eap: ");
            RhinoApp.Write("x:" + eap.EarthBasepointLongitude + " y:" + eap.EarthBasepointLatitude + " z:" + eap.EarthBasepointElevation + " / ");
            RhinoApp.Write("x:" + eap.ModelEast.X + " y:" + eap.ModelEast.Y + " z:" + eap.ModelEast.Z + " / ");
            RhinoApp.Write("x:" + eap.ModelNorth.X + " y:" + eap.ModelNorth.Y + " z:" + eap.ModelNorth.Z + " / ");
            RhinoApp.Write("x:" + eap.ModelBasePoint.X + " y:" + eap.ModelBasePoint.Y + " z:" + eap.ModelBasePoint.Z);


            Rhino.DocObjects.ObjectEnumeratorSettings settings = new Rhino.DocObjects.ObjectEnumeratorSettings();
            IEnumerable<RhinoObject> rhinoobjects = doc.Objects.GetObjectList(settings);
            int akt = 0;
            foreach (RhinoObject obj in rhinoobjects) { akt++; }
            RhinoApp.Write("__Aktuell:" + akt);


            DisplayModeDescription dmd = DisplayModeDescription.FindByName("Rendered");//Rendered Pen Wireframe
            if (null != dmd)
                foreach (RhinoView view in doc.Views.GetViewList(true, true))
                    view.ActiveViewport.DisplayMode = dmd;

            try
            {
                kml.Load(filename);
            }
            catch (XmlException e)
            {
                RhinoApp.WriteLine("__XmlLoadError: " + e.Message);
                return false;
            }
            catch (System.IO.IOException io)
            {
                RhinoApp.WriteLine("__IOLoadError: " + io.Message);
                return false;
            }

            foreach (XmlNode placemark in kml.GetElementsByTagName("Placemark"))
            {
                string name = "Unnamed";
                Color color = Color.Red;
                string pyfill = "";
                double height = 0;
                double elevation = 0;
                Polyline polygon = new Polyline();
                bool coordfound = false;
                NameValueCollection exdata = new NameValueCollection();

                foreach (XmlNode element in placemark.ChildNodes)
                {
                    switch (element.Name)
                    {
                        case "name":
                            if ((name == "") || (name == "Unnamed")) name = element.InnerText;
                            break;

                        case "Style":
                            color = Color.FromArgb(int.Parse(element.FirstChild.FirstChild.InnerText, System.Globalization.NumberStyles.AllowHexSpecifier));
                            pyfill = element.LastChild.FirstChild.InnerText;
                            break;

                        case "ExtendedData":
                            if ((element.HasChildNodes) && (element.FirstChild.Name == "SchemaData"))
                                foreach (XmlNode simpledata in element.FirstChild.ChildNodes)
                                {
                                    exdata.Add(simpledata.Attributes.Item(0).InnerText, simpledata.InnerText);
                                    switch (simpledata.Attributes.Item(0).InnerText)
                                    {
                                        case "Name":
                                            name = simpledata.InnerText;
                                            while (names.Contains(name)) name += "-Renamed";
                                            names.Add(name);
                                            break;
                                        case "Height":
                                            height = double.Parse(simpledata.InnerText, cult);
                                            height /= (degree / cancel);
                                            break;
                                        case "Elevation":
                                            elevation = double.Parse(simpledata.InnerText, cult);
                                            elevation /= (degree / cancel);
                                            break;
                                    }
                                }
                            break;

                        case "Polygon":
                            coordfound = element.FirstChild.FirstChild.LastChild.Name == "coordinates";
                            if (coordfound)
                                foreach (string xy in element.FirstChild.FirstChild.LastChild.InnerText.Split(' '))
                                {
                                    Point3d pt = new Point3d(0, 0, 0);
                                    string[] x_and_y = xy.Split(',');
                                    for (int i = 0; i < x_and_y.Length; i++) pt[i] = double.Parse(x_and_y[i], cult);
                                    if (!zeroset) { zero = pt; zeroset = true; }
                                    pt.X = (pt.X - zero.X) * myp.cancelx * Math.Cos(zero.Y * (Math.PI / 180.0));
                                    pt.Y = (pt.Y - zero.Y) * myp.cancely;
                                    if (x_and_y.Length == 2) pt.Z = elevation;
                                    polygon.Add(pt);
                                }
                            break;
                    }
                }

                if (coordfound)
                {
                    ObjectAttributes attributes = new ObjectAttributes();
                    attributes.UserData.Add(new kmlUserData(pyfill, exdata));
                    attributes.Name = name;
                    attributes.WireDensity = -1;
                    attributes.ObjectColor = color;
                    attributes.ColorSource = ObjectColorSource.ColorFromObject;
                    if (height == 0) height = 0.001 / (degree / cancel);
                    doc.Objects.AddExtrusion(Extrusion.Create(polygon.ToNurbsCurve(), -height, true), attributes);

                    RhinoObject obj = doc.Objects.MostRecentObject();
                    knr.Add(ids.Count);
                    rsn.Add(obj.RuntimeSerialNumber);
                    ids.Add(obj.Id);
                    finekmlfile = true;
                }
            }

            if (finekmlfile)
                foreach (RhinoView view in doc.Views.GetViewList(true, true))
                    view.ActiveViewport.ZoomExtents();
            RhinoApp.WriteLine("__Count:" + myp.rsn.Count + "/" + myp.ids.Count + "/" + myp.kml.GetElementsByTagName("Placemark").Count + "__(Open)DEBUG>");
            doc.Views.Redraw();
            return finekmlfile;
        }

        protected override Rhino.PlugIns.LoadReturnCode OnLoad(ref string errorMessage)
        {
            about = String.Format("PMI Rhino Plug-In, Version {0}.{1}",
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(),
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString());
            RhinoApp.WriteLine("Loading " + about);
            Rhino.UI.Panels.RegisterPanel(this, typeof(PanelHost), "PMI", Icon.FromHandle(MyProject1.Properties.Resources.icon16.GetHicon()));
            return Rhino.PlugIns.LoadReturnCode.Success;
        }
    }

    [System.Runtime.InteropServices.Guid("11111111-2222-3333-4444-555555555555")]
    public class PanelHost : RhinoWindows.Controls.WpfElementHost
    {
        public PanelHost() : base(new Panel(), null) { }
        public static System.Guid PanelId { get { return typeof(PanelHost).GUID; } }
    }
}