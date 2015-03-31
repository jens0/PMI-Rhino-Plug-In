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
    public partial class PanelHelp : Window
    {
        public PanelHelp()
        {
            InitializeComponent();
            this.Resources.Add("about", myp.about);
        }

        private void navHyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

#if testing
#warning testing@PanelHelp
        private void Window_FocusableChanged(object sender, RoutedEventArgs e)
        {
            RhinoApp.WriteLine("Focus");
        }
#endif
    }
}
