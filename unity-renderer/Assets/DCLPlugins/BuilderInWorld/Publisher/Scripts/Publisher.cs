using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Builder
{
    public class Publisher : IPublisher
    {
        private IPublishProjectController projectPublisher;

        public Publisher() { projectPublisher = new PublishProjectController(); }

        public void Initialize() { }

        public void Dipose() { }

        public void Publish(IBuilderScene scene) { projectPublisher.StartPublishFlow(scene); }
    }
}