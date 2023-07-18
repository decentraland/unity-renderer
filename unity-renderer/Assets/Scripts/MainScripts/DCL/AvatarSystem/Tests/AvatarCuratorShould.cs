﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using DefaultWearables = WearableLiterals.DefaultWearables;

namespace Test.AvatarSystem
{
    public class AvatarCuratorShould
    {
        private AvatarCurator curator;
        private IWearableItemResolver resolver;
        private Dictionary<string, WearableItem> catalog;
        private IEmotesCatalogService emotesCatalogService;

        [SetUp]
        public void SetUp()
        {
            SetupWearableCatalog();

            resolver = Substitute.For<IWearableItemResolver>();

            //Resolver returns items from our own dictionary
            resolver.Configure()
                    .ResolveAndSplit(Arg.Any<IEnumerable<string>>())
                    .Returns(x =>
                     {
                         List<WearableItem> wearables = GetWearablesFromIDs(x.ArgAt<IEnumerable<string>>(0)).ToList();
                         return new UniTask<(List<WearableItem> wearableItems, List<WearableItem> emotes)>((wearables, new List<WearableItem>()));
                     });

            resolver.Configure()
                    .Resolve(Arg.Any<IEnumerable<string>>())
                    .Returns(x =>
                     {
                         WearableItem[] wearables = GetWearablesFromIDs(x.ArgAt<IEnumerable<string>>(0));
                         return new UniTask<WearableItem[]>(wearables);
                     });

            resolver.Configure()
                    .Resolve(Arg.Any<string>())
                    .Returns(x =>
                     {
                         WearableItem wearable = GetWearableFromID(x.ArgAt<string>(0));
                         return new UniTask<WearableItem>(wearable);
                     });

            emotesCatalogService = Substitute.For<IEmotesCatalogService>();
            curator = new AvatarCurator(resolver, emotesCatalogService);
        }

        [TearDown]
        public void TearDown()
        {
            curator.Dispose();
        }

        [Test]
        public async Task CurateFollowingHideOrder()
        {
            var avatarSettings = new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.FEMALE };
            string[] wearablesId = { WearableLiterals.BodyShapes.FEMALE, "ubody_id", "lbody_id", "eyes_id", "eyebrows_id", "mouth_id", "feet_id", "hair_id", "helmet_id", "mask_id" };

            (BodyWearables _, List<WearableItem> wearables, List<WearableItem> _) = await curator.Curate(avatarSettings, wearablesId, new string[] { });

            Assert.IsFalse(wearables.Contains(catalog["mask_id"]));
            Assert.IsTrue(wearables.Contains(catalog["helmet_id"]));
        }

