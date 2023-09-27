using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace DCL.ECSComponents
{
    public class AnimatorHandler : IECSComponentHandler<PBAnimator>
    {
        private static readonly ObjectPool<List<InternalAnimationPlayer.State>> animationsStatePool =
            new ObjectPool<List<InternalAnimationPlayer.State>>(() => new List<InternalAnimationPlayer.State>());

        private readonly IInternalECSComponent<InternalAnimationPlayer> internalAnimationPlayer;

        public AnimatorHandler(IInternalECSComponent<InternalAnimationPlayer> internalAnimationPlayer)
        {
            this.internalAnimationPlayer = internalAnimationPlayer;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            var storedData = internalAnimationPlayer.GetFor(scene, entity);

            if (storedData.HasValue)
            {
                animationsStatePool.Release(storedData.Value.model.States);
                internalAnimationPlayer.RemoveFor(scene, entity);
            }
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAnimator model)
        {
            var storedData = internalAnimationPlayer.GetFor(scene, entity);

            InternalAnimationPlayer internalModel = storedData?.model ?? new InternalAnimationPlayer(animationsStatePool.Get());
            List<InternalAnimationPlayer.State> stateList = internalModel.States;
            stateList.Clear();

            for (int i = 0; i < model.States.Count; i++)
            {
                var stateData = model.States[i];

                stateList.Add(new InternalAnimationPlayer.State(
                    stateData.Clip,
                    stateData.Playing,
                    stateData.GetWeight(),
                    stateData.GetSpeed(),
                    stateData.GetLoop(),
                    stateData.GetShouldReset()
                ));
            }

            internalAnimationPlayer.PutFor(scene, entity, internalModel);
        }
    }
}
