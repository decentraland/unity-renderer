using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class AvatarModifierAreaComponentHandler : IECSComponentHandler<PBAvatarModifierArea>
    {
        private readonly AvatarModifierFactory factory;
        private readonly IInternalECSComponent<InternalAvatarModifierArea> internalAvatarModifierArea;

        public AvatarModifierAreaComponentHandler(
            IInternalECSComponent<InternalAvatarModifierArea> internalAvatarModifierArea,
            AvatarModifierFactory factory)
        {
            this.factory = factory;
            this.internalAvatarModifierArea = internalAvatarModifierArea;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            internalAvatarModifierArea.RemoveFor(scene, entity, new InternalAvatarModifierArea()
            {
                removed = true
            });
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAvatarModifierArea model)
        {
            if (model.Modifiers.Count == 0)
                return;

            var internalComponentModel = internalAvatarModifierArea.GetFor(scene, entity)?.model ?? new InternalAvatarModifierArea();

            internalComponentModel.area = ProtoConvertUtils.PBVectorToUnityVector(model.Area);

            foreach (AvatarModifierType modifierKey in model.Modifiers)
            {
                var modifier = factory.GetOrCreateAvatarModifier(modifierKey);

                internalComponentModel.OnAvatarEnter += modifier.ApplyModifier;
                internalComponentModel.OnAvatarExit += modifier.RemoveModifier;
            }

            internalComponentModel.excludedIds = model.ExcludeIds.ToHashSet();

            internalAvatarModifierArea.PutFor(scene, entity, internalComponentModel);
        }
    }
}
