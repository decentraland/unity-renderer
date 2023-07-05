using DCL;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSComponents.UIDropdown;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class UIDropDownVisualTests : ECSUIVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_UIDropDownVisualTests_";

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

            var pool = new WrappedComponentPool<IWrappedComponent<PBUiDropdownResult>>(1, () => new ProtobufWrappedComponent<PBUiDropdownResult>(new PBUiDropdownResult()));
            var uiDropdownHandler = new UIDropdownHandler(
                internalEcsComponents.uiContainerComponent,
                resultComponentId: 1001,
                internalEcsComponents.uiInputResultsComponent,
                AssetPromiseKeeper_Font.i,
                componentId: 1001,
                pool);
            uiDropdownHandler.OnComponentCreated(scene, entity);
            uiDropdownHandler.OnComponentModelUpdated(scene, entity, new PBUiDropdown()
            {
                AcceptEmpty = false,
                Disabled = false,
                FontSize = 16,
                TextAlign = TextAlignMode.TamBottomCenter,
                EmptyLabel = "R'LYEH",
                Options = { "OPTION1", "OPTION2", "OPTION3", "OPTION4" },
            });

            yield return null;
            uiSystem.Update();
            yield return null;

            // useCamera = false is passed as UiToolkit/UiElements don't render on the camera
            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera, useCamera: false);

            AssetPromiseKeeper_Font.i.Cleanup();
            uiDropdownHandler.OnComponentRemoved(scene, entity);
            uiTransformHandler.OnComponentRemoved(scene, entity);
        }
    }
}
