/*! X2TL v1.1.0 | (c) 2010, 2016 Mark Brown, openAirWare | https://opensource.org/licenses/MIT */
#region [ using ]
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Diagnostics;
using System.Security;
using System.Security.Cryptography;
#endregion

#region [ RelWare ]
namespace RelWare
{
    #region [ X2TL ]
    /// <summary>
    /// X2TL - X2A2 Templating Language Class
    /// </summary>
    public class X2TL
    {
        #region [ Supporting Classes and Enumerators ]

        #region [ CustomContext : XsltContext ]

        public class CustomContext : XsltContext
        {
            // XsltArgumentList to store my user defined variables
            private XsltArgumentList m_ArgList;

            // Constructors 
            public CustomContext()
            { }

            public CustomContext(NameTable nt)
                : base(nt)
            {
            }

            public CustomContext(NameTable nt, XsltArgumentList argList)
                : base(nt)
            {
                m_ArgList = argList;
            }

            // Returns the XsltArgumentList that contains custom variable definitions.
            public XsltArgumentList ArgList
            {
                get
                {
                    return m_ArgList;
                }
            }

            // Function to resolve references to my custom functions.
            public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
           {
              XPathRegExExtensionFunction func = null;

              // Create an instance of appropriate extension function class.
              switch (name)
              {
                 case "Split":
                    // Usage 
                    // myFunctions:Split(string source, string Regex_pattern, int n) returns string
                    func = new XPathRegExExtensionFunction("Split", 3, 3, new XPathResultType[] {XPathResultType.String, XPathResultType.String, XPathResultType.Number}, XPathResultType.String);
                    break;
                 case "Replace":
                    // Usage
                    // myFunctions:Replace(string source, string Regex_pattern, string replacement_string) returns string
                    func = new XPathRegExExtensionFunction("Replace", 3, 3, new XPathResultType[] {XPathResultType.String, XPathResultType.String, XPathResultType.String}, XPathResultType.String);
                    break;
              }
              return func;
           }

            
            // Function to resolve references to my custom variables.
            public override IXsltContextVariable ResolveVariable(string prefix, string name)
            {
                // Create an instance of an XPathExtensionVariable.
                XPathExtensionVariable Var;
                Var = new XPathExtensionVariable(name);

                return Var;
            }

            public override int CompareDocument(string baseUri, string nextbaseUri)
            {
                return 0;
            }

            public override bool PreserveWhitespace(XPathNavigator node)
            {
                return true;
            }

            public override bool Whitespace
            {
                get
                {
                    return true;
                }
            }
        }


        #endregion

        #region [ XPathRegExExtensionFunction : IXsltContextFunction ]

        public class XPathRegExExtensionFunction : IXsltContextFunction
        {
            private XPathResultType[] m_ArgTypes;
            private XPathResultType m_ReturnType;
            private string m_FunctionName;
            private int m_MinArgs;
            private int m_MaxArgs;

            // Methods to access the private fields.
            public int Minargs
            {
                get
                {
                    return m_MinArgs;
                }
            }

            public int Maxargs
            {
                get
                {
                    return m_MaxArgs;
                }
            }

            public XPathResultType[] ArgTypes
            {
                get
                {
                    return m_ArgTypes;
                }
            }

            public XPathResultType ReturnType
            {
                get
                {
                    return m_ReturnType;
                }
            }

            // Constructor
            public XPathRegExExtensionFunction(string name, int minArgs, int maxArgs, XPathResultType[] argTypes, XPathResultType returnType)
            {
                m_FunctionName = name;
                m_MinArgs = minArgs;
                m_MaxArgs = maxArgs;
                m_ArgTypes = argTypes;
                m_ReturnType = returnType;
            }

            // This method is invoked at run time to execute the user defined function.
            public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
            {
                Regex r;
                string str = null;

                // The two custom XPath extension functions
                switch (m_FunctionName)
                {
                    case "Split":
                        r = new Regex(args[1].ToString());
                        string[] s1 = r.Split(args[0].ToString());
                        int n = Convert.ToInt32(args[2]);
                        if (s1.Length < n)
                            str = "";
                        else
                            str = s1[n - 1];
                        break;
                    case "Replace":
                        r = new Regex(args[1].ToString());
                        string s2 = r.Replace(args[0].ToString(), args[2].ToString());
                        str = s2;
                        break;
                }
                return (object)str;
            }
        }


        #endregion

        #region [ XPathExtensionVariable : IXsltContextVariable ]

        public class XPathExtensionVariable : IXsltContextVariable
        {
            // The name of the user-defined variable to resolve
            private string m_VarName;

            public XPathExtensionVariable(string VarName)
            {
                m_VarName = VarName;
            }

            // This method is invoked at run time to find the value of the user defined variable.
            public object Evaluate(XsltContext xsltContext)
            {
                XsltArgumentList vars = ((CustomContext)xsltContext).ArgList;
                return vars.GetParam(m_VarName, null);
            }

            public bool IsLocal
            {
                get
                {
                    return false;
                }
            }

            public bool IsParam
            {
                get
                {
                    return false;
                }
            }

            public XPathResultType VariableType
            {
                get
                {
                    return XPathResultType.Any;
                }
            }
        }
        
        #endregion


        #region [ Enum: StringFragmentTypes ]
        /// <summary>
        /// StringFragmentTypes
        /// Description: Each string fragment can be one of type types: text or command.
        /// </summary>
        private enum StringFragmentTypes
        {
            sftText,
            sftCommand
        }
        #endregion

        #region [ Class: StringFragment ]
        /// <summary>
        /// StringFragment
        /// Description: This class is used to hold reference to a segment of a string
        /// </summary>
        private class StringFragment
        {
            #region [ Public Properties ]
            public string source;               // handle to the actual string value
            public int parsedIndex;             // when parsing out the string, what is the index of this fragment
            public StringFragmentTypes type;    // what type of fragment is this
            public int indexStart;              // what is the start index of this fragment within the string
            public int indexEnd;                // what is the end index of this fragment
            public int length;                  // what is the length of this fragment
            public Template ownerTemplate;      // handle to the parent Template 
            #endregion

            #region [ Public Methods ]
            #region [ GetValue ]
            /// <summary>
            /// GetValue
            /// Description: Retrieve the string value of this fragment
            /// </summary>
            /// <returns>string</returns>
            public String GetValue()
            {
                if ((this.indexStart >= 0) && (this.length >= 0))
                    return this.source.Substring(this.indexStart, this.length);
                return "";
            }
            #endregion

            #region [ GetValueEscaped ]
            /// <summary>
            /// GetValueEscaped
            /// Description: Get an XML escaped version of the value
            /// </summary>
            /// <returns>string</returns>
            public string GetValueEscaped()
            {
                if ((this.indexStart >= 0) && (this.length >= 0))
                    return this.EscapeValue(this.source.Substring(this.indexStart, this.length));
                return "";
            }
            #endregion

            #region [ EscapeValue ]
            /// <summary>
            /// EscapeValue
            /// Description: Provide an XML escaped version of the given value
            /// </summary>
            /// <param name="pString">String value to be escaped</param>
            /// <returns>string</returns>
            public string EscapeValue(string pString)
            {
                return this.ownerTemplate.EscapeValue(pString);
            }
            #endregion

            #region [ IsCommandFragment ]
            /// <summary>
            /// IsCommandFragment
            /// Description: Is this fragment a command?
            /// </summary>
            /// <returns>bool</returns>
            public bool IsCommandFragment()
            {
                return (this.type == StringFragmentTypes.sftCommand);
            }
            #endregion

            #region [ IsTextFragment ]
            /// <summary>
            /// IsTextFragment
            /// Description: Is this fragment a block of text?
            /// </summary>
            /// <returns>bool</returns>
            public bool IsTextFragment()
            {
                return (this.type == StringFragmentTypes.sftText);
            }
            #endregion

            #endregion

        }
        #endregion

        #region [ Enum: CommandTypes ]
        /// <summary>
        /// CommandTypes
        /// Description: List of known command types
        /// </summary>
        private enum CommandTypes
        {
            ctIf,
            ctElse,
            ctEndIf,

            ctEach,
            ctEndEach,

            ctDo,       // currently unsupported - work in progress
            ctUntil,    // currently unsupported - work in progress

            ctWhile,    // currently unsupported - work in progress
            ctEndWhile, // currently unsupported - work in progress

            ctResult,
            ctRawResult,
            ctXhtml,
            ctFormattedResult,
            ctDateFormattedResult,
            ctNumberFormattedResult,
            ctStringFormattedResult,

            ctCopy,
            ctCopyEncoded,

            ctApply,
            ctApplyWithParams,
            ctEndApplyWithParams,

            ctBreak,    // currently unsupported - work in progress

            ctReplace,

