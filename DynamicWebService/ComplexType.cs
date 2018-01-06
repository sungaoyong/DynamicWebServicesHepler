using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebService
{
    internal class ComplexType
    {
        private Dictionary<string, ComplexType> _child = new Dictionary<string, ComplexType>();
        public string Namespace { get; set; }
        public string Name { get; set; }
        public Dictionary<string, ComplexType> Child { get; set; }
    }
}