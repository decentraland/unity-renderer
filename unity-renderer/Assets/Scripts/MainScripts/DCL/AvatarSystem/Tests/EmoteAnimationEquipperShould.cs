using AvatarSystem;
using DCL;
using DCL.Emotes;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
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
            tikAnim = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Scripts/MainScripts/DCL/Components/Avatar/Animations/Addressables/tik.anim");
            discoAnim = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Scripts/MainScripts/DCL/Components/Avatar/Animations/Addressables/disco.anim");

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
            dataStore.emotesOnUse.SetRefCount(("female", "emote0"), 0);
            dataStore.emotesOnUse.SetRefCount(("female", "emote1"), 1);
            dataStore.emotesOnUse.SetRefCount(("female", "emote2"), 2);

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
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount(("female", "emote0")));
            Assert.AreEqual(2, dataStore.emotesOnUse.GetRefCount(("female", "emote1")));
            Assert.AreEqual(3, dataStore.emotesOnUse.GetRefCount(("female", "emote2")));
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

            animator.DidNotReceive().EquipEmote(Arg.Any<string>(), Arg.Any<EmoteClipData>());
        }

        [Test]
        public void EquipReadyEmotes()
        {
            var tikEmoteData = new EmoteClipData(tikAnim);
            var discoEmoteData = new EmoteClipData(discoAnim);
            dataStore.animations.Add(("female", "emote0"), tikEmoteData);
            dataStore.animations.Add(("female", "emote1"), discoEmoteData);
            equipper.SetEquippedEmotes("female", new []
            {
                new WearableItem { id = "emote0" },
                new WearableItem { id = "emote1" },
                new WearableItem { id = "emote2" },
            });

            animator.Received().EquipEmote("emote0", tikEmoteData);
            animator.Received().EquipEmote("emote1", discoEmoteData);
            animator.DidNotReceive().EquipEmote("emote2", Arg.Any<EmoteClipData>());
        }

        [Test]
        public void DecreaseRefCountOfPrevEmotes()
        {
            dataStore.emotesOnUse.SetRefCount(("female", "old0"), 10);
            dataStore.emotesOnUse.SetRefCount(("female", "old1"), 11);
            dataStore.emotesOnUse.SetRefCount(("female", "old2"), 12);
            equipper.bodyShapeId = "female";
            equipper.emotes.AddRange(new [] { "old0", "old1", "old2" });

            equipper.SetEquippedEmotes("male", new []
            {
                new WearableItem { id = "new0" },
                new WearableItem { id = "new1" },
                new WearableItem { id = "new2" },
            });

            Assert.AreEqual(10, dataStore.emotesOnUse.GetRefCount(("female", "old0")));
            Assert.AreEqual(11, dataStore.emotesOnUse.GetRefCount(("female", "old1")));
            Assert.AreEqual(12, dataStore.emotesOnUse.GetRefCount(("female", "old2")));
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount(("male", "new0")));
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount(("male", "new1")));
            Assert.AreEqual(1, dataStore.emotesOnUse.GetRefCount(("male", "new2")));
        }

        [Test]
        public void EquipEmoteWhenAnimationReadyForCurrentBodyshape()
        {
            equipper.bodyShapeId = "female";
            equipper.emotes.AddRange(new [] { "old0", "old1", "old2" });

            var emoteClipData = new EmoteClipData(tikAnim);

            dataStore.animations.Add(("female", "old0"), emoteClipData);

            animator.Received().EquipEmote("old0", emoteClipData);
        }

        [Test]
        public void NotEquipEmoteWhenAnimationReadyForOtherBodyshape()
        {
            equipper.bodyShapeId = "female";
            equipper.emotes.AddRange(new [] { "old0", "old1", "old2" });

            var emoteClipData = new EmoteClipData(tikAnim);

            dataStore.animations.Add(("male", "old0"), emoteClipData);

            animator.DidNotReceive().EquipEmote(Arg.Any<string>(), Arg.Any<EmoteClipData>());
        }
    }
}