            ctVariable,
            ctMultipartVariable,
            ctEndMultipartVariable,

            ctVersion,

            ctUnknown
        }
        #endregion

        #region [ Enum: EncodingMethods ]
        /// <summary>
        /// EncodingMethods
        /// Description: List of known encoding methods
        /// </summary>
        private enum EncodingMethods
        {
            emNone,
            emFormatted,
            emXml,
            emXhtml,
            emCopyOf,
            emCopyOfRaw,
            emMD5,
            emSHA1
        }
        #endregion

        #region [ Class: CommandFragment : StringFragment ]
        /// <summary>
        /// CommandFragment : StringFragment
        /// Description: This is a specific type of fragment for performing commands
        /// </summary>
        private class CommandFragment : StringFragment
        {
            #region [ Public Properties ]
            public CommandTypes commandType;    // what type of command is this
            public string[] parameters;         // what parameters were provided
            public object supportingInfo;       // link to more information specific to the command type
            #endregion

            #region [ Constructor: CommandFragment ]
            /// <summary>
            /// Constructor: CommandFragment 
            /// </summary>
            /// <param name="pSource">Source string for this fragment</param>
            /// <param name="pStartIndex">Starting index of this fragment</param>
            /// <param name="pEndIndex">Ending index of this fragment</param>
            /// <param name="pOwnerTemplate">Handle to Template that owns this fragment</param>
            public CommandFragment(string pSource, int pStartIndex, int pEndIndex, Template pOwnerTemplate)
            {
                this.source = pSource;
                this.ownerTemplate = pOwnerTemplate;
                this.indexStart = pStartIndex;
                this.indexEnd = pEndIndex;
                this.length = pEndIndex - pStartIndex;
                this.type = StringFragmentTypes.sftCommand;

                this.parameters = this.ParseParameters();

            }
            #endregion

            #region [ Private Functions ]

            #region [ Function: Unescape ]
            private string UnescapeXML(string s)
            {
                if (string.IsNullOrEmpty(s)) return s;

                string returnString = s;
                returnString = returnString.Replace("&apos;", "'");
                returnString = returnString.Replace("&quot;", "\"");
                returnString = returnString.Replace("&gt;", ">");
                returnString = returnString.Replace("&lt;", "<");
                returnString = returnString.Replace("&amp;", "&");

                return returnString;
            }
            #endregion

            #region [ Function: ParseParameters ]
            private string[] ParseParameters()
            {
                string cmdline = this.UnescapeXML(this.source.Substring(this.indexStart, (this.indexEnd - this.indexStart)));
                
                ArrayList parameters = new ArrayList();

                if ((cmdline.IndexOf("\"") != -1) || (cmdline.IndexOf("(") != -1))
                {
                    // put logic from notes here....
                    int curPos = 0;
                    while ((curPos >= 0) && (curPos < cmdline.Length)) 
                    {
                        int qPos = cmdline.IndexOf('\"', curPos);
                        int pPos = cmdline.IndexOf('(', curPos);
                        if ((qPos>=0)||(pPos>=0))
                        {
                            int minPos = ( (qPos < pPos) ? ( (qPos!=-1)?qPos:pPos ) : ( (pPos!=-1)?pPos:qPos) );
                            if (minPos > curPos)
                            {
                                string[] leadingParts = cmdline.Substring(curPos, (minPos - curPos)).Split(this.ownerTemplate.whitespace);
                                foreach (string leadingPart in leadingParts)
                                    if (leadingPart.Trim().Length!=0)
                                        parameters.Add(leadingPart.Trim());
                            }

                            if (cmdline.Substring(minPos, 1) == "\"")
                            {
                                int nextQPos = cmdline.IndexOf('\"', minPos + 1);
                                if (nextQPos != -1)
                                {
                                    parameters.Add(cmdline.Substring(minPos + 1, (nextQPos - minPos) - 1));
                                    curPos = nextQPos + 1;
                                }
                                else
                                {
                                    parameters.Add(cmdline.Substring(minPos + 1, (cmdline.Length - minPos) - 1));
                                    curPos = cmdline.Length;
                                }
                            }
                            else
                            {
                                int nestCount = 1;
                                for (int i = minPos + 1; (i < cmdline.Length)&&(nestCount>0); i++)
                                {
                                    switch (cmdline.Substring(i, 1))
                                    {
                                        case "(":
                                            nestCount++;
                                            break;
                                        
                                        case ")":
                                            nestCount--;
                                            if (nestCount == 0)
                                            {
                                                parameters.Add(cmdline.Substring(minPos + 1, (i - minPos) - 1));
                                                curPos = i + 1;
                                            }
                                            break;
                                    }
                                }
                                if (nestCount > 0)
                                {
                                    parameters.Add(cmdline.Substring(minPos + 1, (cmdline.Length - minPos) - 1));
                                    curPos = cmdline.Length + 1;
                                }

                            }
                            if ((curPos < cmdline.Length) && (curPos >= 0))
                            {
                                int nextQPos = cmdline.IndexOf('\"', curPos);
                                int nextPPos = cmdline.IndexOf('(', curPos);
                                if ((nextQPos == -1) && (nextPPos == -1))
                                {
                                    string[] trailingParts = cmdline.Substring(curPos, (cmdline.Length - curPos)).Split(this.ownerTemplate.whitespace);
                                    foreach (string trailingPart in trailingParts)
                                        if (trailingPart.Trim().Length!=0)
                                            parameters.Add(trailingPart.Trim());
                                    curPos = cmdline.Length + 1;
                                }
                            }
                        }
                    }

                    string[] result = new string[parameters.Count];
                    for (int j = 0; j < parameters.Count; j++)
                        result[j] = (string)parameters[j];
                    return result;
                }
                else
                {
                    return this.GetValue().Trim().Split(this.ownerTemplate.whitespace);
                }

            }
            #endregion

            #endregion

            #region [ Virtual Methods ]
            #region [ Virtual: Process ]
            /// <summary>
            /// Virtual: Process
            /// Description: Used to process a given command against a context node
            /// </summary>
            /// <param name="pSourceContextNode">Xml node to use as the context</param>
            /// <param name="pSupportingTemplatesNode">Xml node holding supporting templates</param>
            /// <param name="pIndex">(ref) Current Fragment Index which will be updated upon completion</param>
            /// <param name="pResults">(ref) ArrayList of Resulting values so far, which will be updated during processing</param>
            public virtual void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref int pIndex, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                // As commands themselves are not subclassed, we have a basic shell here that is available
                // then we let the subclassed "supportingInfo" overrides handle the specifics.

