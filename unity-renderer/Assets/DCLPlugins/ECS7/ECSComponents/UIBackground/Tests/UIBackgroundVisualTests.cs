using DCL;
using DCL.ECSComponents;
using DCL.Helpers;
using Decentraland.Common;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class UIBackgroundVisualTests : ECSUIVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_UIBackgroundVisualTests_";

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest1()
        {
            Vector3 cameraPos = (Vector3.up * 30);
            VisualTestUtils.RepositionVisualTestsCamera(camera, cameraPos, cameraPos + Vector3.forward);

            var entity = scene.CreateEntity(666);

            var uiTransformHandler = new UITransformHandler(internalEcsComponents.uiContainerComponent, componentId: 34);
            uiTransformHandler.OnComponentCreated(scene, entity);
            uiTransformHandler.OnComponentModelUpdated(scene, entity, new PBUiTransform()
            {
                Display = YGDisplay.YgdFlex,
                AlignContent = YGAlign.YgaCenter,
                AlignSelf = YGAlign.YgaCenter,
                Height = 150,
                Width = 300,
                PaddingBottomUnit = YGUnit.YguPercent,
                PaddingBottom = 10,
                PaddingLeftUnit = YGUnit.YguPoint,
                PaddingLeft = 0,
                PaddingRightUnit = YGUnit.YguPercent,
                PaddingRight = 50,
                PaddingTopUnit = YGUnit.YguPercent,
                PaddingTop = 5,
                PositionType = YGPositionType.YgptAbsolute,
            });

            var uiBackgroundHandler = new UIBackgroundHandler(internalEcsComponents.uiContainerComponent, componentId: 34, AssetPromiseKeeper_Texture.i);
            uiBackgroundHandler.OnComponentCreated(scene, entity);
            uiBackgroundHandler.OnComponentModelUpdated(scene, entity, new PBUiBackground
            {
                Color = new Color4 { R = 0.5f, G = 0.5f, B = 0.1f, A = 0.95f }
            });

            yield return null;
            uiSystem.Update();
            yield return null;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);

            AssetPromiseKeeper_Texture.i.Cleanup();
            uiBackgroundHandler.OnComponentRemoved(scene, entity);
            uiTransformHandler.OnComponentRemoved(scene, entity);
        }
    }
}
