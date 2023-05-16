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
            KeyValueSet<Collider, uint> physicsColliders = sbcComponentModel.physicsColliders;
            KeyValueSet<Collider, uint> pointerColliders = sbcComponentModel.pointerColliders;

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
                var pairs = physicsColliders.Pairs;
                for (int i = 0; i < pairs.Count; i++)
                {
                    pairs[i].key.enabled = isInsideBounds;
                }
            }

            if (pointerColliders != null)
            {
                var pairs = pointerColliders.Pairs;
                for (int i = 0; i < pairs.Count; i++)
                {
                    pairs[i].key.enabled = isInsideBounds;
                }
            }
        }
    }
}