                switch (this.commandType)
                {
                    case CommandTypes.ctResult:
                    case CommandTypes.ctRawResult:
                    case CommandTypes.ctXhtml:
                    case CommandTypes.ctFormattedResult:
                    case CommandTypes.ctDateFormattedResult:
                    case CommandTypes.ctNumberFormattedResult:
                    case CommandTypes.ctStringFormattedResult:
                    case CommandTypes.ctCopy:
                    case CommandTypes.ctCopyEncoded:
                        if (((ResultCommandInfo)this.supportingInfo) != null)
                            ((ResultCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;


                    case CommandTypes.ctEach:
                        if (((EachCommandInfo)this.supportingInfo) != null)
                            ((EachCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;

                    
                    case CommandTypes.ctIf:
                        if (((IfCommandInfo)this.supportingInfo) != null)
                            ((IfCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;


                    case CommandTypes.ctApply:
                    case CommandTypes.ctApplyWithParams:
                        if (((ApplyCommandInfo)this.supportingInfo) != null)
                            ((ApplyCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;

                    case CommandTypes.ctReplace:
                        if (((ReplaceCommandInfo)this.supportingInfo) != null)
                            ((ReplaceCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;


                    case CommandTypes.ctVariable:
                        if (((VariableCommandInfo)this.supportingInfo) != null)
                            ((VariableCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;

/*
                    case CommandTypes.ctEndMultipartVariable:
                        if (((VariableCommandInfo)this.supportingInfo) != null)
                        {
                            int mvarStartIndex = ((VariableCommandInfo)this.supportingInfo).startVarIndex;
                            CommandFragment actualCommandFrag = (CommandFragment)this.ownerTemplate.fragments[mvarStartIndex];
                            if ((VariableCommandInfo)this.supportingInfo != null)
                                ((VariableCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        }
                        else
                            pIndex++;
                        break;
*/
                    case CommandTypes.ctVersion:
                        if (((VersionCommandInfo)this.supportingInfo) != null)
                            ((VersionCommandInfo)this.supportingInfo).Process(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, ref pIndex);
                        else
                            pIndex++;
                        break;



                    default:
                        // do nothing
                        pIndex++;
                        break;
                }
            }
            #endregion
            #endregion

        }
        #endregion


        #region [ Class: SupportingObject ]
        class SupportingObject
        {
            // any base information should go here
            public CommandFragment ownerCommand;

            public virtual void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)
            }

            #region [ Helper Functions (virtual) ]
            #region [ (virtual) GetRequestedXmlNode ]
            public virtual XmlNode GetRequestedXmlNode(XmlNode pSourceContextNode, System.Collections.Specialized.HybridDictionary pVariables, string pRequest)
            {

                XmlNode context = null;
                XmlNode requestedNode = null;
                string trimmed = pRequest.Trim();

                switch (trimmed.Substring(0, 1))
                    {
                        case "/":   // get node value via xpath from document root (STEPS OUTSIDE LOCAL SCOPE)")
                            context = pSourceContextNode.OwnerDocument.DocumentElement;
                            break;

                        default:    // get node value
                            context = pSourceContextNode;
                            break;
                    }

                    XPathNavigator navigator = context.CreateNavigator();
                    XsltArgumentList varList = new XsltArgumentList();

                    foreach (string key in pVariables.Keys)
                    {
                        varList.AddParam(key, string.Empty, pVariables[key]);
                    }

                    CustomContext customContext = new CustomContext(new NameTable(), varList);


                    XPathExpression xpath = XPathExpression.Compile(trimmed);

                    xpath.SetContext(customContext);

                    //object commandOutput = navigator.Evaluate(trimmed);
                    object commandOutput = navigator.Evaluate(xpath);

                    if (commandOutput is XPathNodeIterator)
                    {
                        foreach (XPathNavigator commandOutputNode in (XPathNodeIterator)commandOutput)
                        {
                            requestedNode = (XmlNode)commandOutputNode.UnderlyingObject;
                            // only grab the first node found
                            break;
                        }
                    }
                    else
                    {
                        requestedNode = pSourceContextNode.OwnerDocument.CreateElement("node");
                        XmlText requestedText = pSourceContextNode.OwnerDocument.CreateTextNode(commandOutput.ToString());
                        requestedNode.AppendChild(requestedText);
                    }

                return requestedNode;

            }
            #endregion

            #region [ (virtual) GetRequestedXmlNodeList ]
            public virtual XPathNodeIterator GetRequestedXmlNodeList(XmlNode pSourceContextNode, System.Collections.Specialized.HybridDictionary pVariables, string pRequest)
            {

                #region [ OLD AND BUSTED ]
                /*
                string trimmmed = pRequest.Trim();                
                switch (trimmmed.Substring(0, 1))
                {
                    case "/":   // get node value via xpath from document root (STEPS OUTSIDE LOCAL SCOPE)
                        string rootSelect = trimmmed;
                        requestedNodes = pSourceContextNode.OwnerDocument.DocumentElement.SelectNodes(rootSelect);
                        break;

                    case "(":   // get xpath value (inside "( ... )")
                        string evalSelect = trimmmed.Substring(1, trimmmed.Length - 2);
                        requestedNodes = pSourceContextNode.SelectNodes(evalSelect);
                        break;

                    default:    // get child node by name
                        string nodeName = trimmmed;
                        requestedNodes = pSourceContextNode.SelectNodes(nodeName);
                        break;
                }
                */
                #endregion

                #region [ NEW HOTNESS ]

                XmlNode context = null;
                string trimmed = pRequest.Trim();

                switch (trimmed.Substring(0, 1))
                {
                    case "/":   // get node value via xpath from document root (STEPS OUTSIDE LOCAL SCOPE)")
                        context = pSourceContextNode.OwnerDocument.DocumentElement;
                        break;

                    default:    // get node value
                        context = pSourceContextNode;
                        break;
                }

                XPathNavigator navigator = context.CreateNavigator();
                XsltArgumentList varList = new XsltArgumentList();

                foreach (string key in pVariables.Keys)
                {
                    varList.AddParam(key, string.Empty, pVariables[key]);
                }

                CustomContext customContext = new CustomContext(new NameTable(), varList);


                XPathExpression xpath = XPathExpression.Compile(trimmed);

                xpath.SetContext(customContext);

                object commandOutput = navigator.Evaluate(xpath);

                if (commandOutput is XPathNodeIterator)
                {
                    return (XPathNodeIterator)commandOutput;
                }
                return null;

                #endregion

            }
            #endregion

            #region [ (virtual DoesNodeTestPass ]
            public virtual bool DoesNodeTestPass(XmlNode pSourceContextNode, System.Collections.Specialized.HybridDictionary pVariables, string pTest)
            {
                // see if there is a test and if so, validate it.
                if (pTest.Length != 0)
                {
                    bool retVal = false;
                    XmlNode nodeTest = GetRequestedXmlNode(pSourceContextNode, pVariables, pTest);
                    if (nodeTest != null)
                        if ((nodeTest.Name != "node") || ((nodeTest.Name == "node") && (nodeTest.InnerText.ToLower() != "false")))
                                retVal = true;

                    return retVal;                
                }
                // no length = no test = auto-pass
                return true;
            }
            #endregion

            #region [ (virtual) GetXmlValue ]
            public virtual string GetXmlValue(XmlNode pContext)
            {
                if (pContext != null)
                {
                    if (pContext.Value != null)
                    {
                        return pContext.Value;
                    }
                    else
                    {
                        return pContext.InnerText;
                    }
                }
                return "";
            }
            #endregion
            #endregion

        }
        #endregion

        #region [ Class: IfCommandInfo (supporting) ]
        class IfCommandInfo : SupportingObject
        {
            public string test;
            public int elseFragmentIndex;
            public int endIfFramgentIndex;

            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                string test = this.test.Trim();
                bool fromRoot = (test.Substring(0, 1) == "/");

                if (this.endIfFramgentIndex > this.ownerCommand.parsedIndex)
                {
                    if (this.DoesNodeTestPass(pSourceContextNode, pVariables, test))
                    {
                        int endIndex = ((this.elseFragmentIndex > this.ownerCommand.parsedIndex) ? this.elseFragmentIndex : this.endIfFramgentIndex);
                        this.ownerCommand.ownerTemplate.ProcessNodeArrayListPortion(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, this.ownerCommand.parsedIndex + 1, endIndex - 1);
                    }
                    else
                    {   // if there was a valid "else" block found, then run it
                        if (this.elseFragmentIndex > this.ownerCommand.parsedIndex)
                        {
                            this.ownerCommand.ownerTemplate.ProcessNodeArrayListPortion(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref pVariables, this.elseFragmentIndex + 1, this.endIfFramgentIndex - 1);
                        }
                    }

                    // once the if is complete (regardless pass or fail), move to next after "endif"
                    pIndex = this.endIfFramgentIndex + 1;
                }
                else
                {
                    // malformed IF (missing ENDIF)
                    pIndex++;
                }

            }

        }
        #endregion
    
        #region [ Class: EachCommandInfo (supporting) ]
        class EachCommandInfo : SupportingObject
        {
            public string pattern;
            public int endEachFramgentIndex;
            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                string pattern = this.pattern.Trim();

                XPathNodeIterator nodes = this.GetRequestedXmlNodeList(pSourceContextNode, pVariables, pattern);

                if (nodes != null)
                {
                    foreach (XPathNavigator nodeNavigator in nodes)
                    {
                        XmlNode node = (XmlNode)nodeNavigator.UnderlyingObject;

                        this.ownerCommand.ownerTemplate.ProcessNodeArrayListPortion(node, pSupportingTemplatesNode, ref pResults, ref pVariables, this.ownerCommand.parsedIndex + 1, this.endEachFramgentIndex - 1);
                    }
                }

                // Completion
                pIndex = this.endEachFramgentIndex + 1;
            }


        }
        #endregion

        #region [ Class: DoCommandInfo (supporting) ]
        class DoCommandInfo : SupportingObject
        {
            public string untilTest;
            public int untilFramgentIndex;

        }
        #endregion

        #region [ Class: WhileCommandInfo (supporting) ]
        class WhileCommandInfo : SupportingObject
        {
            //public string whileTest;
            public int endWhileFramgentIndex;

        }
        #endregion

        #region [ Class: ResultCommandInfo (supporting) ]
        class ResultCommandInfo : SupportingObject
        {
            public string variable;
            public string datatype;
            public string format;

            private EncodingMethods encodingMethod;

            public ResultCommandInfo(EncodingMethods pEncodingMethod)
            {
                    encodingMethod = pEncodingMethod;
            }

            public ResultCommandInfo(EncodingMethods pEncodingMethod, string pDatatype)
            {
                encodingMethod = pEncodingMethod;
                datatype = pDatatype;
            }

            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                string request = this.variable.Trim();

                XmlNode requestedNode = this.GetRequestedXmlNode(pSourceContextNode, pVariables, request);

                switch (this.encodingMethod)
                {
                    case EncodingMethods.emXml:
                        pResults.Add(this.ownerCommand.EscapeValue(this.GetXmlValue(requestedNode)));
                        break;

                    case EncodingMethods.emXhtml:
                        if (requestedNode != null)
                        {
                            string xmlIn = this.GetXmlValue(requestedNode), elementName;
                            StringBuilder xmlOut = new StringBuilder(xmlIn.Length);
                            int idxUnprocessed = 0;

                            Regex reTagStart = new Regex("<(?<name>/?[A-Za-z_][A-Za-z0-9._:-]*)\\s*");
                            Regex reTagEnd = new Regex("/?>");
                            Regex reAttribute = new Regex("\\G(?<name>[A-Za-z_][A-Za-z0-9._:-]*)(?<valued>(?<equals>\\s*=\\s*)"
                                + "(?<value>(?<quote>['\"]).*?\\k<quote>|[^ \\f\\n\\r\\t\\v]+)"
                            + ")?(?<trailingSpace>\\s*)");
                            Regex reEmptyElements = new Regex("^(area|base|basefont|br|col|frame|hr|img|input|isindex|link|meta|param)$");
                            Match match;
                            bool attributeRemains;
                            
                            do
                            {
                                // look for an element
                                match = reTagStart.Match(xmlIn, idxUnprocessed);
                                elementName = match.Groups["name"].Value;
                                if (elementName == String.Empty)
                                {
                                    // no element found; time to wrap up
                                    xmlOut.Append(xmlIn, idxUnprocessed, xmlIn.Length - idxUnprocessed);
                                }
                                else
                                {
                                    // copy everything before the first attribute or tag end
                                    xmlOut.Append(xmlIn, idxUnprocessed, match.Index + match.Length - idxUnprocessed);
                                    idxUnprocessed = match.Index + match.Length;
                                    do
                                    {
                                        // look for an attribute
                                        attributeRemains = idxUnprocessed < xmlIn.Length;
                                        match = (attributeRemains ? reAttribute.Match(xmlIn, idxUnprocessed) : null);
                                        attributeRemains = (match != null && match.Success);
                                        if (attributeRemains)
                                        {
                                            // copy the attribute with XML syntax
                                            if (!match.Groups["valued"].Success)  // unvalued attribute (e.g., <option selected>)
                                            {
                                                xmlOut.Append(String.Format("{0}=\"{1}\"{2}"
                                                    , match.Groups["name"].Value
                                                    , match.Groups["name"].Value
                                                    , match.Groups["trailingSpace"].Value
                                                ));
                                            }
                                            else if (!match.Groups["quote"].Success)  // unquoted attribute (e.g., <font size=3>)
                                            {
                                                xmlOut.Append(String.Format("{0}{1}\"{2}\"{3}"
                                                    , match.Groups["name"].Value
                                                    , match.Groups["equals"].Value
                                                    , match.Groups["value"].Value.Replace("<", "&lt;").Replace(">", "&gt;")
                                                    , match.Groups["trailingSpace"].Value
                                                ));
                                            }
                                            else // quoted attribute
                                            {
                                                xmlOut.Append(match.Value.Replace("<", "&lt;").Replace(">", "&gt;"));
                                            }
                                            idxUnprocessed = match.Index + match.Length;
                                        }
                                    } while (attributeRemains);

                                    // copy everything before the tag end
                                    match = (idxUnprocessed < xmlIn.Length ? reTagEnd.Match(xmlIn, idxUnprocessed) : null);
                                    if (match != null && match.Success && match.Index != idxUnprocessed)
                                    {
                                        xmlOut.Append(xmlIn.Substring(idxUnprocessed, match.Index - idxUnprocessed));
                                        idxUnprocessed = match.Index;
                                    }

                                    // force empty elements to self-close
                                    if (reEmptyElements.IsMatch(elementName.ToLower()) && idxUnprocessed < xmlIn.Length && xmlIn[idxUnprocessed] == '>')
                                    {
                                        xmlOut.Append(" /");

                                        // remove an explicit close tag
                                        match = (new Regex(String.Format("\\G>\\s*</{0}>", elementName))).Match(xmlIn, idxUnprocessed);
                                        if (match.Success)
                                        {
                                            idxUnprocessed = match.Index + match.Length - 1; // advance to next >
                                        }
                                    }
                                }
                            } while (elementName != String.Empty);

                            pResults.Add(xmlOut.ToString());
                        }
                        break;

                    case EncodingMethods.emFormatted:
                        if (this.datatype.ToLower().Trim() == "string")
                        {
                            int startIndex = (this.ownerCommand.parameters[0].ToLower().Trim() == "string" ? 2 : 3);
                            if (this.ownerCommand.parameters.Length > startIndex )
                            {
                                string[] variables = new string[this.ownerCommand.parameters.Length - startIndex ];
                                for (int i=startIndex ; i<this.ownerCommand.parameters.Length; i++)
                                    variables[i-startIndex ] = (string)this.ownerCommand.parameters[i];
                            
                                pResults.Add(this.FormatMultipleValues(this.format, variables, pVariables, pSourceContextNode));
                            }
                            
                        }
                        else
                        {
                            pResults.Add(this.FormatValue(this.datatype, this.format, this.GetXmlValue(requestedNode)));
                        }
                        break;


                    case EncodingMethods.emCopyOf:
                        if (requestedNode != null)
                            pResults.Add(requestedNode.OuterXml);
                        break;

                    case EncodingMethods.emCopyOfRaw:
                        if (requestedNode != null)
                            pResults.Add(this.ownerCommand.EscapeValue(requestedNode.OuterXml));
                        
                        break;
                    
                    case EncodingMethods.emMD5:
                        if (requestedNode != null)
                        {// convert result bytes to string
                            //pResults.Add(Encoding.Default.GetString(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.Default.GetBytes(this.GetXmlValue(requestedNode)))));

                            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                            byte[] arrBytes = System.Security.Cryptography.MD5.Create().ComputeHash(encoding.GetBytes( this.GetXmlValue(requestedNode) ));

                            StringBuilder sbOutput = new StringBuilder(arrBytes.Length);
                            foreach (byte b in arrBytes)
                            {
                                sbOutput.AppendFormat("{0:x2}", b);
                            }
                            pResults.Add(sbOutput.ToString());
                        
                        }
                        break;

                    case EncodingMethods.emSHA1:
                        if (requestedNode != null)
                        {// convert result bytes to string
                            //pResults.Add(Encoding.Default.GetString(System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.Default.GetBytes(this.GetXmlValue(requestedNode)))));

                            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                            byte[] arrBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(encoding.GetBytes(this.GetXmlValue(requestedNode)));

                            StringBuilder sbOutput = new StringBuilder(arrBytes.Length);
                            foreach (byte b in arrBytes)
                            {
                                sbOutput.AppendFormat("{0:x2}", b);
                            }
                            pResults.Add(sbOutput.ToString());
                        
                        }
                        break;

                    case EncodingMethods.emNone:
                    default:
                        if (requestedNode != null)
                            pResults.Add(this.GetXmlValue(requestedNode));
                        break;
                }
                

                pIndex++;
            }



            private string FormatValue(string pDatatype, string pFormat, string pValue)
            {
                if (pValue.Length != 0)
                {
                    switch (pDatatype.ToLower())
                    {
                        case "date":
                            DateTime dtValue;
                            dtValue = DateTime.Parse(pValue);
                            return dtValue.ToString(format);

                        case "number":
                            if (pValue.IndexOf(".") != -1)
                            {
                                double dblValue;
                                dblValue = System.Convert.ToDouble(pValue);
                                return dblValue.ToString(format);
                            }
                            else
                            {
                                Int64 intValue;
                                intValue = System.Convert.ToInt64(pValue);
                                return intValue.ToString(format);
                            }

                        default:
                            // NOTE: Unknown format type
                            return pValue;
                    }
                }
                else
                    return "";

            }


            private string FormatMultipleValues(string pFormat, string[] pValueRequests, System.Collections.Specialized.HybridDictionary pVariables, XmlNode pSourceContextNode)
            {
                if (pSourceContextNode != null)
                {
                    // NOTE: String formatting is not limited to a single value, so we will deal directly with the parameters
                    string[] values = new string[pValueRequests.Length];
                    for (int i = 0; i < pValueRequests.Length; i++)
                    {
                        XmlNode tempNode = this.GetRequestedXmlNode(pSourceContextNode, pVariables, pValueRequests[i]);
                        values[i] = this.GetXmlValue(tempNode);
                    }
                    return (string.Format(pFormat, values));
                }
                else
                    return "";
                
            }
        
        
        
        
        }
        #endregion

        #region [ Class: ApplyCommandInfo (supporting) ]
        class ApplyCommandInfo : SupportingObject
        {
            public string templateName = "";
            public string contextXpath = "";
            public int endApplyFramgentIndex = -1;

            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                string templateName = this.templateName.Trim();
                string contextXpath = this.contextXpath.Trim();

                XmlNode applyTemplate = pSupportingTemplatesNode.SelectSingleNode("*[@name='" + templateName + "']");

                System.Collections.Specialized.HybridDictionary mVariables;

                // if this is a paramapply, then deal with temporary variables contained within
                if (this.endApplyFramgentIndex != -1)
                {
                    // make a temporary copy of the variables collection that will be used ONLY for this apply
                    mVariables = new System.Collections.Specialized.HybridDictionary(pVariables.Count);
                    foreach (string key in pVariables.Keys)
                    {
                        mVariables.Add(key, pVariables[key]);
                    }

                    // loop through any command fragments contained between this start index and this end fragment index
                    // process everything??? -- probably no, only var and mvar, but for now, sure
                    ownerCommand.ownerTemplate.ProcessNodeArrayListPortion(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref mVariables, pIndex + 1, this.endApplyFramgentIndex - 1);
                }
                else
                {
                    mVariables = pVariables;
                }


                if (applyTemplate != null)
                {
                    Template myTemplate = new Template(applyTemplate);

                    if (contextXpath.Length != 0)
                    {
                        XPathNodeIterator nodes = this.GetRequestedXmlNodeList(pSourceContextNode, pVariables, contextXpath);


                        foreach (XPathNavigator nodeNavigator in (XPathNodeIterator)nodes)
                        {
                            XmlNode node = (XmlNode)nodeNavigator.UnderlyingObject;
                        
                            myTemplate.ProcessNodeArrayListPortion(node, pSupportingTemplatesNode, ref pResults, ref mVariables, 0, myTemplate.fragments.Count - 1);
                        }
                    }
                    else
                    {
                        myTemplate.ProcessNodeArrayListPortion(pSourceContextNode, pSupportingTemplatesNode, ref pResults, ref mVariables, 0, myTemplate.fragments.Count - 1);
                    }

                }

                // once the apply template is complete, update the index (increment if simple apply, or go past end if paramapply)
                if (this.endApplyFramgentIndex == -1)
                {
                    pIndex++;
                }
                else
                {
                    pIndex = this.endApplyFramgentIndex + 1;
                }

            }
        }
        #endregion

        #region [ Class: ReplaceCommandInfo (supporting) ]
        class ReplaceCommandInfo : SupportingObject
        {
            public string input = "";
            public string regExp = "";
            public string replacement = "";

            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                XmlNode requestedNode = this.GetRequestedXmlNode(pSourceContextNode, pVariables, this.input);

                string input = this.GetXmlValue(requestedNode);

                string result = Regex.Replace(input, this.regExp, this.replacement);
                
                pResults.Add(result);

                // once the replacement is complete, our index simply increments
                pIndex++;

            }
        }
        #endregion

        #region [ Class: VariableCommandInfo (supporting) ]
        class VariableCommandInfo : SupportingObject
        {
            public string variableName = "";
            public string contextXPath = "";
            public int multipartResultStartIndex = -1;
            public int multipartResultEndIndex = -1;
            public bool multipart = false;
            public bool external = false;


            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                if (this.multipart)
                {
                    // Process the block contained within and store the results as the named parameter

                    // make a temporary copy of the variables collection that will be used ONLY for this apply
                    System.Collections.Specialized.HybridDictionary mVariables = new System.Collections.Specialized.HybridDictionary(pVariables.Count);
                    foreach (string key in pVariables.Keys)
                    {
                        mVariables.Add(key, pVariables[key]);
                    }

                    
                    ArrayList newResults = new ArrayList();
                    ArrayList multipartResults = this.ownerCommand.ownerTemplate.ProcessNodeArrayListPortion(pSourceContextNode, pSupportingTemplatesNode, ref newResults, ref mVariables, this.multipartResultStartIndex + 1, this.multipartResultEndIndex - 1);
                    string[] results = new string[multipartResults.Count];

                    for (int q = 0; q < multipartResults.Count; q++)
                    {
                        results[q] = (string)multipartResults[q];
                    }
                    pVariables[variableName] = string.Join("", results);
                    
                    pIndex = this.multipartResultEndIndex + 1;

                }
                else if (this.external)
                {
                    string variableName = this.variableName.Trim();
                    string filepath = this.contextXPath.Trim();

                    XmlNode context;

                    XmlDocument extDoc = new XmlDocument();
                    extDoc.Load(filepath);
                    context = extDoc.DocumentElement;

                    XPathNavigator navigator = context.CreateNavigator();
                    XsltArgumentList varList = new XsltArgumentList();

                    foreach (string key in pVariables.Keys)
                    {
                        varList.AddParam(key, string.Empty, pVariables[key]);
                    }

                    CustomContext customContext = new CustomContext(new NameTable(), varList);

                    XPathExpression xpath = XPathExpression.Compile("/*");

                    xpath.SetContext(customContext);

                    //object commandOutput = navigator.Evaluate(trimmed);
                    object commandOutput = navigator.Evaluate(xpath);

                    if (commandOutput is XPathNodeIterator)
                    {
                        pVariables[variableName] = (XPathNodeIterator)commandOutput;
                    }
                    else
                    {
                        pVariables[variableName] = commandOutput.ToString();
                    }



                    pIndex++;

                }
                else
                {
                    string variableName = this.variableName.Trim();
                    string trimmed = this.contextXPath.Trim();


                    XmlNode context = pSourceContextNode;

                    XPathNavigator navigator = context.CreateNavigator();
                    XsltArgumentList varList = new XsltArgumentList();

                    foreach (string key in pVariables.Keys)
                    {
                        varList.AddParam(key, string.Empty, pVariables[key]);
                    }

                    CustomContext customContext = new CustomContext(new NameTable(), varList);

                    XPathExpression xpath = XPathExpression.Compile(trimmed);

                    xpath.SetContext(customContext);

                    //object commandOutput = navigator.Evaluate(trimmed);
                    object commandOutput = navigator.Evaluate(xpath);

                    if (commandOutput is XPathNodeIterator)
                    {
                        pVariables[variableName] = (XPathNodeIterator)commandOutput;
                    }
                    else
                    {
                        pVariables[variableName] = commandOutput.ToString();
                    }

                    pIndex++;
                }

            
            }
        }
        #endregion

        #region [ Class: VersionCommandInfo (supporting) ]
        class VersionCommandInfo : SupportingObject
        {
            public string version = "";

            public override void Process(XmlNode pSourceContextNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, ref int pIndex)
            {
                // NOTE: Each command should perform its own override for functionality.
                // NOTE: Each command must add your own string(s) to the results ArrayList
                // NOTE: Each command must update the index based on its completion criteria (i.e. 1st node after "/if" fragment)

                System.Reflection.AssemblyName assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                System.Version appVersion = assembly.Version;

                pResults.Add(String.Format("{0} v{1}.{2}.{3} rev.{4}", assembly.Name, appVersion.Major.ToString(), appVersion.Minor.ToString(), appVersion.Build.ToString(), appVersion.Revision.ToString()));

                // once the apply template is complete, our index simply increments
                pIndex++;

            }
        }
        #endregion

        #region [ Class: BreakCommandInfo (supporting) ]
        class BreakCommandInfo : SupportingObject
        {
            public string test;
            //public int code;
            //public string message;
        }
        #endregion

        #region [ Class: TextFragment ]
        private class TextFragment : StringFragment
        {
            public TextFragment(string pSource, int pStartIndex, int pEndIndex, Template pOwnerTemplate)
            {
                this.source = pSource;
                this.ownerTemplate = pOwnerTemplate;
                this.indexStart = pStartIndex;
                this.indexEnd = pEndIndex;
                this.length = pEndIndex - pStartIndex;
                this.type = StringFragmentTypes.sftText;
            }
        }
        #endregion

        #region [ Class: Template ]
        private class Template
        {
            #region [ Private Properties ]
            private string value;
            #endregion

            #region [ Public Properties ]
            private string delimiterStart = "{{";
            private string escapedDelimiterStart = "{{{{";
            private string delimiterEnd = "}}";
            public char[] whitespace = new char[3] { ' ', '\t', '\n' };
            public string variablePrefix = "$";

            public ArrayList fragments;
            #endregion

            #region [ Constructors ]
            public Template(XmlNode pXmlTemplate)
            {
                if (pXmlTemplate != null)
                {
                    XmlNode delimStart = pXmlTemplate.Attributes.GetNamedItem("delim-start");
                    XmlNode delimEnd = pXmlTemplate.Attributes.GetNamedItem("delim-end");
                    XmlNode whitespace = pXmlTemplate.Attributes.GetNamedItem("param-separators");
                    XmlNode varPrefix = pXmlTemplate.Attributes.GetNamedItem("variable-prefix");
                    
                    if (delimStart != null)
                        if (delimStart.Value.Length != 0)
                        {
                            this.delimiterStart = delimStart.Value;
                            this.escapedDelimiterStart = (this.delimiterStart + this.delimiterStart);
                        }

                    if (delimEnd != null)
                        if (delimEnd.Value.Length != 0)
                            this.delimiterEnd = delimEnd.Value;

                    if (whitespace != null)
                        if (whitespace.Value.Length != 0)
                            this.whitespace = whitespace.Value.ToCharArray();

                    if (varPrefix != null)
                        if (varPrefix.Value.Length != 0)
                            this.variablePrefix = varPrefix.Value;

                    if (pXmlTemplate.InnerXml.Length != 0)
                        this.value = pXmlTemplate.InnerXml;

                    this.Initialize();

                }
            }

            public Template(string pValue)
            {
                this.value = pValue;
                this.Initialize();
            }

            public Template(string pValue, string pDelimiterStart, string pDelimiterEnd)
            {
                this.value = pValue;
                this.delimiterStart = pDelimiterStart;
                this.delimiterEnd = pDelimiterEnd;
                this.Initialize();
            }
            #endregion

            #region [ Private Functions ]

            #region [ Initialize ]
            private void Initialize(string pValue, string pDelimiterStart, string pDelimiterEnd)
            {
                if ((pValue != null)&&(pValue.Length!=0))
                    this.value = pValue;

                this.SetDelimiters(pDelimiterStart, pDelimiterEnd);

                this.whitespace = new char[3];
                this.whitespace[0] = ' '; this.whitespace[1] = '\t'; this.whitespace[2] = '\n';

                this.Initialize();
            }
            private void Initialize()
            {

                fragments = this.ParseStringFragments(this.value);

                this.AnalyzeFragments();
                this.AnalyzeCommands();
            }
            #endregion

            #region [ SetDelimiters ]
            /// <summary>
            /// Method: SetDelimiters
            /// Description: Used to set the start and end delimiters of this object.
            /// They also have logic to prevent invalid delimiters from being set (null, empty string, etc.)
            /// </summary>
            /// <param name="pStart">The starting delimiter for a command</param>
            /// <param name="pEnd">The ending delimiter for a command</param>
            private void SetDelimiters(string pStart, string pEnd)
            {
                if ((pStart != null) && (pStart.Length != 0))
                {
                    this.delimiterStart = pStart;
                    this.escapedDelimiterStart = (pStart + pStart);
                }

                if ((pEnd != null) && (pEnd.Length != 0))
                    this.delimiterEnd = pEnd;
            }
            #endregion

            #region [ ParseStringFragments ]
            /// <summary>
            /// Private Function: ParseStringFragments
            /// Description: Used to parse through a template string to find command and text fragments
            /// </summary>
            /// <returns>ArrayList (array of StringFragment objects)</returns>
            private ArrayList ParseStringFragments(string pString)
            {
                ArrayList fragments = new ArrayList();

                for (int position = 0; position < pString.Length; /* manually incrementing below */ )
                {
                    var commandPosition = pString.IndexOf(this.delimiterStart, position);
                    if ((commandPosition >= position) && (pString.Substring(commandPosition, (this.delimiterStart.Length * 2))== this.escapedDelimiterStart))
                    {
                        Debug.WriteLine(String.Format("Text Block Found ({0} - {1}): [{2}]", position, commandPosition, pString.Substring(position, commandPosition - position)));
                        fragments.Add(new TextFragment(pString, position, commandPosition, this));

                        fragments.Add(new TextFragment(pString, commandPosition, commandPosition + (this.delimiterStart.Length), this));

                        position = commandPosition + (this.delimiterStart.Length * 2);
                    }
                    else
                    {
                        if (commandPosition >= position)
                        {
                            if (commandPosition > position)
                            {// if the current block does not start with the command delimiters, then add them as a text fragment
                                Debug.WriteLine(String.Format("Text Block Found ({0} - {1}): [{2}]", position, commandPosition, pString.Substring(position, commandPosition - position)));
                                fragments.Add(new TextFragment(pString, position, commandPosition, this));
                            }

                            var commandEndPosition = pString.IndexOf(this.delimiterEnd, commandPosition + this.delimiterStart.Length);

                            if (commandEndPosition >= 0)
                            {
                                Debug.WriteLine(String.Format("Command Block Found ({0} - {1}): [{2}]", commandPosition + this.delimiterStart.Length, commandEndPosition, pString.Substring(commandPosition + this.delimiterStart.Length, commandEndPosition - (commandPosition + this.delimiterStart.Length))));
                                fragments.Add(new CommandFragment(pString, commandPosition + this.delimiterStart.Length, commandEndPosition, this));
                                position = commandEndPosition + this.delimiterEnd.Length;
                            }
                            else
                            {
                                throw (new ApplicationException(String.Format(@"The given string template was not formatted properly.  Could not find matching end-delimiter for command found at position {0}.", position + commandPosition)));
                            }

                        }
                        else
                        {// add the final text fragment, if there is anything left to add, then move the position to the end
                            Debug.WriteLine(String.Format("End Block Found ({0} - {1}): [{2}]", position, pString.Length, pString.Substring(position, (pString.Length - position))));
                            fragments.Add(new TextFragment(pString, position, pString.Length, this));
                            position = pString.Length;
                        }
                    }
                }

                return fragments;
            }
            #endregion

            #region [ AnalyzeFragments ]
            /// <summary>
            /// Private Function: AnalyzeFragments
            /// Description: Used to iterate through the parsed fragments and analyze them for further processing
            /// </summary>
            /// <returns></returns>
            private void AnalyzeFragments()
            {
                for (int i = 0; i < this.fragments.Count; i++)
                {
                    StringFragment fragString = (StringFragment)this.fragments[i];
                    fragString.parsedIndex = i;

                    switch (fragString.type)
                    {
                        case StringFragmentTypes.sftText:
                            break;

                        case StringFragmentTypes.sftCommand:
                            CommandFragment fragCommand = (CommandFragment)this.fragments[i];

                            if (fragCommand.parameters.Length != 0)
                            {
                                switch (fragCommand.parameters[0].ToLower())
                                {   // PRIMARY COMMAND TYPES.
                                    case "if":
                                        fragCommand.commandType = CommandTypes.ctIf;
                                        fragCommand.supportingInfo = new IfCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((IfCommandInfo)fragCommand.supportingInfo).test = fragCommand.parameters[1];
                                        break;

                                    case "each":
                                        fragCommand.commandType = CommandTypes.ctEach;
                                        fragCommand.supportingInfo = new EachCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((EachCommandInfo)fragCommand.supportingInfo).pattern = fragCommand.parameters[1];
                                        break;
                                        
                                    case "do":
                                        fragCommand.commandType = CommandTypes.ctDo;
                                        // Cannot set "untilTest until it is matched up with its until.
                                        break;

                                    case "while":
                                        fragCommand.commandType = CommandTypes.ctWhile;
                                        fragCommand.supportingInfo = new WhileCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((IfCommandInfo)fragCommand.supportingInfo).test = fragCommand.parameters[1];
                                        break;

                                    case "break":
                                        fragCommand.commandType = CommandTypes.ctBreak;
                                        if (fragCommand.parameters.Length > 1)
                                            ((BreakCommandInfo)fragCommand.supportingInfo).test = fragCommand.parameters[1];
                                        break;

                                    case "%":
                                    case "apply":
                                        fragCommand.commandType = CommandTypes.ctApply;
                                        fragCommand.supportingInfo = new ApplyCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((ApplyCommandInfo)fragCommand.supportingInfo).templateName = fragCommand.parameters[1];
                                        if (fragCommand.parameters.Length > 2)
                                            ((ApplyCommandInfo)fragCommand.supportingInfo).contextXpath = fragCommand.parameters[2];

                                        break;


                                    case "%%":
                                    case "paramapply":
                                        fragCommand.commandType = CommandTypes.ctApplyWithParams;
                                        fragCommand.supportingInfo = new ApplyCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((ApplyCommandInfo)fragCommand.supportingInfo).templateName = fragCommand.parameters[1];
                                        if (fragCommand.parameters.Length > 2)
                                            ((ApplyCommandInfo)fragCommand.supportingInfo).contextXpath = fragCommand.parameters[2];

                                        break;



                                    case "=":
                                    case "result":
                                        fragCommand.commandType = CommandTypes.ctResult;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emXml);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;

                                    case "==":
                                    case "raw":
                                    case "rawresult":
                                        fragCommand.commandType = CommandTypes.ctRawResult;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emNone);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;

                                    case "xhtml":
                                        fragCommand.commandType = CommandTypes.ctXhtml;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emXhtml);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;

                                    case "*":
                                    case "copy":
                                        fragCommand.commandType = CommandTypes.ctCopy;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emCopyOf);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;

                                    case "*=":
                                    case "copyencoded":
                                        fragCommand.commandType = CommandTypes.ctCopyEncoded;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emCopyOfRaw);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;



                                    case "md5":
                                        fragCommand.commandType = CommandTypes.ctCopyEncoded;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emMD5);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;

                                    case "sha1":
                                        fragCommand.commandType = CommandTypes.ctCopyEncoded;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emSHA1);

                                        if (fragCommand.parameters.Length > 1)
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[1];
                                        break;

                                    case "~":
                                    case "replace":
                                        fragCommand.commandType = CommandTypes.ctReplace;
                                        fragCommand.supportingInfo = new ReplaceCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((ReplaceCommandInfo)fragCommand.supportingInfo).regExp = fragCommand.parameters[1];
                                        if (fragCommand.parameters.Length > 1)
                                            ((ReplaceCommandInfo)fragCommand.supportingInfo).replacement = fragCommand.parameters[2];
                                        if (fragCommand.parameters.Length > 1)
                                            ((ReplaceCommandInfo)fragCommand.supportingInfo).input = fragCommand.parameters[3];
                                        break;

                                    case "?":
                                    case "format":
                                        fragCommand.commandType = CommandTypes.ctFormattedResult;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emFormatted);

