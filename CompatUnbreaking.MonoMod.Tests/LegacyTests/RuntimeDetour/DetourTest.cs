﻿#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
#pragma warning disable xUnit1013 // Public method should be marked as test

using Xunit;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MonoMod.UnitTest {
    [Collection("RuntimeDetour")]
    public class DetourTest {
        [Fact]
        public void TestDetours() {
            lock (TestObject.Lock) {
                Console.WriteLine("Detours: none");
                TestObject.TestStep(5, 6, 8);
                Console.WriteLine();

                // Three examples of using Detour.
                // Note that if you need non-layered, low-level hooks, you can use NativeDetour instead.
                // This is also why the variable type is IDetour.
                // System.Linq.Expressions, thanks to leo (HookedMethod) for telling me about how to (ab)use MethodCallExpression!
                IDetour detourTestMethodA = new Detour(
                    () => default(TestObject).TestMethod(default, default),
                    () => TestMethod_A(default, default, default)
                );
                // Method references as delegates.
                IDetour detourTestStaticMethodA = new Detour<Func<int, int, int>>(
                    TestObject.TestStaticMethod,
                    TestStaticMethod_A
                );
                // MethodBase, old syntax.
                // Note: You only need GetTypeInfo() if you target .NET Standard 1.6
                IDetour detourTestVoidMethodA = new Detour(
                    typeof(TestObject).GetMethod("TestVoidMethod", BindingFlags.Static | BindingFlags.Public),
                    typeof(DetourTest).GetMethod("TestVoidMethod_A", BindingFlags.Static | BindingFlags.Public)
                );
                Console.WriteLine("Detours: A");
                TestObject.TestStep(42, 12, 1);
                Console.WriteLine("Testing trampoline, should invoke orig, TestVoidMethod(2, 3)");
                detourTestVoidMethodA.GenerateTrampoline<Action<int, int>>()(2, 3);
                Console.WriteLine();

                IDetour detourTestMethodB = new Detour(
                    () => default(TestObject).TestMethod(default, default),
                    () => TestMethod_B(default, default, default)
                );
                IDetour detourTestStaticMethodB = new Detour(
                     () => TestObject.TestStaticMethod(default, default),
                     () => TestStaticMethod_B(default, default)
                );
                IDetour detourTestVoidMethodB = new Detour(
                     () => TestObject.TestVoidMethod(default, default),
                     () => TestVoidMethod_B(default, default)
                );
                Console.WriteLine("Detours: A + B");
                TestObject.TestStep(120, 8, 2);
                Console.WriteLine("Testing trampoline, should invoke hook A, TestVoidMethod(2, 3)");
                Action<int, int> trampolineTestVoidMethodB = detourTestVoidMethodB.GenerateTrampoline<Action<int, int>>();
                trampolineTestVoidMethodB(2, 3);
                Console.WriteLine();

                detourTestMethodA.Undo();
                detourTestStaticMethodA.Undo();
                detourTestVoidMethodA.Undo();
                Console.WriteLine("Detours: B");
                TestObject.TestStep(120, 8, 2);
                Console.WriteLine("Testing trampoline, should invoke orig, TestVoidMethod(2, 3)");
                trampolineTestVoidMethodB(2, 3);
                Console.WriteLine();

                detourTestMethodB.Undo();
                detourTestStaticMethodB.Undo();
                detourTestVoidMethodB.Undo();
                Console.WriteLine("Detours: none");
                TestObject.TestStep(5, 6, 8);
                Console.WriteLine();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int TestMethod_A(TestObject self, int a, int b) {
            return 42;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int TestStaticMethod_A(int a, int b) {
            return a * b * 2;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestVoidMethod_A(int a, int b) {
            Console.WriteLine("Detour A");
            TestObject.VoidResult += 1;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int TestMethod_B(TestObject self, int a, int b) {
            return 120;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int TestStaticMethod_B(int a, int b) {
            return a * b + 2;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TestVoidMethod_B(int a, int b) {
            Console.WriteLine("Detour B");
            TestObject.VoidResult += 2;
        }
    }
}
