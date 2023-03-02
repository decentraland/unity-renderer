using Cysharp.Threading.Tasks;
using DCL.Helpers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MainScripts.DCL.Helpers.Promise.PromiseTests
{
    [TestFixture]
    public class PromiseShould
    {
        [Test]
        public async Task NotSkipFrameWhenResultSet()
        {
            var promise = new Promise<int>();
            await UniTask.Yield();
            var frame = Time.frameCount;
            promise.Resolve(100);
            await promise;
            Assert.AreEqual(frame, Time.frameCount);
        }

        [Test]
        public async Task NotSkipFrameWhenResultSetConcurrently()
        {
            // TestRunner has problems with custom `PlayerLoopTiming` and subsequently run Tasks

            var promise = new Promise<int>();
            int frame = int.MinValue;

            async UniTask WaitAndSet()
            {
                await UniTask.Yield(PlayerLoopTiming.Initialization);
                frame = Time.frameCount;
                promise.Resolve(100);
            }

            await UniTask.WhenAll(WaitAndSet(), promise.ToUniTask()).Timeout(TimeSpan.FromSeconds(10));

            await promise;
            Assert.AreEqual(frame, Time.frameCount);
        }
    }
}