                                        if (fragCommand.parameters.Length > 3)
                                        {
                                            ((ResultCommandInfo)fragCommand.supportingInfo).datatype = fragCommand.parameters[1];
                                            ((ResultCommandInfo)fragCommand.supportingInfo).format = fragCommand.parameters[2];
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[3];
                                        }
                                        else
                                        {
                                            if (fragCommand.parameters.Length > 2)
                                            {
                                                ((ResultCommandInfo)fragCommand.supportingInfo).datatype = "string";
                                                ((ResultCommandInfo)fragCommand.supportingInfo).format = fragCommand.parameters[1];
                                                ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[2];
                                            }
                                        }
                                        break;

                                    case "date":
                                    case "number":
                                    case "string":
                                        fragCommand.commandType = CommandTypes.ctFormattedResult;
                                        fragCommand.supportingInfo = new ResultCommandInfo(EncodingMethods.emFormatted);

                                        if (fragCommand.parameters.Length > 2)
                                        {
                                            ((ResultCommandInfo)fragCommand.supportingInfo).datatype = fragCommand.parameters[0].ToLower();
                                            ((ResultCommandInfo)fragCommand.supportingInfo).format = fragCommand.parameters[1];
                                            ((ResultCommandInfo)fragCommand.supportingInfo).variable = fragCommand.parameters[2];
                                        }
                                        break;

