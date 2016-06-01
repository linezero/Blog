using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NETCoreTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void TestMethodPassing()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMethodFailing()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void TestStringEqual()
        {
            var blogname = "linezero";
            Assert.AreEqual(blogname,"LineZero");
        }
    }
}
