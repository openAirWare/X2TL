using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace RelWare
{
    public class X2TL_NUnit
    {
        private X2TL x2tl;

        public X2TL_NUnit()
        {
            this.x2tl = new X2TL();
        }

        
        public string testInit()
        {
            return "Initialization " + ((this.x2tl != null) ? "is Complete" : "has FAILED");
        }


        public string RunTest(string pName, string pSource, string pTemplate, string pSupportingTemplates, string pTest)
        {
            return RunTest(pName, pSource, pTemplate, pSupportingTemplates, pTest, false);
        }

        public string RunTest(string pName, string pSource, string pTemplate, string pSupportingTemplates, string pTest, bool isException)
        {
            XmlDocument sourceDoc = new XmlDocument();
            XmlDocument templateDoc = new XmlDocument();
            XmlDocument supportingDoc = new XmlDocument();

            if (pSource.Length != 0)
                sourceDoc.LoadXml(pSource);

            if (pTemplate.Length != 0)
                templateDoc.LoadXml(pTemplate);

            if (pSupportingTemplates.Length != 0)
                supportingDoc.LoadXml(pSupportingTemplates);

            XmlNode source = sourceDoc.DocumentElement;
            XmlNode template = templateDoc.DocumentElement;
            XmlNode otherTemplates = supportingDoc.DocumentElement;

            bool passed;
            string testResult;

            try
            {
                testResult = this.x2tl.TransformNode(source, template, otherTemplates);
                passed = (testResult == pTest) && ( !isException );
            }
            catch(Exception e)
            {
                passed = (isException);
                testResult = e.Message;
            }

            if (passed)
                return (String.Format("{1} : {0} Test", pName, "- PASSED - "));
            else
                return (String.Format("\n{1} : {0} Test\nExpected: {2}\nReceived: {3}\n", pName, " ** FAILED **", pTest, testResult));
        
        }




    }
}