                                    case ":=":
                                    case "var":
                                    case "variable":
                                        fragCommand.commandType = CommandTypes.ctVariable;
                                        fragCommand.supportingInfo = new VariableCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((VariableCommandInfo)fragCommand.supportingInfo).variableName = fragCommand.parameters[1];
                                        if (fragCommand.parameters.Length > 2)
                                            ((VariableCommandInfo)fragCommand.supportingInfo).contextXPath = fragCommand.parameters[2];

                                        break;


                                    case "::=":
                                    case "mvar":
                                    case "multivar":
                                        fragCommand.commandType = CommandTypes.ctVariable;
                                        fragCommand.supportingInfo = new VariableCommandInfo();
                                        ((VariableCommandInfo)fragCommand.supportingInfo).multipart = true;

                                        if (fragCommand.parameters.Length > 1)
                                            ((VariableCommandInfo)fragCommand.supportingInfo).variableName = fragCommand.parameters[1];
                                        break;



                                    case ":::":
                                    case "load":
                                        fragCommand.commandType = CommandTypes.ctVariable;
                                        fragCommand.supportingInfo = new VariableCommandInfo();
                                        ((VariableCommandInfo)fragCommand.supportingInfo).external = true;

                                        if (fragCommand.parameters.Length > 2)
                                        {
                                            ((VariableCommandInfo)fragCommand.supportingInfo).variableName = fragCommand.parameters[1];

                                            ((VariableCommandInfo)fragCommand.supportingInfo).contextXPath = fragCommand.parameters[2];
                                        }
                                        break;



