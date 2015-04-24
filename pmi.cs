// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;


using Rhino;


using Rhino.Commands;




namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("19cfc46f-aa74-4779-898a-7c6211592846")]
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
                RhinoApp.WriteLine("panel off");
            }
            else
            {
                Rhino.UI.Panels.OpenPanel(panelid);
                RhinoApp.WriteLine("panel on");
            }
            return Result.Success;
        }
    }
}
