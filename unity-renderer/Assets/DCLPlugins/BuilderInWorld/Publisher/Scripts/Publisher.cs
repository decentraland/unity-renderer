using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public class Publisher : IPublisher
    {
        private IPublishProjectController projectPublisher;
        private IPublicationDetailsController landPublisher;

        public void Initialize()
        {
            projectPublisher = new PublishProjectController();
            landPublisher = new PublicationDetailsController();

            landPublisher.Initialize();
            projectPublisher.Initialize();
        }

        public void Dipose()
        {
            projectPublisher.Dispose();
            landPublisher.Dispose();
        }

        public void Publish(IBuilderScene scene)
        {
            switch (scene.sceneType)
            {
                case IBuilderScene.SceneType.PROJECT:
                    projectPublisher.StartPublishFlow(scene);
                    break;
                case IBuilderScene.SceneType.LAND:
                    // landPublisher.Publish();
                    break;
                default:
                    Debug.Log("This should no appear, the scene should have a know type!");
                    break;
            }
        }
    }
}