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
            Dictionary<Collider, int> physicsColliders = sbcComponentModel.physicsColliders;
            Dictionary<Collider, int> pointerColliders = sbcComponentModel.pointerColliders;

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
                foreach (var keyValuePair in physicsColliders)
                {
                    keyValuePair.Key.enabled = isInsideBounds;
                }
            }

            if (pointerColliders != null)
            {
                foreach (var keyValuePair in pointerColliders)
                {
                    keyValuePair.Key.enabled = isInsideBounds;
                }
            }
        }
    }
}
