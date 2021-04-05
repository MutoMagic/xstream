using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Xstream.Test
{
    [TestClass]
    class TestNative
    {
        [Flags]
        enum CBool : int { False, True }

        [TestMethod]
        public void TestCBool()
        {
            Assert.IsFalse(Native.CBool(CBool.True));
            Assert.IsTrue(Native.CBool(CBool.False));
            Assert.IsTrue(Native.CBool((CBool)0x0F00));
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException), "Null is supposed to be an illegal parameter")]
        public void TestCBoolDefault() => Native.CBool((Enum)null);

        [TestMethod]
        public void TestCBoolConvert()
        {
            Assert.IsTrue(Native.CBool<sbyte>(true) == 1);
            Assert.IsTrue(Native.CBool<byte>(true) == 1);
            Assert.IsTrue(Native.CBool<short>(true) == 1);
            Assert.IsTrue(Native.CBool<ushort>(true) == 1);
            Assert.IsTrue(Native.CBool<int>(true) == 1);
            //Assert.IsTrue(Native.CBool<uint>(true) == 1);
            Assert.IsTrue(Native.CBool<long>(true) == 1);
            Assert.IsTrue(Native.CBool<ulong>(true) == 1);

            Assert.IsTrue(Native.CBool(true) == 1);
            Assert.IsTrue(Native.CBool(false) == 0);
        }
    }
}
