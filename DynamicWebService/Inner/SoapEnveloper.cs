using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DynamicWebService.Inner
{
    internal class SoapEnveloper
    {
        private readonly string _soapEnvelope = "http://schemas.xmlsoap.org/soap/envelope/";
        private readonly string _soapPrefix = "soap";

        public SoapEnveloper()
        {
        }

        /// <summary>
        /// 给xml文档添加声明
        /// </summary>
        /// <param name="doc"> xml文档</param>
        private void AddDelaration(XmlDocument doc)
        {
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
        }

        /// <summary>
        /// 初始化Header
        /// </summary>
        /// <param name="doc"> soap文档</param>
        private void InitSoapHeader(XmlDocument doc, string id, string pwd)
        {
            // 添加soapheader节点
            XmlElement soapHeader = doc.CreateElement(_soapPrefix, "Header", _soapEnvelope);
            if (!string.IsNullOrEmpty(id))
            {
                XmlElement soapId = doc.CreateElement("userid");
                soapId.InnerText = id;
                XmlElement soapPwd = doc.CreateElement("userpwd");
                soapPwd.InnerText = pwd;
                soapHeader.AppendChild(soapId);
                soapHeader.AppendChild(soapPwd);
            }
            doc.ChildNodes[0].AppendChild(soapHeader);
        }

        /// <summary>
        /// 封装Soap协议正文
        /// </summary>
        ///<param name="pars">
        /// 请求参数值
        ///</param>
        /// <returns> 字节数组</returns>
        public byte[] EnvelopeSoapBody(OperationWSDL wsdl, Dictionary<string, object> pars, string id, string pwd)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement(_soapPrefix, "Envelope", _soapEnvelope);
            doc.AppendChild(root);
            InitSoapHeader(doc, id, pwd);
            XmlElement soapBody = doc.CreateElement(_soapPrefix, "Body", _soapEnvelope);
            XmlElement soapMethod = doc.CreateElement(wsdl.Operation, wsdl.OperaitonNamespace);
            foreach (string key in pars.Keys)
            {
                XmlElement soapPar = doc.CreateElement(wsdl.ParameterNamespace[key].Name, wsdl.ParameterNamespace[key].Namespace);
                soapPar.InnerXml = ObjectToSoapXml(pars[key]);
                soapMethod.AppendChild(soapPar);
            }
            soapBody.AppendChild(soapMethod);
            doc.DocumentElement.AppendChild(soapBody);
            AddDelaration(doc);
            return Encoding.UTF8.GetBytes(doc.OuterXml);
        }

        public string GetSoapBody(XmlDocument doc)
        {
            return doc.GetElementsByTagName("Body", _soapEnvelope)[0].InnerText;
        }

        /// <summary>
        /// 将参数对象中的内容取出
        /// </summary>
        /// <param name="o">参数值对象</param>
        /// <returns>字符型值对象</returns>
        private string ObjectToSoapXml(object o)
        {
            XmlSerializer mySerializer = new XmlSerializer(o.GetType());
            MemoryStream ms = new MemoryStream();
            mySerializer.Serialize(ms, o);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
            if (doc.DocumentElement != null)
            {
                return doc.DocumentElement.InnerXml;
            }
            else
            {
                return o.ToString();
            }
        }
    }
}