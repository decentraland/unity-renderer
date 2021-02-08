using System;
using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using System.Collections;
using UpdateMode = DCL.MapGlobalUsersPositionMarkerController.UpdateMode;

namespace DCL
{
    /// <summary>
    /// Handle the fetch of hot scenes at intervals.
    /// Interval time may change accordingly if set to update in background or foregraound
    /// </summary>
    internal class FetchScenesHandler : IDisposable
    {
        public event Action<List<HotScenesController.HotSceneInfo>> OnScenesFetched;

        float initialIntevalTime;
        float backgroundIntervalTime;
        float foregroundIntervalTime;

        Coroutine updateCoroutine;
        UpdateMode updateMode;

        internal bool isFirstFetch;
        internal float updateInterval;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialIntevalTime">seconds for interval until the first populated scenes are fetched</param>
        /// <param name="foregroundIntervalTime">seconds for interval when update mode is in FOREGROUND</param>
        /// <param name="backgroundIntervalTime">seconds for interval when update mode is in BACKGROUND</param>
        public FetchScenesHandler(float initialIntevalTime, float foregroundIntervalTime, float backgroundIntervalTime)
        {
            this.initialIntevalTime = initialIntevalTime;
            this.backgroundIntervalTime = backgroundIntervalTime;
            this.foregroundIntervalTime = foregroundIntervalTime;
            this.updateInterval = initialIntevalTime;
            this.isFirstFetch = true;
        }

        /// <summary>
        /// Initialize fetch intervals
        /// </summary>
        public void Init()
        {
            if (updateCoroutine != null)
                return;

            updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
        }

        /// <summary>
        /// Set update mode. Scene's fetch intervals will smaller when updating in FOREGROUND than when updating in BACKGROUND
        /// </summary>
        /// <param name="mode">update mode</param>
        public void SetUpdateMode(UpdateMode mode)
        {
            updateMode = mode;
            if (isFirstFetch)
                return;

            switch (updateMode)
            {
                case UpdateMode.BACKGROUND:
                    updateInterval = backgroundIntervalTime;
                    break;
                case UpdateMode.FOREGROUND:
                    updateInterval = foregroundIntervalTime;
                    break;
            }
        }

        public void Dispose()
        {
            CoroutineStarter.Stop(updateCoroutine);

            if (HotScenesController.i)
                HotScenesController.i.OnHotSceneListFinishUpdating -= OnHotSceneListFinishUpdating;
        }

        private IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                float time = Time.realtimeSinceStartup;

                while (Time.realtimeSinceStartup - time < updateInterval)
                {
                    yield return null;
                }

                if (HotScenesController.i.timeSinceLastUpdate > updateInterval)
                {
                    HotScenesController.i.OnHotSceneListFinishUpdating += OnHotSceneListFinishUpdating;
                    WebInterface.FetchHotScenes();
                }
                else
                {
                    OnHotSceneListFinishUpdating();
                }
            }
        }

        private void OnHotSceneListFinishUpdating()
        {
            OnHotScenesFetched(HotScenesController.i.hotScenesList);
        }

        private void OnHotScenesFetched(List<HotScenesController.HotSceneInfo> scenes)
        {
            HotScenesController.i.OnHotSceneListFinishUpdating -= OnHotSceneListFinishUpdating;

            bool fetchSuccess = scenes.Count > 0 && scenes[0].usersTotalCount > 0;

            if (!fetchSuccess)
                return;

            if (isFirstFetch)
            {
                isFirstFetch = false;
                SetUpdateMode(updateMode);
            }

            OnScenesFetched?.Invoke(scenes);
        }
    }
}