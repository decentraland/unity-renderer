using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using ECSSystems.ScenesUiSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tests
{
    public class ECSUIVisualTestsBase : ECSVisualTestsBase
    {
        protected UIDocument uiDocument;
        protected BooleanVariable hideUiEventVariable;
        protected ECSScenesUiSystem uiSystem;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            uiDocument = Object.Instantiate(Resources.Load<UIDocument>("ScenesUI"));
            hideUiEventVariable = CommonScriptableObjects.allUIHidden;
            hideUiEventVariable.Set(false);

            // create scene UI system
            IWorldState worldState = Substitute.For<IWorldState>();
            worldState.GetCurrentSceneNumber().Returns(scene.sceneData.sceneNumber);

            uiSystem = new ECSScenesUiSystem(
                uiDocument,
                internalEcsComponents.uiContainerComponent,
                new BaseList<IParcelScene> { scene },
                worldState,
                hideUiEventVariable,
                new BaseVariable<bool>(true));

            // create root ui for scene
            InternalUiContainer rootSceneContainer = new InternalUiContainer(SpecialEntityId.SCENE_ROOT_ENTITY);
            internalEcsComponents.uiContainerComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, rootSceneContainer);
        }

        [TearDown]
        public override void TearDown()
        {
            Object.Destroy(uiDocument.gameObject);
            CommonScriptableObjects.UnloadAll();
            uiSystem.Dispose();

            base.TearDown();
        }
    }
}
