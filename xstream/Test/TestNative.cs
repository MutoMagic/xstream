using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xstream.Test
{
    [TestClass]
    class TestNative
    {
        [Flags]
        enum ECBool : int { A, B, C = -1 }

        [TestMethod]
        public void TestCBool()
        {
            Assert.IsFalse(Native.CBool(ECBool.A));
            Assert.IsTrue(Native.CBool(ECBool.B));
            Assert.IsTrue(Native.CBool(ECBool.C));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Null is supposed to be an illegal parameter")]
        public void TestCBoolDefault() => Native.CBool(null);

        [TestMethod]
        public void TestCBoolConvert()
        {
            Assert.IsTrue(Native.CBool<uint>(true) == 1);
            Assert.IsTrue(Native.CBool<int>(false) == 0);
        }
    }
}
