using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DCL.Helpers.Tests
{
    [TestFixture]
    public class WaitForBooleanShould
    {
        private bool testBool;

        [Test]
        public async Task FinishWhenRefDeclaredInClass([Values(true, false)] bool targetValue, [Values(true, false)] bool startValue)
        {
            testBool = startValue;

            var task = UniTaskUtils.WaitForBoolean(ref testBool, targetValue);

            async Task SetValue()
            {
                await Task.Delay(TimeSpan.FromSeconds(0.5f));
                testBool = targetValue;
            }

            var whenAll = Task.WhenAll(task.AsTask(), SetValue());

            var firstCompleted = await Task.WhenAny(whenAll, Task.Delay(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(whenAll, firstCompleted, $"{nameof(UniTaskUtils.WaitForBoolean)} didn't fire");
        }

        [Test]
        public async Task UsePool([Values(true, false)] bool targetValue, [Range(2, 20)] int iterations)
        {
            Task ResetAndRestart()
            {
                testBool = !targetValue;
                return UniTaskUtils.WaitForBoolean(ref testBool, targetValue).AsTask();
            }

            async Task SetValue()
            {
                await Task.Delay(TimeSpan.FromSeconds(0.2f / iterations));
                testBool = targetValue;
            }

            for (var i = 0; i < iterations; i++)
            {
                await Task.WhenAll(ResetAndRestart(), SetValue());
            }

            Assert.AreEqual(1, UniTaskUtils.WaitForBooleanPromise.pool.Size);
        }
    }
}
