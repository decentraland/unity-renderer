using System;
using UnityEngine;

namespace UnityGLTF
{
    internal class DownloadQueueHandler : IDisposable
    {
        private static event Action OnCompetitionReset;
        private static GLTFComponent nearestComponent;
        private static float nearestDistance = float.MaxValue;
        private static int competingCount;
        private static bool isCompetitionFinished = false;

        private bool hasCompeted = false;

        private readonly GLTFComponent gltfComponent;
        private readonly Camera mainCamera;

        public DownloadQueueHandler(GLTFComponent component)
        {
            gltfComponent = component;
            mainCamera = Camera.main;
            OnCompetitionReset += OnResetCompetition;
        }

        public void Dispose()
        {
            OnCompetitionReset -= OnResetCompetition;

            if (gltfComponent == null)
                return;

            if (gltfComponent == nearestComponent)
            {
                ResetCompetition();
            }
            else if (gltfComponent.isInQueue)
            {
                CheckCompetitionFinished();
            }
        }

        public bool IsNextInQueue()
        {
            //NOTE: start all downloads while renderer is disabled
            if (mainCamera == null)
            {
                return true;
            }

            if (GLTFComponent.downloadingCount >= GLTFComponent.maxSimultaneousDownloads)
            {
                return false;
            }

            if (!hasCompeted)
            {
                Compete();
            }

            return ShouldDownload();
        }

        private void Compete()
        {
            hasCompeted = true;
            competingCount++;

            Vector3 cameraPosition = mainCamera.transform.position;
            Vector3 gltfPosition = gltfComponent.transform.position;
            gltfPosition.y = cameraPosition.y;

            float dist = (gltfPosition - cameraPosition).sqrMagnitude;

            if (dist < nearestDistance)
            {
                nearestDistance = dist;
                nearestComponent = gltfComponent;
            }

            CheckCompetitionFinished();
        }

        private bool ShouldDownload()
        {
            if (!isCompetitionFinished)
                return false;

            bool result = nearestComponent == gltfComponent;
            if (result)
            {
                ResetCompetition();
            }
            return result;
        }

        private void OnResetCompetition()
        {
            hasCompeted = false;
        }

        private static bool CheckCompetitionFinished()
        {
            isCompetitionFinished = competingCount >= GLTFComponent.queueCount;
            return isCompetitionFinished;
        }

        private static void ResetCompetition()
        {
            nearestDistance = float.MaxValue;
            nearestComponent = null;
            isCompetitionFinished = false;
            competingCount = 0;
            OnCompetitionReset?.Invoke();
        }
    }
}