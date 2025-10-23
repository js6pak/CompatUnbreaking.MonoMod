﻿#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
#pragma warning disable xUnit1013 // Public method should be marked as test

using Xunit;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Utils;
using System.Reflection.Emit;
using System.Text;

namespace MonoMod.UnitTest {
    [Collection("RuntimeDetour")]
    public class DisableInliningTest {
        private bool DidNothing = true;

#if DEBUG
#pragma warning disable xUnit1004 // Test methods should not be skipped
        [Fact(Skip = "Inlining always disabled in debug mode")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
#else
        [Fact(Skip = "Unfinished")]
#endif
        public void TestDisableInlining() {
            // The following use cases are not meant to be usage examples.
            // Please take a look at DetourTest and HookTest instead.

            // Note: This test may fail when inlining is disabled globally or externally.

            // Verify that inlining works in the first place.
            RunWithInlining();

            // RunWithInlining detours DoNothing, which pins it and thus
            // prevents it from getting inlined in RunWithoutInlining.
            RunWithoutInlining();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]

        private void RunWithInlining() {
            DidNothing = true;
            DoNothing();
            Assert.True(DidNothing);

            using (Hook h = new Hook(
                typeof(DisableInliningTest).GetMethod("DoNothing"),
                new Action<DisableInliningTest>(self => {
                    DidNothing = false;
                })
            )) {
                DidNothing = true;
                DoNothing();
                // Assume DoNothing got inlined and did nothing.
                Assert.True(DidNothing);
            }

            DidNothing = true;
            DoNothing();
            Assert.True(DidNothing);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RunWithoutInlining() {
            DidNothing = true;
            DoNothing();
            Assert.True(DidNothing);

            using (Hook h = new Hook(
                typeof(DisableInliningTest).GetMethod("DoNothing"),
                new Action<DisableInliningTest>(self => {
                    DidNothing = false;
                })
            )) {
                DidNothing = true;
                DoNothing();
                // Assume DoNothing got detoured and did something.
                Assert.False(DidNothing);
            }

            DidNothing = true;
            DoNothing();
            Assert.True(DidNothing);
        }

        public void DoNothing() {
        }
        
    }
}
