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
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using System.ComponentModel;
namespace MyProject1
{
    public partial class Panel : UserControl
    {
        public Panel()
        {
            InitializeComponent();
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(200));
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(60000));
            m_select_objects_handler = new EventHandler<RhinoObjectSelectionEventArgs>(OnSelectObjects);
            m_deselect_all_objects_handler = new EventHandler<RhinoDeselectAllObjectsEventArgs>(OnDeselectAllObjects);
            m_deselect_objects_handler = new EventHandler<RhinoObjectSelectionEventArgs>(OnDeselectObjects);
            button_get.Click += ExReadCl;
            button_set.Click += ExWriteCl;
            button_get.Style = (System.Windows.Style)this.Resources["GlassButton"];
            button_set.Style = (System.Windows.Style)this.Resources["GlassButton"];
            if (my.automatic)
                automaticON();
            else
            {
                this.panel.Children.Add(button_get);
                this.panel.Children.Add(button_set);
            }
        }

        void gridload(object sender, RoutedEventArgs e)
        {
            initgrid();
            DataGrid grid = sender as DataGrid;
            grid.ItemsSource = my.gridrows;
        }

        public void initgrid()
        {
            my.gridrows.Clear();
            foreach (String name in my.extnames) my.gridrows.Add(new my.gridrow { name = name, value = "" });
        }

        public void init0grid()
        {
            my.gridrows.Clear();
            my.gridrows.Add(new my.gridrow { name = "", value = "" });
        }

        private readonly EventHandler<RhinoObjectSelectionEventArgs> m_select_objects_handler;
        private readonly EventHandler<RhinoDeselectAllObjectsEventArgs> m_deselect_all_objects_handler;
        private readonly EventHandler<RhinoObjectSelectionEventArgs> m_deselect_objects_handler;

        public void automaticON()
        {
            RhinoDoc.SelectObjects += m_select_objects_handler;
            RhinoDoc.DeselectObjects += m_deselect_objects_handler;
            RhinoDoc.DeselectAllObjects += m_deselect_all_objects_handler;
        }

        public void automaticOFF()
        {
            RhinoDoc.SelectObjects -= m_select_objects_handler;
            RhinoDoc.DeselectObjects -= m_deselect_objects_handler;
            RhinoDoc.DeselectAllObjects -= m_deselect_all_objects_handler;
        }

        Button button_get = new Button
        {
            Content = (char)206,
            Margin = new Thickness(12, 0, 0, 12),
            FontFamily = new FontFamily("Wingdings 3"),
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Foreground = new SolidColorBrush { Color = Color.FromRgb(0x50, 0x50, 0x50) },
            Background = new SolidColorBrush { Color = Color.FromRgb(0xc7, 0xe5, 0xf7) },
            BorderBrush = new SolidColorBrush { Color = Color.FromRgb(0xeb, 0xef, 0xf7) },
            Width = 23,
            Height = 23
        };

        Button button_set = new Button
        {
            Content = (char)201,
            Margin = new Thickness(41, 0, 0, 12),
            FontFamily = new FontFamily("Wingdings 3"),
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Foreground = new SolidColorBrush { Color = Color.FromRgb(0x50, 0x50, 0x50) },
            Background = new SolidColorBrush { Color = Color.FromRgb(0xc7, 0xe5, 0xf7) },
            BorderBrush = new SolidColorBrush { Color = Color.FromRgb(0xeb, 0xef, 0xf7) },
            Width = 23,
            Height = 23
        };

        void OnSelectObjects(object sender, Rhino.DocObjects.RhinoObjectSelectionEventArgs e)
        {
            if (my.automatic)
            {
                my.note("pmi: ** EVENT: Select Objects: " + e.RhinoObjects.Length + "  **");
                if (e.Selected)
                {
                    if (null != ob.ject) my.note("  !!not null!!  **");
                    if (e.RhinoObjects.Length == 1)
                    {
                        if ((null != ob.ject) && (e.RhinoObjects[0].Id == ob.ject.Id))
                            my.noteln("  allready loaded: " + e.RhinoObjects[0].Name + "  **");
                        //else
                        {
                            my.note("  read: " + e.RhinoObjects[0].Name + "  ");
                            datagrid.ItemsSource = null;
                            RhinoApp.RunScript("pmiExtRead", false);
                            datagrid.ItemsSource = my.gridrows;
                            //datagrid.Items.Refresh();
                            //datagrid.UpdateLayout();
                            my.gridcount = my.gridrows.Count;
                            removefxpoly();
                            if (ob.polygon.Count != 0) drawfxpoly();
                        }
                    }
                    else
                        my.noteln("  **");
                }
                else
                {
                    foreach (RhinoObject obj in e.RhinoObjects)
                    {
                        my.note("  !!" + obj.Name + " was deselected!!");
                    }
                    my.noteln("  **");
                }
            }
        }

        void OnDeselectObjects(object sender, Rhino.DocObjects.RhinoObjectSelectionEventArgs e)
        {
            if (my.automatic)
            {
                my.noteln("pmi: ** EVENT: Deselect Objects **");
                if (null != ob.ject)
                    foreach (RhinoObject obj in e.RhinoObjects)
                    {
                        if (obj.Id == ob.ject.Id)
                        {
                            deselect();
                        }
                    }
            }
        }

        void OnDeselectAllObjects(object sender, Rhino.DocObjects.RhinoDeselectAllObjectsEventArgs e)
        {
            if (my.automatic)
            {
                my.noteln("pmi: ** EVENT: Deselect ALL **");
                if (null != ob.ject)
                {
                    deselect();
                }
            }
        }

        public void deselect()
        {
            IEditableCollectionView gridview = datagrid.Items;
            if (gridview.IsAddingNew || gridview.IsEditingItem)
            {
                my.note("<CommitEdit>");
                datagrid.CommitEdit(DataGridEditingUnit.Row, true);
            }
            my.note("<CommitEnd>");
            removefxpoly();
            datagrid.ItemsSource = null;
            initgrid();
            datagrid.ItemsSource = my.gridrows;
            //datagrid.Items.Refresh();
            //datagrid.UpdateLayout();
            //CollectionViewSource.GetDefaultView(datagrid.ItemsSource).Refresh();
            if (!my.storeymode) ob.ject = null;
        }

        void currentCellChanged(object sender, EventArgs e)
        {
            if (my.automatic)
            {
                int i = my.gridrows.Count;
                if (i < my.gridcount)
                {
                    my.noteln("(" + my.gridcount + "=>" + i + ")");
                    my.gridcount = i;
                    if (null != ob.ject) pmiExtWrite.extdatawrite();
                }
            }
        }

        void beginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (my.automatic)
            {
                my.note("[");
            }
        }

        void cellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (my.automatic)
            {
                IEditableCollectionView gridview = datagrid.Items;
                if (gridview.IsAddingNew || gridview.IsEditingItem)
                {
                    my.note("yes");
                }
                else
                    my.note("no");
                my.note("]");
                if (null != ob.ject)
                {
                    var element = e.EditingElement as TextBox;
                    my.note("write: " + ob.ject.Name + "(" + e.Column.DisplayIndex + "/" + e.Row.GetIndex() + ")" + element.Text + "{" + e.EditAction + "}");
                    my.note("  <");
                    pmiExtWrite.extdatawrite();
                    my.noteln(">");
                }
                else
                    my.noteln("no obj to write");
            }
        }

        public void ExReadCl(object sender, RoutedEventArgs e)
        {
            if (!my.automatic)
            {
                RhinoApp.RunScript("pmiExtRead", false);
                datagrid.Items.Refresh();
                my.gridcount = my.gridrows.Count;
                removefxpoly();
                if (ob.polygon.Count != 0) drawfxpoly();
            }
            else
            {
                my.dbln("disabled");
            }
        }

        public void ExWriteCl(object sender, RoutedEventArgs e)
        {
            if (!my.automatic)
            {
                RhinoApp.RunScript("pmiExtWrite", false);
            }
            else
            {
                my.dbln("disabled");
            }
        }

        void AddStoreyCl(object sender, RoutedEventArgs e)
        {
            RhinoApp.RunScript("pmiAddStorey", false);
            if (my.storeymode && my.automatic) ob.ject.Select(true);
            my.storeymode = false;
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

        void HelpCl(object sender, RoutedEventArgs e)
        {
            PanelHelp panelhelp = new PanelHelp();
            panelhelp.ShowDialog();
        }

        void OptionsCl(object sender, RoutedEventArgs e)
        {
            PanelOptions.automatic = my.automatic;
            PanelOptions.showasterisk = my.showasterisk;
            PanelOptions.showdebug = my.showdebug;
            PanelOptions.colorbytype = my.colorbytype;
            PanelOptions.setelevation = my.setelevation; // <--elevation stuff
            PanelOptions.newelevation = my.newelevation;
            if (my.newelevation < -50) PanelOptions.newmelevation = -50;
            else if (my.newelevation > 50) PanelOptions.newmelevation = 50;
            else PanelOptions.newmelevation = my.newelevation;
            PanelOptions.initslider = PanelOptions.newmelevation != 0;
            PanelOptions.dragstarted = false;
            PanelOptions.mouseleftbuttondown = false; // elevation stuff-->
            PanelOptions.setdisplaymode = my.setdisplaymode;
            PanelOptions.setzoom = my.setzoom;
            if (my.createname) PanelOptions.createname = true;
            else if (my.updatename) PanelOptions.createname = null;
            else PanelOptions.createname = false;
            if (my.creategeom) PanelOptions.creategeom = true;
            else if (my.updategeom) PanelOptions.creategeom = null;
            else PanelOptions.creategeom = false;
            PanelOptions paneloptions = new PanelOptions();
            paneloptions.ShowDialog();
            if (paneloptions.DialogResult.HasValue && paneloptions.DialogResult.Value)
            {
                if (my.automatic != PanelOptions.automatic)
                {
                    my.automatic = PanelOptions.automatic;
                    if (my.automatic)
                    {
                        this.panel.Children.Remove(button_get);
                        this.panel.Children.Remove(button_set);
                        automaticON();
                        if (null != ob.ject)
                        {
                            removefxpoly();
                            initgrid();
                            datagrid.Items.Refresh();
                            ob.ject = null;
                        }
                    }
                    else
                    {
                        automaticOFF();
                        this.panel.Children.Add(button_get);
                        this.panel.Children.Add(button_set);
                    }
                }
                //if (my.showasterisk != PanelOptions.showasterisk)
                //{
                //    my.removeasterisk(Rhino.RhinoDoc.ActiveDoc);
                //    if (my.showasterisk) Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
                //    my.showasterisk = PanelOptions.showasterisk;
                //    //if (my.showasterisk) my.drawasterisk(Rhino.RhinoDoc.ActiveDoc);
                //}
                my.showasterisk = PanelOptions.showasterisk;
                my.showdebug = PanelOptions.showdebug;
                my.colorbytype = PanelOptions.colorbytype;
                my.setelevation = PanelOptions.setelevation;
                my.newelevation = PanelOptions.newelevation;
                my.setdisplaymode = PanelOptions.setdisplaymode;
                my.setzoom = PanelOptions.setzoom;
                if (PanelOptions.createname == true) { my.createname = true; my.updatename = true; }
                else if (PanelOptions.createname == null) { my.creategeom = false; my.updatename = true; }
                else { my.createname = false; my.updatename = false; }
                if (PanelOptions.creategeom == true) { my.creategeom = true; my.updategeom = true; }
                else if (PanelOptions.creategeom == null) { my.creategeom = false; my.updategeom = true; }
                else { my.creategeom = false; my.updategeom = false; }
            }
        }

        Polygon fxpoly = new Polygon
        {
            StrokeThickness = 0.5,
            StrokeMiterLimit = 1.414,
            Stroke = new SolidColorBrush { Color = Color.FromArgb(0x80, 0x54, 0x64, 0x80) },
            Fill = new SolidColorBrush { Color = Color.FromArgb(0x80, 0xd0, 0xd6, 0xe2) }
        };

        TextBlock fxtext = new TextBlock
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 15,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = new SolidColorBrush { Color = Color.FromRgb(0x50, 0x50, 0x50) },
            Margin = new Thickness(20, 150, 20, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextAlignment = TextAlignment.Left,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                RenderingBias = System.Windows.Media.Effects.RenderingBias.Performance,
                Color = Color.FromArgb(0x80, 0xe0, 0xe0, 0xe0),
                BlurRadius = 2,
                ShadowDepth = 2
            }
        };

        void removefxpoly()
        {
            if (my.fxdrawn)
            {
                this.panel.Children.Remove(fxtext);
                this.panel.Children.Remove(fxpoly);
            }
            my.fxdrawn = false;
        }

        void drawfxpoly()
        {
            removefxpoly();
            my.fxdrawn = true;

            double height = ob.height * my.meter < 0.0011 ? 0 : ob.height * my.meter;
            double perimeter = ob.polygon.Length * my.meter;
            double area = ob.area * my.meter * my.meter;
            string unit = "m "; if (perimeter < 10000) { perimeter *= 1000; unit = "mm"; }
            string format = "F0"; if (area < 0.01) format = "0.0e+0"; else if (area < 10) format = "F3";
            fxpoly.ToolTip = new ToolTip
            {
                Content = String.Format(my.cult, "E ={0,8:F3} m \nH ={1,8:F3} m \nP ={2,8:F0} {3}\nA ={4,8} m²\nV ={5,8} m³",
                    ob.elevation * my.meter + my.zero.Z,
                    height,
                    perimeter, unit,
                    area.ToString(format, my.cult),
                    (area * height).ToString(format, my.cult)),
                FontFamily = new FontFamily("Consolas"),
                BorderThickness = new Thickness(2)
            };

            if (ob.ject.Attributes.Name != "")
            {
                fxtext.Text = ob.ject.Attributes.Name;
                fxtext.FontStyle = FontStyles.Normal;
            }
            else
            {
                fxtext.Text = "name not set";
                fxtext.FontStyle = FontStyles.Oblique;
            }
            fxtext.ToolTip = new ToolTip
            {
                Content = String.Format("<name>{0}\n<LineStyle>{1:x8}{2}{3}",
                    ob.ject.Attributes.Name,
                    ob.ject.Attributes.ObjectColor.ToArgb(),
                    (null != ob.kud && null != ob.kud.fill) ? "\n<PolyStyle>" + ob.kud.fill : "",
                    (null != ob.kud && null != ob.kud.exda) ? "\n<ExtendedData>#" + ob.kud.exda.Count : ""),
                FontFamily = new FontFamily("Consolas"),
                BorderThickness = new Thickness(2)
            };

            const double offx = 12 + 5, offy = 68 + 10, dimx = 87 - 5 * 2, dimy = 82 - 10 * 2;
            double x1, x2, y1, y2; x1 = x2 = ob.center.X; y1 = y2 = ob.center.Y;
            foreach (Point3d pt in ob.polygon)
            {
                if (pt.X < x1) x1 = pt.X; if (pt.X > x2) x2 = pt.X;
                if (pt.Y < y1) y1 = pt.Y; if (pt.Y > y2) y2 = pt.Y;
            }
            double x = (x2 - x1) / (dimx);
            double y = (y2 - y1) / (dimy);
            double zoom = x > y ? x : y;
            double zoon = x < y ? x : y;
            zoom = x > y ?
                (x2 - x1) / (dimx - (dimx / 3) * (zoon / zoom)) :
                (y2 - y1) / (dimy - (dimy / 3) * (zoon / zoom));
            fxpoly.Points.Clear();
            for (int p = 0; p < ob.polygon.Count - 1; p++)
                fxpoly.Points.Add(new System.Windows.Point(
                    ((ob.polygon[p].X - x1) / zoom + (dimx / 2) - (dimx / 2) * (x / zoom)) + offx, dimy -
                    ((ob.polygon[p].Y - y1) / zoom + (dimy / 2) - (dimy / 2) * (y / zoom)) + offy));

            this.panel.Children.Add(fxpoly);
            this.panel.Children.Add(fxtext);
        }
    }
}