                                    case "ver":
                                    case "version":
                                        fragCommand.commandType = CommandTypes.ctVersion;
                                        fragCommand.supportingInfo = new VersionCommandInfo();
                                        break;



                                    // SUPPORTING/END COMMAND TYPES
                                    case "else":
                                        fragCommand.commandType = CommandTypes.ctElse;
                                        break;

                                    case "/if":
                                    case "endif":
                                        fragCommand.commandType = CommandTypes.ctEndIf;
                                        break;

                                    case "/each":
                                    case "endeach":
                                        fragCommand.commandType = CommandTypes.ctEndEach;
                                        break;

                                    case "until":  // NOTE: Until is a little tricky, as the test info is in the tail, not the head
                                        fragCommand.commandType = CommandTypes.ctUntil;
                                        fragCommand.supportingInfo = new DoCommandInfo();

                                        if (fragCommand.parameters.Length > 1)
                                            ((DoCommandInfo)fragCommand.supportingInfo).untilTest = fragCommand.parameters[1];
                                        break;

                                    case "/while":
                                    case "wend":
                                        fragCommand.commandType = CommandTypes.ctEndWhile;
                                        break;

                                    case "/%%":
                                    case "/paramapply":
                                    case "endparamapply":
                                        fragCommand.commandType = CommandTypes.ctEndApplyWithParams;
                                        break;

