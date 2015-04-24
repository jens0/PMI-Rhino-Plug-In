// PMI Rhino Plug-In, Copyright (c) 2015 QUT
using System;

using System.Collections.Generic;
using Rhino;





using System.Collections.Specialized;

namespace MyProject1
{
    [System.Runtime.InteropServices.Guid("15d2ad90-61db-4a53-a119-90f8cfef005c")]
    public class kmlUserData : Rhino.DocObjects.Custom.UserData
    {
        public override string Description { get { return "ExtendedData"; } }
        public string fill { get; set; }
        public NameValueCollection exda { get; set; }

        protected override void OnDuplicate(Rhino.DocObjects.Custom.UserData source)
        {
            kmlUserData src = source as kmlUserData;
            if (src != null)
            {
                fill = src.fill;
                exda = src.exda;
                if (null == src.fill) my.note("*DuplFill*");
                if (null == src.exda) my.note("*DuplExda*");
            }
            else
                my.note("*DuplNo*");
        }

        public override bool ShouldWrite
        {
            get
            {
                if (null != fill || null != exda)
                    return true;
                return false;
            }
        }

        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            int i = 0;
            IEnumerable<string> exs = new string[] { };
            Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
            if (dict.ContainsKey("fill"))
            {
                fill = (string)dict["fill"];
            }
            else
                my.note("*NoFill*");
            if (dict.ContainsKey("exda"))
            {
                exs = (IEnumerable<string>)dict["exda"];
                NameValueCollection exdata = new NameValueCollection();
                string namestr = "";
                foreach (String s in exs)
                {
                    if (i % 2 == 0)
                        namestr = s;
                    else
                        exdata.Add(namestr, s);
                    i++;
                }
                exda = exdata;
            }
            else
                my.note("*NoExda*");
            //my.note("," + i / 2);
            return true;
        }

        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            int i = 0;
            var dict = new Rhino.Collections.ArchivableDictionary(1, "pmi");
            if (null != fill)
            {
                dict.Set("fill", fill);
            }
            if (null != exda)
            {
                List<string> exs = new List<string>();
                foreach (String s in exda.AllKeys)
                {
                    i++;
                    exs.Add(s);
                    exs.Add(exda[s]);
                }
                dict.Set("exda", exs);
            }
            archive.WriteDictionary(dict);
            //my.note("," + i);
            return true;
        }

        public kmlUserData() { }
        public kmlUserData(string _fill, NameValueCollection _exda)
        {
            fill = _fill;
            exda = _exda;
        }
    }
}
