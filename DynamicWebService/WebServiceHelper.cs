using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DynamicWebService.Inner;

namespace DynamicWebService
{
    /// <summary>
    /// 动态WebServices调用的辅助类
    /// </summary>
    public class WebServiceHelper
    {
        private static readonly Dictionary<String, OperationWSDL> _soapNameSpace = new Dictionary<string, OperationWSDL>();

        private readonly SoapEnveloper _soapEnveloper = new SoapEnveloper();

        private readonly string _id;

        private readonly string _pwd;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="strUserId"> 用户名</param>
        /// <param name="strPwd"> 密码</param>
        public WebServiceHelper(string strUserId, string strPwd)
        {
            this._id = strUserId;
            this._pwd = strPwd;
        }

        public WebServiceHelper()
        { }

        /// <summary>
        /// 通过SOAP协议动态调用webservice
        /// </summary>
        /// <param name="url"> webservice地址</param>
        /// <param name="methodName"> 调用方法名</param>
        /// <param name="pars"> 参数表</param>
        /// <returns> 结果集</returns>
        public string QuerySoapWebService(String url, String methodName, Dictionary<string, object> pars)
        {
            var key = url + "/" + methodName;
            if (!_soapNameSpace.ContainsKey(key))
            {
                BaseXmlStream stream = new WebXmlStream();
                WsdlAnalysis analysis = new WsdlAnalysis(stream);
                var wsdl = analysis.Analysize(url + "?wsdl", methodName);
                _soapNameSpace.Add(key, wsdl);
            }
            HttpWebRequest request = CreateRequest(url);

            SetWebRequestHeader(_soapNameSpace[key].SoapAction, request);
            // 设置请求身份
            SetWebRequest(request);
            // 将soap协议写入请求
            WriteRequestData(request, _soapEnveloper.EnvelopeSoapBody(_soapNameSpace[key], pars, _id, _pwd));

            // 读取服务端响应
            // 返回结果
            return _soapEnveloper.GetSoapBody(ReadXmlResponse(request));
        }

        /// <summary>
        /// 通过SOAP的wsdl文件访问
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="methodName"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public string AnalysisSoapXml(String xml, String methodName, Dictionary<string, object> pars)
        {
            var key = xml + "/" + methodName;
            try
            {
                if (!_soapNameSpace.ContainsKey(key))
                {
                    BaseXmlStream stream = new FileXmlStream();
                    WsdlAnalysis analysis = new WsdlAnalysis(stream);
                    var wsdl = analysis.Analysize(xml, methodName);
                    _soapNameSpace.Add(key, wsdl);
                }
            }catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                
            }
            return "Ok";
        }

        #region Soap请求

        /// <summary>
        /// 设置请求身份
        /// </summary>
        /// <param name="request"> 请求</param>
        private void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            //request.Timeout = 10000;
        }

        /// <summary>
        /// 将soap协议写入请求
        /// </summary>
        /// <param name="request"> 请求</param>
        /// <param name="data"> soap协议</param>
        private void WriteRequestData(HttpWebRequest request, byte[] data)
        {
            request.ContentLength = data.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(data, 0, data.Length);
            writer.Close();
        }

        /// <summary>
        /// 将响应对象读取为xml对象
        /// </summary>
        /// <param name="response"> 响应对象</param>
        /// <returns> xml对象</returns>
        private XmlDocument ReadXmlResponse(HttpWebRequest request)
        {
            try
            {
                var response = request.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(sr);
                    return doc;
                }
            }
            catch (WebException e)
            {
                using (StreamReader sr = new StreamReader(e.Response.GetResponseStream(), Encoding.UTF8))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(sr);
                    // To do log
                    _soapEnveloper.GetSoapBody(doc);
                }
                throw e;
            }
            catch (Exception e)

            {
                throw e;
            }
        }

        private static void SetWebRequestHeader(string soapAction, HttpWebRequest request)
        {
            request.Headers.Add("SOAPAction", soapAction);
        }

        private static HttpWebRequest CreateRequest(string url)
        {
            // 获取请求对象
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            return request;
        }

        #endregion Soap请求
    }
}