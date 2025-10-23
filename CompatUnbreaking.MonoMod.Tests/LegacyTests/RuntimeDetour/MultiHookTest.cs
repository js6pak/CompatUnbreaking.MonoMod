﻿#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
#pragma warning disable xUnit1013 // Public method should be marked as test

using MonoMod.RuntimeDetour;
using System;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using Xunit;

namespace MonoMod.UnitTest {
    public class ManualMultiHookTest {
        Hook h1;
        Hook h2;
        ILHook hIL;

        private bool h1Run;
        private bool h2Run;
        private bool hILRun;

        private void Setup() {
            h1 = new Hook(
                typeof(ManualMultiHookTest).GetMethod("DoNothing"),
                new Action<Action<ManualMultiHookTest>, ManualMultiHookTest>((orig, self) => {
                    orig(self);
                    h1Run = true;
                }),
                new HookConfig {
                    ManualApply = true
                }
            );
            h2 = new Hook(
                typeof(ManualMultiHookTest).GetMethod("DoNothing"),
                new Action<Action<ManualMultiHookTest>, ManualMultiHookTest>((orig, self) => {
                    orig(self);
                    h2Run = true;
                }),
                new HookConfig {
                    ManualApply = true
                }
            );
            hIL = new ILHook(
                typeof(ManualMultiHookTest).GetMethod("DoNothing"),
                il => {
                    ILCursor c = new ILCursor(il);
                    c.EmitDelegate<Action>(() => {
                        hILRun = true;
                    });
                },
                new ILHookConfig {
                    ManualApply = true
                }
            );
            h1Run = false;
            h2Run = false;
            hILRun = false;
        }

        [SkipRemoteLinuxMonoFact]
        public void DoNothingTest() {
            Setup();
            DoNothing();
            Assert.False(h1Run);
            Assert.False(h2Run);
            Assert.False(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H1() {
            Setup();
            h1.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.False(h2Run);
            Assert.False(hILRun);
            h1.Undo();
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H2() {
            Setup();
            h2.Apply();
            DoNothing();
            Assert.False(h1Run);
            Assert.True(h2Run);
            Assert.False(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void HIL() {
            Setup();
            hIL.Apply();
            DoNothing();
            Assert.False(h1Run);
            Assert.False(h2Run);
            Assert.True(hILRun);
            TearDown();
        }


        [SkipRemoteLinuxMonoFact]
        public void HILH1() {
            Setup();
            hIL.Apply();
            h1.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.False(h2Run);
            Assert.True(hILRun);
            TearDown();
        }


        [SkipRemoteLinuxMonoFact]
        public void HILH1H2() {
            Setup();
            hIL.Apply();
            h1.Apply();
            h2.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            TearDown();
        }


        [SkipRemoteLinuxMonoFact]
        public void HILH2H1() {
            Setup();
            hIL.Apply();
            h2.Apply();
            h1.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H1H2HIL() {
            Setup();
            h1.Apply();
            h2.Apply();
            hIL.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H2H1HIL() {
            Setup();
            h2.Apply();
            h1.Apply();
            hIL.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H2HIL() {
            Setup();
            h2.Apply();
            hIL.Apply();
            DoNothing();
            Assert.False(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            hIL.Undo();
            h2.Undo();
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H1HILH2() {
            Setup();
            h1.Apply();
            hIL.Apply();
            h2.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void H1HIL() {
            Setup();
            h1.Apply();
            hIL.Apply();
            DoNothing();
            Assert.True(h1Run);
            Assert.False(h2Run);
            Assert.True(hILRun);
            TearDown();
        }

        [SkipRemoteLinuxMonoFact]
        public void HILH2() {
            Setup();
            hIL.Apply();
            h2.Apply();
            DoNothing();
            Assert.False(h1Run);
            Assert.True(h2Run);
            Assert.True(hILRun);
            TearDown();
        }

        private void TearDown() {
            h1.Dispose();
            h2.Dispose();
            hIL.Dispose();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DoNothing() {
        }
    }
}