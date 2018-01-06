using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DynamicWebService.Inner
{
    internal class BaseXmlStream
    {
        protected Func<String, Stream> _getStream;

        public XDocument GetXDocument(string uri)
        {
            Stream stream = _getStream(uri);
            if (stream.Length > 0)
            {
                return XDocument.Load(stream);
            }
            return null;
        }

        public XmlDocument GetXmlDocument(string uri)
        {
            Stream stream = _getStream(uri);
            if (stream.Length > 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                return doc;
            }
            return null;
        }
    }
}