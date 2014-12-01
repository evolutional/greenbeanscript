using System;
using System.IO;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using GreenBeanScript.Libs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GreenBeanScript.Tests
{
    [TestClass]
    [UseReporter(typeof (DiffReporter))]
    public class LegacyApprovalTests
    {
        private void Do(StringBuilder log, string libFileName)
        {
            Action<string> printFunc = (s) =>
            {
                log.AppendLine(s);
            };
            var stdLib = new StdLibrary(printFunc);
            var uut = new Machine(stdLib);
            var newLib = new Library();
            newLib.LoadFromFile(uut, libFileName);

            uut.ExecuteLibrary(newLib);
        }

        private void List(StringBuilder log, string libFileName)
        {
            var newLib = new Library();
            newLib.ListLibraryFromFile(log, libFileName);
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void CompiledHeapsort()
        {
            var log = new StringBuilder();
            Do(log, @"./ExampleScripts/heapsort.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void CompiledFib()
        {
            var log = new StringBuilder();
            Do(log, @"./ExampleScripts/fib.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void CompiledMatrix()
        {
            var log = new StringBuilder();
            Do(log, @"./ExampleScripts/matrix.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void CompiledHash()
        {
            var log = new StringBuilder();
            Do(log, @"./ExampleScripts/hash.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void CompiledFloat()
        {
            var log = new StringBuilder();
            Do(log, @"./ExampleScripts/float.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void ListCompiledFib()
        {
            var log = new StringBuilder();
            List(log, @"./../../ExampleScripts/fib.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void ListCompiledMatrix()
        {
            var log = new StringBuilder();
            List(log, @"./../../ExampleScripts/matrix.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void ListCompiledHeapsort()
        {
            var log = new StringBuilder();
            List(log, @"./../../ExampleScripts/heapsort.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void ListCompiledHash()
        {
            var log = new StringBuilder();
            List(log, @"./../../ExampleScripts/hash.gml");
            Approvals.Verify(log.ToString());
        }

        [UseReporter(typeof(DiffReporter))]
        [TestMethod]
        public void ListCompiledFloat()
        {
            var log = new StringBuilder();
            List(log, @"./../../ExampleScripts/float.gml");
            Approvals.Verify(log.ToString());
        }
    }
}
