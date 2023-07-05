using DCL;
using DCL.ECSComponents;
using DCL.ECSComponents.UIText;
using DCL.Helpers;
using Decentraland.Common;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class UITextVisualTests : ECSUIVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_UITextVisualTests_";

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest1()
        {
            Vector3 cameraPos = (Vector3.up * 10);
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
                Width = 50,
                PaddingBottomUnit = YGUnit.YguPercent,
                PaddingBottom = 10,
                PaddingLeftUnit = YGUnit.YguPoint,
                PaddingLeft = 50,
                PaddingRightUnit = YGUnit.YguPercent,
                PaddingRight = 50,
                PaddingTopUnit = YGUnit.YguPercent,
                PaddingTop = 5,
                PositionType = YGPositionType.YgptAbsolute,
            });

            var uiTextHandler = new UiTextHandler(internalEcsComponents.uiContainerComponent, AssetPromiseKeeper_Font.i, componentId: 34);
            uiTextHandler.OnComponentCreated(scene, entity);
            uiTextHandler.OnComponentModelUpdated(scene, entity, new PBUiText()
            {
                Value = "TEMPTATION",
                FontSize = 34,
                Color = new Color4() { R = 0.1f, G = 0.2f, B = 0.3f, A = 1 },
                TextAlign = TextAlignMode.TamMiddleRight
            });

            yield return null;
            uiSystem.Update();
            yield return null;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);

            AssetPromiseKeeper_Font.i.Cleanup();
            uiTextHandler.OnComponentRemoved(scene, entity);
            uiTransformHandler.OnComponentRemoved(scene, entity);
        }
    }
}
