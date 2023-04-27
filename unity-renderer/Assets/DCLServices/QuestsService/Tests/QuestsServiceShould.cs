using Cysharp.Threading.Tasks;
using Decentraland.Quests;
using NUnit.Framework;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace DCLServices.QuestsService.Tests
{
    public class QuestsServiceShould
    {
        [Test]
        public async Task TestCachedTasks()
        {
            block = true;
            var orig = GetCachedDefinition("hola");
            var copy = GetCachedDefinition("hola");
            var copy2 = GetCachedDefinition("hola");

            UnblockIn(3).Forget();
            (QuestDefinition origin, QuestDefinition copy, QuestDefinition copy2) results = await UniTask.WhenAll(orig, copy, copy2);

            Assert.AreEqual("hola", results.origin.Steps[0].Id);
            Assert.AreEqual("0", results.origin.Steps[0].Description);
            Assert.AreEqual(results.origin, results.copy);
            Assert.AreEqual(results.origin, results.copy2);
        }

        private static int created = 0;
        private bool block = false;
        private Dictionary<string, UniTask<QuestDefinition>> cachedDefinitions = new ();

        private async UniTask UnblockIn(float seconds)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds));
            block = false;
        }

        private async UniTask<QuestDefinition> GetCachedDefinition(string questId)
        {
            if (!cachedDefinitions.TryGetValue(questId, out var cached))
            {
                cached = GetDefinition(questId);
                cachedDefinitions[questId] = cached;
            }

            return await cached;
        }

        async UniTask<QuestDefinition> GetDefinition(string questId)
        {
            await UniTask.WaitUntil(() => block != false);

            return new QuestDefinition() { Steps = { new[] { new Step() { Id = questId, Description = (created++).ToString() } } } };
        }
    }
}
