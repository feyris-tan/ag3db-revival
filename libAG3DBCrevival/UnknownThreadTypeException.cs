using System;
using System.Collections.Generic;
using System.Text;

namespace libAG3DBCrevival
{
    public class UnknownThreadTypeException : NotImplementedException
    {
        public UnknownThreadTypeException(string s)
            : base(s)
        {

        }
    }
}
