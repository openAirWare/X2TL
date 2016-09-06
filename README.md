# X2TL

## Table of Contents
* [Synopsis](#synopsis)
* [Understanding Templating](#understanding-templating)
* [Code Example](#code-example)
* [Motivation](#motivation)
* [Installation](#installation)
* [Language Reference](#language-reference)
  * [X2TL Delimiters](#x2tl-delimiters)
    * [Escaped Delimiters](#escaped-delimiters)
    * [Many Escaped Delimiters](#many-escaped-delimiters)
  * [X2TL Commands](#x2tl-commands)
    * [Command Delimiters](#command-delimiters)
  * [X2TL Command Set](#x2tl-command-set)
    * [Value Replacement Commands](#value-replacement-commands)
      * [Result (=)](#result-)
      * [RawResult (==, raw)](#rawresult-result-)
      * [Format (?)](#format-)
        * [Format Date (Date)](#format-date-date-)
        * [Format Number (Number)](#format-number-number-)
        * [Format String (String)](#format-string-string-)
    * [Node Replacement Commands](#node-replacement-commands)
      * [Copy (*)](#copy-)
      * [CopyEncoded (*=)](#copyencoded-)
    * [Conditional Commands](#conditional-commands)
      * [If ... Else ...EndIf (/If)](#if-else-endif)
    * [Looping Commands](#looping-commands)
      * [Each ... EndEach (/Each)](#each-endeach)
    * Variable Commands
      * [Variable (var, :=)](#variable-)
      * [MultiVar (mvar, ::=) ... /MultiVar (/mvar, /::=, endmultivar, endmvar)](#multivar)
    * [Template Commands](#template-commands)
      * [Apply (%)](#apply-)
      * [ParamApply (%%)](#paramapply-)
    * [Manipulation Commands](#manipulation-commands)
      * [Replace (~)](#replace-)
    * [Miscellaneous Commands](#miscellaneous-commands)
      * [Version (ver)](#version-ver-)
* [Unit Tests](#unit-tests)
* [Contributors](#contributors)
* [License](#license)




## Synopsis

**X2TL** is an XML-safe "mustache" templating language, written in C# .Net.  It was written as a software development tool for [**RelWare**](http://relware.com), now being offered open-source by [**openAirWare**](http://openairware.com).  This library has been tested and used in several enterprise production applications, including medical record systems.  

This C# .Net Class/DLL provides a single public method **TransformNode** which accepts an XmlNode **pSourceNode** and an XmlNode **pTemplateNode**, and optionally an XmlNode **pSupportingTemplatesNode**. The inner xml of the Template is parsed as a string, based on starting ( **{{** ) and ending ( **}}** ) delimiters. Text found inside the delimiters are considered **Command Fragments**, and text outside of the delimiters are considered **Text Fragments**. The list of fragments are then sequentially processed, with **Text Fragments** simply being copied as is, and the **Command Fragments** being analyzed individually and procesed. Each command is applied against the context node, unless the command itself changes the context, such as the EACH command. Templates are also allowed to reference other templates, allowing for complex logic to be built from numerous single focused templates. The **pSupportingTemplatesNode** is expected to be a container element, with a flat list of child nodes having a **@name** attribute for each _named_ template. Templates can be nested as much as the stack space will allow.


## Understanding Templating
Templating is used to expand out data in a standardized formatting and styling manner. Generalized information on HTML Templating can be found at [Wikipedia - Web Template System](http://en.wikipedia.org/wiki/Web_template_system). The templates are made of either **Text Fragments** or **Command Fragments**.

**Command Fragments** are determined by finding blocks wrappered in a start and end delimiter. By default these are **{{** and **}}** respectively, referred to in the templating communnity as _Mustaches_. Within the _"mustaches"_ are commands that are evaluated and processed against the context node. Anything outside of the _"mustaches"_ is considered a **Text Fragment** and is copied as-is.

A simple example of an HTML Template is as follows:

```
 <span>{{ = @lastName}}, {{ = @firstName }} ({{ = user/@id }})</span> 
```

 Given an XML context like the following:

```
 <Person lastName="Fine" firstName="Howard">
     <user id="HFINE"/>
 </Person> 
 ```

will result in the following output:

```
 <span>Fine, Howard (HFINE)</span>
```




## Code Example

The **X2TL** has a single public method **TransformNode**, which is used to apply a given template against an XML node, optionally using other supporting templates, and return the resulting string. 

```
public static string TransformNode( XmlNode pSourceNode, XmlNode pTemplateNode, XmlNode pSupportingTemplatesNode )
```

It requires a **pSourceNode** and **pTemplateNode**, with **pSupportingTemplatesNode** being optional, each being an XmlNode datatype.

As this was used primarily in a medical record context, following is an example which loops through a person's allergies and displays them in a resulting table.

**pSourceNode**
For this example, the root node **Patient** of the below document will be used

```
<Patient lastName="Fine" firstName="Howard">
  <Allergies>
	  <Allergy id="1001" onset="19671004" name="PEANUTS" type="FOOD" reaction="ANAPHYLAXIS" severity="CRITICAL" />
	  <Allergy id="1002" onset="19850630" name="CODEINE" type="DRUG" reaction="HIVES" />
	  <Allergy id="1003" onset="19890530" name="LATEX" type="ENVIRONMENT" reaction="RASH" />
  </Allergies>
</Patient>
```

**pTemplateNode**
The root node **Template** of the below document will be used, which is small and itself references other templates.

```
<Template>
{{ EACH Patient }}
  <div>
    <h1>{{ APPLY FullName }}</h1>
    {{ APPLY AllergyTable Allergies }}
  </div>
{{ /EACH }}
</Template>
```

**pSupportingTemplateNode**
While optional, this can be very powerful, and provides more templates that can be used.  In this case, the root node **SupportingTemplates** will be used.

```
<SupportingTemplates>
	<Template name="FullName">{{ = @lastName}}, {{ = @firstName }}</Template>
	<Template name="ToLower">{{ = translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') }}</Template>
	<Template name="FormatDate">{{ = "substring(., 5, 2)" }}/{{ = "substring(., 7, 2)" }}/{{ = "substring(., 1, 4)" }}</Template>
	<Template name="AllergyTable">
		<table>
			<thead>
				<tr>
					<th>Allergy (Type)</th>
					<th>Onset Date</th>				
					<th>Reaction</th>
				</tr>
			</thead>
			<tbody>
				{{ EACH Allergy }}
					<tr id="Allergy_{{ = @id }}" class="allergy {{ IF @severity }}{{ % ToLower @severity }}{{ /IF }}">
						<td>{{ = @name }} <span class="type">({{ = @type }})</span></td>
						<td>{{ % FormatDate @onset }}</td>
						<td>{{ = @reaction }}</td>
					</tr>
				{{ /EACH }}
			</tbody>
		</table>
	</Template>
</SupportingTemplates>
```

When the **TransformNode()** method is invoked with the above parameters, it will produce back the following string.

```
<div>
	<h1>Fine, Howard</h1>
	<table>
		<thead>
			<tr>
				<th>Allergy (Type)</th>
				<th>Onset Date</th>				
				<th>Reaction</th>
			</tr>
		</thead>
		<tbody>
				<tr id="Allergy_1001" class="allergy critical">
					<td>PEANUTS <span class="type">(FOOD)</span></td>
					<td>10/04/1967</td>
					<td>ANAPHYLAXIS</td>
				</tr>
				<tr id="Allergy_1002" class="allergy">
					<td>CODEINE <span class="type">(DRUG)</span></td>
					<td>06/30/1985</td>
					<td>HIVES</td>
				</tr>
				<tr id="Allergy_1003" class="allergy">
					<td>LATEX <span class="type">(ENVIRONMENT)</span></td>
					<td>05/30/1989</td>
					<td>RASH</td>
				</tr>
		</tbody>
	</table>
</div>
```

Please note that while in these cases, the root node was always passed, that it purely for demonstration.  The source document of the nodes being passed, or their location within their parent documents does not matter.

**NOTICE:** This method returns a **string**, *NOT* an **XmlNode**, as one might originally suspect.  This is by design to allow for templates to create any type of output.


**NOTICE:** While the template passed in is an XmlNode, only the CONTENTS of the template are processed.  The template element itself will _NOT_ be carried foward.



## Motivation

Our software development team had a need of a templating library.  Its required feature list included the following:

* HTML and XML safe
* Support standard templating commands
* Fast and lightweight
* .Net compliant

I did some quick research at the time and found nothing that met our needs.  So, I wrote the **X2TL**.

## Installation

You can obtain the project files from GitHub.  To implement, simply add the Class file to your project, and call the public method.


## Language Reference

### X2TL Delimiters

By default, the X2TL engine uses the **{{** and **}}** as start and end delimiter respectively. These are referred to as mustaches in the templating community. These can be overridden within the Class object by instantiating the object with a different delimiter set. 

#### Escaped Delimiters

There may come a time that you actually wish to include the delimiters within your template and *NOT* have them process as a command. This is referred to as escaping the delimiters. The X2TL handles this as many languages do, by allowing the author of the template to 'double-delimit'. This concept should be familiar to anyone that has needed to double-quote a double-quote in SQL, VB, or a host of other languages. In X2TL, you only need to double-up the start delimiter. The end delimiter must remain intact and will be handled properly during processing.

##### Escaped Delimiters Example
Template
```
<template>
<span>If you want to show mustaches in your template, you will need to double them up: {{{{</span> 
</template>
```

Result
```
<span>If you want to show mustaches in your template, you will need to double them up: {{</span> 
```


#### Many Escaped Delimiters 
##### Many Escaped Delimiters Example
Template
```
<template>
<span>{{{{{{{{{{{{{{{{ a lot of mustaches to start, but only ending with 4 }}}}</span>
</template>
```
Result
```
<span>{{{{{{{{ a lot of mustaches to start, but only ending with 4 }}}}</span>
```
**NOTE:** Only the leading delimiter needs to be doubled-up. The trailing delimiter must be left as is.


### X2TL Commands

The X2TL has a limited command set, however, its flexibility allow for countless variations. The X2TL currently supports the following commands, with the syntax for each command being described in detail below.
Please also note that some commands provide for one or more "shorthand" alternatives. These are shown in parenthesis following the command itself _(i.e. **Result (=)**)_. Shorthand versions of commands are treated identical to the engine, and it does not distinguish between either writing of the command. Additionally, X2TL command words are _NOT_ case-sensitive, _HOWEVER_ their XPath parameters are. With that, the following are multiple ways of writing the same command, each of which will provide identical output.

```
{{ RESULT node/@attrib }} 
{{ Result node/@attrib }}
{{ result node/@attrib }}
{{ = node/@attrib }}
```

#### Command Delimiters

A command itself may be broken down into parameters, the first of which is always the command name. Some commands do not require any parameters, and some can take any number of parameters. To properly determine the parameters within the command fragment, X2TL understands the following command delimiters: **double-quotes ( " )**, **parenthesis ( ( and ) )**, and **whitespace ( space, newline, tab )**. Any text contained within a quoted- string, or within an outer set of parenthesis, is considered its own parameter. Any other text is broken down via whitespace.

### X2TL Command Set

Following is the list of available commands.


#### Value Replacement Commands

##### Result (=)

The **Result** command, which has a shorthand equivalent of **=**, is the most commonly used command, as value- replacement is primarily the point of a templating language. It expects a single parameter being an XPath within the current node context to locate the value that you wish resulted. It should be noted that the resulting value of the XML node will be XML-escaped before it is added to the resulting string. [(see RawResult below for more information)](#raw-result)

**NOTE:** This is "value-replacement" only. For full **"node-replacement"**, please refer to the [Copy (*)](#copy) command. 

Result Example: 
Context Node
```
<User userNo="1001" lastName="Fine" firstName="Howard" userName="hfine">
  <Check-In status="CHECKED-IN" availability="Busy" lastUpdate="08/31/2010 16:18"/>
</User>
```
Template Sample
```
<template>
  <div id="User_{{ = @userNo }}">      
    <span>{{ result @lastName }}, {{ result @firstName }} ({{ = @userName }})</span>    
    <span>User is {{ = Check-In/@status }} - ( {{ = /User/Check-In/@availability }} ) as of {{ Result "*/@lastUpdate"}}</div>
</template>
```

Output
```
<div id="User_1001">
  <span>Fine, Howard (hfine)</span>
  <span>User is CHECKED-IN - ( Busy ) as of 08/31/2010 16:18</span>
</div>
```

##### RawResult (==, raw)

The **RawResult** command, which has two shorthand equivalents of **==** and **raw**, will be lesser used than its counterpart **Result**, as **RawResult** provides back its resulting data as-is in string form, without any XML escaping performed. To better illustrate the use of this command, it will be compared to the Result command in the example below.

RawResult Example: 

Context Node
```
<User userNo="1001" lastName="Fine" firstName="Howard" userName="hfine"> 
  <Pref favURL="http://www.google.com/#hl=en&source=hp&q=RelWare&aq=f&aqi=g1gUs1"/>
</User>
```

Template Sample
```
<template>
  <div>
    <span>User's favorite URL is: {{ Result Pref/@favURL }}</span>
    <span>This is Invalid XML:    {{ RawResult Pref/@favURL }}</span>
    <hr />
    <span>User's favorite URL is: {{ = Pref/@favURL }}</span>   
    <span>This is Invalid XML:    {{ == Pref/@favURL }}</span> 
  </div>
</template>
```

Output

```
<div>
  <span>User's favorite URL is: http://www.google.com/#hl=en&amp;source=hp&amp;q=RelWare&amp;aq=f&amaqi=g1gUs1</span> 
  <span>This is Invalid XML: http://www.google.com/#hl=en&source=hp&q=RelWare&aq=f&aqi=g1gUs1</span>
  <hr />
  <span>User's favorite URL is: http://www.google.com/#hl=en&amp;source=hp&amp;q=RelWare&amp;aq=f&amaqi=g1gUs1</span> 
  <span>This is Invalid XML: http://www.google.com/#hl=en&source=hp&q=RelWare&aq=f&aqi=g1gUs1</span>
</div>
```


##### Format (?)

The **Format** command, which has a shorthand equivalent of **?**, is similar to the **Result** command, except that it is used to specifically change the output format of a given value, or in the case of a **String** format, multiple variables. The Format command expects (3) parameters: **datatype** _(date, number, string)_, **format** _(see .Net Framework Format Strings)_, and **value** _(XPath reference to node value)_.

Unlike other commands, **Format** also has three 'shortcuts', being that you can _omit_ the **Format (or ?)** portion of the command, and simply use the commands: **Date**, **Number** or **String**, each of which are described in more detail below. Other than the obvious lack of initial parameter, they operate identically to the **Format** command itself.

Format Example:

Context Node:
```
<User userNo="1001" lastName="Fine" firstName="Howard" userName="hfine">
  <Check-In status="CHECKED-IN" availability="Busy" lastUpdate="08/31/2010 16:18" phoneNum="123456789012345"/>
</User>
```

Template Sample:
```
<template>
  <span>{{ Each Check-In }}User is {{ = @status }} as of {{ Format Date MM/dd/yy @lastUpdate }} and can be reached at {{ ? Number "#(###)###-####x.####" @phoneNum }}{{ /Each }}</span>
</template>
```

Output:
```
<span>User is CHECKED-IN as of 08/31/10 and can be reached at 1(234)567-8901 x.2345</span>
```



###### Format Date (--> Date)

The **Date** command is a shortcut to the fully qualified **Format Date** command. It expects to receive a valid date format string, and an XPath value that points to a date value. 

**NOTICE:** There is no error handling for this, nor "invalid date message". Please use with care.

**NOTE:** While the command name only specifies *Date*, it is truly a _DateTime_ format, and supports any .Net supported date/time string.

(see [Supported Date Format Strings](https://msdn.microsoft.com/en-us/library/az4se3k1.aspx) and [Custom Date Format Strings](https://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx))

Context Node:
```
<SystemTime local24="08/26/2010 13:44:47" /> 
```

Template Sample:
```
<template>
  <span>Date: {{ Format Date "MM/dd/yyyy" @local24 }} - Full: {{ Date F @local24 }}</span>
</template>
```

Output:
```
<span>Date: 08/26/2010 - Full: Thursday, August 26, 2010 1:44:47 PM</span>
```



###### Format Number (--> Number)

The **Number** command is a shortcut to the fully qualified **Format Number** command. It expects to receive a valid number format string, and an XPath value that points to a numeric value (either Int64 or Double). 

**NOTICE:** There is no error handling for this, nor "invalid number message". Please use with care.

(see [.Net Standard Number Format strings](http://msdn.microsoft.com/en-us/library/dwhawy9k.aspx) and [Custom Numerical Format Strings](http://msdn.microsoft.com/en-us/library/0c899ak8.aspx))

Context Node:
```
<node>
  <data n0="0" n1="123" n2="1234567890" n3="U765.4321" n4="12345678901.2345" />
</node>
```

Template Sample:
```
<template>
  <div>
    <span>Original: {{ = data/@n1 }} Zero-padded: {{ format Number 000000 data/@n1 }}</span>
    <span>Comma-Separated: {{ ? Number "#,#" data/@n2 }}</span>
    <span>Negative Currency: {{ Number "$#,#.##" data/@n3 }}</span>
    <span>Telephone Ext: {{ Number "#(###)###-#### x.####" data/@n4 }}</span>
    <span>Pos: {{ number "##.#;(##.#);**Zero**" data/@n1 }} Neg: {{ number "##.#;(##.#);**Zero**" data/@n3 }}  Zero: {{ number "##.#;(##.#);**Zero**" data/@n0 }}</span>
    <span>Dec: {{ = data/@n1}} Hex: {{ ? number X data/@n1 }}</span>
  </div>
</template>
```

Output:
```
<div>
  <span>Original: 123 Zero-padded: 000123</span>
  <span>Comma-Separated: 1,234,567,890</span>
  <span>Negative Currency: -$765.43</span>
  <span>Telephone Ext: 1(234)567-8901 x.2345</span>
  <span>Pos: 123 Neg: (765.4)  Zero: **Zero**</span>
  <span>Dec: 123 Hex: 78</span>
</div>
```



###### Format String (--> String)

The **String** command is a shortcut to the fully qualified **Format String** command. It expects to receive a format string, and an unlike the other Format commands, it accepts **_ANY_** number of XPath values, each pointing to their own string value nodes.
This command uses the **.Net String.Format()**  method for its internal resulting.

**NOTICE:** There is no error handling for this, nor "invalid format string" messages. Please use with care.

(see [.Net Composite String Formatting](https://msdn.microsoft.com/en-us/library/txafckwd.aspx#)

Context Node:
```
<User username="HFINE" first="Howard" last="Fine" />
```

Template Sample:
```
<template>
  <span>{{ format String "User: {0}, {1}, ({2})", @last, @first, @username }}</span>
</template>
```

Output:
```
<span>User: Fine, Howard (HFINE)</span>
```



#### Node Replacement Commands

##### Copy (*)

The **Copy** command, which has a shorthand equivalent of *****, is used to bring a copy of the actual XML of a selected node into the result.  Unlike the above Value Replacement commands, which work only on the requested node's inner text value, this actually brings forward full "outer-XML" of a requested node.  With that, no additional formatting options apply.

Context Node:
```
<User userNo="1001">
  <Name lastName="Fine" firstName="Howard" userName="hfine" />
  <Check-In status="CHECKED-IN" availability="Busy" lastUpdate="08/31/2010 16:18" />
</User>
```

Template Sample:
```
<template>
  <xml id="User_{{ = @userNo }}
    {{ * Check-In }}
    {{ COPY Name }}
  </xml>
</template>
```

Output:
```
<xml id="User_1001">
  <Check-In status="CHECKED-IN" availability="Busy" lastUpdate="08/31/2010 16:18" />
  <Name lastName="Fine" firstName="Howard" userName="hfine" />
</xml>
```



##### CopyEncoded (*=)

The **CopyEncoded** command, which has a shorthand equivalent of ***=**, is used to bring an XML-encoded copy of the actual XML of a selected node into the result.  Unlike the above **Copy** command, the inner XML node or nodeset is encoded to be able to be dealt with as a string (i.e. **<** becomes **&lt;**).

Context Node:
```
<User userNo="1001">
  <Name lastName="Fine" firstName="Howard" userName="hfine" />
  <Check-In status="CHECKED-IN" availability="Busy" lastUpdate="08/31/2010 16:18" />
</User>
```

Template Sample:
```
<template>
  <xml id="User_{{ = @userNo }}
    {{ *= Check-In }}
    {{ copyencoded Name }}
  </xml>
</template>
```

Output:
```
<xml id="User_1001">
  &lt;Check-In status=&quot;CHECKED-IN&quot; availability=&quot;Busy&quot; lastUpdate=&quot;08/31/2010 16:18&quot; /&gt;
  &lt;Name lastName=&quot;Fine&quot; firstName=&quot;Howard&quot; userName=&quot;hfine&quot; /&gt;
</xml>
```



#### Conditional Commands

##### If ... Else ... Endif (/If)

Any language worth its salt has some form of the If/Else/EndIf logic, and X2TL is no different.  The **If** command takes an XPath parameter that is used to perform a _"node-existance"_ test.  The test can consist of a simple relative XPath, a root-node based XPath, or a logic-based XPath, each of which will be shown below.
Like most languages, the *Else* command is optional, however, the **EndIf** _(or its shorthand equivalent **/If**)_ is required. 

Context Node:
```
<User userNo="1001">
  <Name lastName="Fine" firstName="Howard" userName="hfine" />
  <Check-In status="CHECKED-IN" availability="Busy" lastUpdate="08/31/2010 16:18">
    <Message>In meetings until lunch</Message>
  </Check-In>
</User>
```

Template Sample:
```
<template>
  <div id="User_{{ = @userNo }}
    <span>{{ = Name/@firstName }} {{ = Name/@lastName }}</span>
  {{ IF Check-In }}
    <span class="checkin">{{ = Check-In/@status }}</span>
    {{ if (count(Check-In/Message)!=0) }}
    <div>{{ = Check-In/Message }}</div>
    {{ /if }}
  {{ ELSE }}
    <span class="no-checkin">has not checked in</span>
  {{ EndIF }}
  </div>
</template>
```

Output:
```
<div id="User_{{ = @userNo }}
  <span>Howard Fine</span>
  <span class="checkin">CHECKED-IN</span>
  <div>In meetings until lunch</div>
</div>
```



#### Looping Commands

##### Each ... EndEach (/Each)

As with the simple conditional logic, one cannot get far in templating without providing looping logic.  More complex conditional looping commands such as **Do...Until** and **While...EndWhile** may come later, but as of yet, the simple **Each** command has handled all of our needs. 
**Each** acts almost identically to XSL's **for-each** command, taking an XPath as its parameter, which can either be a relative XPath against the context node, or a root-based XPath which will be applied against the context node's document root element. Each XmlNode matching the given XPath will then be "looped", with any **Text** or **Command Fragments** contained within the **Each ... EndEach** block _(or its shorthand equivalent of **/Each**)_ being repeated against the looped node as their context node. 

Context Node
```
<Patient lastName="Brown" firstName="Charlie">
  <Allergies>
    <row allergyNo="1001" description="PEANUTS" reaction="Hives"/>
    <row allergyNo="1002" description="LUCYVANPELT" reaction="Irritation"/>
  </Allergies>
</Patient>
```

Template Sample:
```
<template>
  <div>
    <span>{{ = @lastName}}, {{ = @firstName }}</span>
    <table>
      <thead>
        <tr>
          <th>Allergy</th>
          <th>Reaction</th>
        </tr>
      <tbody>
      {{ EACH Allergies/row }}
        <tr id="Allergy_{{ = @allergyNo }}">
          <td>{{ = @description }}</td>
          <td>{{ = @reaction }}</td>
        </tr>
      {{ /EACH ]}
      </tbody>
    </table>
  </div>
</template>
```

Output:
```
  <div>
    <span>Brown, Charlie</span>
    <table>
      <thead>
        <tr>
          <th>Allergy</th>
          <th>Reaction</th>
        </tr>
      <tbody>
        <tr id="Allergy_1001">
          <td>PEANUTS</td>
          <td>Hives</td>
        </tr>
        <tr id="Allergy_1002">
          <td>LUCYVANPELT</td>
          <td>Irritation</td>
        </tr>
      </tbody>
    </table>
  </div>
```



#### Variable Commands

##### Variable (var, :=)

The **Variable** command, which has the shorthand equivalents of **var** and **:=** is used to store a textual value as a "global" variable that can be used later. This command takes (2) parameters: Name and XPath. 
The first parameter sets the Name to be used, and the second indicates the value should be stored.  A named variable can then be referenced at any point thereafter where a single xpath expression is expected.
This is done by providing the variable name prefixed by the variable delimiter _(default being **$**)_.

Context Node:
```
<node>
  <data d1="08/26/2010"  />
</node>
```

Template Sample:
```
<template>
{{ := myVar data/@d1 }}<span>From Xpath: {{ = data/@d1 }}  From Variable: {{ = $myVar }}</span>
</template>
```

Output:
```
<span>From Xpath: 08/26/2010 From Variable: 08/26/2010</span>
```



##### MultiVar (mvar, ::=) ... EndMultiVar (endmvar, /MultiVar, /mvar, /::=)

The *MultiVar* command is used to start creating a variable out of multiple pieces, and is completed with an **EndMultiVar** command. Any data that would have been resulted that is contained within a multivar start and end command will be stored as the contents of the variable. The start command has two shorthand equivalents: **mvar**, **::=**, and several shorthand versions of the end command: **endmvar**, **/MultiVar**, **/mvar**, **/::=**. 
This command takes a single parameter of the Name, where the contents will be the value.  Once created, Multipart Variables act identically to standard variables.

Context Node:
```
<User username="HFINE" >
  <Check-In status="CHECKED-IN">
   <Message>Busy until after lunch</Message>
  </Check-In>
</User>
```

Template Sample:
```
<template>
{{ MVAR FullString }}User {{ = @username }} has {{ = Check-In/@status }} {{ IF Check-In/Message }}({{ = Check-In/Message }}){{ ENDIF }}{ ENDMVAR }}
<span>{{ = $FullString }}</span>
</template>
```

Output:
```
<span>User HFINE has CHECKED-IN (Busy until after lunch)/span>
```


#### Template Commands

##### Apply (%)

While templates as described so far, provide for powerful formatting options, the true potential is unlocked when allowing them to **Apply** _(with shorthand equivalent of **%**)_ other templates, either to the current context node, or against a different yet relative context.
**Apply** always expects the name of a supporting template to be provided. Optionally, you can also provide an XPath to apply the template against a different node context.  Otherwise, the template will be applied against the current node context.

Context Node:
```
<Patient lastName="Brown" firstName="Charlie">
  <Diagnoses>
    <row DiagnosisNo="1001" description="Depression"/>
  </Diagnoses>
  <Allergies>
    <row allergyNo="1001" description="PEANUTS" reaction="Hives"/>
    <row allergyNo="1002" description="LUCYVANPELT" reaction="Irritation"/>
  </Allergies>
  <Medications />
</Patient>
```

Template Sample:
```
<template>
  <div>
    <h1>{{ % FullName }}</h1>
    {{ Apply PatientDiagnoses Diagnoses }}
    {{ Apply PatientAllergies Allergies }}
    {{ Apply PatientMedications Medications }} 
  </div>
</template>
```

Supporting Templates Sample:
```
<templates>

  <t name="FullName">{{ = @lastName }}, {{ = @firstName }}</t>

  <t name="PatientDiagnoses">
    {{ if (count(row)!=0 }}
      <table>
        <tbody>
          {{ % PatientDiagnosisRow row }}
        </tbody>
      </table>
    {{ else }}
      <span>No active diagnoses</span>
    {{ endif }}
  </t>

  <t name="PatientDiagnosisRow">
    <tr>
      <td>{{ = @description }}</td>
    </tr>
  </t>

  <t name="PatientAllergies">
    {{ if (count(row)!=0 }}
      <table>
        <tbody>
          {{ % PatientAllergyRow row }}
        </tbody>
      </table>
    {{ else }}
      <span>No active allergies</span>
    {{ endif }}
  </t>

  <t name="PatientAllergyRow">
    <tr>
      <td>{{ = @description }}</td>
      <td>{{ = @reaction }}</td>
    </tr>
  </t>


  <t name="PatientMedications">
    {{ if (count(row)!=0 }}
      <table>
        <tbody>
          {{ % PatientMedicationRow row }}
        </tbody>
      </table>
    {{ else }}
      <span>No active medications</span>
    {{ endif }}
  </t>

  <t name="PatientMedicationRow">
    <tr>
      <td>{{ = @description }}</td>
    </tr>
  </t>

</templates>
```

Output:
```
<div>
  <h1>Brown, Charlie</h1>
  <table>
    <tbody>
      <tr>
        <td>Depression</td>
      </tr>
    </tbody>
  </table>
  <table>
    <tbody>
      <tr>
        <td>PEANUTS</td>
        <td>Hives</td>
      </tr>
      <tr>
        <td>LUCYVANPELT</td>
        <td>Irritation</td>
      </tr>
    </tbody>
  </table>
  <span>No active medications</span>
</div>
```


##### ParamApply (%%)

In some cases, you will find a desire to pass parameters to supporting templates, which can be accomplished with the **ParamApply** command _(with shorthand equivalent of **%%**)_.  Unlike the **Apply** command, the **ParamApply** command requires an end command, being **EndParamApply** _(or its shorthand options **/ParamApply** or **/%%**)_.  Within the begin and end wrapper commands, you can specify either **Variable (:=)** or **MultiVar (::=)** commands, which will be scoped to within this template application. These variable declarations will act exactly like standard variables, however they will fall out of scope immediately after the template call.

Context Node:
```
<Root>
  <input click="this.focus();" desc="text field"/>
  <emptyEl/>
  <button click="alert(this.desc);" desc="stuff"/>
</Root>
```

Template Sample:
```
<template>
<!-- Set up variables and call apply -->
{{ := onclick "'return void();'" }}
{{ := description "'Howdy!'" }}
{{ apply BuildElement }}

<!-- Loop each element and apply with scoped variables -->
{{ EACH * }}
  {{ paramapply BuildElement . }}
     {{ := element "name(.)" }}
     {{ := onclick @click }}
     {{ := description @desc }}
  {{ /paramapply }}
{{ /EACH }}

<!-- Looped variables fall out of scope, and the old variables apply -->
{{ apply BuildElement }}
</template>
```

Supporting Templates Sample
```
<templates>
  <t name="BuildElement">
    <span onclick="{{ = $onclick }}" sourceEl="{{ = $element }}">{{ = $description }}</span>
  </t>
</templates>
```

Output:
```
<!-- Set up variables and call apply -->
<span onclick="return void();" sourceEl="">Howdy!</span>

<!-- Loop each element and apply with scoped variables -->
<span onclick="this.focus();" sourceEl="input">text field</span>
<span onclick="" sourceEl="emptyEl"></span>
<span onclick="alert(this.desc);" sourceEl="button">stuff</span>

<!-- Looped variables fall out of scope, and the old variables apply -->
<span onclick="return void();" sourceEl="">Howdy!</span>
```



#### Manipulation Commands

##### Replace (~)

The **Replace** command, which has a shorthand of **~** is used to perform a regular-expression based string replacement on either an Xpath or variable value.  This command takes (3) parameters: **regExp**, **replaceText**, **input**.  
The first two parameters are as defined by Microsoft .Net framework relating to the [RegEx Replace](http://msdn.microsoft.com/en-us/library/system.text.regularexpressions.regex.replace(VS.71).aspx) method. 
(see also [.NET Framework Regular Expressions](https://msdn.microsoft.com/en-us/library/hs600312.aspx))

Context Node:
```
<node>
  <data d1="08/26/2010"  />
</node>
```

Template Sample:
```
<template>
  <span>Original: {{ = data/@d1 }}  Updated: {{ ~ "\b(?&lt;month&gt;\d{1,2})/(?&lt;day&gt;\d{1,2})/(?&lt;year&gt;\d{2,4})\b" "${day}-${month}-${year}" data/@d1 }}</span>
</template>
```

Output:
```
<span>Original: 08/26/2010  Updated: 26-08-2010</span>
```


#### Miscellaneous Commands

##### Version (ver)

The **Version** command _(with shorthand **ver**)_ is used to simply display the current version information of the **X2TL** engine.

Context Node:
```
<dontcare />
```

Template Sample:
```
<template>
  <span>{{ ver }}</span>
</template>
```

Output:
```
<span>X2TL v1.1.0 rev.0</span>
```




## Unit Tests

An NUnit Test Harness application is included which runs individually through a list of Unit Tests to validate functionality and processing logic.


## Contributors

The original author of this component is [Mark Brown](https://www.linkedin.com/in/openairwaremark), openAirWare / RelWare


## License

This code is made available by [openAirWare](http://openairware.com) under the [MIT License](https://opensource.org/licenses/MIT)

