using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArchiveLibrary;

namespace ArchiveLibraryTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(FailureToArchiveException))]
        public void TestMethod1()
        {
            Archiving.VerifyArchiveResult("", null);
        }
        [TestMethod]
        [ExpectedException(typeof(FailureToArchiveException))]
        public void TestMethod2()
        {
            Archiving.VerifyArchiveResult("", "http://archive.is/submit/");
        }
    }
}
