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
    public partial class PanelOptions : Window
    {
        public PanelOptions()
        {
            InitializeComponent();
            this.DataContext = this;
            slider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(mouseLeftButtonDown), true);
            slider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(mouseLeftButtonUp), true);
        }

        static public bool automatic { get; set; }
        static public bool showasterisk { get; set; }
        static public bool showdebug { get; set; }
        static public bool colorbytype { get; set; }
        static public bool setelevation { get; set; }
        static public double newelevation { get; set; }
        static public double newmelevation { get; set; }
        static public bool setdisplaymode { get; set; }
        static public bool setzoom { get; set; }
        static public bool? createname { get; set; }
        static public bool? creategeom { get; set; }
        static public bool initslider = true;
        static public bool dragstarted = false;
        static public bool mouseleftbuttondown = false;

        void OK(object sender, RoutedEventArgs e) { DialogResult = true; }

        private void Test(object sender, RoutedEventArgs e)
        {
            //ob.ject.Select(false,true);
            //RhinoDoc.ActiveDoc.Views.Redraw();
            //MessageBox.Show(string.Format(
            //    "val: {0}, col: {1}, elv: {2}\n" +
            //    "val: {3}, col: {4}, elv: {5}\n" +
            //    "bool: {6}",
            //    creategeom, colorfromtype, newelevation.ToString("F2"),
            //    my.creategeom, my.colorfromtype, my.newelevation.ToString("F2"),
            //    newelevation
            //    ));
        }

        void valueChanged(object sender, RoutedEventArgs e)
        {
            if (!initslider)
            {
                if (!dragstarted)
                {
                    if (mouseleftbuttondown)
                        my.note("/C1");
                    else
                    {
                        my.note("/C0");
                        refreshelevation();
                    }
                }
                else
                    my.note("/-");
            }
            else
            {
                PanelOptions.initslider = false;
                my.note("/initslider");
            }
        }

        void dragStarted(object sender, RoutedEventArgs e)
        {
            dragstarted = true;
            my.note("/Ds");
            if (newelevation != newmelevation)
                refreshelevation();
        }

        void dragCompleted(object sender, RoutedEventArgs e)
        {
            dragstarted = false;
            my.note("/Dc");
            if (!mouseleftbuttondown)
                refreshelevation();
        }

        void mouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            mouseleftbuttondown = true;
            my.note("/Bs");
        }

        void mouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            mouseleftbuttondown = false;
            my.note("/Bc");
            if (newelevation != newmelevation)
                refreshelevation();
        }

        void refreshelevation()
        {
            my.note("^");
            newelevation = newmelevation;
            BindingExpression binding = enterelevation.GetBindingExpression(TextBox.TextProperty);
            binding.UpdateTarget();
        }
    }
}
