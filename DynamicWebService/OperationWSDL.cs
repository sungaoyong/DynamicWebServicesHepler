using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebService
{
    internal class OperationWSDL
    {
        public string ServiceLocation { get; set; }

        public string Operation { get; set; }

        public string OperaitonNamespace { get; set; }

        public string SoapAction { get; set; }

        public Dictionary<string, ComplexType> ParameterNamespace { get; set; }
    }
}