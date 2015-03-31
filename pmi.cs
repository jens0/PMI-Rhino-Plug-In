// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;


using Rhino;


using Rhino.Commands;




namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("99999999-8888-7777-6666-555555555555")]
    public class pmi : Command
    {
        public pmi() { }
        public override string EnglishName { get { return "pmi"; } }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Guid panelid = PanelHost.PanelId;
            if (Rhino.UI.Panels.IsPanelVisible(panelid))
            {
                Rhino.UI.Panels.ClosePanel(panelid);
                RhinoApp.WriteLine("panel is now hidden");
            }
            else
            {
                Rhino.UI.Panels.OpenPanel(panelid);
                RhinoApp.WriteLine("panel is now visible");
            }
            return Result.Success;
        }
    }
}
