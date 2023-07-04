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
    public class TextShapeVisualTests : ECSVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_TextShapeVisualTests_";

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest1()
        {
            Vector3 cameraPos = (Vector3.up * 10) + (Vector3.back * 4);
            VisualTestUtils.RepositionVisualTestsCamera(camera, cameraPos, cameraPos + Vector3.forward);

            var textEntity1 = scene.CreateEntity(666);
            textEntity1.gameObject.transform.position = new Vector3(-10, 15, 10);
            var componentHandler1 = CreateTextShape(textEntity1, new PBTextShape()
            {
                Text = "CaRpEdIeM"
            });
            yield return null;

            var textEntity2 = scene.CreateEntity(667);
            textEntity2.gameObject.transform.position = new Vector3(0, 10, 10);
            var componentHandler2 = CreateTextShape(textEntity2, new PBTextShape()
            {
                Text = "Ph’nglui mglw’nafh Cthulhu \nR’lyeh wgah’nagl fhtagn...",
                TextColor = new Color4() { R = 0.1f, G = 0.3f, B = 0.6f, A = 1f },
                FontSize = 17f,
                LineSpacing = 5f,
                LineCount = 2,
                OutlineWidth = 0.1f,
                OutlineColor = new Color3() { R = 0.3f, G = 1.0f, B = 0.3f }
            });
            yield return null;

            var textEntity3 = scene.CreateEntity(668);
            textEntity3.gameObject.transform.position = new Vector3(0, 5, 10);
            var componentHandler3 = CreateTextShape(textEntity3, new PBTextShape()
            {
                Text = "Iä! Iä! Cthulhu fhtagn!",
                TextColor = new Color4() { R = 1.0f, G = 0.3f, B = 0.3f, A = 1f },
                FontSize = 20f,
                OutlineWidth = 0.03f,
                OutlineColor = new Color3() { R = 0f, G = 0f, B = 0f }
            });
            yield return null;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);

            AssetPromiseKeeper_Font.i.Cleanup();
            componentHandler1.OnComponentRemoved(scene, textEntity1);
            componentHandler2.OnComponentRemoved(scene, textEntity2);
            componentHandler3.OnComponentRemoved(scene, textEntity3);
        }

        private ECSTextShapeComponentHandler CreateTextShape(ECS7TestEntity entity, PBTextShape model)
        {
            var componentHandler = new ECSTextShapeComponentHandler(AssetPromiseKeeper_Font.i,
                internalEcsComponents.renderersComponent,
                internalEcsComponents.sceneBoundsCheckComponent);
            componentHandler.OnComponentCreated(scene, entity);
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            return componentHandler;
        }
    }
}
