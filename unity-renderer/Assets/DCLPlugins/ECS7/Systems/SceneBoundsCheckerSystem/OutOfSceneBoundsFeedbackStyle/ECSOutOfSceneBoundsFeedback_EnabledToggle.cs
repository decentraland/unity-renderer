using DCL.ECS7.InternalComponents;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSOutOfSceneBoundsFeedback_EnabledToggle : IECSOutOfSceneBoundsFeedbackStyle
    {
        public void ApplyFeedback(IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel, bool isVisible, bool isInsideBounds)
        {
            IList<Renderer> renderers = sbcComponentModel.renderers;
            IList<Collider> physicsColliders = sbcComponentModel.physicsColliders;
            IList<Collider> pointerColliders = sbcComponentModel.pointerColliders;

            if (renderers != null)
            {
                int count = renderers.Count;

                for (var i = 0; i < count; i++)
                {
                    renderers[i].enabled = isInsideBounds && isVisible;
                }
            }

            if (physicsColliders != null)
            {
                int count = physicsColliders.Count;

                for (var i = 0; i < count; i++)
                {
                    physicsColliders[i].enabled = isInsideBounds;
                }
            }

            if (pointerColliders != null)
            {
                int count = pointerColliders.Count;

                for (var i = 0; i < count; i++)
                {
                    pointerColliders[i].enabled = isInsideBounds;
                }
            }
        }
    }
}
