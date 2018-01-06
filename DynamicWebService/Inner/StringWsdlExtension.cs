using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebService.Inner
{
    public static class StringWsdlExtension
    {
        public static string LocalName(this string str)
        {
            return str.Substring(str.IndexOf(':') + 1);
        }
    }
}