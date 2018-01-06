using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebService.Inner
{
    internal class WebXmlStream : BaseXmlStream
    {
        public WebXmlStream()
        {
            _getStream = (url) =>
            {
                MemoryStream stream = new MemoryStream();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //SetWebRequest(request);
                WebResponse response = request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    sr.BaseStream.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream;
                }
            };
        }
    }
}