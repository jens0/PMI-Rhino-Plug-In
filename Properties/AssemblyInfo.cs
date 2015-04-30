using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rhino.PlugIns;

// Plug-in Description Attributes - all of these are optional
// These will show in Rhino's option dialog, in the tab Plug-ins
[assembly: PlugInDescription(DescriptionType.Address, "2 George Street, Brisbane QLD 4000")]
[assembly: PlugInDescription(DescriptionType.Country, "Australia")]
[assembly: PlugInDescription(DescriptionType.Email, "askqut@qut.edu.au")]
[assembly: PlugInDescription(DescriptionType.Phone, "+61 7 3138 2000")]
[assembly: PlugInDescription(DescriptionType.Fax, "-")]
[assembly: PlugInDescription(DescriptionType.Organization, "Queensland University of Technology")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "https://www.qut.edu.au")]
[assembly: PlugInDescription(DescriptionType.WebSite, "https://www.qut.edu.au")]
                    
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PMI Rhino Plug-In")] // Plug-In title is extracted from this
[assembly: AssemblyDescription("Ubran Geo-Modeling")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("QUT")]
[assembly: AssemblyProduct("PMI Rhino Plug-In")]
[assembly: AssemblyCopyright("Copyright © 2015")]
[assembly: AssemblyTrademark("Open Source")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4234181f-331c-4655-a5dd-b04be0843cfd")] // This will also be the Guid of the Rhino plug-in

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.02.*")]
[assembly: AssemblyFileVersion("1.02.0.0")]