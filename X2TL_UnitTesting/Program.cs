using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace X2TL_UnitTesting
{
    class Program
    {
        class Tester
        {
            private string testName;
            private string description;
            private string source;
            private string template;
            private string supportngTemplates;
            private string expectedResult;
            private bool isException;
            private bool verbose;

            public RelWare.X2TL_NUnit nUnit;

            public Tester(bool verbose)
            {
                this.nUnit = new RelWare.X2TL_NUnit();
                this.verbose = verbose;
            }

            public void Init(string pTestName, string pDescription, string pSource, string pTemplate, string pSupportngTemplates, string pResult)
            {
                Init(pTestName, pDescription, pSource, pTemplate, pSupportngTemplates, pResult, false);
            }


            public void Init(string pTestName, string pDescription, string pSource, string pTemplate, string pSupportngTemplates, string pResult, bool pIsException)
            {
                this.testName = pTestName;
                this.description = pDescription;
                this.source = pSource;
                this.template = pTemplate;
                this.supportngTemplates = pSupportngTemplates;
                this.expectedResult = pResult;
                this.isException = pIsException;

                if (this.verbose)
                {
                    Console.WriteLine( string.Format("===[ {0} ] =================================", this.testName)  );
                    if (this.description.Length != 0)
                    {
                        Console.WriteLine(this.description);
                    }
                    Console.WriteLine(string.Format("> Source:\n{0}\n\t", this.source));
                    Console.WriteLine(string.Format("> Template:\n{0}\n\t", this.template));
                    if (this.supportngTemplates.Length != 0)
                    {
                        Console.WriteLine(string.Format("> Supporting Templates:\n{0}\n\t", this.supportngTemplates));
                    }
                    Console.WriteLine(string.Format("> Expected Result:\n{0}\n", this.expectedResult));
                }
            }

            public void RunTest()
            {
                //Console.WriteLine(String.Format("\n\n---Testing {0} ---\n{1}\nSource: {2}\nTemplate: {3}\nSupporting: {4}\n", this.testName, this.description, this.source, this.template, this.supportngTemplates, this.expectedResult));
                Console.WriteLine(this.nUnit.RunTest(this.testName, this.source, this.template, this.supportngTemplates, this.expectedResult, this.isException));

                if (this.verbose)
                {
                    Console.WriteLine("\n\n");
                }
            }
        }

        static void Main(string[] args)
        {
            bool verbose = false;

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "/?":
                    case "/help":
                        Console.WriteLine("Usage:\n/? or /help to show this screen\n/v to provide verbose details");
                        return;
                    case "/v":
                        verbose = true;
                        break;
                }
            }

            Tester tester = new Tester(verbose);

            /* Immediate test */

            //Console.WriteLine("--- Testing Simple Initiailization ---");
            //Console.WriteLine("[ Verify that component can initialize properly ]\n");
            //Console.WriteLine(tester.nUnit.testInit());

            string Name;
            string Desc;
            string Source;
            string Template;
            string Support;
            string Test;

            Name = "Simple Parse";
            Desc = "[ Verify that component can parse properly with no actual commands ]";
            Source = @"<Patient account=""DH:H3770"" patientNo=""11111111""><row patientNo=""1234"" lastName=""Howard"" firstName=""Moe""/><message>Please come in to review your recent results.</message></Patient>";
            Template = @"<template><div patientNo=""_{_{ = @patientNo }_}_"">_{_{= (row/@lastName)}_}_, _{_{ = (row/@firstName) }_}_ - <span>_{_{ = /Patient/@account }_}_</span><br /><h1>_{_{ result message }_}_</h1></div></template>";
            Test = @"<div patientNo=""_{_{ = @patientNo }_}_"">_{_{= (row/@lastName)}_}_, _{_{ = (row/@firstName) }_}_ - <span>_{_{ = /Patient/@account }_}_</span><br /><h1>_{_{ result message }_}_</h1></div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();



            Name = "Escaped Delimeters";
            Desc = @"[ Verify that component can handle ""escaped"" delimeters ]";
            Source = @"<Patient account=""DH:H3770"" patientNo=""11111111""><row patientNo=""1234"" lastName=""Howard"" firstName=""Moe""/><message>Please come in to review your recent results.</message></Patient>";
            Template = @"<template><div patientNo=""{{{{ = @patientNo }}"">{{{{= (row/@lastName)}}, {{{{ = (row/@firstName) }} - <span>{{{{ = /Patient/@account }}</span></div></template>";
            Test = @"<div patientNo=""{{ = @patientNo }}"">{{= (row/@lastName)}}, {{ = (row/@firstName) }} - <span>{{ = /Patient/@account }}</span></div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "Results (=) Command";
            Desc = "[ Verify that component can replace variables of any replacement type ]";
            Source = @"<Patient account=""DH:H3770"" patientNo=""11111111""><row patientNo=""1234"" lastName=""Howard"" firstName=""Moe""/><message>Please come in to review your recent results.</message></Patient>";
            Template = @"<template><div patientNo=""{{ = @patientNo }}"">{{= (row/@lastName)}}, {{ = (row/@firstName) }} - <span>{{ = /Patient/@account }}</span><br/><h1>{{ result message }}</h1></div></template>";
            Test = @"<div patientNo=""11111111"">Howard, Moe - <span>DH:H3770</span><br /><h1>Please come in to review your recent results.</h1></div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "Results (=) with Encoded Characters";
            Desc = "[ Verify that component can properly encode replacement values ]";
            Source = @"<PageData><Request Method=""GET""><URL url=""http://192.168.5.2/onerecord/default.aspx?XCI=HomePage_Portlet&amp;XCE=NursingCalculators&amp;UserNo=10021&amp;SessionSalt=28800000:1884:0E432035F2101328086A50261BEF178BEC527338075F4A"" runtime=""0.0977""/></Request></PageData>";
            Template = @"<template>{{ EACH Request/URL }}<span><strong>URL: </strong>{{ = @url }}</span>{{ /EACH }}</template>";
            Test = @"<span><strong>URL: </strong>http://192.168.5.2/onerecord/default.aspx?XCI=HomePage_Portlet&amp;XCE=NursingCalculators&amp;UserNo=10021&amp;SessionSalt=28800000:1884:0E432035F2101328086A50261BEF178BEC527338075F4A</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "RawResults (==) Command";
            Desc = "[ Verify that component can replace variables of any replacement type ]";
            Source = @"<Patient account=""DH:H3770"" patientNo=""11111111""><row patientNo=""1234"" lastName=""Howard"" firstName=""Moe""/><message>Please come in to review your recent results.</message></Patient>";
            Template = @"<template><div patientNo=""{{ = @patientNo }}"">{{== (row/@lastName)}}, {{ == (row/@firstName) }} - <span>{{ == /Patient/@account }}</span><br/><h1>{{ result message }}</h1></div></template>";
            Test = @"<div patientNo=""11111111"">Howard, Moe - <span>DH:H3770</span><br /><h1>Please come in to review your recent results.</h1></div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "RawResults (==) with Encoded Characters";
            Desc = "[ Verify that component can get raw (un-encoded) result values ]";
            Source = @"<PageData><Request Method=""GET""><URL url=""http://192.168.5.2/onerecord/default.aspx?XCI=HomePage_Portlet&amp;XCE=NursingCalculators&amp;UserNo=10021&amp;SessionSalt=28800000:1884:0E432035F2101328086A50261BEF178BEC527338075F4A"" runtime=""0.0977""/></Request></PageData>";
            Template = @"<template>{{ EACH /PageData/Request/URL }}<span><strong>URL: </strong>{{ == @url }}</span>{{ /EACH }}</template>";
            Test = @"<span><strong>URL: </strong>http://192.168.5.2/onerecord/default.aspx?XCI=HomePage_Portlet&XCE=NursingCalculators&UserNo=10021&SessionSalt=28800000:1884:0E432035F2101328086A50261BEF178BEC527338075F4A</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "XHTML (xhtml) Command";
            Desc = "[ Verify that component can convert HTML to XHTML ]";
            Source = @"<html><![CDATA[<div unquoted=1 specials=""<>&amp;&quot;""><br></br><BR><textarea>&lt;&gt;&amp;&quot;</textarea><HR empty ><hr />&lt;&gt;&amp;&quot;</div> ]]></html>";
            Template = @"<template><div>{{xhtml /html }}</div></template>";
            Test = @"<div><div unquoted=""1"" specials=""&lt;&gt;&amp;&quot;""><br /><BR /><textarea>&lt;&gt;&amp;&quot;</textarea><HR empty=""empty""  /><hr />&lt;&gt;&amp;&quot;</div> </div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "MD5 (md5) Command";
            Desc = "[ Verify that component can create an MD5 hash ]";
            Source = @"<html><![CDATA[<div unquoted=1 specials=""<>&amp;&quot;""><br></br><BR><textarea>&lt;&gt;&amp;&quot;</textarea><HR empty ><hr />&lt;&gt;&amp;&quot;</div> ]]></html>";
            Template = @"<template><div>{{md5 /html }}</div></template>";
            Test = @"<div>f2e35c11b7bbee227c6201a534c02010</div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();

            Name = "SHA1 (sha1) Command";
            Desc = "[ Verify that component can create an SHA1 hash ]";
            Source = @"<html><![CDATA[<div unquoted=1 specials=""<>&amp;&quot;""><br></br><BR><textarea>&lt;&gt;&amp;&quot;</textarea><HR empty ><hr />&lt;&gt;&amp;&quot;</div> ]]></html>";
            Template = @"<template><div>{{sha1 /html }}</div></template>";
            Test = @"<div>31d69b566323ed39bc018799b6616ad7b2c51458</div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            // Apostrophe tests
            Name = "Results (=) with Apostrophe";
            Desc = "[ Verify that component can handle apostrophe values properly ]";
            Source = @"<data><node attrib=""'""/></data>";
            Template = @"<template><span>testing apostrophe: {{ = node/@attrib }}</span></template>";
            Test = @"<span>testing apostrophe: &#39;</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "RawResults (==) with Apostrophe";
            Desc = "[ Verify that component can handle apostrophe values properly ]";
            Source = @"<data><node attrib=""'""/></data>";
            Template = @"<template><span>testing apostrophe: {{ == node/@attrib }}</span></template>";
            Test = @"<span>testing apostrophe: '</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();




            Name = "EACH Command";
            Desc = "[ Verify that component can loop via Each ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Demographics><row lastName=""BROWN"" firstName=""CHARLIE""/></Demographics><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><div patientNo=""{{ = @patientNo }}"">{{= (Demographics/row/@lastName)}}, {{ result (Demographics/row/@firstName) }} - <span>{{ = /Patient/@account }}</span></div>{{ Each Allergies/row }}<div allergyNo=""{{= @allergyNo}}"">{{= @description}}</div>{{ /Each }}</template>";
            Test = @"<div patientNo=""11111111"">BROWN, CHARLIE - <span>DH:SHULTZ</span></div><div allergyNo=""1001"">PEANUTS</div><div allergyNo=""1002"">CODIENE</div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            Name = "EACH Command nested with attributes";
            Desc = "[ Verify that component can loop nested and can loop attributes ]";
            Source = @"<SomeNode><row data1=""abc"" data2=""def"" data3=""ghi"" data4=""jkl""/><row data1=""123"" data2=""456"" data3=""789"" data4=""000""/></SomeNode>";
            Template = @"<template><table><tbody>{{ EACH row }}<tr>{{ each (@*) }}<td>{{= (.)}}</td>{{ /each }}</tr>{{/EACH}}</tbody></table></template>";
            Test = @"<table><tbody><tr><td>abc</td><td>def</td><td>ghi</td><td>jkl</td></tr><tr><td>123</td><td>456</td><td>789</td><td>000</td></tr></tbody></table>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();



            Name = "Simple IF - ENDIF";
            Desc = "[ Verify that IF command can parse correctly ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Demographics><row lastName=""BROWN"" firstName=""CHARLIE""/></Demographics><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><span id=""fullName"">{{ IF Demographics }}{{= Demographics/row/@lastName}}, {{= Demographics/row/@firstName}}{{ ENDIF }}</span></template>";
            Test = @"<span id=""fullName"">BROWN, CHARLIE</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();




            Name = "Simple IF / ELSE / ENDIF";
            Desc = "[ Verify that IF can work with an ELSE block ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Demographics><row lastName=""BROWN"" firstName=""CHARLIE""/></Demographics><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><span id=""fullName"">{{ IF Demographics }}{{= Demographics/row/@lastName}}, {{= Demographics/row/@firstName}}{{ ELSE }}N/A{{ ENDIF }}</span></template>";
            Test = @"<span id=""fullName"">BROWN, CHARLIE</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();

            Name = "Simple IF / *ELSE* / ENDIF";
            Desc = "[ Verify that ELSE portion of IE / ELSE block works ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><span id=""fullName"">{{ IF Demographics }}{{= Demographics/row/@lastName}}, {{= Demographics/row/@firstName}}{{ ELSE }}N/A{{ ENDIF }}</span></template>";
            Test = @"<span id=""fullName"">N/A</span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();



            Name = "Nested IF / *ELSE* / ENDIF";
            Desc = "[ Verify that Nested IF block work ]";
            Source = @"<node test1=""true"" test2=""false"" test3=""true""/>";
            Template = @"<template><span>{{ IF ""@test1='true'"" }}1... {{ IF ""@test2='true'"" }}2... {{ IF ""@test3='true'"" }}3... {{ ENDIF }}{{ ENDIF }}{{ ENDIF }}</span></template>";
            Test = @"<span>1... </span>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();




            Name = "Combination Test (IF/ELSE, EACH, RESULT)";
            Desc = "[ Verify a combination of these items works properly ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><div patientNo=""{{ = @patientNo }}"">{{ IF Demographics }}{{= Demographics/row/@lastName}}, {{= Demographics/row/@firstName}}{{ ELSE }}N/A{{ ENDIF }} - <span>{{ = /Patient/@account }}</span>{{ if message }}<br/><h1>{{ result message }}</h1>{{ /if }}</div>{{ IF Allergies }}<table><tbody>{{ EACH Allergies/row }}<tr><td allergyNo=""{{= @allergyNo}}"">{{= @description}}</td></tr>{{/each}}</tbody></table>{{ else }}<div>No allergy information provided</div>{{ /if }}</template>";
            Test = @"<div patientNo=""11111111"">N/A - <span>DH:SHULTZ</span></div><table><tbody><tr><td allergyNo=""1001"">PEANUTS</td></tr><tr><td allergyNo=""1002"">CODIENE</td></tr></tbody></table>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();




            Name = "Apply (Supporting Templates)";
            Desc = "[ Verify that applying supporting templates works ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Demographics><row lastName=""BROWN"" firstName=""CHARLIE""/></Demographics><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><div patientNo=""{{ = @patientNo }}"">{{ EACH Demographics }}{{ apply FullName }}{{ /EACH }} - <span>{{ = /Patient/@account }}</span>{{ if message }}<br/><h1>{{ result message }}</h1>{{ /if }}</div>{{ EACH Allergies }}{{ apply Allergies }}{{ /EACH }}</template>";
            Support = @"<templates><template name=""FullName"">{{ IF row }}{{= row/@lastName}}, {{= row/@firstName}}{{ ELSE }}N/A{{ ENDIF }}</template><template name=""Allergies"">{{ IF (count(row)!=0) }}<table><tbody>{{ EACH row }}<tr><td allergyNo=""{{= @allergyNo}}"">{{= @description}}</td></tr>{{/each}}</tbody></table>{{ else }}<div>No allergy information provided</div>{{ /if }}</template></templates>";
            Test = @"<div patientNo=""11111111"">BROWN, CHARLIE - <span>DH:SHULTZ</span></div><table><tbody><tr><td allergyNo=""1001"">PEANUTS</td></tr><tr><td allergyNo=""1002"">CODIENE</td></tr></tbody></table>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Apply (Supporting Templates with Context)";
            Desc = "[ Verify that applying supporting templates against a given context works ]";
            Source = @"<Patient account=""DH:SHULTZ"" patientNo=""11111111""><Demographics><row lastName=""BROWN"" firstName=""CHARLIE""/></Demographics><Allergies><row allergyNo=""1001"" description=""PEANUTS""/><row allergyNo=""1002"" description=""CODIENE""/></Allergies></Patient>";
            Template = @"<template><div patientNo=""{{ = @patientNo }}"">{{ apply FullName Demographics/row }} - <span>{{ = /Patient/@account }}</span>{{ if message }}<br/><h1>{{ result message }}</h1>{{ /if }}</div>{{ apply Allergies Allergies }}</template>";
            Support = @"<templates><template name=""FullName"">{{ IF (.) }}{{= @lastName}}, {{= @firstName}}{{ ELSE }}N/A{{ ENDIF }}</template><template name=""Allergies"">{{ IF (count(row)!=0) }}<table><tbody>{{ % AllergyRow row }}</tbody></table>{{ else }}<div>No allergy information provided</div>{{ /if }}</template><template name=""AllergyRow""><tr><td allergyNo=""{{= @allergyNo}}"">{{= @description}}</td></tr></template></templates>";
            Test = @"<div patientNo=""11111111"">BROWN, CHARLIE - <span>DH:SHULTZ</span></div><table><tbody><tr><td allergyNo=""1001"">PEANUTS</td></tr><tr><td allergyNo=""1002"">CODIENE</td></tr></tbody></table>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Format Date";
            Desc = "[ Verify that formatting of date values works ]";
            Source = @"<PageData><SystemTime utc=""8/26/2010 2:30:51 PM"" utciso=""2010-08-26T14:30:51Z"" local=""8/26/2010 10:30:51 AM"" local24=""08/26/2010 10:30:51"" month=""8"" monthName=""August"" monthAbbrev=""Aug"" day=""26"" dayName=""Thursday"" year=""2010"" hour24=""10"" hour12=""10"" minutes=""30"" seconds=""51"" ampm=""am"" /></PageData>";
            Template = @"<template><div><span>Original: {{ = SystemTime/@utc }}</span><br /><span>Standard Date: {{ format date MM\/dd\/yyyy SystemTime/@utc }}</span><span>English Date: {{ ? Date ""MMMM dd, yyyy"" SystemTime/@utc }}</span><span>Long Date: {{ Date D SystemTime/@utc }}</span><span>Short Time: {{ date t SystemTime/@utc }}</span></div></template>";
            Support = "";
            Test = @"<div><span>Original: 8/26/2010 2:30:51 PM</span><br /><span>Standard Date: 08/26/2010</span><span>English Date: August 26, 2010</span><span>Long Date: Thursday, August 26, 2010</span><span>Short Time: 2:30 PM</span></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Format Number";
            Desc = "[ Verify that formatting of numberic values works ]";
            Source = @"<node><data n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345""/></node>";
            Template = @"<template><div><span>Original: {{ = data/@n1 }} Zero-padded: {{ format Number 000000 data/@n1 }}</span><span>Comma-Separated: {{ ? Number ""#,#"" data/@n2 }}</span><span>Negative currency: {{ Number ""$#,#.##"" data/@n3  }}</span><span>Telephone Ext: {{ Number ""#(###)###-#### x.####"" data/@n4  }}</span><span>Pos: {{ number ""##.#;(##.#);**Zero**"" data/@n1 }} Neg: {{ number ""##.#;(##.#);**Zero**"" data/@n3 }} Zero: {{ number ""##.#;(##.#);**Zero**"" data/@n0 }}</span><span>Dec: {{ = data/@n1 }} Hex: {{ ? number X data/@n1 }}</span></div></template>";
            Support = "";
            Test = @"<div><span>Original: 123 Zero-padded: 000123</span><span>Comma-Separated: 1,234,567,890</span><span>Negative currency: -$765.43</span><span>Telephone Ext: 1(234)567-8901 x.2345</span><span>Pos: 123 Neg: (765.4) Zero: **Zero**</span><span>Dec: 123 Hex: 7B</span></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Format String";
            Desc = "[ Verify that formatting of concatenated string values works ]";
            Source = @"<node><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345""/></node>";
            Template = @"<template><span>{{ format string ""Name: {0}, {1} ({2})"" data/@last data/@first data/@user }}</span></template>";
            Support = "";
            Test = @"<span>Name: Fine, Howard (HFINE)</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Copy (*) Nodes";
            Desc = "[ Verify that copying of source nodes works ]";
            Source = @"<node><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345"" /></node>";
            Template = @"<template><xml>{{ copy data }}</xml></template>";
            Support = "";
            Test = @"<xml><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345"" /></xml>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Copy (*) Nodes";
            Desc = "[ Verify that copying with use of shortcut command works ]";
            Source = @"<node><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345"" /></node>";
            Template = @"<template><xml>{{ * data }}</xml></template>";
            Support = "";
            Test = @"<xml><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345"" /></xml>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "CopyEncoded (*=) Nodes";
            Desc = "[ Verify that copying raw xml with use of shortcut command works ]";
            Source = @"<node><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345"" /></node>";
            Template = @"<template><xml>{{ *= data }}</xml></template>";
            Support = "";
            Test = @"<xml>&lt;data user=&quot;HFINE&quot; first=&quot;Howard&quot; last=&quot;Fine&quot; d1=&quot;8/26/2010 2:30:51 PM&quot; n0=&quot;0&quot; n1=&quot;123&quot; n2=&quot;1234567890&quot; n3=&quot;-765.4321&quot; n4=&quot;12345678901.2345&quot; /&gt;</xml>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Replace (~) Value";
            Desc = "[ Verify that regular expression string replacement command works ]";
            Source = @"<node><data d1=""08/26/2010""  /></node>";
            Template = @"<template><span>Original: {{ = data/@d1 }}  Updated: {{ ~ ""\b(?&lt;month&gt;\d{1,2})/(?&lt;day&gt;\d{1,2})/(?&lt;year&gt;\d{2,4})\b"" ""${day}-${month}-${year}"" data/@d1 }}</span></template>";
            Support = "";
            Test = @"<span>Original: 08/26/2010  Updated: 26-08-2010</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Version";
            Desc = "[ Get version information ]";
            Source = @"<node/>";
            Template = @"<template><span>{{ ver }}</span></template>";
            Support = "";
            Test = @"<span>X2TL v1.1.0 rev.0</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Variables";
            Desc = "[ Test variable functions ]";
            Source = @"<node><data attBase=""stuff"" att1=""some"" att2=""more"" att3=""other"" att4=""weird""/></node>";
            Template = @"<template>{{ var base data/@attBase }}{{ := adjective data/@att1 }}{{ % expand }}{{ := adjective data/@att2 }}{{ % expand }}{{ := adjective data/@att3 }}{{ % expand }}{{ := adjective data/@att4 }}{{ % expand }}</template>";
            Support = @"<templates><template name=""expand""><span>{{ string ""{0} {1}"" $adjective $base }}</span></template></templates>";
            Test = @"<span>some stuff</span><span>more stuff</span><span>other stuff</span><span>weird stuff</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Multipart Variables";
            Desc = "[ Test multipart variables functions ]";
            Source = @"<node><data att1=""some"" att2=""more"" att3=""other"" att4=""weird"">stuff</data></node>";
            Template = @"<template>Multipart: {{ mvar totalstuff }}{{ each data/@* }}{{ = . }} {{ /each }}{{ = data }}{{ /mvar }}<span>{{ = $totalstuff }}</span></template>";
            Support = "";
            Test = @"Multipart: <span>some more other weird stuff</span>";


            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Empty Results Test";
            Desc = "[ Verify that requesting non-existing results works properly ]";
            Source = @"<node/>";
            Template = @"<template><span>Empty Test: {{ = data/@q }}{{ == data/@q }}</span></template>";
            Support = "";
            Test = @"<span>Empty Test: </span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Empty Format Test";
            Desc = "[ Verify that requesting non-existing node format works properly ]";
            Source = @"<node/>";
            Template = @"<template><span>Empty Test: {{ ? number ""###"" data/@q }}{{ ? date ""MMM/dd/yyyy"" data/@q }}{{ ? string ""{0}"" data/@q }}</span></template>";
            Support = "";
            Test = @"<span>Empty Test: </span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Encoded Template XML Test";
            Desc = "[ Verify that templates containing encoded XML work properly ]";
            Source = @"<node><data><q name=""match'd"">Stuff</q></data></node>";
            Template = @"<template><span>{{= (data/*[@name=concat('match',""'"",'d')])}}</span></template>";
            Support = "";
            Test = @"<span>Stuff</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Test of XPath Navigator";
            Desc = "[ Verify that templates can use true xpath navigation properly ]";
            Source = @"<node><data><q name=""match'd"">Stuff</q></data></node>";
            Template = @"<template>{{ IF data/z }}{{ := myVar data/z }}{{ ELSE }}{{ := myVar ""'My Constant Value'"" }}<span>{{ = $myVar }}</span></template>";
            Support = "";
            Test = @"<span>My Constant Value</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Test of XPath Navigator (count)";
            Desc = "[ Verify that templates can use xpath functions properly ]";
            Source = @"<node><data><q>Stuff</q><q>Other stuff</q><q>More stuff</q></data></node>";
            Template = @"<template><span>Q Count: {{ = ""count(data/q)"" }}</span></template>";
            Support = "";
            Test = @"<span>Q Count: 3</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Test of Variables in XPaths";
            Desc = "[ Verify that templates can use variables within xpath functions properly ]";
            Source = @"<node><data><q id=""1"">Stuff</q><q id=""2"">Other stuff</q><q id=""3"">More stuff</q></data></node>";
            Template = @"<template><span>You selected: {{ := selectedId ""'2'"" }}{{ = ""data/q[@id=$selectedId]"" }}</span></template>";
            Support = "";
            Test = @"<span>You selected: Other stuff</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Test of Variables in IF conditions";
            Desc = "[ Verify that templates can use variables within IF test conditions properly ]";
            Source = @"<node><data value=""1""/><data value=""0""/></node>";
            Template = @"<template>{{ EACH data }}{{ := raw @value }}<span>The raw value ({{ = $raw }}) is translated to ({{ IF ($raw='1') }}true{{ ELSE }}false{{ ENDIF }}).</span>{{/EACH}}</template>";
            Support = "";
            Test = @"<span>The raw value (1) is translated to (true).</span><span>The raw value (0) is translated to (false).</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();




            Name = "Test of Variables in IF conditions for NON-Existent Variable";
            Desc = "[ Verify that templates can use variables within IF test against variables that don't exist ]";
            Source = @"<node><data value=""1""/><data value=""0""/></node>";
            Template = @"<template>{{ EACH data }}{{ := raw @value }}<span>The raw value ({{ = $raw }}) is translated to ({{ IF ($QQQraw='1') }}true{{ ELSE }}false{{ ENDIF }}).</span>{{/EACH}}</template>";
            Support = "";
            Test = @"<span>The raw value (1) is translated to (false).</span><span>The raw value (0) is translated to (false).</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Test of Variables in IF conditions of Variable existence";
            Desc = "[ Verify that templates can use variables within IF with simple Variable existence check ]";
            Source = @"<node><data value=""1""/><data/></node>";
            Template = @"<template>{{ EACH data }}{{ := raw @value }}<span>The raw value ({{ = $raw }}) is translated to ({{ IF (string-length($raw)!=0) }}true{{ ELSE }}false{{ ENDIF }}).</span>{{/EACH}}</template>";
            Support = "";
            Test = @"<span>The raw value (1) is translated to (true).</span><span>The raw value () is translated to (false).</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();




            Name = "Test of Variables in EACH conditions of Variable existence";
            Desc = "[ Verify that templates can use variables within EACH with simple Variable existence check ]";
            Source = @"<form><input type=""text"" name=""firstname""/><input type=""text"" name=""lastname""/><input type=""text"" name=""middlename""/><input type=""button"" name=""submit"" value=""Ok""/><input type=""button"" name=""reset"" value=""Clear""/></form>";
            Template = @"<template>{{:= InputType 'button'}}<span>The available actions are: {{EACH ""/form/input[@type=$InputType]""}}[{{ = @value }}] {{/EACH}}</span></template>";
            Support = "";
            Test = @"<span>The available actions are: [Ok] [Clear] </span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Test of Nodeset Variables as driver for EACH command";
            Desc = "[ Verify that templates can use variables as nodesets to drive EACH commands ]";
            Source = @"<root><thing type=""some""/><thing type=""any""/><thing type=""no""/><thing type=""every""/></root>";
            Template = @"<template><span>{{:= Things /root/thing}}{{EACH ($Things[not(@type='no')]/@type)}}{{= .}}thing... {{/EACH}}</span></template>";
            Support = "";
            Test = @"<span>something... anything... everything... </span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Test of Nodeset Variables as driver for EACH command in ParamApply";
            Desc = "[ Verify that templates can use variables as EACH driver in ParamApply ]";
            Source = @"<PageData><PtPage.EncountersGet><row ignore=""true""/><row EncounterNo=""12345"" Description=""IPD @ DemoHealth""/><row EncounterNo=""67890"" Description=""ED @ SpeedyCare""/></PtPage.EncountersGet></PageData>";
            Template = @"<template>{{PARAMAPPLY EncounterPicker}}{{:= Encounters /PageData/PtPage.EncountersGet/row}}{{/PARAMAPPLY}}</template>";
            Support = @"<templates><template name=""EncounterPicker""><select>{{EACH ($Encounters[@EncounterNo])}}<option value=""{{= @EncounterNo}}"">{{= @Description}}</option>{{/EACH}}</select></template></templates>";
            Test = @"<select><option value=""12345"">IPD @ DemoHealth</option><option value=""67890"">ED @ SpeedyCare</option></select>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();












//            Name = "Complex Example";
//            Desc = "[ Verify that a complex working example works properly ]";
//            Source = @"<xpath><to name=""options"" selectedValue=""2""><option name=""A"" value=""1""/><option name=""B"" value=""2""/><option name=""C"" value=""3""/></to></xpath>";
//            //Template = @"<template><form>{{VAR selectedValue /xpath/to[@name='options']/@selectedValue}} {{EACH /xpath/to[@name='options']/option}} {{MVAR checked}}{{IF @value=$selectedValue}}checked=""checked""{{/IF}}{{/MVAR}}{{ APPLY  labeledInput }}{{/EACH}}</form></template>";
//
//            Template = @"<template><form>{{VAR selectedValue /xpath/to[@name='options']/@selectedValue}}{{EACH /xpath/to/option}}{{MVAR checked}}{{IF @value='2'}}checked=""checked""{{/IF}}{{/MVAR}}{{ APPLY labeledInput }}{{/EACH}}</form></template>";
//            Support = @"<templates><template name=""labeledInput"" delim-start=""__"" delim-end=""=''"" param-separators=""-"" variable-prefix="".""><label><input type=""radio"" name=""options"" value=""__RESULT-@value=''"" __RESULT-.checked='' /> __RESULT-@name=''</label></template></templates>";
//            Test = @"<form><label><input type=""radio"" name=""options"" value=""1""  /> A</label><label><input type=""radio"" name=""options"" value=""2"" checked=""checked"" /> B</label><label><input type=""radio"" name=""options"" value=""3""  /> C</label></form>";
//
//            tester.Init(Name, Desc, Source, Template, Support, Test);
//            tester.RunTest();



            Name = "Empty Copy Test";
            Desc = "[ Verify that requesting non-existing node copy works properly ]";
            Source = @"<node/>";
            Template = @"<template><xml emptyTest=""true"">{{ * data }}</xml></template>";
            Support = "";
            Test = @"<xml emptyTest=""true""></xml>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Apply With Parameters (%%)";
            Desc = "[ Test Apply With Parameters (%%) functions ]";
            Source = @"<Root><input click=""this.focus();"" desc=""text field""/><emptyEl/><button click=""alert(this.desc);"" desc=""stuff""/></Root>";
			Template = @"<template>{{ := onclick ""'return void();'"" }}{{ := description ""'Howdy!'"" }}{{ apply BuildElement }}{{ EACH * }}{{ paramapply BuildElement . }}{{ := element ""name(.)"" }}{{ := onclick @click }}{{ := description @desc }}{{ /paramapply }}{{ /EACH }}{{ apply BuildElement }}</template>";
            Support = @"<templates><t name=""BuildElement""><span onclick=""{{ = $onclick }}"" sourceEl=""{{ = $element }}"">{{ = $description }}</span></t></templates>";
            Test = @"<span onclick=""return void();"" sourceEl="""">Howdy!</span><span onclick=""this.focus();"" sourceEl=""input"">text field</span><span onclick="""" sourceEl=""emptyEl""></span><span onclick=""alert(this.desc);"" sourceEl=""button"">stuff</span><span onclick=""return void();"" sourceEl="""">Howdy!</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Apply With Parameters - Shortcut (%%)";
            Desc = "[ Test Shortcut version of Apply With Parameters (%%) ]";
            Source = @"<Root><input click=""this.focus();"" desc=""text field""/><emptyEl/><button click=""alert(this.desc);"" desc=""stuff""/></Root>";
            Template = @"<template>{{ := onclick ""'return void();'"" }}{{ := description ""'Howdy!'"" }}{{ apply BuildElement }}{{ EACH * }}{{ %% BuildElement . }}{{ := element ""name(.)"" }}{{ := onclick @click }}{{ := description @desc }}{{ /%% }}{{ /EACH }}{{ apply BuildElement }}</template>";
            Support = @"<templates><t name=""BuildElement""><span onclick=""{{ = $onclick }}"" sourceEl=""{{ = $element }}"">{{ = $description }}</span></t></templates>";
            Test = @"<span onclick=""return void();"" sourceEl="""">Howdy!</span><span onclick=""this.focus();"" sourceEl=""input"">text field</span><span onclick="""" sourceEl=""emptyEl""></span><span onclick=""alert(this.desc);"" sourceEl=""button"">stuff</span><span onclick=""return void();"" sourceEl="""">Howdy!</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            // TEST EXAMPLES FROM THE WIKI PAGE

            Name = "Wiki: #understandingexample";
            Desc = "[ Wiki: #understandingexample ]";
            Source = @"<Patient><row lastName=""Fine"" firstName=""Howard"" userName=""HFINE""/></Patient>";
            Template = @"<template><span>{{ = row/@lastName}}, {{ = row/@firstName }} ({{ = row/@userName }})</span></template>";
            Support = "";
            Test = @"<span>Fine, Howard (HFINE)</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #transformnode";
            Desc = "[ Wiki: #transformnode ]";
            Source = @"<Common.ADM_LoginMessagesGet><row MessageNo=""1001"" Subject=""Sample bulletin board message"" isAdmin=""true"" /></Common.ADM_LoginMessagesGet>";
            Template = @"<template><div>{{ IF (count(row)!=0) }}<table><tbody>{{ APPLY BulletinBoardRow row }}</tbody></table>{{ ELSE }}No bulletin messages{{ /IF }}</div></template>";
            Support = @"<templates><template name=""BulletinBoardRow""><tr><td id=""Bulletin_{{ = @MessageNo }}"">{{ = @Subject }}</td>{{ IF @isAdmin = 'true' }}{{ APPLY BulletinBoardAdmin }}{{ /IF }}</tr></template><template name=""BulletinBoardAdmin""><td onclick=""BulletinBoard_Edit('Bulletin_{{ = @MessageNo }}')"">[edit]</td></template></templates>";
            Test = @"<div><table><tbody><tr><td id=""Bulletin_1001"">Sample bulletin board message</td><td onclick=""BulletinBoard_Edit('Bulletin_1001')"">[edit]</td></tr></tbody></table></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Wiki: #escapeddelimiters";
            Desc = "[ Wiki: #escapeddelimiters ]";
            Source = @"<dontCare/>";
            Template = @"<template><span>If you want to show mustaches in your template, you will need to double them up: {{{{</span></template>";
            Support = @"";
            Test = @"<span>If you want to show mustaches in your template, you will need to double them up: {{</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #manyescapeddelimiters";
            Desc = "[ Wiki: #manyescapeddelimiters ]";
            Source = @"<dontCare/>";
            Template = @"<template><span>{{{{{{{{{{{{{{{{ a lot of mustaches }}}}}}}}</span></template>";
            Support = @"";
            Test = @"<span>{{{{{{{{ a lot of mustaches }}}}}}}}</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #result";
            Desc = "[ Wiki: #result ]";
            Source = @"<User userNo=""1001"" lastName=""Fine"" firstName=""Howard"" userName=""hfine""><Check-In status=""CHECKED-IN"" availability=""Busy"" lastUpdate=""8/31/2010 16:18""/></User>";
            Template = @"<template><div id=""User_{{ = @userNo }}""><span>{{ result @lastName }}, {{ result @firstName }} ({{ = @userName }})</span><br /><span>User is {{ = Check-In/@status }} - ( {{ = /User/Check-In/@availability }} ) as of {{ Result ""*/@lastUpdate"" }}</span></div></template>";
            Support = @"";
            Test = @"<div id=""User_1001""><span>Fine, Howard (hfine)</span><br /><span>User is CHECKED-IN - ( Busy ) as of 8/31/2010 16:18</span></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #resultraw";
            Desc = "[ Wiki: #resultraw ]";
            Source = @"<User userNo=""1001"" lastName=""Fine"" firstName=""Howard"" userName=""hfine""><Pref favURL=""http://www.google.com/#hl=en&amp;source=hp&amp;q=RelWare&amp;aq=f&amp;aqi=g1g-s1""/></User>";
            Template = @"<template><div><span>User's favorite URL is: {{ Result Pref/@favURL }}</span><br /><span>This is Invalid XML:    {{ RawResult Pref/@favURL }}</span><hr /><span>User's favorite URL is: {{ = Pref/@favURL }}</span><br /><span>This is Invalid XML:    {{ == Pref/@favURL }}</span></div></template>";
            Support = @"";
            Test = @"<div><span>User's favorite URL is: http://www.google.com/#hl=en&amp;source=hp&amp;q=RelWare&amp;aq=f&amp;aqi=g1g-s1</span><br /><span>This is Invalid XML:    http://www.google.com/#hl=en&source=hp&q=RelWare&aq=f&aqi=g1g-s1</span><hr /><span>User's favorite URL is: http://www.google.com/#hl=en&amp;source=hp&amp;q=RelWare&amp;aq=f&amp;aqi=g1g-s1</span><br /><span>This is Invalid XML:    http://www.google.com/#hl=en&source=hp&q=RelWare&aq=f&aqi=g1g-s1</span></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            Name = "Wiki: #format";
            Desc = "[ Wiki: #format ]";
            Source = @"<User userNo=""1001"" lastName=""Fine"" firstName=""Howard"" userName=""hfine""><Check-In status=""CHECKED-IN"" availability=""Busy"" lastUpdate=""8/31/2010 16:18"" phoneNum=""12345678901.2345""/></User>";
            Template = @"<template><span>{{ Each Check-In }}User is {{ = @status }} as of {{ Format Date MM\/dd\/yyyy @lastUpdate }}<br />and can be reached at {{ ? Number ""#(###)###-#### x.####"" @phoneNum }}{{ /Each }}</span></template>";
            Support = @"";
            Test = @"<span>User is CHECKED-IN as of 08/31/2010<br />and can be reached at 1(234)567-8901 x.2345</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #date";
            Desc = "[ Wiki: #date ]";
            Source = @"<SystemTime utc=""8/26/2010 5:44:47 PM"" local24=""08/26/2010 13:44:47"" />";
            Template = @"<template><span>UTC: {{ Format Date MM\/dd\/yyyy @utc }} - Local: {{ ? Date MM\/dd\/yyyy @local24 }} - English: {{ date ""MMMM d, yyyy"" @utc }} - Time: {{ date t @local24 }}</span></template>";
            Support = @"";
            Test = @"<span>UTC: 08/26/2010 - Local: 08/26/2010 - English: August 26, 2010 - Time: 1:44 PM</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #number";
            Desc = "[ Wiki: #number ]";
            Source = @"<node><data n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345""/></node>";
            Template = @"<template><div><span>Original: {{ = data/@n1 }} Zero-padded: {{ format Number 000000 data/@n1 }}</span><span>Comma-Separated: {{ ? Number ""#,#"" data/@n2 }}</span><span>Negative currency: {{ Number ""$#,#.##"" data/@n3  }}</span><span>Telephone Ext: {{ Number ""#(###)###-#### x.####"" data/@n4  }}</span><span>Pos: {{ number ""##.#;(##.#);**Zero**"" data/@n1 }} Neg: {{ number ""##.#;(##.#);**Zero**"" data/@n3 }} Zero: {{ number ""##.#;(##.#);**Zero**"" data/@n0 }}</span><span>Dec: {{ = data/@n1 }} Hex: {{ ? number X data/@n1 }}</span></div></template>";
            Support = @"";
            Test = @"<div><span>Original: 123 Zero-padded: 000123</span><span>Comma-Separated: 1,234,567,890</span><span>Negative currency: -$765.43</span><span>Telephone Ext: 1(234)567-8901 x.2345</span><span>Pos: 123 Neg: (765.4) Zero: **Zero**</span><span>Dec: 123 Hex: 7B</span></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #string";
            Desc = "[ Wiki: #string ]";
            Source = @"<node><data user=""HFINE"" first=""Howard"" last=""Fine"" d1=""8/26/2010 2:30:51 PM"" n0=""0"" n1=""123"" n2=""1234567890"" n3=""-765.4321"" n4=""12345678901.2345""/></node>";
            Template = @"<template><span>{{ format string ""Name: {0}, {1} ({2})"" data/@last data/@first data/@user }}</span></template>";
            Support = @"";
            Test = @"<span>Name: Fine, Howard (HFINE)</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #copy";
            Desc = "[ Wiki: #copy ]";
            Source = @"<User userNo=""1001"" lastName=""Fine"" firstName=""Howard"" userName=""hfine""><Check-In status=""CHECKED-IN"" availability=""Busy"" lastUpdate=""8/31/2010 16:18"" /></User>";
            Template = @"<template><xml id=""User_{{ = @userNo }}"">{{ copy Check-In }}</xml></template>";
            Support = @"";
            Test = @"<xml id=""User_1001""><Check-In status=""CHECKED-IN"" availability=""Busy"" lastUpdate=""8/31/2010 16:18"" /></xml>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #copyencoded";
            Desc = "[ Wiki: #copyencoded ]";
            Source = @"<User userNo=""1001"" lastName=""Fine"" firstName=""Howard"" userName=""hfine""><Check-In status=""CHECKED-IN"" availability=""Busy"" lastUpdate=""8/31/2010 16:18"" /></User>";
            Template = @"<template><xml id=""User_{{ = @userNo }}"">{{ copyencoded Check-In }}</xml></template>";
            Support = @"";
            Test = @"<xml id=""User_1001"">&lt;Check-In status=&quot;CHECKED-IN&quot; availability=&quot;Busy&quot; lastUpdate=&quot;8/31/2010 16:18&quot; /&gt;</xml>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            
            Name = "Wiki: #if";
            Desc = "[ Wiki: #if ]";
            Source = @"<User userNo=""1001"" lastName=""Fine"" firstName=""Howard"" userName=""hfine""><Check-In status=""CHECKED-IN"" availability=""Busy"" lastUpdate=""8/31/2010 16:18""><message>In meetings until lunch</message></Check-In></User>";
            Template = @"<template><div id=""User_{{ = @userNo }}""><span>{{ result @lastName }}, {{ result @firstName }} ({{ = @userName }})</span><br />{{ IF /User/Check-In }}<span>User is {{ = Check-In/@status }} {{ If Check-In/@facility }}at the {{ = Check-In/@facility }}{{ Else }}but whereabouts are unknown{{ /If }}</span>{{ if (count(*/message)!=0) }}<div>{{ = */message }}</div>{{ /if }}{{ ENDIF }}</div></template>";
            Support = @"";
            Test = @"<div id=""User_1001""><span>Fine, Howard (hfine)</span><br /><span>User is CHECKED-IN but whereabouts are unknown</span><div>In meetings until lunch</div></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();


            Name = "Wiki: #each";
            Desc = "[ Wiki: #each ]";
            Source = @"<Patient lastName=""Brown"" firstName=""Charlie""><Allergies><row AllergyNo=""1001"" description=""PEANUTS"" reaction=""Hives""/><row AllergyNo=""1002"" description=""LUCYVANPELT"" reaction=""Irritation""/></Allergies></Patient>";
            Template = @"<template><div>{{ = @lastName}}, {{ = @firstName }}<br /><table><tbody>{{ EACH Allergies/row }}<tr><td>{{ = @description }}</td><td>{{ = @reaction }}</td></tr>{{ ENDEACH }}</tbody></table></div></template>";
            Support = @"";
            Test = @"<div>Brown, Charlie<br /><table><tbody><tr><td>PEANUTS</td><td>Hives</td></tr><tr><td>LUCYVANPELT</td><td>Irritation</td></tr></tbody></table></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #each (alt)";
            Desc = "[ Wiki: #each (alt) ]";
            Source = @"<Patient lastName=""Brown"" firstName=""Charlie""><Allergies><row AllergyNo=""1001"" description=""PEANUTS"" reaction=""Hives""/><row AllergyNo=""1002"" description=""LUCYVANPELT"" reaction=""Irritation""/></Allergies></Patient>";
            Template = @"<template><div>{{ = @lastName}}, {{ = @firstName }}<br/><table><tbody>{{ EACH Allergies/row }}<tr>{{ Each (@*[name()!='AllergyNo']) }}<td>{{ = . }}</td>{{ /Each }}</tr>{{ /EACH }}</tbody></table></div></template>";
            Support = @"";
            Test = @"<div>Brown, Charlie<br /><table><tbody><tr><td>PEANUTS</td><td>Hives</td></tr><tr><td>LUCYVANPELT</td><td>Irritation</td></tr></tbody></table></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #apply";
            Desc = "[ Wiki: #apply ]";
            Source = @"<Patient lastName=""Brown"" firstName=""Charlie""><Diagnoses><row DiagnosisNo=""1001"" description=""Depression""/></Diagnoses><Allergies><row AllergyNo=""1001"" description=""PEANUTS"" reaction=""Hives""/><row AllergyNo=""1002"" description=""LUCYVANPELT"" reaction=""Irritation""/></Allergies><Medications/></Patient>";
            Template = @"<template><div><h1>{{ % FullName }}</h1>{{ Apply PatientDiagnoses Diagnoses }}{{ Apply PatientAllergies Allergies }}{{ Apply PatientMedications Medications }}</div></template>";
            Support = @"<templates><t name=""FullName"">{{ = @lastName}}, {{ = @firstName }}</t><t name=""PatientDiagnoses"">{{ if (count(row)!=0) }}<table><tbody>{{ % PatientDiagnosisRow row }}</tbody></table>{{ else }}<span>No active diagnoses</span>{{ /if }}</t><t name=""PatientDiagnosisRow""><tr><td>{{ = @description }}</td></tr></t><t name=""PatientAllergies"">{{ if (count(row)!=0) }}<table><tbody>{{ % PatientAllergyRow row }}</tbody></table>{{ else }}<span>No documented allergies</span>{{ /if }}</t><t name=""PatientAllergyRow""><tr><td>{{ = @description }}</td><td>{{ = @reaction }}</td></tr></t><t name=""PatientMedications"">{{ if (count(row)!=0) }}<table><tbody>{{ % PatientMedicationRow row }}</tbody></table>{{ else }}<span>No active medications</span>{{ /if }}</t><t name=""PatientMedicationRow""><tr><td>{{ = @description }}</td><td>{{ = @strength }}</td></tr></t></templates>";
            Test = @"<div><h1>Brown, Charlie</h1><table><tbody><tr><td>Depression</td></tr></tbody></table><table><tbody><tr><td>PEANUTS</td><td>Hives</td></tr><tr><td>LUCYVANPELT</td><td>Irritation</td></tr></tbody></table><span>No active medications</span></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #var";
            Desc = "[ Wiki: #var ]";
            Source = @"<node><data d1=""08/26/2010"" /></node>";
            Template = @"<template>{{ := myVar data/@d1 }}<span>From Xpath: {{ = data/@d1 }}  From Variable: {{ = $myVar }}</span></template>";
            Support = @"";
            Test = @"<span>From Xpath: 08/26/2010  From Variable: 08/26/2010</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #var (across templates)";
            Desc = "[ Wiki: #var (across templates) ]";
            Source = @"<node><data d1=""08/26/2010"" /></node>";
            Template = @"<template>{{ := InputDate data/@d1 }}<span>Original: {{ = $InputDate }}  EuroDate: {{ % EuroDate }}</span></template>";
            Support = @"<templates><template name=""EuroDate"">{{ ~ ""\b(?&lt;month&gt;\d{1,2})/(?&lt;day&gt;\d{1,2})/(?&lt;year&gt;\d{2,4})\b"" ""${day}-${month}-${year}"" $InputDate }}</template></templates>";
            Test = @"<span>Original: 08/26/2010  EuroDate: 26-08-2010</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #multivar";
            Desc = "[ Wiki: #multivar ]";
            Source = @"<node><data att1=""some"" att2=""more"" att3=""other"" att4=""weird"">stuff</data></node>";
            Template = @"<template>Multipart: {{ mvar totalstuff }}{{ each data/@* }}{{ = . }} {{ /each }}{{ = data }}{{ /mvar }}<span>{{ = $totalstuff }}</span></template>";
            Support = @"";
            Test = @"Multipart: <span>some more other weird stuff</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #replace";
            Desc = "[ Wiki: #replace ]";
            Source = @"<node><data d1=""08/26/2010"" /></node>";
            Template = @"<template><span>Original: {{ = data/@d1 }}  Updated: {{ ~ ""\b(?&lt;month&gt;\d{1,2})/(?&lt;day&gt;\d{1,2})/(?&lt;year&gt;\d{2,4})\b"" ""${day}-${month}-${year}"" data/@d1 }}</span></template>";
            Support = @"";
            Test = @"<span>Original: 08/26/2010  Updated: 26-08-2010</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();

            Name = "Wiki: #ver";
            Desc = "[ Wiki: #ver ]";
            Source = @"<dontCare />";
            Template = @"<template><span>{{ ver }}</span></template>";
            Support = @"";
            Test = @"<span>X2TL v1.1.0 rev.0</span>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();




            // Errors found during use
            Name = "MVAR Error";
            Desc = "[ Verify that the error found with MVARs contaminating pre-existing results is resolved ]";
            Source = @"<INPUT><context><User UserNo=""10006"" AssociateFamilyName=""Fisher"" /></context></INPUT>";
            Template = @"<template><div><input type=""text"" value=""{{= context/User/@UserNo}}"" /><input type=""text"" value=""{{= context/User/@AssociateFamilyName}}"" />{{mvar myVar}}{{= context/User/@UserNo}}_{{= context/User/@AssociateFamilyName}}{{/mvar}} But the mvar works: {{= $myVar}}</div></template>";
            Test = @"<div><input type=""text"" value=""10006"" /><input type=""text"" value=""Fisher"" /> But the mvar works: 10006_Fisher</div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();








            // Errors found during use
            Name = "Parse Error 1";
            Desc = "[ Verify that the error found with malformed IF statements is resolved ]";
            Source = @"<INPUT><context><User UserNo=""10006"" AssociateFamilyName=""Fisher"" /></context></INPUT>";
            Template = @"<template><div>{{ IF true }}<input type=""text"" value=""{{= context/User/@UserNo}}"" /><input type=""text"" value=""{{= context/User/@AssociateFamilyName}}"" />{{mvar myVar}}{{= context/User/@UserNo}}_{{= context/User/@AssociateFamilyName}}{{/mvar}} But the mvar works: {{= $myVar}}</div></template>";
            Test = @"<div><input type=""text"" value=""10006"" /><input type=""text"" value=""Fisher"" /> But the mvar works: 10006_Fisher</div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            // Errors found during use
            Name = "Parse Error 2";
            Desc = "[ Verify that the error found with malformed {{ is resolved ]";
            Source = @"<INPUT><context><User UserNo=""10006"" AssociateFamilyName=""Fisher"" /></context></INPUT>";
            Template = @"<template><div>{{ IF true <input type=""text"" value=""{{= context/User/@UserNo}}"" /><input type=""text"" value=""{{= context/User/@AssociateFamilyName}}"" />{{mvar myVar}}{{= context/User/@UserNo}}_{{= context/User/@AssociateFamilyName}}{{/mvar}} But the mvar works: {{= $myVar}}</div></template>";
            Test = @"<div>"" /><input type=""text"" value=""Fisher"" /> But the mvar works: 10006_Fisher</div>";

            tester.Init(Name, Desc, Source, Template, "", Test);
            tester.RunTest();


            // Errors found during use
            Name = "Parse Error 3";
            Desc = "[ Verify that the error found with ending malformed {{ is resolved ]";
            Source = @"<INPUT><context><User UserNo=""10006"" AssociateFamilyName=""Fisher"" /></context></INPUT>";
            Template = @"<template><div><input type=""text"" value=""{{= context/User/@UserNo}}"" />{{ </div></template>";
            Test = @"<div>"" /><input type=""text"" value=""1006"" />";


            tester.Init(Name, Desc, Source, Template, "", Test, true);
            tester.RunTest();


            Name = "ReadMe: Allergies";
            Desc = "[ Allergies sample from README.MD ]";
            Source = @"<Patient lastName=""Fine"" firstName=""Howard""><Allergies><Allergy id=""1001"" onset=""19671004"" name=""PEANUTS"" type=""FOOD"" reaction=""ANAPHYLAXIS"" severity=""CRITICAL"" /><Allergy id=""1002"" onset=""19850630"" name=""CODEINE"" type=""DRUG"" reaction=""HIVES"" /><Allergy id=""1003"" onset=""19890530"" name=""LATEX"" type=""ENVIRONMENT"" reaction=""RASH"" /></Allergies></Patient>";
            Template = @"<Template>{{ EACH /Patient }}<div><h1>{{ APPLY FullName }}</h1>{{ APPLY AllergyTable Allergies }}</div>{{ /EACH }}</Template>";
            Support = @"<SupportingTemplates><Template name=""FullName"">{{ = @lastName}}, {{ = @firstName }}</Template><Template name=""ToLower"">{{ = ""translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"" }}</Template><Template name=""FormatDate"">{{ = ""substring(., 5, 2)"" }}/{{ = ""substring(., 7, 2)"" }}/{{ = ""substring(., 1, 4)"" }}</Template><Template name=""AllergyTable""><table><thead><tr><th>Allergy (Type)</th><th>Onset Date</th><th>Reaction</th></tr></thead><tbody>{{ EACH Allergy }}<tr id=""Allergy_{{ = @id }}"" class=""allergy {{ IF @severity }}{{ % ToLower @severity }}{{ /IF }}""><td>{{ = @name }} <span class=""type"">({{ = @type }})</span></td><td>{{ % FormatDate @onset }}</td><td>{{ = @reaction }}</td></tr>{{ /EACH }}</tbody></table></Template></SupportingTemplates>";
            Test = @"<div><h1>Fine, Howard</h1><table><thead><tr><th>Allergy (Type)</th><th>Onset Date</th><th>Reaction</th></tr></thead><tbody><tr id=""Allergy_1001"" class=""allergy critical""><td>PEANUTS <span class=""type"">(FOOD)</span></td><td>10/04/1967</td><td>ANAPHYLAXIS</td></tr><tr id=""Allergy_1002"" class=""allergy ""><td>CODEINE <span class=""type"">(DRUG)</span></td><td>06/30/1985</td><td>HIVES</td></tr><tr id=""Allergy_1003"" class=""allergy ""><td>LATEX <span class=""type"">(ENVIRONMENT)</span></td><td>05/30/1989</td><td>RASH</td></tr></tbody></table></div>";

            tester.Init(Name, Desc, Source, Template, Support, Test);
            tester.RunTest();



            /* TEMPLATE 
                        Name = "Wiki: #";
                        Desc = "[ Wiki: # ]";
                        Source = @"";
                        Template = @"";
                        Support = @"";
                        Test = @"";

                        tester.Init(Name, Desc, Source, Template, Support, Test);
                        tester.RunTest();
            / TEMPLATE */




            return;
        
        }
    }
}
