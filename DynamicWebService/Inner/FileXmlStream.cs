using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWebService.Inner
{
    internal class FileXmlStream : BaseXmlStream
    {
        public FileXmlStream()
        {
            _getStream = (filepath) =>
            {
                MemoryStream stream = new MemoryStream();
                if (File.Exists(filepath))
                {
                    using (var fileStream = new FileStream(filepath, FileMode.Open))
                    {
                        fileStream.CopyTo(stream);
                    }
                }
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            };
        }
    }
}