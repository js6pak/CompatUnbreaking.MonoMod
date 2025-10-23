﻿using Xunit;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoMod.UnitTest {
    public class ModInteropTest {
        [Fact]
        public void TestModInterop() {
            typeof(UtilsC).ModInterop();
            typeof(UtilsD).ModInterop();

            typeof(UtilsA).ModInterop();
            Assert.Equal(UtilsA.Something(2, 3),     UtilsC.Something(2, 3));
            Assert.Equal(UtilsA.AnotherThing(2, 3),  UtilsC.AnotherThing(2, 3));

            typeof(UtilsB).ModInterop();
            Assert.Equal(UtilsA.Something(2, 3),     UtilsC.Something(2, 3));
            Assert.Equal(UtilsB.AnotherThing(2, 3),  UtilsC.AnotherThing(2, 3));

            Assert.Equal(UtilsB.Something(2, 3),     UtilsD.Something(2, 3));
            Assert.Equal(UtilsB.AnotherThing(2, 3),  UtilsD.AnotherThing(2, 3));
        }

        [ModExportName("ModA")]
        public static class UtilsA {

            public static int Something(int a, int b) {
                return a + b;
            }

            public static int AnotherThing(int a, int b) {
                return a;
            }

        }

        [ModExportName("ModB")]
        public static class UtilsB {

            public static int Something(int a, int b) {
                return a * b;
            }

            public static int AnotherThing(int a, int b) {
                return b;
            }

        }

        [ModExportName("ModC")]
        public static class UtilsC {

            // Simple use case: Get the first registered "Something".
            public static Func<int, int, int> Something;

            // More complicated use case: Get AnotherThing, apply it on a field of another name.
            [ModImportName("AnotherThing")]
            public static Func<int, int, int> AnotherThingFromAnywhere;

            // More complicated use case: Only get AnotherThing from ModB specifically.
            [ModImportName("ModB.AnotherThing")]
            // This is only called AnotherThing because we're wrapping this.
            public static Func<int, int, int> AnotherThingFromModB;

            // Example of a wrapper.
            // If ModB.AnotherThing exists, use it.
            // If not, use any other AnotherThing.
            // Internal so that only our mod sees this.
            internal static int AnotherThing(int a, int b) {
                if (AnotherThingFromModB != null)
                    return AnotherThingFromModB(a, b);
                return AnotherThingFromAnywhere(a, b);
            }

        }

        [ModExportName("ModD")]
        [ModImportName("ModB")] // We want to only import things from ModB.
        public static class UtilsD {
            public static Func<int, int, int> Something;
            public static Func<int, int, int> AnotherThing;
        }

    }
}
