using DCL;
using DCL.ECSComponents;
using DCL.Helpers;
using Decentraland.Common;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class UITransformVisualTests : ECSUIVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_UITransformVisualTests_";

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
                Overflow = YGOverflow.YgoVisible,
                FlexDirection = YGFlexDirection.YgfdColumnReverse,
                FlexBasis = float.NaN,
                FlexGrow = 23,
                FlexShrink = 1,
                FlexWrap = YGWrap.YgwWrapReverse,
                AlignContent = YGAlign.YgaCenter,
                AlignItems = YGAlign.YgaStretch,
                AlignSelf = YGAlign.YgaCenter,
                JustifyContent = YGJustify.YgjSpaceAround,
                Height = 99,
                Width = 34,
                MaxWidth = float.NaN,
                MaxHeight = 10,
                MinHeight = 0,
                MinWidth = 0,
                PaddingBottom = 10,
                PaddingBottomUnit = YGUnit.YguPercent,
                PaddingLeft = 0,
                PaddingLeftUnit = YGUnit.YguPoint,
                PaddingRight = 111,
                PaddingRightUnit = YGUnit.YguPoint,
                PaddingTop = 5,
                PaddingTopUnit = YGUnit.YguPercent,
                MarginBottom = 10,
                MarginBottomUnit = YGUnit.YguPercent,
                MarginLeft = 0,
                MarginLeftUnit = YGUnit.YguPoint,
                MarginRight = 111,
                MarginRightUnit = YGUnit.YguPoint,
                MarginTop = 5,
                MarginTopUnit = YGUnit.YguPercent,
                PositionType = YGPositionType.YgptAbsolute
            });

            var uiBackgroundHandler = new UIBackgroundHandler(internalEcsComponents.uiContainerComponent, componentId: 34, AssetPromiseKeeper_Texture.i);
            uiBackgroundHandler.OnComponentCreated(scene, entity);
            uiBackgroundHandler.OnComponentModelUpdated(scene, entity, new PBUiBackground
            {
                Color = new Color4 { R = 1f, G = 0f, B = 0f, A = 0.9f }
            });

            yield return null;
            uiSystem.Update();
            yield return null;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);

            uiBackgroundHandler.OnComponentRemoved(scene, entity);
            uiTransformHandler.OnComponentRemoved(scene, entity);
        }
    }
}
