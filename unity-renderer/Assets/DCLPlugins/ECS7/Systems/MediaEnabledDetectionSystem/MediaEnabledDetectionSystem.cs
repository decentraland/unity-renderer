using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using System;
using UnityEngine;

namespace ECSSystems.MediaEnabledDetectionSystem
{
    public class MediaEnabledDetectionSystem : IDisposable
    {
        private IInternalECSComponent<InternalMediaEnabledTag> mediaEnabledTagComponent;
        private BaseList<IParcelScene> loadedScenes;
        private bool hasUserInteraction = false;

        public MediaEnabledDetectionSystem(IInternalECSComponent<InternalMediaEnabledTag> mediaEnabledTagComponent,
            BaseList<IParcelScene> loadedScenes)
        {
            this.mediaEnabledTagComponent = mediaEnabledTagComponent;
            this.loadedScenes = loadedScenes;

            loadedScenes.OnAdded += OnSceneAdded;
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= OnSceneAdded;
        }

        public void Update()
        {
            if (!hasUserInteraction)
            {
                if (Input.anyKeyDown)
                {
                    hasUserInteraction = true;

                    for (int i = 0; i < loadedScenes.Count; i++)
                    {
                        mediaEnabledTagComponent.PutFor(loadedScenes[i], SpecialEntityId.SCENE_ROOT_ENTITY, new InternalMediaEnabledTag());
                    }
                }
            }
        }

        private void OnSceneAdded(IParcelScene scene)
        {
            if (!hasUserInteraction)
            {
                mediaEnabledTagComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalMediaEnabledTag());
            }
        }
    }
}
