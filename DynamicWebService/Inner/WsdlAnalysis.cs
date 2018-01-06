using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DynamicWebService.Inner
{
    internal class WsdlAnalysis
    {
        private Dictionary<string, string> primitive = new Dictionary<string, string>();

        private Dictionary<string, XContainer> typeDocument = new Dictionary<string, XContainer>();

        private BaseXmlStream _baseStream;

        public WsdlAnalysis(BaseXmlStream baseStream)
        {
            _baseStream = baseStream;
            Init();
        }

        private void Init()
        {
            primitive.Add("anyType", "anyType");
            primitive.Add("anyURI", "anyURI");
            primitive.Add("base64Binary", "base64Binary");
            primitive.Add("boolean", "boolean");
            primitive.Add("byte", "byte");
            primitive.Add("datetime", "datetime");
            primitive.Add("decimal", "decimal");
            primitive.Add("double", "double");
            primitive.Add("float", "float");
            primitive.Add("int", "int");
            primitive.Add("integer", "integer");
            primitive.Add("long", "long");
            primitive.Add("QName", "QName");
            primitive.Add("short", "short");
            primitive.Add("string", "string");
            primitive.Add("unsignedByte", "unsignedByte");
            primitive.Add("unsignedInt", "unsignedInt");
            primitive.Add("unsignedLong", "unsignedLong");
            primitive.Add("unsignedShort", "unsignedShort");
        }

        public OperationWSDL Analysize(String url, string operation)
        {
            operation = operation.ToLower();
            var doc = _baseStream.GetXDocument(url);
            doc.Descendants().Attributes().Where(a => a.IsNamespaceDeclaration && a.Name != "import").Remove();
            foreach (var element in doc.Descendants())
            {
                element.Name = element.Name.LocalName;
            }
            var opXpath = @"/definitions/binding/operation[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '" + operation + @"']";
            var opName = doc.XPathSelectElement(opXpath).Attribute("name").Value;
            var actXpath = @"/definitions/binding/operation[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '" + operation + @"']/operation";
            var action = doc.XPathSelectElement(actXpath).Attribute("soapAction").Value;
            var tgtNs = doc.XPathSelectElement(@"/definitions").Attribute("targetNamespace").Value;
            var actBindingName = doc.XPathSelectElement(@"/definitions/binding").Attribute("name").Value;
            var lctXpath = @"/definitions/service/port[contains(@binding,'" + actBindingName + @"')]/address";
            var location = doc.XPathSelectElement(lctXpath).Attribute("location").Value;
            var inputMsgPath = @"/definitions/portType/operation[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = '" + operation + @"']/input";
            var inputMsg = doc.XPathSelectElement(inputMsgPath).Attribute("message").Value.LocalName();

            var partPath = @"/definitions/message[contains(@name,'" + inputMsg + @"')]/part";

            var parameters = new Dictionary<string, ComplexType>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in doc.XPathSelectElements(partPath))
            {
                if (part.Attributes().Any(a => a.Name == "element"))
                {
                    //to do need resuse
                    var schemaPath = @"/definitions/types/schema";
                    var schemas = doc.XPathSelectElements(schemaPath);
                    foreach (var schema in schemas)
                    {
                        SetParamNamespace(schema, tgtNs, part.Attribute("element").Value.LocalName(), parameters);
                    }
                }
                else
                {
                    var param = part.Attribute("name").Value;
                    parameters.Add(param, new ComplexType { Namespace = tgtNs });
                }
            }
            OperationWSDL opWsdl = new OperationWSDL();
            opWsdl.ServiceLocation = location;
            opWsdl.SoapAction = action;
            opWsdl.Operation = opName;
            opWsdl.OperaitonNamespace = tgtNs;
            opWsdl.ParameterNamespace = parameters;
            return opWsdl;
        }

        private void SetParamNamespace(XContainer container, string @namespace, string msg, Dictionary<string, ComplexType> paramNamespace)
        {
            if (!typeDocument.ContainsKey(@namespace))
            {
                var tgtNs = container.XPathSelectElement(@".").Attribute("targetNamespace").Value;
                if (tgtNs != @namespace)
                {
                    var import = container.XPathSelectElement(@"./import[@namespace='" + @namespace + @"']");

                    var url = import.Attribute("schemaLocation").Value;
                    var imp = _baseStream.GetXDocument(url);

                    //imp.Descendants().Attributes().Where(a => a.IsNamespaceDeclaration && a.Name != "import").Remove();

                    foreach (var element in imp.Descendants())
                    {
                        element.Name = element.Name.LocalName;
                    }

                    typeDocument.Add(@namespace, imp.XPathSelectElement(@"/schema"));
                }
                else
                {
                    typeDocument.Add(@namespace, container);
                }
            }

            //这个有问题暂时只支持一层complexType
            var msgElm = typeDocument[@namespace].XPathSelectElement(@"./element[@name='" + msg + @"']");

            IEnumerable<XElement> childs = new List<XElement>();

            //ComplexType 可能存在element SimpleType 不存在，所以不处理简单类型（SimpleType）命名空间
            if (msgElm.Attribute("type") != null)
            {
                if (!primitive.ContainsKey(msgElm.Attribute("type").Value.LocalName()))
                {
                    childs = typeDocument[@namespace].XPathSelectElements(@"./complexType[@name='" + msgElm.Attribute("type").Value.LocalName() + @"']//element");
                }
            }
            else
            {
                childs = msgElm.XPathSelectElements(@"./complexType//element");
            }
            foreach (var child in childs)
            {
                if (child.Attribute("type") != null)
                {
                    if (primitive.ContainsKey(child.Attribute("type").Value.LocalName()))
                    {
                        paramNamespace.Add(child.Attribute("name").Value, new ComplexType { Name = child.Attribute("name").Value, Namespace = @namespace });
                    }
                    else
                    {
                        if (child.Attributes().Any(a => a.Name.NamespaceName == @"http://www.w3.org/2000/xmlns/"))
                        {
                            var tarNs = child.Attributes().First(a => a.Name.NamespaceName == @"http://www.w3.org/2000/xmlns/").Value;
                            var childParamNs = new Dictionary<string, ComplexType>();
                            paramNamespace.Add(child.Attribute("name").Value, new ComplexType { Name = child.Attribute("name").Value, Namespace = @namespace, Child = childParamNs });
                            SetParamNamespace(typeDocument[@namespace], tarNs, child.Attribute("type").Value.LocalName(), childParamNs);
                        }
                    }
                }
                if (child.Attribute("ref") != null)
                {
                    var childParamNs = new Dictionary<string, ComplexType>();
                    paramNamespace.Add(child.Attribute("ref").Value.LocalName(), new ComplexType { Name = child.Attribute("ref").Value.LocalName(), Namespace = @namespace, Child = childParamNs });
                    SetParamNamespace(typeDocument[@namespace], @namespace, child.Attribute("ref").Value.LocalName(), childParamNs);
                }
            }
        }
    }
}