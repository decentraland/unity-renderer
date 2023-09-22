using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests.Components.Animator
{
    public class AnimatorComponentShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private AnimatorHandler componentHandler;
        private IInternalECSComponent<InternalAnimationPlayer> internalAnimationPlayer;

        [SetUp]
        protected void SetUp()
        {
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            internalAnimationPlayer = Substitute.For<IInternalECSComponent<InternalAnimationPlayer>>();
            componentHandler = new AnimatorHandler(internalAnimationPlayer);
        }

        [Test]
        public void CreateInternalComponent()
        {
            var model = new PBAnimator()
            {
                States =
                {
                    new PBAnimationState()
                    {
                        Clip = "someClip",
                        Loop = false,
                        Playing = false,
                        Speed = 47,
                        Weight = 666,
                        ShouldReset = false
                    }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalAnimationPlayer.Received(1)
                                   .PutFor(scene, entity, Arg.Is<InternalAnimationPlayer>(comp =>
                                        comp.States[0].Clip == model.States[0].Clip
                                        && comp.States[0].Weight == model.States[0].Weight
                                        && comp.States[0].Loop == model.States[0].Loop
                                        && comp.States[0].Playing == model.States[0].Playing
                                        && comp.States[0].Speed == model.States[0].Speed
                                        && comp.States[0].ShouldReset == model.States[0].ShouldReset
                                    ));
        }

        [Test]
        public void UpdateInternalComponent()
        {
            var model = new PBAnimator()
            {
                States =
                {
                    new PBAnimationState()
                    {
                        Clip = "someClip",
                        Loop = false,
                        Playing = false,
                        Speed = 47,
                        Weight = 666,
                        ShouldReset = false
                    }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, new PBAnimator() { States = { new PBAnimationState() { Clip = "clip" } } });
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalAnimationPlayer.Received(1)
                                   .PutFor(scene, entity, Arg.Is<InternalAnimationPlayer>(comp =>
                                        comp.States[0].Clip == model.States[0].Clip
                                        && comp.States[0].Weight == model.States[0].Weight
                                        && comp.States[0].Loop == model.States[0].Loop
                                        && comp.States[0].Playing == model.States[0].Playing
                                        && comp.States[0].Speed == model.States[0].Speed
                                        && comp.States[0].ShouldReset == model.States[0].ShouldReset
                                    ));
        }

        [Test]
        public void RemoveInternalComponent()
        {
            internalAnimationPlayer.GetFor(scene, entity)
                                   .Returns(
                                        new ECSComponentData<InternalAnimationPlayer>(
                                            scene,
                                            entity,
                                            new InternalAnimationPlayer(new List<InternalAnimationPlayer.State>()),
                                            null));

            componentHandler.OnComponentRemoved(scene, entity);
            internalAnimationPlayer.Received(1).RemoveFor(scene, entity);
        }
    }
}
