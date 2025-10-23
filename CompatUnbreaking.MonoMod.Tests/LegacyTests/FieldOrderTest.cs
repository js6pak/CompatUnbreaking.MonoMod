﻿using Xunit;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

namespace MonoMod.UnitTest {
    public class FieldOrderTest {
        [Fact]
        public void TestFieldOrder() {
            // We could hardcode the field metadata tokens but ehhh, compilers.
            // DynamicMethodDefinition will call ResolveField *and* FixReflectionCache for us.
            new DynamicMethodDefinition(typeof(FieldOrderTest).GetMethod("DummyMethod", BindingFlags.NonPublic | BindingFlags.Static)).Dispose();

            Type dummy = typeof(FieldOrderTest).GetNestedType("DummyClass", BindingFlags.NonPublic);
            Assert.True(dummy.GetFields().SequenceEqual(dummy.GetFields()), "dummy.GetFields() isn't consistent");
#if NETFRAMEWORK
            Assert.Equal(dummy.GetField("A"), dummy.GetFields()[0]);
            Assert.Equal(dummy.GetField("B"), dummy.GetFields()[1]);
            Assert.Equal(dummy.GetField("C"), dummy.GetFields()[2]);
#endif
        }

        private static void DummyMethod() => new DummyClass().B = 3;

        private class DummyClass {
            public string A;
            public int B;
            public string C;
        }

    }
}