                                    case "/::=":
                                    case "/mvar":
                                    case "/multivar":
                                    case "endmvar":
                                    case "endmultivar":
                                        fragCommand.commandType = CommandTypes.ctEndMultipartVariable;
                                        break;

                                    // NOTE: Unhandled command types are ignored.
                                    default:
                                        fragCommand.commandType = CommandTypes.ctUnknown;
                                        break;
                                }
                            }

                            if (fragCommand != null)
                                if (fragCommand.supportingInfo != null)
                                    ((SupportingObject)fragCommand.supportingInfo).ownerCommand = fragCommand;
                            break;
                    }

                }
            }
            #endregion

            #region [ AnalyzeCommands ]
            /// <summary>
            /// Private Function: AnalyzeCommands
            /// Description: Used to iterate through the parsed commands match them with their ends to be ready for processing
            /// </summary>
            /// <returns></returns>
            private void AnalyzeCommands()
            {
                for (int i = 0; i < this.fragments.Count; i++)
                {
                    if (((StringFragment)this.fragments[i]).IsCommandFragment())
                    {
                        CommandFragment commandFrag = (CommandFragment)this.fragments[i];
                        switch (commandFrag.commandType)
                        {
                            case CommandTypes.ctResult:
                                break;

                            case CommandTypes.ctRawResult:
                                break;

                            case CommandTypes.ctXhtml:
                                break;

                            case CommandTypes.ctApply:
                                break;

                            case CommandTypes.ctApplyWithParams:
                                int applyCount = 1;
                                for (int j = i + 1; (applyCount != 0) && (j < this.fragments.Count); j++)
                                {
                                    if (((StringFragment)this.fragments[j]).IsCommandFragment())
                                    {
                                        CommandFragment checkFrag = (CommandFragment)this.fragments[j];
                                        if (checkFrag.commandType == CommandTypes.ctApplyWithParams)
                                            applyCount++;

                                        if (checkFrag.commandType == CommandTypes.ctEndApplyWithParams)
                                        {
                                            applyCount--;
                                            if (applyCount == 0)
                                                ((ApplyCommandInfo)commandFrag.supportingInfo).endApplyFramgentIndex = j;
                                        }
                                    }
                                }
                                break;


                            case CommandTypes.ctIf:
                                int ifCount = 1;
                                for (int j = i + 1; (ifCount != 0) && (j < this.fragments.Count); j++)
                                {
                                    if (((StringFragment)this.fragments[j]).IsCommandFragment())
                                    {
                                        CommandFragment checkFrag = (CommandFragment)this.fragments[j];
                                        if (checkFrag.commandType == CommandTypes.ctIf)
                                            ifCount++;

                                        if ((checkFrag.commandType == CommandTypes.ctElse) && (ifCount == 1))
                                            ((IfCommandInfo)commandFrag.supportingInfo).elseFragmentIndex = j;

                                        if (checkFrag.commandType == CommandTypes.ctEndIf)
                                        {
                                            ifCount--;
                                            if (ifCount == 0)
                                                ((IfCommandInfo)commandFrag.supportingInfo).endIfFramgentIndex = j;
                                        }
                                    }
                                }
                                break;

                            case CommandTypes.ctVariable:
                                if (((VariableCommandInfo)commandFrag.supportingInfo).multipart)
                                {
                                    ((VariableCommandInfo)commandFrag.supportingInfo).multipartResultStartIndex = commandFrag.parsedIndex;
                                    int mvarCount = 1;

                                    for (int j = i + 1; (mvarCount != 0) && (j < this.fragments.Count); j++)
                                    {
                                        if (((StringFragment)this.fragments[j]).IsCommandFragment())
                                        {
                                            CommandFragment checkFrag = (CommandFragment)this.fragments[j];
                                            if (checkFrag.commandType == CommandTypes.ctMultipartVariable)
                                                mvarCount++;

                                            if (checkFrag.commandType == CommandTypes.ctEndMultipartVariable)
                                            {
                                                mvarCount--;
                                                if (mvarCount == 0)
                                                {
                                                    ((VariableCommandInfo)commandFrag.supportingInfo).multipartResultEndIndex = j;
                                                    checkFrag.supportingInfo = commandFrag.supportingInfo;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;

                            case CommandTypes.ctDo:
                                int doCount = 1;
                                for (int j = i + 1; (doCount != 0) && (j < this.fragments.Count); j++)
                                {
                                    if (((StringFragment)this.fragments[j]).IsCommandFragment())
                                    {
                                        CommandFragment checkFrag = (CommandFragment)this.fragments[j];
                                        if (checkFrag.commandType == CommandTypes.ctDo)
                                            doCount++;

                                        if (checkFrag.commandType == CommandTypes.ctUntil)
                                        {
                                            doCount--;
                                            if (doCount == 0)
                                                ((DoCommandInfo)commandFrag.supportingInfo).untilFramgentIndex = j;
                                        }
                                    }
                                }
                                break;

                            case CommandTypes.ctWhile:
                                int whileCount = 1;
                                for (int j = i + 1; (whileCount != 0) && (j < this.fragments.Count); j++)
                                {
                                    if (((StringFragment)this.fragments[j]).IsCommandFragment())
                                    {
                                        CommandFragment checkFrag = (CommandFragment)this.fragments[j];
                                        if (checkFrag.commandType == CommandTypes.ctWhile)
                                            whileCount++;

                                        if (checkFrag.commandType == CommandTypes.ctEndWhile)
                                        {
                                            whileCount--;
                                            if (whileCount == 0)
                                                ((WhileCommandInfo)commandFrag.supportingInfo).endWhileFramgentIndex = j;
                                        }
                                    }
                                }
                                break;

                            case CommandTypes.ctEach:
                                int eachCount = 1;
                                for (int j = i + 1; (eachCount != 0) && (j < this.fragments.Count); j++)
                                {
                                    if (((StringFragment)this.fragments[j]).IsCommandFragment())
                                    {
                                        CommandFragment checkFrag = (CommandFragment)this.fragments[j];
                                        if (checkFrag.commandType == CommandTypes.ctEach)
                                            eachCount++;

                                        if (checkFrag.commandType == CommandTypes.ctEndEach)
                                        {
                                            eachCount--;
                                            if (eachCount == 0)
                                                ((EachCommandInfo)commandFrag.supportingInfo).endEachFramgentIndex = j;
                                        }
                                    }
                                }
                                break;

                        }

                    }

                }

            }
            #endregion

            #endregion

            #region [ Public Methods ]

            #region [ ProcessNode ]
            /// <summary>
            /// Public Method: ProcessNode
            /// Description: Used to process a parsed Template against a source node singularly
            /// </summary>
            /// <param name="pSourceNode"></param>
            /// <param name="pSupportingTemplatesNode"></param>
            /// <returns></returns>
            public string[] ProcessNode(XmlNode pSourceNode)
            {
                return this.ProcessNode(pSourceNode, null);
            }
            #endregion

            #region [ ProcessNode (w/supporting templates) ]
            /// <summary>
            /// Public Method: ProcessNode
            /// Description: Used to process a parsed Template against a source node with supporting template nodes
            /// </summary>
            /// <param name="pSourceNode"></param>
            /// <param name="pSupportingTemplatesNode"></param>
            /// <returns>string[]</returns>
            public string[] ProcessNode(XmlNode pSourceNode, XmlNode pSupportingTemplatesNode)
            {
                ArrayList alResults = new ArrayList();
                System.Collections.Specialized.HybridDictionary hdVariables = new System.Collections.Specialized.HybridDictionary();

                alResults = this.ProcessNodeArrayList(pSourceNode, pSupportingTemplatesNode, ref alResults, ref hdVariables);

                string[] result = new string[alResults.Count];
                for (int i = 0; i < alResults.Count; i++)
                    result[i] = (string)alResults[i];

                return result;
            }
            #endregion

            #region [ ProcessNodeArrayList ]
            /// <summary>
            /// Public Method: ProcessNodeArrayList
            /// Description: Used to process a parsed Template against a source node with supporting template nodes
            /// </summary>
            /// <param name="pSourceNode"></param>
            /// <param name="pSupportingTemplatesNode"></param>
            /// <returns>ArrayList</returns>
            public ArrayList ProcessNodeArrayList(XmlNode pSourceNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables)
            {
                return this.ProcessNodeArrayListPortion(pSourceNode, pSupportingTemplatesNode, ref pResults, ref pVariables, 0, this.fragments.Count - 1);
            }
            #endregion

            #region [ ProcessNodeArrayListPortion ]
            /// <summary>
            /// Public Method: ProcessNodeArrayListPortion
            /// Description: Used to process a parsed Template against a source node with supporting template nodes
            /// </summary>
            /// <param name="pSourceNode"></param>
            /// <param name="pSupportingTemplatesNode"></param>
            /// <returns></returns>
            public ArrayList ProcessNodeArrayListPortion(XmlNode pSourceNode, XmlNode pSupportingTemplatesNode, ref ArrayList pResults, ref System.Collections.Specialized.HybridDictionary pVariables, int pStartIndex, int pEndIndex)
            {
                ArrayList results = pResults;

                XmlNode context = pSourceNode;
                bool stopped = false;

                for (int index = pStartIndex; ((index <= pEndIndex) && (!stopped)); /* manually incrementing */)
                {
                    StringFragment sfFrag = (StringFragment)this.fragments[index];

                    switch (sfFrag.type)
                    {

                        case StringFragmentTypes.sftText:
                            results.Add(sfFrag.GetValue());
                            index++;
                            break;

                        case StringFragmentTypes.sftCommand:
                            CommandFragment cmdFrag = (CommandFragment)sfFrag;
                            cmdFrag.Process(pSourceNode, pSupportingTemplatesNode, ref index, ref results, ref pVariables);
                            break;

                    }

                }

                pResults = results;

                return results;
            }
            #endregion

            #region [ EscapeValue ]
            public string EscapeValue(string pString)
            {
                if (pString != null)
                    return System.Security.SecurityElement.Escape(pString).Replace("&apos;", "&#39;");
                return "";
            }
            #endregion

            #region [ GetSubstringValue ]
            public string GetSubstringValue(int pStartIndex, int pEndIndex)
            {
                if ((this.value.Length >= 0) && (pStartIndex >= 0) && (pEndIndex <= this.value.Length))
                    return this.value.Substring(pStartIndex, pEndIndex - pStartIndex);
                return "";
            }
            #endregion

            #region [ GetSubstringValueEscaped ]
            public string GetSubstringValueEscaped(int pStartIndex, int pEndIndex)
            {
                if ((this.value.Length >= 0) && (pStartIndex >= 0) && (pEndIndex <= this.value.Length))
                    return this.EscapeValue(this.value.Substring(pStartIndex, pEndIndex - pStartIndex));
                return "";
            }
            #endregion

            #endregion
        }

        #endregion

        #endregion


        #region [ Constructor: X2TL ]
        /// <summary>
        /// Constructor:
        /// </summary>
        public X2TL()
        {
            // 
        }
        #endregion

        #region [ Public Methods: X2TL ]

        #region [ TransformNode ]
        /// <summary>
        /// Method: TransformNode
        /// Description: Used to transform a source node using one or more templates
        /// </summary>
        /// <param name="pSourceNode"></param>
        /// <param name="pTemplateNode"></param>
        /// <returns></returns>
        public string TransformNode(XmlNode pSourceNode, XmlNode pTemplateNode, XmlNode pSupportingTemplatesNode)
        {
            Template template = new Template(pTemplateNode);

            string[] resultString = template.ProcessNode(pSourceNode, pSupportingTemplatesNode);

            string result = string.Join("", resultString);

            Debug.WriteLine(String.Format("Final Result: {0}", result));
            return result;
        }
        #endregion

        #endregion

    }
    #endregion
}
#endregion
