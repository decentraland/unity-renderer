using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public class Publisher : IPublisher
    {
        private IPublishProjectController projectPublisher;
        private ILandPublisherController landPublisher;

        public void Initialize(IContext context)
        {
            projectPublisher = new PublishProjectController();
            landPublisher = new LandPublisherController();

            landPublisher.Initialize();
            projectPublisher.Initialize();
        }

        public void Dipose()
        {
            projectPublisher.Dispose();
            landPublisher.Dispose();
        }

        public void StartPublish(IBuilderScene scene)
        {
            switch (scene.sceneType)
            {
                case IBuilderScene.SceneType.PROJECT:
                    projectPublisher.StartPublishFlow(scene);
                    break;
                case IBuilderScene.SceneType.LAND:
                    landPublisher.StartPublishFlow(scene);
                    break;
                default:
                    Debug.Log("This should no appear, the scene should have a know type!");
                    break;
            }
        }

        public void PublishProject() { }
    }
}