using AvatarSystem;
using DCL;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Test.AvatarSystem
{
    public class EmoteAnimationEquipperShould
    {
        private EmoteAnimationEquipper equipper;
        private IAnimator animator;
        private DataStore_Emotes dataStore;
        private AnimationClip tikAnim;
        private AnimationClip discoAnim;

        [SetUp]
        public void SetUp()
        {
            tikAnim = Resources.Load<AnimationClip>("tik");
            discoAnim = Resources.Load<AnimationClip>("disco");

            animator = Substitute.For<IAnimator>();
            dataStore = new DataStore_Emotes();
            equipper = new EmoteAnimationEquipper(animator, dataStore);
        }

        [TearDown]
        public void TearDown() { equipper.Dispose(); }

        [Test]
        public void AssignReferencesOnConstruction()
        {
            Assert.AreEqual(animator, equipper.animator);
            Assert.AreEqual(dataStore, equipper.dataStoreEmotes);
            Assert.AreEqual(0, equipper.emotes.Count);
        }

        [Test]
        public void SetEmoteListAndUpdateUses()
        {
            dataStore.emotesOnUse.SetRefCount("emote0", 0);
            dataStore.emotesOnUse.SetRefCount("emote1", 1);
            dataStore.emotesOnUse.SetRefCount("emote2", 2);

            equipper.SetEquippedEmotes("female", new []
            {
                new WearableItem { id = "emote0" },
                new WearableItem { id = "emote1" },
                new WearableItem { id = "emote2" },
            });

            Assert.AreEqual("female", equipper.bodyShapeId);
            Assert.AreEqual("emote0" , equipper.emotes[0]);
            Assert.AreEqual("emote1" , equipper.emotes[1]);
            Assert.AreEqual("emote2" , equipper.emotes[2]);
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount("emote0"));
            Assert.AreEqual(2, dataStore.emotesOnUse.GetRefCount("emote1"));
            Assert.AreEqual(3, dataStore.emotesOnUse.GetRefCount("emote2"));
        }

        [Test]
        public void NotEquipNotReadyEmotesOnSet()
        {
            equipper.SetEquippedEmotes("", new []
            {
                new WearableItem { id = "emote0" },
                new WearableItem { id = "emote1" },
                new WearableItem { id = "emote2" },
            });

            animator.DidNotReceive().EquipEmote(Arg.Any<string>(), Arg.Any<AnimationClip>());
        }

        [Test]
        public void EquipReadyEmotes()
        {
            dataStore.animations.Add(("female", "emote0"), tikAnim);
            dataStore.animations.Add(("female", "emote1"), discoAnim);
            equipper.SetEquippedEmotes("female", new []
            {
                new WearableItem { id = "emote0" },
                new WearableItem { id = "emote1" },
                new WearableItem { id = "emote2" },
            });

            animator.Received().EquipEmote("emote0", tikAnim);
            animator.Received().EquipEmote("emote1", discoAnim);
            animator.DidNotReceive().EquipEmote("emote2", Arg.Any<AnimationClip>());
        }

        [Test]
        public void DecreaseRefCountOfPrevEmotes()
        {
            dataStore.emotesOnUse.SetRefCount("old0", 10);
            dataStore.emotesOnUse.SetRefCount("old1", 11);
            dataStore.emotesOnUse.SetRefCount("old2", 12);
            equipper.emotes.AddRange(new [] { "old0", "old1", "old2" });

            equipper.SetEquippedEmotes("", new []
            {
                new WearableItem { id = "new0" },
                new WearableItem { id = "new1" },
                new WearableItem { id = "new2" },
            });

            Assert.AreEqual(9, dataStore.emotesOnUse.GetRefCount("old0"));
            Assert.AreEqual(10, dataStore.emotesOnUse.GetRefCount("old1"));
            Assert.AreEqual(11, dataStore.emotesOnUse.GetRefCount("old2"));
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount("new0"));
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount("new1"));
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount("new2"));
        }

        [Test]
        public void EquipEmoteWhenAnimationReadyForCurrentBodyshape()
        {
            equipper.bodyShapeId = "female";
            equipper.emotes.AddRange(new [] { "old0", "old1", "old2" });

            dataStore.animations.Add(("female", "old0"), tikAnim);

            animator.Received().EquipEmote("old0", tikAnim);
        }

        [Test]
        public void NotEquipEmoteWhenAnimationReadyForOtherBodyshape()
        {
            equipper.bodyShapeId = "female";
            equipper.emotes.AddRange(new [] { "old0", "old1", "old2" });

            dataStore.animations.Add(("male", "old0"), tikAnim);

            animator.DidNotReceive().EquipEmote(Arg.Any<string>(), Arg.Any<AnimationClip>());
        }
    }
}