        [Test]
        public async Task CurateWithForceRenderCategories()
        {
            var firstSettings = new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.FEMALE };
            var secondSettings = new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.FEMALE, forceRender = new HashSet<string>() { "tiara" } };

            string[] wearablesId = { WearableLiterals.BodyShapes.FEMALE, "ubody_id", "lbody_id", "eyes_id", "eyebrows_id", "mouth_id", "feet_id", "hair_id", "tiara_id", "top_head_id" };

            (BodyWearables _, List<WearableItem> wearables, List<WearableItem> _) = await curator.Curate(firstSettings, wearablesId, new string[] { });
            Assert.IsFalse(wearables.Contains(catalog["tiara_id"]));

            (BodyWearables _, List<WearableItem> wearables2, List<WearableItem> _) = await curator.Curate(secondSettings, wearablesId, new string[] { });
            Assert.IsTrue(wearables2.Contains(catalog["tiara_id"]));
        }

        [Test]
        public async Task CurateAProperConstructedModel()
        {
            var avatarSettings = new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.FEMALE };
            string[] wearablesId = { WearableLiterals.BodyShapes.FEMALE, "ubody_id", "lbody_id", "eyes_id", "eyebrows_id", "mouth_id", "feet_id", "hair_id" };
            (BodyWearables bodyWearables, List<WearableItem> wearables,List<WearableItem> emotes) = await curator.Curate(avatarSettings, wearablesId, new string[] { });

            Assert.NotNull(bodyWearables.BodyShape);
            Assert.AreEqual(catalog[WearableLiterals.BodyShapes.FEMALE], bodyWearables.BodyShape);

            Assert.NotNull(bodyWearables.Eyes);
            Assert.AreEqual(catalog["eyes_id"], bodyWearables.Eyes);

            Assert.NotNull(bodyWearables.Eyebrows);
            Assert.AreEqual(catalog["eyebrows_id"], bodyWearables.Eyebrows);

            Assert.NotNull(bodyWearables.Mouth);
            Assert.AreEqual(catalog["mouth_id"], bodyWearables.Mouth);

            Assert.NotNull(wearables);
            Assert.IsTrue(wearables.Contains(catalog["ubody_id"]));
            Assert.IsTrue(wearables.Contains(catalog["lbody_id"]));
        }

        [Test]
        public async Task FallbackToADefaultSetOfWearables()
        {
            var avatarSettings = new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.FEMALE };
            string[] wearablesId = { WearableLiterals.BodyShapes.FEMALE, "WontFindThis" };
            (BodyWearables bodyWearables, List<WearableItem> wearables, List<WearableItem> emotes) = await curator.Curate(avatarSettings, wearablesId, new string[] { });

            Assert.NotNull(bodyWearables.BodyShape);
            Assert.AreEqual(catalog[WearableLiterals.BodyShapes.FEMALE], bodyWearables.BodyShape);

            Assert.NotNull(bodyWearables.Eyes);
            Assert.AreEqual(catalog[DefaultWearables.GetDefaultWearable(WearableLiterals.BodyShapes.FEMALE, WearableLiterals.Categories.EYES)], bodyWearables.Eyes);

            Assert.Null(bodyWearables.Eyebrows);

            Assert.NotNull(bodyWearables.Mouth);
            Assert.AreEqual(catalog[DefaultWearables.GetDefaultWearable(WearableLiterals.BodyShapes.FEMALE, WearableLiterals.Categories.MOUTH)], bodyWearables.Mouth);

            Assert.NotNull(wearables);
            Assert.IsFalse(wearables.Contains(catalog[DefaultWearables.GetDefaultWearable(WearableLiterals.BodyShapes.FEMALE, WearableLiterals.Categories.UPPER_BODY)]));
            Assert.IsFalse(wearables.Contains(catalog[DefaultWearables.GetDefaultWearable(WearableLiterals.BodyShapes.FEMALE, WearableLiterals.Categories.LOWER_BODY)]));
        }

        [Test]
        public async Task ThrowOnCurateIfCancellationTokenIsCancelled()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            //Assert
            TestUtils.ThrowsAsync<OperationCanceledException>(curator.Curate(new AvatarSettings { bodyshapeId = WearableLiterals.BodyShapes.FEMALE }, null, null, cts.Token));
        }

        [Test]
        public void DisposeResolver()
        {
            //Act
            curator.Dispose();

            //Arrange
            resolver.Received().Dispose();
        }

        private WearableItem[] GetWearablesFromIDs(IEnumerable<string> ids)
        {
            List<WearableItem> toReturn = new List<WearableItem>();

            foreach (string id in ids)
            {
                if (!catalog.TryGetValue(id, out var wearable))
                    continue;

                toReturn.Add(wearable);
            }

            return toReturn.ToArray();
        }

        private WearableItem GetWearableFromID(string id)
        {
            catalog.TryGetValue(id, out WearableItem wearable);
            return wearable;
        }

        private void SetupWearableCatalog()
        {
            catalog = new Dictionary<string, WearableItem>();
            catalog.Add(WearableLiterals.BodyShapes.FEMALE, GetWearableForFemaleBodyshape(WearableLiterals.BodyShapes.FEMALE, WearableLiterals.Categories.BODY_SHAPE));
            catalog.Add("ubody_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.UPPER_BODY));
            catalog.Add("lbody_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.LOWER_BODY));
            catalog.Add("eyes_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.EYES));
            catalog.Add("eyebrows_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.EYEBROWS));
            catalog.Add("mouth_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.MOUTH));
            catalog.Add("top_head_id", GetWearableForFemaleBodyshapeWithHides("top_head_id", "top_head", "tiara"));
            catalog.Add("helmet_id", GetWearableForFemaleBodyshapeWithHides("helmet_id", "helmet", "mask"));
            catalog.Add("mask_id", GetWearableForFemaleBodyshapeWithHides("mask_id", "mask", "helmet"));
            catalog.Add("tiara_id", GetWearableForFemaleBodyshape("ubody", "tiara"));
            catalog.Add("feet_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.FEET));
            catalog.Add("hair_id", GetWearableForFemaleBodyshape("ubody", WearableLiterals.Categories.HAIR));

            // Fill with default wearables
            foreach (((string bodyshapeId, string category), string id) in DefaultWearables.defaultWearables) { catalog.Add(id, GetWearableForFemaleBodyshape(id, category)); }
        }

        private WearableItem GetWearableForFemaleBodyshape(string id, string category)
        {
            return new WearableItem
            {
                id = id,
                data = new WearableItem.Data
                {
                    category = category,
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { WearableLiterals.BodyShapes.FEMALE },
                        }
                    },
                }
            };
        }

        private WearableItem GetWearableForFemaleBodyshapeWithHides(string id, string category, string hide)
        {
            return new WearableItem
            {
                id = id,
                data = new WearableItem.Data
                {
                    category = category,
                    representations = new[]
                    {
                        new WearableItem.Representation
                        {
                            bodyShapes = new[] { WearableLiterals.BodyShapes.FEMALE },
                        }
                    },
                    hides = new[] { hide }
                }
            };
        }
    }
}
