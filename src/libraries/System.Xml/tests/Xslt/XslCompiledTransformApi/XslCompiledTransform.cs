using Xunit;
using Xunit.Abstractions;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using XmlCoreTest.Common;
using System.Collections.Generic;

namespace System.Xml.Tests
{
    public class ReflectionTestCaseBase : XsltApiTestCaseBase2
    {
        //protected static readonly MethodInfo methCompileToType = GetStaticMethod(typeof(XslCompiledTransform), "CompileToType");

        private ITestOutputHelper _output;
        public ReflectionTestCaseBase(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        public static MethodInfo GetInstanceMethod(Type type, string methName)
        {
            MethodInfo methInfo = type.GetTypeInfo().GetMethod(methName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(methInfo != null, "Instance method " + type.Name + "." + methName + " not found");
            return methInfo;
        }

        public static MethodInfo GetStaticMethod(Type type, string methName)
        {
            MethodInfo methInfo = type.GetTypeInfo().GetMethod(methName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(methInfo != null, "Static method " + type.Name + "." + methName + " not found");
            return methInfo;
        }

        protected static CompilerErrorCollection WCompileToType(
            XmlReader stylesheet,
            XsltSettings settings,
            XmlResolver stylesheetResolver,
            bool debug,
            TypeBuilder typeBuilder,
            string scriptAssemblyPath)
        {
            return XslCompiledTransform.CompileToType(stylesheet, settings, stylesheetResolver, debug, typeBuilder, scriptAssemblyPath);
            //return (CompilerErrorCollection)methCompileToType.Invoke(/*this:*/null,
            //  new object[] { stylesheet, settings, stylesheetResolver, debug, typeBuilder, scriptAssemblyPath });
        }

        protected String scriptTestPath = null;

        protected String ScriptTestPath
        {
            get
            {
                if (scriptTestPath == null) //not thread safe
                {
                    scriptTestPath = _standardTests;

                    if (!scriptTestPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        scriptTestPath += Path.DirectorySeparatorChar;

                    scriptTestPath += "Scripting\\";
                }

                return scriptTestPath;
            }
        }

        //protected TypeBuilder ATypeBuilder
        //{
        //    get
        //    {
        //        AppDomain cd = System.Threading.Thread.GetDomain();
        //        AssemblyName an = new AssemblyName();
        //        an.Name = "HelloClass";

        //        AssemblyBuilder ab = cd.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
        //        ModuleBuilder mb = ab.DefineDynamicModule("HelloModule", "HelloModule.dll", true);
        //        TypeBuilder tb = mb.DefineType("Hello", TypeAttributes.Class | TypeAttributes.Public);
        //        return tb;
        //    }
        //}

        protected MethodInfo AMethodInfo
        {
            get
            {
                String asmPath = Path.Combine(FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\bftBaseLine.dll");
                String type = "bftBaseLine";

                Assembly asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath);
                Type t = asm.GetType(type);

                return GetStaticMethod(t, "Execute");
            }
        }

        protected void WLoad(XslCompiledTransform instance, MethodInfo meth, Byte[] bytes, Type[] types)
        {
            instance.Load(meth, bytes, types);
            /*
            Type[] paramTypes = new Type[3] { typeof(MethodInfo), typeof(Byte[]), typeof(Type[]) };

            MethodInfo m = typeof(XslCompiledTransform).GetMethod("Load",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                paramTypes,
                null);

            m.Invoke(instance, new Object[] { meth, bytes, types });
            */
        }

        protected void TestWLoad(XslCompiledTransform xslt, String asmPath, String type)
        {
            Assembly asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath);
            Type t = asm.GetType(type);

            MethodInfo meth = GetStaticMethod(t, "Execute");
            Byte[] staticData = (Byte[])t.GetTypeInfo().GetField("staticData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(t);
            Type[] ebTypes = (Type[])t.GetTypeInfo().GetField("ebTypes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(t);

            WLoad(xslt, meth, staticData, ebTypes);
        }
    }

    //[TestCase(Name = "CompileToType tests", Desc = "This testcase tests XslCompiledTransform private CompileToType method via Reflection. Same method is also exercised by xsltc.exe")]
    public class CCompileToTypeTest : ReflectionTestCaseBase
    {
        private ITestOutputHelper _output;
        public CCompileToTypeTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //BinCompat TODO: Add this test back
        ////[Variation("CompileToType(XmlReader = null)", Pri = 1)]
        //[InlineData()]
        //[Theory]
        //public void Var1()
        //{
        //    try
        //    {
        //        WCompileToType((XmlReader)null, XsltSettings.Default, null, false, ATypeBuilder, String.Empty);
        //    }
        //    catch (Exception e)
        //    {
        //        _output.WriteLine(e.ToString());

        //        if (e is ArgumentNullException || e.InnerException is ArgumentNullException)
        //            return;

        //        _output.WriteLine("Did not throw ArgumentNullException");
        //    }
        //    Assert.True(false);
        //}

        //[Variation("CompileToType(TypeBuilder= null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var2()
        {
            try
            {
                WCompileToType(XmlReader.Create(FullFilePath("identity.xsl")), XsltSettings.Default, null, false, null, String.Empty);
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());

                if (e is ArgumentNullException || e.InnerException is ArgumentNullException)
                    return;

                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //BinCompat TODO: Add this test back
        ////[Variation("CompileToType(AsmPath= No Extension file name)", Pri = 1)]
        //[InlineData()]
        //[Theory]
        //public void Var3()
        //{
        //    AppDomain cd = System.Threading.Thread.GetDomain();
        //    AssemblyName an = new AssemblyName();
        //    an.Name = "HelloClass";

        //    AssemblyBuilder ab = cd.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
        //    ModuleBuilder mb = ab.DefineDynamicModule("HelloModule", "HelloModule.dll", true);
        //    TypeBuilder tb = mb.DefineType("Hello", TypeAttributes.Class | TypeAttributes.Public);

        //    try
        //    {
        //        String xslPath = ScriptTestPath + "Scripting28.xsl";
        //        CompilerErrorCollection errors;
        //        using (XmlReader reader = XmlReader.Create(xslPath))
        //        {
        //            errors = WCompileToType(reader, XsltSettings.TrustedXslt, null, false, tb, "ScName");
        //        }

        //        // Print errors and warnings
        //        bool hasError = false;
        //        foreach (CompilerError error in errors)
        //        {
        //            _output.WriteLine(error.ToString());
        //            hasError = true;
        //        }

        //        if (hasError) Assert.True(false);
        //    }
        //    catch (Exception e)
        //    {
        //        _output.WriteLine(e.ToString());
        //        Assert.True(false);
        //    }

        //    return;
        //}

        //BinCompat TODO: Add this test back
        ////[Variation("CompileToType(Valid case with scripts)", Pri = 1)]
        //[InlineData()]
        //[Theory]
        //public void Var4()
        //{
        //    AppDomain cd = System.Threading.Thread.GetDomain();
        //    AssemblyName an = new AssemblyName();
        //    an.Name = "HelloClass4";

        //    AssemblyBuilder ab = cd.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
        //    ModuleBuilder mb = ab.DefineDynamicModule("HelloModule4", "HelloModule4.dll", true);
        //    TypeBuilder tb = mb.DefineType("Hello4", TypeAttributes.Class | TypeAttributes.Public);

        //    try
        //    {
        //        String xslPath = ScriptTestPath + "Scripting28.xsl";
        //        CompilerErrorCollection errors;
        //        using (XmlReader reader = XmlReader.Create(xslPath))
        //        {
        //            errors = WCompileToType(reader, XsltSettings.TrustedXslt, null, false, tb, "SomeAsm4.dll");
        //        }

        //        // Print errors and warnings
        //        bool hasError = false;
        //        foreach (CompilerError error in errors)
        //        {
        //            _output.WriteLine(error.ToString());
        //            hasError = true;
        //        }

        //        if (hasError) Assert.True(false);
        //    }
        //    catch (Exception e)
        //    {
        //        _output.WriteLine(e.ToString());
        //        Assert.True(false);
        //    }

        //    return;
        //}

        // Q>       In CompileToType when scriptAssemblyPath is null, should we really throw ArgumentNullExc
        //          if the stylesheet does not have any scripts (but the settings are enabled)?
        //
        // Anton>   Sergey and I think it�s acceptable behavior. Implementing the other way will require extra
        //          code churn in 3 files, which we tried to avoid.

        //BinCompat TODO: Add this test back
        ////[Variation("CompileToType(ScriptPath = null, sytlesheet with no scripts)", Pri = 1)]
        //[InlineData()]
        //[Theory]
        //public void Var5()
        //{
        //    AppDomain cd = System.Threading.Thread.GetDomain();
        //    AssemblyName an = new AssemblyName();
        //    an.Name = "HelloClass5";

        //    AssemblyBuilder ab = cd.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
        //    ModuleBuilder mb = ab.DefineDynamicModule("HelloModule5", "HelloModule5.dll", true);
        //    TypeBuilder tb = mb.DefineType("Hello5", TypeAttributes.Class | TypeAttributes.Public);

        //    try
        //    {
        //        String xslPath = FullFilePath("identity.xsl");
        //        CompilerErrorCollection errors;
        //        using (XmlReader reader = XmlReader.Create(xslPath))
        //        {
        //            errors = WCompileToType(reader, XsltSettings.TrustedXslt, null, false, tb, null);
        //        }

        //        // Print errors and warnings
        //        bool hasError = false;
        //        foreach (CompilerError error in errors)
        //        {
        //            _output.WriteLine(error.ToString());
        //            hasError = true;
        //        }

        //        if (hasError) Assert.True(false);
        //    }
        //    catch (ArgumentNullException e)
        //    {
        //        _output.WriteLine(e.ToString());
        //        return;
        //    }

        //    Assert.True(false);
        //}

        //BinCompat TODO: Add this test back
        //[Variation("CompileToType(ScriptPath = null, stylesheet with scripts)", Pri = 1)]
        //[InlineData()]
        //[Theory]
        //public void Var6()
        //{
        //    AppDomain cd = System.Threading.Thread.GetDomain();
        //    AssemblyName an = new AssemblyName();
        //    an.Name = "HelloClass6";

        //    AssemblyBuilder ab = cd.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
        //    ModuleBuilder mb = ab.DefineDynamicModule("HelloModule6", "HelloModule6.dll", true);
        //    TypeBuilder tb = mb.DefineType("Hello6", TypeAttributes.Class | TypeAttributes.Public);

        //    try
        //    {
        //        String xslPath = ScriptTestPath + "Scripting28.xsl";
        //        CompilerErrorCollection errors;
        //        using (XmlReader reader = XmlReader.Create(xslPath))
        //        {
        //            errors = WCompileToType(reader, XsltSettings.TrustedXslt, null, false, tb, null);
        //        }

        //        // Print errors and warnings
        //        bool hasError = false;
        //        foreach (CompilerError error in errors)
        //        {
        //            _output.WriteLine(error.ToString());
        //            hasError = true;
        //        }

        //        if (hasError) Assert.True(false);
        //    }
        //    catch (Exception e)
        //    {
        //        _output.WriteLine(e.ToString());
        //        return;
        //    }

        //    Assert.True(false);
        //}

        //BinCompat TODO: Add this test back
    //    //[Variation("CompileToType(ScriptPath = null, XsltSettings = null)", Pri = 1)]
    //    [InlineData()]
    //    [Theory]
    //    public void Var7()
    //    {
    //        AppDomain cd = System.Threading.Thread.GetDomain();
    //        AssemblyName an = new AssemblyName();
    //        an.Name = "HelloClass7";

    //        AssemblyBuilder ab = cd.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
    //        ModuleBuilder mb = ab.DefineDynamicModule("HelloModule7", "HelloModule7.dll", true);
    //        TypeBuilder tb = mb.DefineType("Hello7", TypeAttributes.Class | TypeAttributes.Public);

    //        try
    //        {
    //            CompilerErrorCollection errors;
    //            using (XmlReader reader = XmlReader.Create(FullFilePath("identity.xsl")))
    //            {
    //                errors = WCompileToType(reader, null, null, false, tb, null);
    //            }

    //            // Print errors and warnings
    //            bool hasError = false;
    //            foreach (CompilerError error in errors)
    //            {
    //                _output.WriteLine(error.ToString());
    //                hasError = true;
    //            }

    //            if (!hasError) return;
    //        }
    //        catch (Exception e)
    //        {
    //            _output.WriteLine(e.ToString());
    //            Assert.True(false);
    //        }

    //        Assert.True(false);
    //    }
    }

    //[TestCase(Name = "Load(MethodInfo, ByteArray, TypeArray) tests", Desc = "This testcase tests private Load method via Reflection. This method is used by sharepoint")]
    public class CLoadMethInfoTest : ReflectionTestCaseBase
    {
        private ITestOutputHelper _output;
        public CLoadMethInfoTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Load(MethodInfo = null, ByteArray, TypeArray)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var1()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            try
            {
                WLoad(xslt, (MethodInfo)null, new Byte[5], new Type[5]);
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());

                if (e is ArgumentNullException || e.InnerException is ArgumentNullException)
                    return;

                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(MethodInfo, ByteArray = null, TypeArray)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var2()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            try
            {
                WLoad(xslt, AMethodInfo, null, new Type[5]);
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());

                if (e is ArgumentException || e.InnerException is ArgumentException)
                    return;

                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Valid Load after error case Load(MethodInfo, ByteArray, TypeArray)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var3()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            try
            {
                //error case
                WLoad(xslt, AMethodInfo, new Byte[5], null);
            }
            catch (Exception e)
            {
                /*
                 * Anton> If staticData array is malformed, you end up with weird exceptions.
                 * We didn�t expect this method to be public and didn�t implement any range/sanity
                 * checks in deserialization code path, besides CLR implicit range checks for array indices.
                 * You have found one such place, but there are dozens of them. Fixing all of them will cause
                 * code churn in many files and another round of [code review/test sign-off/approval] process.
                 * Right now it works according to the �Garbage In, Garbage Out� principle.
                 *
                 */
                _output.WriteLine(e.ToString());
            }

            String asmPath = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\bftBaseLine.dll");
            String type = "bftBaseLine";
            String xml = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\sft1.xml");
            TestWLoad(xslt, Path.GetFullPath(asmPath), type);
            xslt.Transform(xml, "errout.txt");
            return;
        }

        //[Variation("Multiple Loads Load(MethodInfo, ByteArray, TypeArray)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var7()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();

            String asmPath = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\bftBaseLine.dll");
            String type = "bftBaseLine";
            String xml = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\sft1.xml");

            Assembly asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath);
            Type t = asm.GetType(type);

            MethodInfo meth = GetStaticMethod(t, "Execute");
            Byte[] staticData = (Byte[])t.GetTypeInfo().GetField("staticData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(t);
            Type[] ebTypes = (Type[])t.GetTypeInfo().GetField("ebTypes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(t);

            asmPath = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\Scripting28.dll");
            type = "Scripting28";

            asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(asmPath));
            t = asm.GetType(type);

            MethodInfo meth2 = GetStaticMethod(t, "Execute");
            Byte[] staticData2 = (Byte[])t.GetTypeInfo().GetField("staticData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(t);
            Type[] ebTypes2 = (Type[])t.GetTypeInfo().GetField("ebTypes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(t);

            for (int i = 0; i < 100; i++)
            {
                WLoad(xslt, meth, staticData, ebTypes);
                WLoad(xslt, meth2, staticData2, ebTypes2);
                xslt.Transform(xml, "out.txt");
                xslt.Transform(xml, "out1.txt");
            }

            return;
        }

        //[Variation("Load(MethodInfo, ByteArray, TypeArray) simple assembly", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var4()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            String asmPath = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\bftBaseLine.dll");
            String type = "bftBaseLine";
            String xml = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\sft1.xml");

            try
            {
                TestWLoad(xslt, Path.GetFullPath(asmPath), type);
                xslt.Transform(xml, "out.txt");
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
            }
            Assert.True(false);
        }

        //[Variation("Load(MethodInfo, ByteArray, TypeArray) assembly with scripts", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var5()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            String asmPath = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\Scripting28.dll");
            String type = "Scripting28";
            String xml = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\sft1.xml");

            try
            {
                TestWLoad(xslt, Path.GetFullPath(asmPath), type);
                xslt.Transform(xml, "out.txt");
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
            }
            Assert.True(false);
        }

        //[Variation("Load(MethodInfo, ByteArray, TypeArray) old xsltc assembly", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var6()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            String asmPath = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\CTT1.dll");
            String type = "CCT1";
            String xml = Path.Combine(@"TestFiles\", FilePathUtil.GetTestDataPath(), @"xsltc\precompiled\sft1.xml");

            try
            {
                TestWLoad(xslt, Path.GetFullPath(asmPath), type);
                xslt.Transform(xml, "out.txt");
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
            }
            Assert.True(false);
        }
    }

    //[TestCase(Name = "Null argument tests", Desc = "This testcase passes NULL arguments to all XslCompiledTransform methods")]
    public class CNullArgumentTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CNullArgumentTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Load(string = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var0()
        {
            try
            {
                new XslCompiledTransform().Load((string)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(IXPathNavigable = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var1()
        {
            try
            {
                new XslCompiledTransform().Load((IXPathNavigable)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(XmlReader = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var2()
        {
            try
            {
                new XslCompiledTransform().Load((XmlReader)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(IXPathNavigable = null, XmlResolver = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var3()
        {
            try
            {
                new XslCompiledTransform().Load((IXPathNavigable)null, XsltSettings.TrustedXslt, (XmlResolver)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(XmlReader = null, XmlResolver = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var4()
        {
            try
            {
                new XslCompiledTransform().Load((XmlReader)null, XsltSettings.TrustedXslt, (XmlResolver)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(IXPathNavigable = null, XmlResolver = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var5()
        {
            try
            {
                new XslCompiledTransform().Load((IXPathNavigable)null, XsltSettings.TrustedXslt, (XmlResolver)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Load(XmlReader = null, XmlResolver = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var6()
        {
            try
            {
                new XslCompiledTransform().Load((XmlReader)null, XsltSettings.TrustedXslt, (XmlResolver)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var7()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                StringWriter sw = new StringWriter();
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, sw);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var8()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                StringWriter sw = new StringWriter();
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, sw);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null, TextWriter = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var9()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, (TextWriter)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null, TextWriter = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var10()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, (TextWriter)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null, Stream = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var11()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, (Stream)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null, Stream = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var12()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, (Stream)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null, XmlWriter = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var13()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, (XmlWriter)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }

        //[Variation("Transform(IXPathNavigable = null, XsltArgumentList = null, XmlWriter = null)", Pri = 1)]
        [InlineData()]
        [Theory]
        public void Var14()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                xslt.Load(FullFilePath("identity.xsl"));
                xslt.Transform((IXPathNavigable)null, (XsltArgumentList)null, (XmlWriter)null);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Did not throw ArgumentNullException");
            }
            Assert.True(false);
        }
    }

    /***********************************************************/
    /*          XslCompiledTransform.Resolver - Integrity              */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Reader, Reader", Desc = "READER,READER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Reader, Stream", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Reader, Writer", Desc = "READER,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Reader, TextWriter", Desc = "READER,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : URI, Reader", Desc = "URI,READER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : URI, Stream", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : URI, Writer", Desc = "URI,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : URI, TextWriter", Desc = "URI,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Navigator, Reader", Desc = "NAVIGATOR,READER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Navigator, Stream", Desc = "NAVIGATOR,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Navigator, Writer", Desc = "NAVIGATOR,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.XmlResolver : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CXmlResolverTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CXmlResolverTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation(id = 1, Desc = "Set XmlResolver property to null, load style sheet with import/include, should not affect transform")]
        [InlineData()]
        [Theory]
        public void XmlResolver1()
        {
            try
            {
                LoadXSL("xmlResolver_main.xsl", null);
                Assert.True(false);
            }
            catch (XsltException e1)
            {
                _output.WriteLine(e1.Message);
                return;
            }
            catch (ArgumentNullException e2)
            {
                _output.WriteLine(e2.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation(id = 2, Desc = "Set XmlResolver property to null, load style sheet with document function, should not resolve during load")]
        [InlineData()]
        [Theory]
        public void XmlResolver2()
        {
            try
            {
                LoadXSL("xmlResolver_main.xsl", null);
                _output.WriteLine("No exception was thrown");
                Assert.True(false);
            }
            catch (XsltException e1)
            {
                _output.WriteLine(e1.Message);
                return;
            }
            catch (ArgumentNullException e2)
            {
                _output.WriteLine(e2.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation(id = 3, Desc = "Default XmlResolver, load style sheet with document function, should resolve during transform", Pri = 1, Param = "DefaultResolver.txt")]
        [InlineData("DefaultResolver.txt")]
        [Theory]
        public void XmlResolver3(object param)
        {
            string Baseline = "baseline\\" + (string)param;

            if (LoadXSL("xmlResolver_document_function.xsl") == 1)
            {
                if (Transform("fruits.xml") == 1)
                {
                    VerifyResult(Baseline, _strOutFile);
                    return;
                }
            }
            else
            {
                _output.WriteLine("Problem loading stylesheet with document function and default resolver!");
                Assert.True(false);
            }
            Assert.True(false);
        }

        /*
                //[Variation("Set to NULL many times in a loop, then to proper cred.")]
                [InlineData()]
                [Theory]
                public void XmlResolver6()
                {
                    // Skip this test for Load(URI)
                    // Reason: When style sheet URI = Intranet zone, XmlSecureResolver does not resolve document function

                    if(LoadXSL("xmlResolver_cred.xsl") == TEST_PASS)
                    {
                        for(int i=0; i<100; i++)
                            xslt.XmlResolver = null;

                        for(int i=0; i<100; i++)
                            xslt.XmlResolver = GetDefaultCredResolver();

                        if ((Transform("fruits.xml") == TEST_PASS) && (CheckResult(377.8217373898) == TEST_PASS))
                            return;
                        else
                        {
                            _output.WriteLine("Failed to use XmlResolver property to resolve document function");
                            Assert.True(false);
                        }
                    }
                    else
                    {
                        _output.WriteLine("Failed to load style sheet!");
                        Assert.True(false);
                    }
                }
        */

        //[Variation(id = 7, Desc = "document() has absolute URI", Pri = 0)]
        [InlineData()]
        [Theory]
        public void XmlResolver7()
        {
            // copy file on the local machine (this is now done with createAPItestfiles.js, see Oasys scenario.)
            if (LoadXSL("xmlResolver_document_function_absolute_uri.xsl") == 1)
            {
                if ((Transform("fruits.xml") == 1) && (CheckResult(377.8217373898) == 1))
                    return;
                else
                {
                    _output.WriteLine("Failed to resolve document function with absolute URI.");
                    Assert.True(false);
                }
            }
            else
            {
                _output.WriteLine("Failed to load style sheet!");
                Assert.True(false);
            }
        }
    }

    /***********************************************************/
    /*          XslCompiledTransform.Load - Integrity                  */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Reader, Reader", Desc = "READER,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Reader, Stream", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Reader, Writer", Desc = "READER,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Reader, TextWriter", Desc = "READER,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : URI, Reader", Desc = "URI,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : URI, Stream", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : URI, Writer", Desc = "URI,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : URI, TextWriter", Desc = "URI,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Navigator, Reader", Desc = "NAVIGATOR,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Navigator, Stream", Desc = "NAVIGATOR,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Navigator, Writer", Desc = "NAVIGATOR,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load() - Integrity : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CLoadTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation(id = 1, Desc = "Call Load with null value")]
        [InlineData()]
        [Theory]
        public void LoadGeneric1()
        {
            try
            {
                LoadXSL(null);
            }
            catch (System.ArgumentException)
            {
                // System.Xml.XmlUrlResolver.ResolveUri(Uri baseUri, String relativeUri) throws System.ArgumentException here for null
                return;
            }
            _output.WriteLine("Exception not generated for null parameter name");
            Assert.True(false);
        }

        //[Variation(id = 2, Desc = "Load with valid, then invalid, then valid again")]
        [InlineData()]
        [Theory]
        public void LoadGeneric2()
        {
            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    LoadXSL("IDontExist.xsl");
                }
                catch (System.IO.FileNotFoundException)
                {
                    try
                    {
                        Transform("fruits.xml");
                    }
                    catch (System.InvalidOperationException e)
                    {
                        CheckExpectedError(e, "System.xml", "Xslt_NoStylesheetLoaded", new string[] { "" });
                        return;
                    }
                }
            }
            else
            {
                _output.WriteLine("Failed to load style sheet!");
                Assert.True(false);
            }
            Assert.True(false);
        }

        //[Variation(id = 3, Desc = "Load an invalid, then a valid and transform", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadGeneric3(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            try
            {
                LoadXSL("IDontExist.xsl");
            }
            catch (System.IO.FileNotFoundException)
            {
                if ((LoadXSL("ShowParam.xsl") == 1) && (Transform("fruits.xml") == 1))
                {
                    VerifyResult(Baseline, _strOutFile);
                    return;
                }
            }
            _output.WriteLine("Exception not generated for non-existent file name");
            Assert.True(false);
        }

        //[Variation(id = 4, Desc = "Call several overloaded functions", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadGeneric4(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            if (MyXslInputType() != XslInputType.Reader)
                LoadXSL("showParamLongName.xsl", XslInputType.Reader, new XmlUrlResolver());
            if (MyXslInputType() != XslInputType.URI)
                LoadXSL("showParamLongName.xsl", XslInputType.URI, new XmlUrlResolver());
            if (MyXslInputType() != XslInputType.Navigator)
                LoadXSL("showParamLongName.xsl", XslInputType.Navigator, new XmlUrlResolver());

            if ((LoadXSL("ShowParam.xsl") == 0) || (Transform("fruits.xml") == 0))
                Assert.True(false);

            VerifyResult(Baseline, _strOutFile);

            if (MyXslInputType() != XslInputType.Navigator)
                LoadXSL("showParamLongName.xsl", XslInputType.Navigator, new XmlUrlResolver());
            if (MyXslInputType() != XslInputType.URI)
                LoadXSL("showParamLongName.xsl", XslInputType.URI, new XmlUrlResolver());
            if (MyXslInputType() != XslInputType.Reader)
                LoadXSL("showParamLongName.xsl", XslInputType.Reader, new XmlUrlResolver());

            if ((LoadXSL("ShowParam.xsl") == 1) && (Transform("fruits.xml") == 1))
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }

            Assert.True(false);
        }

        //[Variation(id = 5, Desc = "Call same overloaded Load() many times then transform", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadGeneric5(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            for (int i = 0; i < 100; i++)
            {
                if (LoadXSL("showParam.xsl") != 1)
                {
                    _output.WriteLine("Failed to load stylesheet showParam.xsl on the {0} attempt", i);
                    Assert.True(false);
                }
            }
            if ((LoadXSL("ShowParam.xsl") == 1) && (Transform("fruits.xml") == 1))
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }

            Assert.True(false);
        }

        //[Variation(id = 6, Desc = "Call load with non-existing stylesheet")]
        [InlineData()]
        [Theory]
        public void LoadGeneric6()
        {
            try
            {
                LoadXSL("IDontExist.xsl");
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }
            _output.WriteLine("Exception not generated for non-existent file parameter name");
            Assert.True(false);
        }

        //[Variation(id = 7, Desc = "Verify that style sheet is closed properly after Load - Shared Read Access")]
        [InlineData()]
        [Theory]
        public void LoadGeneric7()
        {
            FileStream s2;

            // check immediately after load and after transform
            if (LoadXSL("XmlResolver_main.xsl") == 1)
            {
                s2 = new FileStream(FullFilePath("XmlResolver_main.xsl"), FileMode.Open, FileAccess.Read, FileShare.Read);
                s2.Dispose();
                if ((Transform("fruits.xml") == 1) && (CheckResult(428.8541842246) == 1))
                {
                    s2 = new FileStream(FullFilePath("XmlResolver_main.xsl"), FileMode.Open, FileAccess.Read, FileShare.Read);
                    s2.Dispose();
                    return;
                }
            }
            Assert.True(false);
        }

        /*
        //[Variation(id =8, Desc ="Verify that style sheet is closed properly after Load - ReadWrite Access")]
        [InlineData()]
        [Theory]
        public void LoadGeneric8()
        {
            FileStream s2;

            // check immediately after load and after transform

            if(LoadXSL("XmlResolver_main.xsl") == TEST_PASS)
            {
                s2 = new FileStream(FullFilePath("XmlResolver_main.xsl"), FileMode.Open, FileAccess.ReadWrite);
                s2.Dispose();
                if((Transform("fruits.xml") == TEST_PASS) && (CheckResult(428.8541842246)== TEST_PASS))
                {
                    s2 = new FileStream(FullFilePath("XmlResolver_main.xsl"), FileMode.Open, FileAccess.ReadWrite);
                    s2.Dispose();
                    return;
                }
            }
            _output.WriteLine("Appeared to not close style sheet file properly after loading.");
            Assert.True(false);
        }
        */

        //[Variation(id = 9, Desc = "Verify that included files are closed properly after Load - Read Access")]
        [InlineData()]
        [Theory]
        public void LoadGeneric9()
        {
            FileStream s2;

            // check immediately after load and after transform
            if (LoadXSL("XmlResolver_main.xsl") == 1)
            {
                s2 = new FileStream(FullFilePath("XmlResolver_sub.xsl"), FileMode.Open, FileAccess.Read);
                s2.Dispose();
                if ((Transform("fruits.xml") == 1) && (CheckResult(428.8541842246) == 1))
                {
                    s2 = new FileStream(FullFilePath("XmlResolver_Include.xsl"), FileMode.Open, FileAccess.Read, FileShare.Read);
                    s2.Dispose();
                    return;
                }
            }
            _output.WriteLine("Appeared to not close file properly after loading.");
            Assert.True(false);
        }

        /*
        //[Variation(id =10, Desc ="Verify that included files are closed properly after Load - ReadWrite Access")]
        [InlineData()]
        [Theory]
        public void LoadGeneric10()
        {
            FileStream s2;

            // check immediately after load and after transform
            if(LoadXSL("XmlResolver_main.xsl") == TEST_PASS)
            {
                s2 = new FileStream(FullFilePath("XmlResolver_sub.xsl"), FileMode.Open, FileAccess.ReadWrite);
                s2.Dispose();
                if((Transform("fruits.xml") == TEST_PASS) && (CheckResult(428.8541842246)== TEST_PASS))
                {
                    s2 = new FileStream(FullFilePath("XmlResolver_Include.xsl"), FileMode.Open, FileAccess.ReadWrite);
                    s2.Dispose();
                    return;
                }
            }
            _output.WriteLine("Appeared to not close file properly after loading.");
            Assert.True(false);
        }
        */

        //[Variation(id = 11, Desc = "Load stylesheet with entity reference: Bug #68450 ")]
        [InlineData()]
        [Theory]
        public void LoadGeneric11()
        {
            if (MyDocType().ToString() == "DataDocument")
                // Skip the test for DataDocument
                return;
            else
            {
                if (LoadXSL("books_entity_ref.xsl", XslInputType.Reader, new XmlUrlResolver()) != 1)
                {
                    _output.WriteLine("Failed to load stylesheet books_entity_ref.xsl");
                    Assert.True(false);
                }
                if ((LoadXSL("books_entity_ref.xsl") == 1) && (Transform("books_entity_ref.xml") == 1)
                    && (CheckResult(371.4148215954) == 1))
                    return;
                Assert.True(false);
            }
        }

        //[Variation(id = 12, Desc = "Load with invalid stylesheet and verify that file is closed properly")]
        [InlineData()]
        [Theory]
        public void LoadGeneric12()
        {
            Stream strmTemp;

            try
            {
                int i = LoadXSL("xslt_error.xsl");
            }
            catch (XsltException)
            {
                // Try to open the xsl file
                try
                {
                    strmTemp = new FileStream(FullFilePath("xslt_error.xsl"), FileMode.Open, FileAccess.Read);
                }
                catch (Exception ex)
                {
                    _output.WriteLine("Did not close stylesheet properly after load");
                    _output.WriteLine(ex.Message);
                    Assert.True(false);
                }
                return;
            }
            _output.WriteLine("Did not throw compile exception for stylesheet");
            Assert.True(false);
        }
    }

    /**************************************************************************/
    /*          XslCompiledTransform.Load(XmlResolver) - Integrity   */
    /**************************************************************************/

    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Reader, Reader", Desc = "READER,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Reader, Stream", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Reader, Writer", Desc = "READER,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Reader, TextWriter", Desc = "READER,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Navigator, Reader", Desc = "NAVIGATOR,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Navigator, Stream", Desc = "NAVIGATOR,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Navigator, Writer", Desc = "NAVIGATOR,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : URI, Reader", Desc = "URI,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : URI, Stream", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : URI, Writer", Desc = "URI,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(XmlResolver) - Integrity : URI, TextWriter", Desc = "URI,TEXTWRITER")]
    public class CLoadXmlResolverTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadXmlResolverTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Call Load with null source value and null resolver")]
        [InlineData()]
        [Theory]
        public void LoadGeneric1()
        {
            try
            {
                LoadXSL_Resolver(null, null);
            }
            catch (System.ArgumentNullException e)
            {
                _output.WriteLine(e.ToString());
                return;
            }
            _output.WriteLine("Passing null argument should have thrown ArgumentNullException");
            Assert.True(false);
        }

        //[Variation("Call Load with null source value and valid resolver")]
        [InlineData()]
        [Theory]
        public void LoadGeneric2()
        {
            try
            {
                LoadXSL_Resolver(null, new XmlUrlResolver());
            }
            catch (System.ArgumentNullException e)
            {
                _output.WriteLine(e.ToString());
                return;
            }
            _output.WriteLine("Passing null stylesheet should have thrown ArgumentNullException");
            Assert.True(false);
        }

        //[Variation("Call Load with null XmlResolver, style sheet does not have include/import, URI should throw ArgumentNullException and the rest shouldn't error", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadGeneric3(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            try
            {
                LoadXSL_Resolver("ShowParam.xsl", null);
                Transform("fruits.xml");
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            catch (ArgumentNullException e)
            {
                _output.WriteLine(e.ToString());
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation("Call Load with null XmlResolver and stylesheet has import/include, URI should throw ArgumentNullException, rest throw XsltException")]
        [InlineData()]
        [Theory]
        public void LoadGeneric4()
        {
            try
            {
                LoadXSL_Resolver("xmlResolver_main.xsl", null);
                _output.WriteLine("No exception was thrown when a null resolver is passed");
                Assert.True(false);
            }
            catch (XsltException e1)
            {
                _output.WriteLine(e1.Message);
                return;
            }
            catch (ArgumentNullException e2)
            {
                _output.WriteLine(e2.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation("Call Load with null custom resolver and style sheet has import/include, should throw Exception")]
        [InlineData()]
        [Theory]
        public void LoadGeneric5()
        {
            CustomNullResolver myResolver = new CustomNullResolver(_output);

            try
            {
                LoadXSL_Resolver("xmlResolver_main.xsl", myResolver);
                _output.WriteLine("No exception is thrown");
                Assert.True(false);
            }
            catch (XsltException e1)
            {
                _output.WriteLine(e1.Message);
                return;
            }
            catch (ArgumentNullException e2)
            {
                _output.WriteLine(e2.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
            catch (XmlException e3)
            {
                _output.WriteLine(e3.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("XmlException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation("Call Load with null custom resolver and style sheet has no import/include, should error for URI only", Param = "ShowParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadGeneric6(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            CustomNullResolver myResolver = new CustomNullResolver(_output);
            try
            {
                LoadXSL_Resolver("ShowParam.xsl", myResolver);
                Transform("fruits.xml");
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            catch (ArgumentNullException e)
            {
                _output.WriteLine(e.ToString());
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
            catch (XsltException e3)
            {
                _output.WriteLine(e3.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("XmlException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation("Style sheet has import/include, call Load first with custom null resolver and then default resolver, should not fail", Param = "XmlResolverTestMain.txt")]
        [InlineData("XmlResolverTestMain.txt")]
        [Theory]
        public void LoadGeneric7(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            CustomNullResolver myResolver = new CustomNullResolver(_output);

            try
            {
                LoadXSL_Resolver("xmlResolver_main.xsl", myResolver);
            }
            //For input types != URI
            catch (System.Xml.Xsl.XsltException e1)
            {
                // The lovely thing about this test is that the stylesheet, XmlResolver_main.xsl, has an include. GetEntity is therefore called twice. We need to have both these
                // checks here to ensure that both the XmlResolver_main.xsl and XmlResolver_Include.xsl GetEntity() calls are handled.
                try
                {
                    CheckExpectedError(e1, "System.Data.SqlXml", "Xslt_CannotLoadStylesheet", new string[] { new Uri(FullFilePath("XmlResolver_Include.xsl")).ToString(), "null" });
                }
                catch(Xunit.Sdk.TrueException)
                {
                    CheckExpectedError(e1, "System.Data.SqlXml", "Xslt_CannotLoadStylesheet", new string[] { new Uri(FullFilePath("xmlResolver_main.xsl")).ToString(), "null" });
                }

                if (LoadXSL("xmlResolver_main.xsl") == 1)
                {
                    if ((Transform("fruits.xml") == 1) && (CheckResult(428.8541842246) == 1))
                        return;
                    else
                        Assert.True(false);
                }
                else
                {
                    _output.WriteLine("Failed to load stylesheet using default resolver");
                    Assert.True(false);
                }
            }

            //For URI
            catch (System.ArgumentNullException e2)
            {
                try {
                    CheckExpectedError(e2, "System.Data.SqlXml", "Xslt_CannotLoadStylesheet", new string[] { new Uri(FullFilePath("XmlResolver_Include.xsl")).ToString(), "null" });
                }
                catch(Xunit.Sdk.TrueException)
                {
                    CheckExpectedError(e2, "System.Data.SqlXml", "Xslt_CannotLoadStylesheet", new string[] { new Uri(FullFilePath("xmlResolver_main.xsl")).ToString(), "null" });
                }

                if (LoadXSL("xmlResolver_main.xsl") == 1)
                {
                    if (Transform("fruits.xml") == 1)
                    {
                        VerifyResult(Baseline, _strOutFile);
                        return;
                    }
                    else
                        Assert.True(false);
                }
                else
                {
                    _output.WriteLine("Failed to load stylesheet using default resolver");
                    Assert.True(false);
                }
            }
            catch (XmlException e3)
            {
                _output.WriteLine(e3.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("XmlException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }

            Assert.True(false);
        }

        //[Variation("Style sheet has import/include, call Load first with default resolver and then with custom null resolver, should fail", Param = "XmlResolverTestMain.txt")]
        [InlineData("XmlResolverTestMain.txt")]
        [Theory]
        public void LoadGeneric8(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            CustomNullResolver myResolver = new CustomNullResolver(_output);

            if ((LoadXSL("xmlResolver_main.xsl") == 1))
            {
                try
                {
                    LoadXSL_Resolver("xmlResolver_main.xsl", myResolver);
                }
                catch (System.Xml.Xsl.XsltException e1)
                {
                    // The lovely thing about this test is that the stylesheet, XmlResolver_main.xsl, has an include. GetEntity is therefore called twice. We need to have both these
                    // checks here to ensure that both the XmlResolver_main.xsl and XmlResolver_Include.xsl GetEntity() calls are handled.
                    // Yes, this is effetively the same test as LoadGeneric7, in that we use the NullResolver to return null from a GetEntity call.
                    try {
                        CheckExpectedError(e1, "System.Data.SqlXml", "Xslt_CannotLoadStylesheet", new string[] { new Uri(FullFilePath("XmlResolver_Include.xsl")).ToString(), "null" });
                        return;
                    }
                    catch(Xunit.Sdk.TrueException)
                    {
                        CheckExpectedError(e1, "System.Data.SqlXml", "Xslt_CannotLoadStylesheet", new string[] { new Uri(FullFilePath("xmlResolver_main.xsl")).ToString(), "null" });
                        return;
                    }
                }
                catch (ArgumentNullException e2)
                {
                    _output.WriteLine(e2.Message);
                    if (MyXslInputType() == XslInputType.URI)
                        return;
                    else
                    {
                        _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                        Assert.True(false);
                    }
                }
                catch (XmlException e3)
                {
                    _output.WriteLine(e3.Message);
                    if (MyXslInputType() == XslInputType.URI)
                        return;
                    else
                    {
                        _output.WriteLine("XmlException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                        Assert.True(false);
                    }
                }
                _output.WriteLine("No exception generated when loading with an invalid resolver after loading with valid resolver");
                Assert.True(false);
            }
            _output.WriteLine("Could not load style sheet with default resolver");
            Assert.True(false);
        }

        //[Variation("Load with resolver with credentials, then load XSL that does not need cred.")]
        [InlineData()]
        [Theory]
        public void LoadGeneric9()
        {
            if ((LoadXSL_Resolver("xmlResolver_Main.xsl", GetDefaultCredResolver()) == 1))
            {
                if ((LoadXSL("xmlResolver_Main.xsl") == 1) && (Transform("fruits.xml") == 1)
                    && (CheckResult(428.8541842246) == 1))
                    return;
            }
            else
            {
                _output.WriteLine("Failed to load!");
                Assert.True(false);
            }
            Assert.True(false);
        }

        /* This test doesn't make sense coz loading a stylesheet with null resolver will throw ArgumentNullException
        //[Variation("Call Load() many times with null resolver then perform a transform")]
        [InlineData()]
        [Theory]
        public void LoadGeneric10()
        {
            for(int i=0; i < 100; i++)
            {
                if(LoadXSL_Resolver("showParam.xsl", null) != TEST_PASS)
                {
                    _output.WriteLine("Failed to load stylesheet showParam.xsl on the {0} attempt", i);
                    Assert.True(false);
                }
            }
            if((LoadXSL_Resolver("showParam.xsl", null) == TEST_PASS) && (Transform("fruits.xml") == TEST_PASS)
                && (VerifyResult(Baseline, _strOutFile)== TEST_PASS))
                return;
            Assert.True(false);
        }
        */

        //[Variation("Call Load with null Resolver, file does not exist")]
        [InlineData()]
        [Theory]
        public void LoadGeneric11()
        {
            try
            {
                LoadXSL_Resolver("IDontExist.xsl", null);
                _output.WriteLine("No exception was thrown");
                Assert.True(false);
            }
            catch (FileNotFoundException e1)
            {
                _output.WriteLine(e1.Message);
                return;
            }
            catch (ArgumentNullException e2)
            {
                _output.WriteLine(e2.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        /* This test doesn't make sense anymore coz passing null resolver throws ArgumentNullException
         * right on the first Load on showParam.xsl
         *
        //[Variation("Load non existing stylesheet with null resolver and try to transform")]
        [InlineData()]
        [Theory]
        public void LoadGeneric12()
        {
            if(LoadXSL_Resolver("showParam.xsl", null) == TEST_PASS)
            {
                try
                {
                    LoadXSL_Resolver("IDontExist.xsl", null);
                }
                catch(System.IO.FileNotFoundException)
                {
                    //no stylesheet loaded, should throw error
                    try
                    {
                        Transform("fruits.xml");
                    }
                    catch(System.InvalidOperationException e2)
                    {
                        return CheckExpectedError(e2, "system.xml", "Xslt_NoStylesheetLoaded", new string[] { "IDontExist.xsl" });
                    }
                }
                _output.WriteLine("Exception not generated for non-existent file parameter name");
            }
            else
            {
                _output.WriteLine("Errors loading initial file");
                Assert.True(false);
            }
            Assert.True(false);
        }
        */
    }

    /***********************************************************/
    /*          XslCompiledTransform.Load(Url, Resolver)               */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Load(Url, Resolver) : URI, Reader", Desc = "URI,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load(Url, Resolver) : URI, Stream", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load(Url, Resolver) : URI, Writer", Desc = "URI,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(Url, Resolver) : URI, TextWriter", Desc = "URI,TEXTWRITER")]
    public class CLoadUrlResolverTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadUrlResolverTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Basic check for usage of credentials on resolver, load XSL that needs cred. with correct resolver", Param = "XmlResolverTestMain.txt")]
        [InlineData("XmlResolverTestMain.txt")]
        [Theory]
        public void LoadUrlResolver1(object param)
        {
            // XsltResolverTestMain.xsl is placed in IIS virtual directory
            // which requires integrated Windows NT authentication
            string Baseline = "baseline\\" + (string)param;
            if ((LoadXSL_Resolver("XmlResolver/XmlResolverTestMain.xsl", GetDefaultCredResolver()) == 1) &&
                (Transform("fruits.xml") == 1))
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }

            Assert.True(false);
        }

        //[Variation("Load XSL that needs cred. with null resolver, should fail", Param = "XmlResolverTestMain.txt")]
        [InlineData()]
        [Theory]
        public void LoadUrlResolver2()
        {
            try
            {
                LoadXSL_Resolver("XmlResolver/XmlResolverTestMain.xsl", null);
            }
            catch (XsltException e)
            {
                _output.WriteLine(e.ToString());
                return;
            }
            _output.WriteLine("Passing null resolver should have thrown XsltException");
            Assert.True(false);
        }

        //[Variation("Call Load with null source value")]
        [InlineData()]
        [Theory]
        public void LoadUrlResolver3()
        {
            try
            {
                LoadXSL_Resolver(null, new XmlUrlResolver());
            }
            catch (ArgumentNullException e)
            {
                _output.WriteLine(e.ToString());
                return;
            }
            _output.WriteLine("Passing null stylesheet parameter should have thrown ArgumentNullException");
            Assert.True(false);
        }
    }

    /****************************************************************************************/
    /*          XslCompiledTransform.Load(Reader/Navigator,XmlResolver,Evidence) - Integrity        */
    /****************************************************************************************/

    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Reader, Reader", Desc = "READER,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Reader, Stream", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Reader, Writer", Desc = "READER,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Reader, TextWriter", Desc = "READER,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Navigator, Reader", Desc = "NAVIGATOR,READER")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Navigator, Stream", Desc = "NAVIGATOR,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Navigator, Writer", Desc = "NAVIGATOR,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Load(,XmlResolver,Evidence) : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CLoadReaderResolverEvidenceTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadReaderResolverEvidenceTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Call Load with null source value, null evidence")]
        [InlineData()]
        [Theory]
        public void LoadGeneric1()
        {
            try
            {
                LoadXSL_Resolver_Evidence(null, new XmlUrlResolver(), null);
            }
            catch (ArgumentNullException e)
            {
                _output.WriteLine(e.ToString());
                return;
            }
            _output.WriteLine("Passing null stylesheet parameter should have thrown ArgumentNullException");
            Assert.True(false);
        }

        //[Variation("Call Load with style sheet that has script, pass null evidence, should throw security exception")]
        [InlineData()]
        [Theory]
        public void LoadGeneric2()
        {
            /*
            try
            {
                LoadXSL_Resolver_Evidence("scripting_unsafe_object.xsl", new XmlUrlResolver(), null);
            }
            catch(System.Security.Policy.PolicyException e)
            {
                _output.WriteLine(e.ToString());
                return;
            }
            _output.WriteLine("Did not throw a security exception for null evidence!");
            Assert.True(false);
            */
            return; //TEST_SKIPPED;
        }

        //BinCompat TODO: Add this test back
        ////[Variation("Call Load with style sheet that has script, pass correct evidence")]
        //[InlineData()]
        //[Theory]
        //public void LoadGeneric3()
        //{
        //    if (_isInProc)
        //        return; //TEST_SKIPPED;

        //    Evidence evidence = new Evidence();
        //    evidence.AddHost(new Zone(SecurityZone.MyComputer));
        //    try
        //    {
        //        LoadXSL_Resolver_Evidence("scripting_unsafe_object.xsl", new XmlUrlResolver(), evidence);
        //    }
        //    catch (System.Security.Policy.PolicyException)
        //    {
        //        _output.WriteLine("Should not throw a security exception for correct evidence!");
        //        Assert.True(false);
        //    }
        //    return;
        //}
    }

    /***********************************************************/
    /*          XslCompiledTransform.Load(Url)                         */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Load(Url) Integrity : URI, Stream", Desc = "URI,STREAM")]
    public class CLoadStringTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadStringTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Call Load with an invalid uri")]
        [InlineData()]
        [Theory]
        public void LoadUrl1()
        {
            try
            {
                LoadXSL("IDontExist.xsl", XslInputType.URI, new XmlUrlResolver());
                _output.WriteLine("No exception was thrown");
                Assert.True(false);
            }
            catch (FileNotFoundException e1)
            {
                _output.WriteLine(e1.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("FileNotFoundException is supposed to be thrown");
                    Assert.True(false);
                }
            }
            catch (ArgumentNullException e2)
            {
                _output.WriteLine(e2.Message);
                if (MyXslInputType() == XslInputType.URI)
                    return;
                else
                {
                    _output.WriteLine("ArgumentNullException is not supposed to be thrown for the input type '" + MyXslInputType() + "'");
                    Assert.True(false);
                }
            }
        }

        //[Variation("Load file with empty string")]
        [InlineData()]
        [Theory]
        public void LoadUrl2()
        {
            try
            {
                LoadXSL(szEmpty, XslInputType.URI, new XmlUrlResolver());
            }
            catch (System.ArgumentException)
            {
                // System.Xml.XmlUrlResolver.ResolveUri(Uri baseUri, String relativeUri) throws ArgumentException
                return;
            }
            _output.WriteLine("Exception not generated for an empty string filename");
            Assert.True(false);
        }

        //[Variation("Load with \".\"")]
        [InlineData()]
        [Theory]
        public void LoadUrl3()
        {
            try
            {
                LoadXSL(".", XslInputType.URI, new XmlUrlResolver());
            }
            catch (System.UnauthorizedAccessException)
            {
                return;
            }
            _output.WriteLine("Exception not generated for non-existent file parameter name");
            Assert.True(false);
        }

        //[Variation("Load with \"..\"")]
        [InlineData()]
        [Theory]
        public void LoadUrl()
        {
            try
            {
                LoadXSL("..", XslInputType.URI, new XmlUrlResolver());
            }
            catch (System.UnauthorizedAccessException)
            {
                return;
            }
            _output.WriteLine("Exception not generated for non-existent file parameter name");
            Assert.True(false);
        }

        //[Variation("Load with \"\\\\\"")]
        [InlineData()]
        [Theory]
        public void LoadUrl5()
        {
            try
            {
                LoadXSL("\\\\", XslInputType.URI, new XmlUrlResolver());
            }
            catch (System.ArgumentException)
            {
                // System.Xml.XmlUrlResolver.ResolveUri(Uri baseUri, String relativeUri) throws ArgumentException
                return;
            }
            _output.WriteLine("Exception not generated for non-existent file parameter name");
            Assert.True(false);
        }

        /*

        //[Variation("Call Load with style sheet that has script, pass Url which does not have correct evidence, should fail")]
        [InlineData()]
        [Theory]
        public void LoadUrl6()
        {
            try
            {
                LoadXSL_Resolver(FullHttpPath("XmlResolver/scripting_unsafe_object.xsl"), GetDefaultCredResolver());
            }
            catch(System.Security.Policy.PolicyException)
            {
                return;
            }
            _output.WriteLine("Should throw a security exception for incorrect evidence!");
            Assert.True(false);
        }

        //[Variation("Call Load with style sheet that has script, pass Url which has correct evidence, should pass")]
        [InlineData()]
        [Theory]
        public void LoadUrl7()
        {
            try
            {
                LoadXSL("scripting_unsafe_object.xsl");
            }
            catch(System.Security.Policy.PolicyException)
            {
                _output.WriteLine("Should not throw a security exception for correct evidence!");
                Assert.True(false);
            }
            return;
        }

        */
    }

    /***********************************************************/
    /*          XslCompiledTransform.Load(IXPathNavigable)             */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform .Load(IXPathNavigable) : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CLoadXPathNavigableTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadXPathNavigableTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Basic Verification Test", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadNavigator1(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            xslt = new XslCompiledTransform();
            String _strXslFile = "showParam.xsl";

            _strXslFile = FullFilePath(_strXslFile);
            _output.WriteLine("Compiling {0}", _strXslFile);

            XmlReader xrLoad = XmlReader.Create(_strXslFile);
            XPathDocument xdTemp = new XPathDocument(xrLoad, XmlSpace.Preserve);
            xrLoad.Dispose();
            xslt.Load(xdTemp);

            if (Transform("fruits.xml") == 1)
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            Assert.True(false);
        }

        //[Variation("Create Navigator and navigate away from root", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadNavigator2(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            xslt = new XslCompiledTransform();
            XmlReader xrLoad = XmlReader.Create(FullFilePath("showParam.xsl"));
            XPathDocument xdTemp = new XPathDocument(xrLoad, XmlSpace.Preserve);
            xrLoad.Dispose();
            XPathNavigator xP = ((IXPathNavigable)xdTemp).CreateNavigator();

            xP.MoveToNext();
            xslt.Load(xP);

            if (Transform("fruits.xml") == 1)
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            Assert.True(false);
        }

        //[Variation("Basic check for usage of credentials on resolver, load XSL that needs cred. with correct resolver", Param = "XmlResolverTestMain.txt")]
        [InlineData("XmlResolverTestMain.txt")]
        [Theory]
        public void LoadNavigator3(object param)
        {
            xslt = new XslCompiledTransform();
            string Baseline = "baseline\\" + (string)param;
            XmlReader xrLoad = XmlReader.Create(FullFilePath("XmlResolver/XmlResolverTestMain.xsl"));
            //xrLoad.XmlResolver = GetDefaultCredResolver();

            XPathDocument xdTemp = new XPathDocument(xrLoad, XmlSpace.Preserve);
            XPathNavigator xP = ((IXPathNavigable)xdTemp).CreateNavigator();

            xslt.Load(xP, XsltSettings.TrustedXslt, GetDefaultCredResolver());
            if (Transform("fruits.xml") == 1)
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            Assert.True(false);
        }

        //[Variation("Regression case for bug 80768")]
        [InlineData()]
        [Theory]
        public void LoadNavigator4()
        {
            if (_isInProc)
                return; //TEST_SKIPPED;

            xslt = new XslCompiledTransform();
            XmlReader xrLoad = XmlReader.Create(FullFilePath("Bug80768.xsl"));
            XPathDocument xd = new XPathDocument(xrLoad, XmlSpace.Preserve);

            xslt.Load(xd, XsltSettings.TrustedXslt, new XmlUrlResolver());

            FileStream fs = new FileStream(_strOutFile, FileMode.Create, FileAccess.ReadWrite);
            XPathNavigator xn = new MyNavigator(FullFilePath("foo.xml"));
            xslt.Transform(xn, null, fs);
            fs.Dispose();

            if (CheckResult(383.0855503831) == 1)
                return;
            else
                Assert.True(false);
        }
    }

    /***********************************************************/
    /*          XslCompiledTransform.Load(Reader)                      */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Load(Reader) : Reader, Stream", Desc = "READER,STREAM")]
    public class CLoadReaderTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CLoadReaderTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Basic Verification Test", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void LoadXmlReader1(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            Boolean fTEST_FAIL = false;
            xslt = new XslCompiledTransform();

            XmlReaderSettings xrs = new XmlReaderSettings();
#pragma warning disable 0618
            xrs.ProhibitDtd = false;
#pragma warning restore 0618
            XmlReader xrTemp = XmlReader.Create(FullFilePath("showParam.xsl"));

            try
            {
                xslt.Load(xrTemp);
            }
            catch (Exception ex)
            {
                fTEST_FAIL = true;
                throw (ex);
            }
            finally
            {
                xrTemp.Dispose();
            }
            if (fTEST_FAIL)
                Assert.True(false);
            if (Transform("fruits.xml") == 1)
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            Assert.True(false);
        }

        //[Variation("Calling with a closed reader, should throw exception")]
        [InlineData()]
        [Theory]
        public void LoadXmlReader2()
        {
            xslt = new XslCompiledTransform();

            XmlReaderSettings xrs = new XmlReaderSettings();
#pragma warning disable 0618
            xrs.ProhibitDtd = false;
#pragma warning restore 0618
            XmlReader xrTemp = XmlReader.Create(FullFilePath("showParam.xsl"), xrs);
            xrTemp.Dispose();

            try
            {
                xslt.Load(xrTemp);
            }
            catch (System.Xml.Xsl.XsltException e)
            {
                CheckExpectedError(e, "system.data.sqlxml", "Xslt_WrongStylesheetElement", new string[] { "" });
                return;
            }
            _output.WriteLine("No exception thrown for a loading a closed reader!");
            Assert.True(false);
        }

        //[Variation("Verify Reader isn't closed after Load")]
        [InlineData()]
        [Theory]
        public void LoadXmlReader3()
        {
            Boolean fTEST_FAIL = false;
            xslt = new XslCompiledTransform();

            XmlReaderSettings xrs = new XmlReaderSettings();
#pragma warning disable 0618
            xrs.ProhibitDtd = false;
#pragma warning restore 0618
            XmlReader xrTemp = XmlReader.Create(FullFilePath("showParam.xsl"), xrs);

            try
            {
                xslt.Load(xrTemp);
            }
            catch (Exception ex)
            {
                fTEST_FAIL = true;
                throw (ex);
            }
            finally
            {
                if (!fTEST_FAIL || (xrTemp.ReadState != ReadState.Closed))
                    fTEST_FAIL = false;
                xrTemp.Dispose();
            }
            if (fTEST_FAIL)
            {
                _output.WriteLine("Appear to have accidently closed the Reader");
                Assert.True(false);
            }
            return;
        }

        //[Variation("Verify position of node in Reader is at EOF after Load")]
        [InlineData()]
        [Theory]
        public void LoadXmlReader4()
        {
            Boolean fTEST_FAIL = false;
            xslt = new XslCompiledTransform();

            XmlReaderSettings xrs = new XmlReaderSettings();
#pragma warning disable 0618
            xrs.ProhibitDtd = false;
#pragma warning restore 0618
            XmlReader xrTemp = XmlReader.Create(FullFilePath("showParam.xsl"), xrs);
            try
            {
                xslt.Load(xrTemp);
            }
            catch (Exception ex)
            {
                fTEST_FAIL = true;
                throw (ex);
            }
            finally
            {
                if (!fTEST_FAIL && (!xrTemp.EOF))
                    fTEST_FAIL = false;
                xrTemp.Dispose();
            }
            if (fTEST_FAIL)
            {
                _output.WriteLine("Reader does not appear to be at the end of file.");
                Assert.True(false);
            }
            return;
        }

        //[Variation("Load with reader position at EOF, should throw exception")]
        [InlineData()]
        [Theory]
        public void LoadXmlReader5()
        {
            Boolean fTEST_FAIL = false;
            xslt = new XslCompiledTransform();

            XmlReaderSettings xrs = new XmlReaderSettings();
#pragma warning disable 0618
            xrs.ProhibitDtd = false;
#pragma warning restore 0618
            XmlReader xrTemp = XmlReader.Create(FullFilePath("showParam.xsl"), xrs);
            xslt.Load(xrTemp);
            try
            {
                xslt.Load(xrTemp);  // should now be at end and should give exception
                fTEST_FAIL = true;
            }
            catch (System.Xml.Xsl.XsltException e)
            {
                CheckExpectedError(e, "system.data.sqlxml", "Xslt_WrongStylesheetElement", new string[] { "" });
            }
            finally
            {
                xrTemp.Dispose();
            }
            if (fTEST_FAIL)
                Assert.True(false);
            return;
        }

        //[Variation("Load with NULL reader, should throw System.ArgumentNullException")]
        [InlineData()]
        [Theory]
        public void LoadXmlReader6()
        {
            xslt = new XslCompiledTransform();

            XmlTextReader xrTemp = null;

            try
            {
                xslt.Load(xrTemp);  // should now be at end and should give exception
            }
            catch (System.ArgumentNullException)
            {
                return;
            }
            _output.WriteLine("Failed to throw System.ArgumentNullException for NULL reader input");
            Assert.True(false);
        }

        //[Variation("Basic check for usage of credentials on resolver, load XSL that needs cred. with correct resolver", Param = "XmlResolverTestMain.txt")]
        [InlineData("XmlResolverTestMain.txt")]
        [Theory]
        public void LoadXmlReader7(object param)
        {
            xslt = new XslCompiledTransform();
            string Baseline = "baseline\\" + (string)param;
            XmlReader xrLoad = XmlReader.Create(FullFilePath("XmlResolver/XmlResolverTestMain.xsl"));
            //xrLoad.XmlResolver = GetDefaultCredResolver();
            xslt.Load(xrLoad, XsltSettings.TrustedXslt, GetDefaultCredResolver());
            xrLoad.Dispose();

            if (Transform("fruits.xml") == 1)
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            Assert.True(false);
        }

        //[Variation("bug 380138 NRE during XSLT compilation")]
        [InlineData()]
        [Theory]
        public void Bug380138()
        {
            string xsl = @"<?xml version='1.0' encoding='utf-8'?>
    <xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
        xmlns:ms='urn:schemas-microsoft-com:xslt' exclude-result-prefixes='ms'>
      <xsl:template match='asf'><xsl:value-of select=""ms:namespace-uri('ms:b')""/></xsl:template>
    </xsl:stylesheet>";
            XslCompiledTransform xslt = new XslCompiledTransform();
            try
            {
                xslt.Load(XmlReader.Create(new StringReader(xsl)));
            }
            catch (NullReferenceException nre)
            {
                _output.WriteLine(nre.Message);
                Assert.True(false);
            }

            return;
        }
    }

    public class SimpleWrapperNavigator : XPathNavigator
    {
        private XPathNavigator innerNavigator;
        private XmlNameTable nt;

        public SimpleWrapperNavigator(XPathNavigator nav)
        {
            this.innerNavigator = nav;
            this.nt = new NameTable();
        }

        public SimpleWrapperNavigator(XPathNavigator nav, XmlNameTable nt)
        {
            this.innerNavigator = nav;
            this.nt = nt;
        }

        public override string BaseURI
        {
            get { return innerNavigator.BaseURI; }
        }

        public override XPathNavigator Clone()
        {
            return new SimpleWrapperNavigator(innerNavigator.Clone(), nt);
        }

        public override bool IsEmptyElement
        {
            get { return innerNavigator.IsEmptyElement; }
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            if (other is SimpleWrapperNavigator)
            {
                return innerNavigator.IsSamePosition((other as SimpleWrapperNavigator).innerNavigator);
            }
            else
            {
                return innerNavigator.IsSamePosition(other);
            }
        }

        public override string LocalName
        {
            get { return nt.Add(innerNavigator.LocalName); }
        }

        public override bool MoveTo(XPathNavigator other)
        {
            SimpleWrapperNavigator nav = other as SimpleWrapperNavigator;
            if (nav != null)
            {
                return innerNavigator.MoveTo(nav.innerNavigator);
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            return innerNavigator.MoveToFirstAttribute();
        }

        public override bool MoveToFirstChild()
        {
            return innerNavigator.MoveToFirstChild();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return innerNavigator.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToId(string id)
        {
            return innerNavigator.MoveToId(id);
        }

        public override bool MoveToNext()
        {
            return innerNavigator.MoveToNext();
        }

        public override bool MoveToNextAttribute()
        {
            return innerNavigator.MoveToNextAttribute();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return innerNavigator.MoveToNextNamespace(namespaceScope);
        }

        public override bool MoveToParent()
        {
            return innerNavigator.MoveToParent();
        }

        public override bool MoveToPrevious()
        {
            return innerNavigator.MoveToPrevious();
        }

        public override string Name
        {
            get { return innerNavigator.Name; }
        }

        public override XmlNameTable NameTable
        {
            get { return nt; }
        }

        public override string NamespaceURI
        {
            get { return nt.Add(innerNavigator.NamespaceURI); }
        }

        public override XPathNodeType NodeType
        {
            get { return innerNavigator.NodeType; }
        }

        public override string Prefix
        {
            get { return nt.Add(innerNavigator.Prefix); }
        }

        public override string Value
        {
            get { return innerNavigator.Value; }
        }
    }

    /***********************************************************/
    /*          XslCompiledTransform.Transform - Integrity     */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Reader , Reader", Desc = "READER,READER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Reader, Stream", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Reader, Writer", Desc = "READER,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Reader, TextWriter", Desc = "READER,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : URI, Reader", Desc = "URI,READER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : URI, Stream", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : URI, Writer", Desc = "URI,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : URI, TextWriter", Desc = "URI,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Navigator, Reader", Desc = "NAVIGATOR,READER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Navigator, Stream", Desc = "NAVIGATOR,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Navigator, Writer", Desc = "NAVIGATOR,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform() Integrity : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CTransformTestGeneric : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CTransformTestGeneric(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Basic Verification Test", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void TransformGeneric1(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            if ((LoadXSL("showParam.xsl") == 1) && (Transform("fruits.xml") == 1))
            {
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            else
                Assert.True(false);
        }

        //[Variation("Load and Transform multiple times", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void TransformGeneric2(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            for (int i = 0; i < 5; i++)
            {
                if ((LoadXSL("showParam.xsl") != 1) || (Transform("fruits.xml") != 1))
                    Assert.True(false);

                VerifyResult(Baseline, _strOutFile);
            }
            return;
        }

        //[Variation("Load once, Transform many times", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void TransformGeneric3(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            if (LoadXSL("showParam.xsl") == 1)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (Transform("fruits.xml") != 1)
                    {
                        _output.WriteLine("Test failed to transform after {0} iterations", i);
                        Assert.True(false);
                    }
                     VerifyResult(Baseline, _strOutFile);
                }
                return;
            }
            Assert.True(false);
        }

        //[Variation("Call Transform without loading")]
        [InlineData()]
        [Theory]
        public void TransformGeneric4()
        {
            xslt = new XslCompiledTransform();
            try
            {
                Transform("fruits.xml");
            }
            catch (System.InvalidOperationException e)
            {
                CheckExpectedError(e, "system.xml", "Xslt_NoStylesheetLoaded", new string[] { "" });
                return;
            }
            _output.WriteLine("Exception not given for a transform that didn't have a Load method instantiated");
            Assert.True(false);
        }

        //[Variation("Closing XSL and XML files used in transform, Read access")]
        [InlineData()]
        [Theory]
        public void TransformGeneric5()
        {
            FileStream s2;

            if ((LoadXSL("showParam.xsl") == 1) && (Transform("fruits.xml") == 1))
            {
                s2 = new FileStream(FullFilePath("showParam.xsl"), FileMode.Open, FileAccess.Read);
                s2.Dispose();

                s2 = new FileStream(FullFilePath("fruits.xml"), FileMode.Open, FileAccess.Read);
                s2.Dispose();

                return;
            }
            _output.WriteLine("Encountered errors performing transform and could not verify if files were closed");
            Assert.True(false);
        }

        /*
        //[Variation("Closing XSL and XML files used in transform, ReadWrite access")]
        [InlineData()]
        [Theory]
        public void TransformGeneric6()
        {
            FileStream s2;

            if((LoadXSL("showParam.xsl") == TEST_PASS) && (Transform("fruits.xml") == TEST_PASS))
            {
                s2 = new FileStream(FullFilePath("showParam.xsl"), FileMode.Open, FileAccess.ReadWrite);
                s2.Dispose();

                s2 = new FileStream(FullFilePath("fruits.xml"), FileMode.Open, FileAccess.ReadWrite);
                s2.Dispose();

                return;
            }
            _output.WriteLine("Encountered errors performing transform and could not verify if files were closed");
            Assert.True(false);
        }
        */

        //[Variation("Bug20003707 - InvalidProgramException for 2.0 stylesheets in forwards-compatible mode")]
        [InlineData()]
        [Theory]
        public void TransformGeneric7()
        {
            if (_isInProc)
                return; //TEST_SKIPPED;

            try
            {
                LoadXSL("ForwardComp.xsl");
                Transform("data.xml");
            }
            catch (XsltException e)
            {
                CheckExpectedError(e, "system.data.sqlxml", "XPath_ScientificNotation", new string[] { "" });
                return;
            }
            _output.WriteLine("XsltException (XPath_ScientificNotation) was expected");
            Assert.True(false);
        }

        //[Variation("Bug382506 - Loading stylesheet from custom navigator with enableDebug = true causes ArgumentOutOfRangeException")]
        [InlineData()]
        [Theory]
        public void TransformGeneric8()
        {
            xslt = new XslCompiledTransform();
            xslt.Load(new SimpleWrapperNavigator(new XPathDocument(FullFilePath("CustomNav.xsl")).CreateNavigator()));

            return;
        }

        //[Variation("Bug378293 - Incorrect error message when an attribute is added to a root node")]
        [InlineData()]
        [Theory]
        public void TransformGeneric9()
        {
            if (_isInProc)
                return; //TEST_SKIPPED;

            try
            {
                LoadXSL("RootNodeAtt.xsl");
                Transform("data.xml");
            }
            catch (XsltException e)
            {
                CheckExpectedError(e, "system.data.sqlxml", "XmlIl_BadXmlState", new string[] { "Attribute", "Root" });
                return;
            }
            _output.WriteLine("XslTransformException (XmlIl_BadXmlState) was expected");
            Assert.True(false);
        }

        //[Variation("Bug349757 - document() function does not work when stylesheet was loaded from a stream or reader or constructed DOM")]
        [InlineData()]
        [Theory]
        public void TransformGeneric10()
        {
            xslt = new XslCompiledTransform();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<doc xsl:version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" + "<xsl:copy-of select='document(\"test.xml\")'/>" + "</doc>");

            xslt.Load(doc, XsltSettings.TrustedXslt, new XmlUrlResolver());
            return;
        }

        //[Variation("Bug369463 - Invalid XPath exception in forward compatibility mode should render lineNumber linePosition")]
        [InlineData()]
        [Theory]
        public void TransformGeneric11()
        {
            if (_isInProc)
                return; //TEST_SKIPPED;

            try
            {
                LoadXSL("Bug369463.xsl");
                Transform("data.xml");
            }
            catch (XsltException e)
            {
                CheckExpectedError(e, "system.data.sqlxml", "XPath_UnexpectedToken", new string[] { "+" });
                return;
            }
            _output.WriteLine("XslException (XPath_UnexpectedToken) was expected");
            Assert.True(false);
        }
    }

    /*************************************************************/
    /*          XslCompiledTransform(Resolver) - Integrity       */
    /*************************************************************/

    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Reader, Reader", Desc = "READER,READER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Reader, Stream", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Reader, Writer", Desc = "READER,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Reader, TextWriter", Desc = "READER,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : URI, Reader", Desc = "URI,READER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : URI, Stream", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : URI, Writer", Desc = "URI,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : URI, TextWriter", Desc = "URI,TEXTWRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Navigator, Reader", Desc = "NAVIGATOR,READER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Navigator, Stream", Desc = "NAVIGATOR,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Navigator, Writer", Desc = "NAVIGATOR,WRITER")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlResolver) : Navigator, TextWriter", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CTransformResolverTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CTransformResolverTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Pass null XmlResolver, load style sheet with import/include, should not affect transform")]
        [InlineData()]
        [Theory]
        public void XmlResolver1()
        {
            try
            {
                if (LoadXSL("xmlResolver_main.xsl") == 1)
                {
                    if ((TransformResolver("fruits.xml", null) == 1) && (CheckResult(428.8541842246) == 1))
                        return;
                    else
                        Assert.True(false);
                }
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                Assert.True(false);
            }
            Assert.True(false);
        }

        //[Variation("Pass null XmlResolver, load style sheet with document function, should not resolve during transform", Param = "xmlResolver_document_function.txt")]
        [InlineData("xmlResolver_document_function.txt")]
        [Theory]
        public void XmlResolver2(object param)
        {
            // "xmlResolver_document_function.xsl" contains
            // <xsl:for-each select="document('xmlResolver_document_function.xml')//elem">
            string Baseline = "baseline\\" + (string)param;

            if (LoadXSL("xmlResolver_document_function.xsl") == 1)
            {
                if (TransformResolver("fruits.xml", null) == 1)
                {
                    VerifyResult(Baseline, _strOutFile);
                    return;
                }
            }
            else
            {
                _output.WriteLine("Problem loading stylesheet!");
                Assert.True(false);
            }
            Assert.True(false);
        }

        //[Variation("Default XmlResolver, load style sheet with document function, should resolve during transform", Param = "DefaultResolver.txt")]
        [InlineData("DefaultResolver.txt")]
        [Theory]
        public void XmlResolver3(object param)
        {
            // "xmlResolver_document_function.xsl" contains
            // <xsl:for-each select="document('xmlResolver_document_function.xml')//elem">
            string Baseline = "baseline\\" + (string)param;
            if (LoadXSL("xmlResolver_document_function.xsl") == 1)
            {
                if (Transform("fruits.xml") == 1)
                {
                    VerifyResult(Baseline, _strOutFile);
                    return;
                }
            }
            else
            {
                _output.WriteLine("Problem loading stylesheet with document function and default resolver!");
                Assert.True(false);
            }
            Assert.True(false);
        }

        //[Variation("document() has absolute URI")]
        [InlineData()]
        [Theory]
        public void XmlResolver5()
        {
            // copy file on the local machine
            try
            {
                string curDir = Directory.GetCurrentDirectory();
                string testFile = curDir + "\\" + "xmlResolver_document_function.xml";
                if (File.Exists(testFile))
                {
                    File.SetAttributes(testFile, FileAttributes.Normal);
                    File.Delete(testFile);
                }
                string xmlFile = FullFilePath("xmlResolver_document_function.xml");
                File.Copy(xmlFile, testFile, true);
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                _output.WriteLine("Could not copy file to local. Some other issues prevented this test from running");
                return; //TEST_SKIPPED;
            }

            if (LoadXSL("xmlResolver_document_function_absolute_uri.xsl") == 1)
            {
                if ((TransformResolver("fruits.xml", new XmlUrlResolver()) == 1) && (CheckResult(377.8217373898) == 1))
                    return;
                else
                {
                    _output.WriteLine("Failed to resolve document function with absolute URI.");
                    Assert.True(false);
                }
            }
            else
            {
                _output.WriteLine("Failed to load style sheet!");
                Assert.True(false);
            }
        }

        //[Variation("Pass null resolver but stylesheet doesn't have any include/imports")]
        [InlineData()]
        [Theory]
        public void XmlResolver7()
        {
            LoadXSL("Bug382198.xsl");
            // Pass null
            TransformResolver("fruits.xml", null);
            return;
        }
    }

    /***********************************************************/
    /*          XslCompiledTransform.Transform - (String, String)                    */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Transform(String, String) : Reader , String", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(String, String) : URI, String", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(String, String) : Navigator, String", Desc = "NAVIGATOR,STREAM")]
    public class CTransformStrStrTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CTransformStrStrTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Basic Verification Test", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void TransformStrStr1(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            String szFullFilename = FullFilePath("fruits.xml");

            if (LoadXSL("showParam.xsl") == 1)
            {
                xslt.Transform(szFullFilename, _strOutFile);
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            Assert.True(false);
        }

        //[Variation("Input is null")]
        [InlineData()]
        [Theory]
        public void TransformStrStr2()
        {
            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform(null, _strOutFile);
                }
                catch (System.ArgumentException)
                { return; }
            }
            _output.WriteLine("Exception not generated for null input filename");
            Assert.True(false);
        }

        //[Variation("Output file is null")]
        [InlineData()]
        [Theory]
        public void TransformStrStr3()
        {
            String szFullFilename = FullFilePath("fruits.xml");

            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform(szFullFilename, (string)null);
                }
                catch (System.ArgumentException)
                {
                    return;
                }
            }
            _output.WriteLine("Exception not generated for null output filename");
            Assert.True(false);
        }

        //[Variation("Input is nonexisting file")]
        [InlineData()]
        [Theory]
        public void TransformStrStr4()
        {
            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform("IDontExist.xsl", _strOutFile);
                }
                catch (System.IO.FileNotFoundException)
                {
                    return;
                }
            }
            _output.WriteLine("Exception not generated for invalid input file");
            Assert.True(false);
        }

        //[Variation("Output file is invalid")]
        [InlineData()]
        [Theory]
        public void TransformStrStr5()
        {
            String szFullFilename = FullFilePath("fruits.xml");

            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform(szFullFilename, szInvalid);
                }
                catch (System.ArgumentException)
                {
                    return;
                }
            }
            _output.WriteLine("Exception not generated for invalid output destination");
            Assert.True(false);
        }

        //[Variation("Input is empty string")]
        [InlineData()]
        [Theory]
        public void TransformStrStr6()
        {
            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform(szEmpty, _strOutFile);
                }
                catch (System.ArgumentException)
                {
                    return;
                }
            }
            _output.WriteLine("Exception not generated for empty string input file");
            Assert.True(false);
        }

        //[Variation("Output file is empty string")]
        [InlineData()]
        [Theory]
        public void TransformStrStr7()
        {
            String szFullFilename = FullFilePath("fruits.xml");

            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform(szFullFilename, szEmpty);
                }
                catch (System.ArgumentException)
                {
                    return;
                }
            }
            _output.WriteLine("Exception not generated for empty output file name");
            Assert.True(false);
        }

        //[Variation("Call Transform many times", Param = "showParam.txt")]
        [InlineData("showParam.txt")]
        [Theory]
        public void TransformStrStr8(object param)
        {
            string Baseline = "baseline\\" + (string)param;
            String szFullFilename = FullFilePath("fruits.xml");

            for (int i = 0; i < 50; i++)
            {
                if (LoadXSL("showParam.xsl") == 1)
                {
                    xslt.Transform(szFullFilename, _strOutFile);
                    VerifyResult(Baseline, _strOutFile);
                    //_output.WriteLine("Failed to process Load after calling {0} times", i);
                }
            }
            return;
        }

        //[Variation("Call without loading")]
        [InlineData()]
        [Theory]
        public void TransformStrStr9()
        {
            xslt = new XslCompiledTransform();
            SetExpectedError("Xslt_NoStylesheetLoaded");
            try
            {
                xslt.Transform(FullFilePath("fruits.xml"), _strOutFile);
            }
            catch (System.InvalidOperationException e)
            {
                CheckExpectedError(e, "System.xml", "Xslt_NoStylesheetLoaded", new string[] { "" });
                return;
            }
            _output.WriteLine("Exception attempting a transform without loading an XSL file");
            Assert.True(false);
        }

        //[Variation("Output to unreachable destination")]
        [InlineData()]
        [Theory]
        public void TransformStrStr10()
        {
            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform("fruits.xml", "http://www.IdontExist.com/index.xml");
                }
                catch (System.Exception e)
                {
                    _output.WriteLine(e.ToString());
                    return;
                }
            }
            _output.WriteLine("Exception not generated for invalid output destination");
            Assert.True(false);
        }

        //[Variation("Input filename is \'.\', \'..\', and \'\\\\\'")]
        [InlineData()]
        [Theory]
        public void TransformStrStr11()
        {
            int iCount = 0;
            if (LoadXSL("showParam.xsl") == 1)
            {
                SetExpectedError("Xml_ResolveUrl");
                try
                {
                    xslt.Transform("..", _strOutFile);
                }
                catch (System.Exception)
                {
                    iCount++;
                }

                try
                {
                    xslt.Transform(".", _strOutFile);
                }
                catch (System.Exception)
                {
                    iCount++;
                }

                try
                {
                    xslt.Transform("\\\\", _strOutFile);
                }
                catch (System.Exception)
                {
                    iCount++;
                }
            }

            if (iCount.Equals(3))
                return;

            _output.WriteLine("Exception not generated for invalid input sources");
            Assert.True(false);
        }

        //[Variation("Output filename is \'.\', \'..\', and \'\\\\\'")]
        [InlineData()]
        [Theory]
        public void TransformStrStr12()
        {
            String szFullFilename = FullFilePath("fruits.xml");
            int iCount = 0;
            if (LoadXSL("showParam.xsl") == 1)
            {
                try
                {
                    xslt.Transform(szFullFilename, "..");
                }
                catch (System.Exception)
                {
                    iCount++;
                }

                try
                {
                    xslt.Transform(szFullFilename, ".");
                }
                catch (System.Exception)
                {
                    iCount++;
                }

                try
                {
                    xslt.Transform(szFullFilename, "\\\\");
                }
                catch (System.Exception)
                {
                    iCount++;
                }
            }

            if (iCount.Equals(3))
                return;
            _output.WriteLine("Exception not generated for invalid ouput destinations");
            Assert.True(false);
        }

        //[Variation("Closing files after transform")]
        [InlineData()]
        [Theory]
        public void TransformStrStr13()
        {
            String szFullFilename = FullFilePath("fruits.xml");
            Stream strmTemp;

            if (LoadXSL("showParam.xsl") == 1)
            {
                xslt.Transform(szFullFilename, _strOutFile);
                StreamReader fs = null;

                // check if I can open and close the xml file
                fs = new StreamReader(new FileStream(szFullFilename, FileMode.Open, FileAccess.Read));
                fs.Dispose();

                strmTemp = new FileStream(szFullFilename, FileMode.Open, FileAccess.Read);
                strmTemp.Dispose();

                // check if I can open and close the output file
                fs = new StreamReader(new FileStream(_strOutFile, FileMode.Open, FileAccess.Read));
                fs.Dispose();

                strmTemp = new FileStream(_strOutFile, FileMode.Open, FileAccess.Read);
                strmTemp.Dispose();

                return;
            }
            Assert.True(false);
        }

        /*
        //[Variation("Transform(test.xml, test.xml)")]
        [InlineData()]
        [Theory]
        public void TransformStrStr14()
        {
            String szFullFilename = FullFilePath("Bug75295.xml");

            // Copy this file to current directory
            File.Delete("out.xml");
            File.Copy(szFullFilename, "out.xml");

            if(LoadXSL("Bug75295.xsl") == TEST_PASS)
            {
                xslt.Transform("out.xml", "out.xml");

                if (CheckResult(270.5223692973) == TEST_PASS)
                    return;
            }
            Assert.True(false);
        }
        */
    }

    /***********************************************************/
    /*          XslCompiledTransform.Transform - (String, String, Resolver)          */
    /***********************************************************/

    //[TestCase(Name = "XslCompiledTransform.Transform(String, String, Resolver) : Reader , String", Desc = "READER,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(String, String, Resolver) : URI, String", Desc = "URI,STREAM")]
    //[TestCase(Name = "XslCompiledTransform.Transform(String, String, Resolver) : Navigator, String", Desc = "NAVIGATOR,STREAM")]
    public class CTransformStrStrResolverTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CTransformStrStrResolverTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Pass null XmlResolver to Transform, load style sheet with import/include, should not affect transform")]
        [InlineData()]
        [Theory]
        public void TransformStrStrResolver1()
        {
            String szFullFilename = FullFilePath("fruits.xml");
            try
            {
                if (LoadXSL("xmlResolver_main.xsl", new XmlUrlResolver()) == 1)
                {
                    XmlTextReader xr = new XmlTextReader(szFullFilename);
                    XmlTextWriter xw = new XmlTextWriter("out.xml", Encoding.Unicode);
                    xslt.Transform(xr, null, xw, null);
                    xr.Dispose();
                    xw.Dispose();
                    if (CheckResult(403.7784431795) == 1)
                        return;
                    else
                        Assert.True(false);
                }
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
                Assert.True(false);
            }
            Assert.True(false);
        }

        //[Variation("Pass null XmlResolver, load style sheet with document function, should not resolve during transform")]
        [InlineData()]
        [Theory]
        public void TransformStrStrResolver2()
        {
            // "xmlResolver_document_function.xsl" contains
            // <xsl:for-each select="document('xmlResolver_document_function.xml')//elem">

            String szFullFilename = FullFilePath("fruits.xml");

            if (LoadXSL("xmlResolver_document_function.xsl") == 1)
            {
                xslt.Transform(szFullFilename, "out.xml");
                if (CheckResult(422.3877210723) == 1)
                    return;
            }
            else
            {
                _output.WriteLine("Problem loading stylesheet!");
                Assert.True(false);
            }
            Assert.True(false);
        }

        //[Variation("Pass XmlUrlResolver, load style sheet with document function, should resolve during transform", Param = "xmlResolver_document_function.txt")]
        [InlineData("xmlResolver_document_function.txt")]
        [Theory]
        public void TransformStrStrResolver3(object param)
        {
            // "xmlResolver_document_function.xsl" contains
            // <xsl:for-each select="document('xmlResolver_document_function.xml')//elem">

            String szFullFilename = FullFilePath("fruits.xml");
            string Baseline = "baseline\\" + (string)param;

            if (LoadXSL("xmlResolver_document_function.xsl") == 1)
            {
                xslt.Transform(szFullFilename, "out.xml");
                VerifyResult(Baseline, _strOutFile);
                return;
            }
            else
            {
                _output.WriteLine("Problem loading stylesheet with document function and default resolver!");
                Assert.True(false);
            }
            Assert.True(false);
        }
    }

    //[TestCase(Name = "XslCompiledTransform.Transform(IXPathNavigable, XsltArgumentList, XmlWriter, XmlResolver)", Desc = "Constructor Tests", Param = "IXPathNavigable")]
    //[TestCase(Name = "XslCompiledTransform.Transform(XmlReader, XsltArgumentList, XmlWriter, XmlResolver)", Desc = "Constructor Tests", Param = "XmlReader")]
    public class CTransformConstructorWihtFourParametersTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CTransformConstructorWihtFourParametersTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        internal class CustomXmlResolver : XmlUrlResolver
        {
            private string baseUri;

            public CustomXmlResolver(string baseUri)
            {
                this.baseUri = baseUri;
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                if (baseUri == null)
                    return base.ResolveUri(new Uri(this.baseUri), relativeUri);
                return base.ResolveUri(baseUri, relativeUri);
            }
        }

        //[Variation("Document function 1, CustomXmlResolver", Pri = 0, Params = new object[] { "xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "CustomXmlResolver", true })]
        [InlineData("xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "CustomXmlResolver", true, "IXPathNavigable")]
        [InlineData("xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "CustomXmlResolver", true, "XmlReader")]
        //[Variation("Import/Include, CustomXmlResolver", Pri = 0, Params = new object[] { "xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "CustomXmlResolver", true })]
        //[InlineData("xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "CustomXmlResolver", true, "IXPathNavigable")]   //Skipping this test as resolving external URIs is no longer allowed
        //[InlineData("xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "CustomXmlResolver", true, "XmlReader")]   //Skipping this test as resolving external URIs is no longer allowed
        //[Variation("No Import/Include, CustomXmlResolver", Pri = 0, Params = new object[] { "Bug382198.xsl", "fruits.xml", "Bug382198.txt", "CustomXmlResolver", true })]
        [InlineData("Bug382198.xsl", "fruits.xml", "Bug382198.txt", "CustomXmlResolver", true, "IXPathNavigable")]
        [InlineData("Bug382198.xsl", "fruits.xml", "Bug382198.txt", "CustomXmlResolver", true, "XmlReader")]
        //[Variation("Document function 1, XmlUrlResolver", Pri = 0, Params = new object[] { "xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "XmlUrlResolver", true })]
        [InlineData("xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "XmlUrlResolver", true, "IXPathNavigable")]
        [InlineData("xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "XmlUrlResolver", true, "XmlReader")]
        //[Variation("Import/Include, XmlUrlResolver", Pri = 0, Params = new object[] { "xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "XmlUrlResolver", true })]
        [InlineData("xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "XmlUrlResolver", true, "IXPathNavigable")]
        [InlineData("xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "XmlUrlResolver", true, "XmlReader")]
        //[Variation("No Import/Include, XmlUrlResolver", Pri = 0, Params = new object[] { "Bug382198.xsl", "fruits.xml", "Bug382198.txt", "XmlUrlResolver", true })]
        [InlineData("Bug382198.xsl", "fruits.xml", "Bug382198.txt", "XmlUrlResolver", true, "IXPathNavigable")]
        [InlineData("Bug382198.xsl", "fruits.xml", "Bug382198.txt", "XmlUrlResolver", true, "XmlReader")]
        //[Variation("Document function 1, NullResolver", Pri = 0, Params = new object[] { "xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "NullResolver", false })]
        [InlineData("xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "NullResolver", false, "IXPathNavigable")]
        [InlineData("xmlResolver_document_function.xsl", "fruits.xml", "xmlResolver_document_function.txt", "NullResolver", false, "XmlReader")]
        //[Variation("Import/Include, NullResolver", Pri = 0, Params = new object[] { "xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "NullResolver", false })]
        //[InlineData("xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "NullResolver", false, "IXPathNavigable")]   //Skipping this test as resolving external URIs is no longer allowed
        //[InlineData("xmlResolver_main.xsl", "fruits.xml", "xmlResolver_main.txt", "NullResolver", false, "XmlReader")]   //Skipping this test as resolving external URIs is no longer allowed
        //[Variation("No Import/Include, NullResolver", Pri = 0, Params = new object[] { "Bug382198.xsl", "fruits.xml", "Bug382198.txt", "NullResolver", true })]
        [InlineData("Bug382198.xsl", "fruits.xml", "Bug382198.txt", "NullResolver", true, "IXPathNavigable")]
        [InlineData("Bug382198.xsl", "fruits.xml", "Bug382198.txt", "NullResolver", true, "XmlReader")]
        [Theory]
        public void ValidCases(object param0, object param1, object param2, object param3, object param4, object param5)
        {
            string xslFile = FullFilePath(param0 as string);
            string xmlFile = FullFilePath(param1 as string);
            string baseLineFile = @"\baseline\" + param2 as string;
            bool expectedResult = (bool)param4;
            bool actualResult = false;

            XmlReader xmlReader = XmlReader.Create(xmlFile);
            //Let's select randomly how to create navigator
            IXPathNavigable navigator = null;
            Random randGenerator = new Random((int)DateTime.Now.Ticks);
            switch (randGenerator.Next(2))
            {
                case 0:
                    _output.WriteLine("Using XmlDocument.CreateNavigator()");
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlFile);
                    navigator = xmlDoc.CreateNavigator();
                    break;

                case 1:
                    _output.WriteLine("Using XPathDocument.CreateNavigator()");
                    XPathDocument xpathDoc;
                    using (XmlReader reader = XmlReader.Create(xmlFile))
                    {
                        xpathDoc = new XPathDocument(reader);
                        navigator = xpathDoc.CreateNavigator();
                    }
                    break;

                default:
                    break;
            }

            XmlResolver resolver = null;
            switch (param3 as string)
            {
                case "NullResolver":
                    break;

                case "XmlUrlResolver":
                    resolver = new XmlUrlResolver();
                    break;

                case "CustomXmlResolver":
                    resolver = new CustomXmlResolver(Path.Combine(FilePathUtil.GetTestDataPath(), @"XsltApiV2\"));
                    break;

                default:
                    break;
            }

            try
            {
                XslCompiledTransform localXslt = new XslCompiledTransform();
                XsltSettings settings = new XsltSettings(true, true);
                using (XmlReader xslReader = XmlReader.Create(xslFile))
                    localXslt.Load(xslReader, settings, resolver);

                using (XmlWriter writer = XmlWriter.Create("outputFile.txt"))
                {
                    if (param5 as string == "XmlReader")
                        localXslt.Transform(xmlReader, null, writer, resolver);
                    else
                        localXslt.Transform(navigator, null, writer, resolver);
                }
                VerifyResult(baseLineFile, "outputFile.txt");
                actualResult = true;
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
                actualResult = false;
            }

            if (actualResult != expectedResult)
                Assert.True(false);

            return;
        }

        //[Variation("Invalid Arguments: null, valid, valid, valid", Pri = 0, Params = new object[] { 1, false })]
        [InlineData(1, false, "IXPathNavigable")]
        [InlineData(1, false, "XmlReader")]
        //[Variation("Invalid Arguments: valid, null, valid, valid", Pri = 0, Params = new object[] { 2, true })]
        [InlineData(2, true, "IXPathNavigable")]
        [InlineData(2, false, "XmlReader")]
        //[Variation("Invalid Arguments: valid, valid, null, valid", Pri = 0, Params = new object[] { 3, false })]
        [InlineData(3, false, "IXPathNavigable")]
        [InlineData(3, false, "XmlReader")]
        //[Variation("Invalid Arguments: valid, valid, valid, null", Pri = 0, Params = new object[] { 4, true })]
        [InlineData(4, true, "IXPathNavigable")]
        [InlineData(4, true, "XmlReader")]
        [Theory]
        public void InValidCases(object param0, object param1, object param2)
        {
            int argumentNumber = (int)param0;
            bool expectedResult = (bool)param1;
            bool actualResult = false;

            XslCompiledTransform localXslt = new XslCompiledTransform();
            string stylesheet = @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" />";
            using (XmlReader xslReader = XmlReader.Create(new StringReader(stylesheet)))
            {
                localXslt.Load(xslReader);
            }

            string xmlString = "<root />";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            XmlReader xmlReader = XmlReader.Create(new StringReader(xmlString));
            XPathNavigator nav = xmlDoc.CreateNavigator();
            object[] testInput = new object[] { xmlReader, nav, new XsltArgumentList(), XmlWriter.Create(Stream.Null), new XmlUrlResolver() };
            if (argumentNumber == 1)
                testInput[0] = null;
            testInput[argumentNumber] = null;

            try
            {
                if (param2 as string == "XmlReader")
                    localXslt.Transform(testInput[0] as XmlReader, testInput[2] as XsltArgumentList, testInput[3] as XmlWriter, testInput[4] as XmlResolver);
                else
                    localXslt.Transform(testInput[1] as IXPathNavigable, testInput[2] as XsltArgumentList, testInput[3] as XmlWriter, testInput[4] as XmlResolver);
                actualResult = true;
            }
            catch (ArgumentNullException ex)
            {
                _output.WriteLine(ex.Message);
                actualResult = false;
            }

            if (actualResult != expectedResult)
                Assert.True(false);

            return;
        }
    }

    // This testcase is for bugs 109429, 111075 and 109644 fixed in Everett SP1
    //[TestCase(Name = "NDP1_1SP1 Bugs (URI,STREAM)", Desc = "URI,STREAM")]
    //[TestCase(Name = "NDP1_1SP1 Bugs (NAVIGATOR,TEXTWRITER)", Desc = "NAVIGATOR,TEXTWRITER")]
    public class CNDP1_1SP1Test : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CNDP1_1SP1Test(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //[Variation("Local parameter gets overwritten with global param value", Pri = 1)]
        [InlineData()]
        [Theory]
        public void var1()
        {
            m_xsltArg = new XsltArgumentList();
            m_xsltArg.AddParam("param1", string.Empty, "global-param1-arg");

            if ((LoadXSL("paramScope.xsl") == 1) && (Transform_ArgList("fruits.xml") == 1) && (CheckResult(473.4644857331) == 1))
                return;
            else
                Assert.True(false);
        }

        //[Variation("Local parameter gets overwritten with global variable value", Pri = 1)]
        [InlineData()]
        [Theory]
        public void var2()
        {
            m_xsltArg = new XsltArgumentList();
            m_xsltArg.AddParam("param1", string.Empty, "global-param1-arg");

            if ((LoadXSL("varScope.xsl") == 1) && (Transform_ArgList("fruits.xml") == 1) && (CheckResult(473.4644857331) == 1))
                return;
            else
                Assert.True(false);
        }

        //[Variation("Subclassed XPathNodeIterator returned from an extension object or XsltFunction is not accepted by XPath", Pri = 1)]
        [InlineData()]
        [Theory]
        public void var3()
        {
            m_xsltArg = new XsltArgumentList();
            m_xsltArg.AddExtensionObject("http://foo.com", new MyXsltExtension());

            if ((LoadXSL("Bug111075.xsl") == 1) && (Transform_ArgList("Bug111075.xml") == 1) && (CheckResult(441.288076277) == 1))
                return;
            else
                Assert.True(false);
        }

        //[Variation("Iterator using for-each over a variable is not reset correctly while using msxsl:node-set()", Pri = 1)]
        [InlineData()]
        [Theory]
        public void var4()
        {
            if ((LoadXSL("Bug109644.xsl") == 1) && (Transform("foo.xml") == 1) && (CheckResult(417.2501860011) == 1))
                return;
            else
                Assert.True(false);
        }
    }

    //[TestCase(Name = "XslCompiledTransform Regression Tests for API", Desc = "XslCompiledTransform Regression Tests")]
    public class CTransformRegressionTest : XsltApiTestCaseBase2
    {
        private ITestOutputHelper _output;
        public CTransformRegressionTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
        }

        //BinCompat TODO: Add this test back
        ////[Variation("Bug398968 - Globalization is broken for document() function")]
        //[InlineData()]
        //[Theory]
        //public void RegressionTest1()
        //{
        //    // <SQL BU Defect Tracking 410060>
        //    if (_isInProc)
        //        return; //TEST_SKIPPED;
        //    // </SQL BU Defect Tracking 410060>

        //    string testFile = "Stra\u00DFe.xml";

        //    // Create the file.
        //    using (FileStream fs = File.Open(testFile, FileMode.Open))
        //    {
        //        Byte[] info = new UTF8Encoding(true).GetBytes("<PASSED/>");
        //        fs.Write(info, 0, info.Length);
        //    }

        //    LoadXSL("398968repro.xsl");
        //    Transform("data.xml");
        //    return;
        //}

        //BinCompat TODO: Add this test back
        ////[Variation("Bug410158 - Debug flag on XslCompiledTransform contaminates XsltSettings")]
        //[InlineData()]
        //[Theory]
        //public void RegressionTest2()
        //{
        //    // <SQL BU Defect Tracking 410060>
        //    // This test uses Reflection, which doenst work inproc
        //    if (_isInProc)
        //        return; //TEST_SKIPPED;
        //    // </SQL BU Defect Tracking 410060>

        //    string stylesheet = "<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>";
        //    XsltSettings settings = new XsltSettings();
        //    XmlResolver resolver = new XmlUrlResolver();

        //    int debuggableCnt1 = 0;
        //    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        string asmName = asm.GetName().Name;
        //        if (asmName.StartsWith("System.Xml.Xsl.CompiledQuery.", StringComparison.Ordinal))
        //        {
        //            IEnumerable<Attribute> debuggableAttrs = asm.GetCustomAttributes(typeof(DebuggableAttribute));
        //            if (debuggableAttrs.Count() > 0 && ((DebuggableAttribute)debuggableAttrs.First()).IsJITTrackingEnabled)
        //            {
        //                debuggableCnt1++;
        //            }
        //        }
        //    }
        //    _output.WriteLine("Number of debuggable XSLT stylesheets loaded (before load): " + debuggableCnt1);

        //    XslCompiledTransform xct1 = new XslCompiledTransform(true);
        //    xct1.Load(XmlReader.Create(new StringReader(stylesheet)), settings, resolver);

        //    XslCompiledTransform xct2 = new XslCompiledTransform(false);
        //    xct2.Load(XmlReader.Create(new StringReader(stylesheet)), settings, resolver);

        //    int debuggableCnt2 = 0;
        //    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        string asmName = asm.GetName().Name;
        //        if (asmName.StartsWith("System.Xml.Xsl.CompiledQuery.", StringComparison.Ordinal))
        //        {
        //            IEnumerable<Attribute> debuggableAttrs = asm.GetCustomAttributes(typeof(DebuggableAttribute));
        //            if (debuggableAttrs.Count() > 0 && ((DebuggableAttribute)debuggableAttrs.First()).IsJITTrackingEnabled)
        //            {
        //                debuggableCnt2++;
        //            }
        //        }
        //    }
        //    _output.WriteLine("Number of debuggable XSLT stylesheets loaded (after load): " + debuggableCnt2);

        //    if (debuggableCnt2 - debuggableCnt1 == 1)
        //        return;
        //    else
        //        Assert.True(false);
        //}

        //[Variation("Bug412703 - Off-by-one errors for XSLT loading error column")]
        [InlineData()]
        [Theory]
        public void RegressionTest3()
        {
            try
            {
                LoadXSL("bug370868.xsl");
            }
            catch (System.Xml.Xsl.XsltException e)
            {
                // Should be 3,2
                if (e.LineNumber == 3 && e.LinePosition == 2)
                    return;
                else
                    _output.WriteLine("412703: LineNumber and position were incorrect. Expected {0}, {1}. Actual {2}, {3}", 3, 2, e.LineNumber, e.LinePosition);
            }
            Assert.True(false);
        }

        //[Variation("Bug423641 - XslCompiledTransform.Load() [retail] throws a NullReferenceException when scripts are prohibited")]
        [InlineData()]
        [Theory]
        public void RegressionTest4()
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            // Should not throw
            xslt.Load(FullFilePath("XSLTFilewithscript.xslt"), XsltSettings.Default, new XmlUrlResolver());
            return;
        }

        //[Variation("Bug423641 - XslCompiledTransform.Load() [debug] throws a NullReferenceException when scripts are prohibited")]
        [InlineData()]
        [Theory]
        public void RegressionTest5()
        {
            XslCompiledTransform xslt = new XslCompiledTransform(true);
            // Should not throw
            xslt.Load(FullFilePath("XSLTFilewithscript.xslt"), XsltSettings.Default, new XmlUrlResolver());
            return;
        }

        // TODO: DENY is deprecated
        ////[Variation("Bug429365 & SRZ070601000889 - XslCompiledTransform.Transform() fails on x64 when caller does not have UnmanagedCode privilege")]
        //[InlineData()]
        //public int RegressionTest6()
        //{
        //    // Should not throw
        //    ExecuteTransform();
        //    return;
        //}

        //[Variation("Bug469781 - Replace shouldn't relax original type 'assertion failure'")]
        [InlineData()]
        [Theory]
        public void RegressionTest7()
        {
            string xslString = "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:user=\"urn:user\">"
                + "<xsl:template match=\"/\">"
                + "<xsl:variable name=\"foo\"/>"
                + "<xsl:for-each select=\"user:func()\">"
                + "<xsl:variable name=\"bar\"/>"
                + "<xsl:if test=\"self::node()[0]\"/>"
                + "</xsl:for-each>"
                + "</xsl:template>"
                + "</xsl:stylesheet>";

            try
            {
                XmlReader r = XmlReader.Create(new StringReader(xslString));
                XslCompiledTransform trans = new XslCompiledTransform();
                trans.Load(r);
            }
            catch (XsltException e)
            {
                _output.WriteLine(e.ToString());
                Assert.True(false);
            }
            return;
        }

        //[Variation("Bug737816 - Dynamic method will have declaring type == null")]
        [InlineData()]
        [Theory]
        public void RegressionTest8()
        {
            try
            {
                DynamicMethod hello = new DynamicMethod("Hello",
                                typeof(int),
                                new Type[] { },
                                typeof(string).GetTypeInfo().Module);

                ILGenerator il = hello.GetILGenerator(256);
                il.Emit(OpCodes.Ret);

                // Load into XslCompiledTransform
                var xslt = new XslCompiledTransform();
                xslt.Load(hello, new byte[] { }, new Type[] { });

                // Run the transformation
                Stream res = new MemoryStream();
                xslt.Transform(XmlReader.Create(new StringReader("<Root><Price>9.50</Price></Root>")), (XsltArgumentList)null, res);
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (Exception e)
            {
                _output.WriteLine(e.ToString());
            }

            Assert.True(false);
        }

        //[SecurityPermission(SecurityAction.Deny, UnmanagedCode = true)]
        //private void ExecuteTransform()
        //{
        //    XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
        //    xslCompiledTransform.Load(FullFilePath("xslt_sortnames.xsl"));

        //    MemoryStream ms = new MemoryStream();
        //    StreamReader stmRead = new StreamReader(ms, true);
        //    xslCompiledTransform.Transform(FullFilePath("xslt_names.xml"), null, ms);
        //    ms.Seek(0, new SeekOrigin());
        //    string foo = stmRead.ReadToEnd();
        //    _output.WriteLine(foo);
        //}
    }

    internal class MyArrayIterator : XPathNodeIterator
    {
        protected ArrayList array;
        protected int index;

        public MyArrayIterator(ArrayList array)
        {
            this.array = array;
            this.index = 0;
        }

        public MyArrayIterator(MyArrayIterator it)
        {
            this.array = it.array;
            this.index = it.index;
        }

        public override XPathNodeIterator Clone()
        {
            return new MyArrayIterator(this);
        }

        public override bool MoveNext()
        {
            if (index < array.Count)
            {
                index++;
                return true;
            }
            return false;
        }

        public override XPathNavigator Current
        {
            get
            {
                return (index > 0) ? (XPathNavigator)array[index - 1] : null;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return index;
            }
        }

        public override int Count
        {
            get
            {
                return array.Count;
            }
        }

        public void Reset()
        {
            index = 0;
        }

        // BUGBUG: DCR 104760
        public override IEnumerator GetEnumerator()
        {
            MyArrayIterator it = (MyArrayIterator)this.Clone();
            it.Reset();
            return it.array.GetEnumerator();
        }
    }

    internal class MyXsltExtension
    {
        public XPathNodeIterator distinct(XPathNodeIterator nodeset)
        {
            Hashtable nodelist = new Hashtable();
            while (nodeset.MoveNext())
            {
                if (!nodelist.Contains(nodeset.Current.Value))
                {
                    nodelist.Add(nodeset.Current.Value, nodeset.Current);
                }
            }
            return new MyArrayIterator(new ArrayList(nodelist.Values));
        }
    }
}