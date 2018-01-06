using System;
using System.Collections.Generic;
using DynamicWebService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicWebServices.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //返回值： 0 = 请重新验证；1 = 邮件地址合法；2 = 只是域名正确；3 = 一个未知错误；4 = 邮件服务器没有找到；5 = 电子邮件地址错误；6 = 免费用户验证超过数量（50次 / 24小时）；7 = 商业用户不能通过验证
            Dictionary<string, object> pars = new Dictionary<string, object>();
            pars.Add("theEmail", "1111@qq.com");
            WebServiceHelper webHelper = new WebServiceHelper();
            string d = webHelper.QuerySoapWebService("http://www.webxml.com.cn/WebServices/ValidateEmailWebService.asmx", "ValidateEmailAddress", pars);
            Assert.IsTrue(d=="1");
        }

        [TestMethod]
        public void TestMethod2()
        {
            //返回值： 0 = 请重新验证；1 = 邮件地址合法；2 = 只是域名正确；3 = 一个未知错误；4 = 邮件服务器没有找到；5 = 电子邮件地址错误；6 = 免费用户验证超过数量（50次 / 24小时）；7 = 商业用户不能通过验证
            Dictionary<string, object> pars = new Dictionary<string, object>();
            pars.Add("theEmail", "用户名");
            WebServiceHelper webHelper = new WebServiceHelper();
            string d = webHelper.QuerySoapWebService("http://www.webxml.com.cn/WebServices/ValidateEmailWebService.asmx", "ValidateEmailAddress", pars);
            Assert.IsTrue(d == "5");
        }
    }
}
