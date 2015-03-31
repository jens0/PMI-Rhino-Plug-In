// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rhino;

namespace MyProject1
{
    public partial class Panel : UserControl
    {
        public Panel()
        {
            InitializeComponent();
            datagrid.ItemsSource = myp.gridrows;
        }

        void AddStoreyCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("pmiAddStorey", false);
        }

        void OpenCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("_Open", false);
        }

        void ImportCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("_Import", false);
        }

        void SaveCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("pmiSave", false);
        }

        void ExReadCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("pmiExtRead", false);
            datagrid.Items.Refresh();
            //CreatePolygon();
        }

        void ExWriteCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("pmiExtWrite", false);
            //this.panel.Children.Remove(yellowPolygon);
        }

        void HelpCl(object sender, RoutedEventArgs e)
        {
            PanelHelp panelhelp = new PanelHelp();
            panelhelp.ShowDialog();
        }

#if calcvalues
#warning calcvalues@Panel
        void calculate_Checked(object sender, RoutedEventArgs e)
        {
            myp.calculate = true;
        }

        void calculate_Unchecked(object sender, RoutedEventArgs e)
        {
            myp.calculate = false;
        }
#endif

#if testing
#warning testing@Panel
        static public Polygon yellowPolygon = new Polygon();
        private void CreateAPolygon()
        {
            yellowPolygon.Points.Clear();
            // Create a blue and a black Brush
            SolidColorBrush yellowBrush = new SolidColorBrush();
            yellowBrush.Color = Color.FromArgb(0x11, 0x11, 0x11, 0x11);
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            // Create a Polygon
            yellowPolygon.Stroke = blackBrush;
            yellowPolygon.Fill = yellowBrush;
            yellowPolygon.StrokeThickness = 4;

            // Create a collection of points for a polygon
            System.Windows.Point Point1 = new System.Windows.Point(50, 100);
            System.Windows.Point Point2 = new System.Windows.Point(200, 100);
            System.Windows.Point Point3 = new System.Windows.Point(200, 200);
            System.Windows.Point Point4 = new System.Windows.Point(300, 30);

            PointCollection polygonPoints = new PointCollection();
            polygonPoints.Add(Point1);
            polygonPoints.Add(Point2);
            polygonPoints.Add(Point3);
            polygonPoints.Add(Point4);

            // Set Polygon.Points properties
            yellowPolygon.Points = polygonPoints;

            // Add Polygon to the page
            this.panel.Children.Add(yellowPolygon);
        }
#endif
    }
}
