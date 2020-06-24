using System;
using System.Collections.Generic;
using System.Text;

namespace libAG3DBCrevival
{
    public class Ag3DbRevivalSettings
    {
        public string ServerURL;

        public void SetDefaults()
        {
            ServerURL = "http://127.0.0.1:42054/js3db-revival";
        }
    }
}
