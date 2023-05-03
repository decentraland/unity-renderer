using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AssetPromiseKeeper
    {
        public static float PROCESS_PROMISES_TIME_BUDGET = 0.006f;
    }

    /// <summary>
    /// The AssetPromiseKeeper is the user entry point interface of the asset management system.
    /// It manages stuff like requesting something that's already being loaded, etc.
    ///
    /// It also handles the special cases of promises that depend on another to be completed (blocked promise)
    /// </summary>
    /// <typeparam name="AssetType">Asset type to be handled</typeparam>
    /// <typeparam name="AssetLibraryType">Asset library type. It must handle the same type as AssetType.</typeparam>
    /// <typeparam name="AssetPromiseType">Asset promise type. It must handle the same type as AssetType.</typeparam>
    public class AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>
        where AssetType: Asset, new()
        where AssetLibraryType: AssetLibrary<AssetType>, new()
        where AssetPromiseType: AssetPromise<AssetType>
    {
        public AssetLibraryType library;

        //NOTE(Brian): All waiting promises. Only used for cleanup and to keep count.
        HashSet<AssetPromiseType> waitingPromises = new HashSet<AssetPromiseType>();
        public int waitingPromisesCount => waitingPromises.Count;

        //NOTE(Brian): List of promises waiting for assets not in library.
        protected Dictionary<object, AssetPromiseType> masterPromiseById = new Dictionary<object, AssetPromiseType>(100);

        //NOTE(Brian): List of promises waiting for assets that are currently being loaded by another promise.
        HashSet<AssetPromiseType> blockedPromises = new HashSet<AssetPromiseType>();

        //NOTE(Brian): Master promise id -> blocked promises HashSet
        protected Dictionary<object, HashSet<AssetPromiseType>> masterToBlockedPromises = new Dictionary<object, HashSet<AssetPromiseType>>(100);

        public bool useTimeBudget => CommonScriptableObjects.rendererState.Get();

        float startTime;

        private CancellationTokenSource blockedPromiseSolverCancellationTokenSource;

        public bool IsBlocked(AssetPromiseType promise)
        {
            return blockedPromises.Contains(promise);
        }

        public string GetMasterState(AssetPromiseType promise)
        {
            object promiseId = promise.GetId();

            if (!masterToBlockedPromises.ContainsKey(promiseId))
                return "Master not found";

            if (!masterToBlockedPromises[promiseId].Contains(promise))
                return "Promise is not blocked???";

            if (!masterPromiseById.ContainsKey(promiseId))
                return "not registered as master?";

            return $"master state = {masterPromiseById[promiseId].state}";
        }

        public AssetPromiseKeeper(AssetLibraryType library)
        {
            this.library = library;
            EnsureProcessBlockerPromiseQueueTask();
        }

        private void EnsureProcessBlockerPromiseQueueTask()
        {
            if (blockedPromiseSolverCancellationTokenSource == null)
            {
                blockedPromiseSolverCancellationTokenSource = new CancellationTokenSource();
                ProcessBlockedPromisesQueueAsync(blockedPromiseSolverCancellationTokenSource.Token).Forget();
            }
        }

        public AssetPromiseType Keep(AssetPromiseType promise)
        {
            if (promise == null || promise.state != AssetPromiseState.IDLE_AND_EMPTY || waitingPromises.Contains(promise))
                return promise;

            object id = promise.GetId();

            if (id == null)
            {
                Debug.LogError("ERROR: ID == null. Promise is not set up correctly.");
                return promise;
            }

            promise.isDirty = true;

            //NOTE(Brian): We already have a master promise for this id, add to blocked list.
            if (masterPromiseById.ContainsKey(id))
            {
                // TODO(Brian): Remove once this class has a proper lifecycle and is on service locator.
                EnsureProcessBlockerPromiseQueueTask();
                waitingPromises.Add(promise);

                if (!masterToBlockedPromises.ContainsKey(id))
                    masterToBlockedPromises.Add(id, new HashSet<AssetPromiseType>());

                masterToBlockedPromises[id].Add(promise);

                blockedPromises.Add(promise);
                promise.SetWaitingState();
                return promise;
            }

            // NOTE(Brian): Not in library, add to corresponding lists...
            if (!library.Contains(id))
            {
                waitingPromises.Add(promise);
                masterPromiseById.Add(id, promise);
            }

            promise.library = library;
            promise.OnPreFinishEvent += OnRequestCompleted;
            promise.Load();

            return promise;
        }

        public virtual AssetPromiseType Forget(AssetPromiseType promise)
        {
            if (promise == null)
                return null;

            if (promise.state == AssetPromiseState.IDLE_AND_EMPTY || promise.state == AssetPromiseState.WAITING)
            {
                CleanPromise(promise);
                promise.OnForget();
                return promise;
            }

            object id = promise.GetId();

            bool isMasterPromise = masterPromiseById.ContainsKey(id) && masterPromiseById[id] == promise;
            bool hasBlockedPromises = masterToBlockedPromises.ContainsKey(id) && masterToBlockedPromises[id].Count > 0;

            if (isMasterPromise && hasBlockedPromises)
            {
                //NOTE(Brian): Pending promises are waiting for this one.
                //             We clear the events because we shouldn't call them, as this promise is forgotten.
                OnSilentForget(promise);
                promise.OnForget();
                return promise;
            }

            promise.Unload();
            CleanPromise(promise);
            promise.OnForget();

            return promise;
        }

        Queue<AssetPromiseType> toResolveBlockedPromisesQueue = new Queue<AssetPromiseType>();

        private void OnRequestCompleted(AssetPromise<AssetType> loadedPromise)
        {
            object id = loadedPromise.GetId();

            if (!masterToBlockedPromises.ContainsKey(id) || !masterPromiseById.ContainsKey(id))
            {
                CleanPromise(loadedPromise);
                return;
            }

            toResolveBlockedPromisesQueue.Enqueue(loadedPromise as AssetPromiseType);
        }

        private bool IsToResolveQueueEmpty()
        {
            return toResolveBlockedPromisesQueue.Count <= 0;
        }

        private async UniTask ProcessBlockedPromisesQueueAsync(CancellationToken cancellationToken)
        {
            startTime = Time.unscaledTime;

            while (true)
            {
                await UniTask.WaitWhile(IsToResolveQueueEmpty, PlayerLoopTiming.Update, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                AssetPromiseType promise = toResolveBlockedPromisesQueue.Dequeue();
                await ProcessBlockedPromisesDeferredAsync(promise, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                CleanPromise(promise);

                if (promise.isForgotten) { promise.Unload(); }

                await SkipFrameIfOverBudgetAsync();
            }
        }

        private async UniTask ProcessBlockedPromisesDeferredAsync(AssetPromiseType loadedPromise, CancellationToken cancellationToken)
        {
            object loadedPromiseId = loadedPromise.GetId();

            if (!masterToBlockedPromises.ContainsKey(loadedPromiseId))
                return;

            if (!masterPromiseById.ContainsKey(loadedPromiseId))
                return;

            if (masterPromiseById[loadedPromiseId] != loadedPromise)
            {
                Debug.LogWarning($"Unexpected issue: masterPromiseById promise isn't the same as loaded promise? id: {loadedPromiseId} (can be harmless)");
                return;
            }

            //NOTE(Brian): We have to keep checking to support the case in which
            //             new promises are enqueued while this promise ID is being
            //             resolved.
            while (masterToBlockedPromises.ContainsKey(loadedPromiseId) &&
                   masterToBlockedPromises[loadedPromiseId].Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<AssetPromiseType> promisesToLoadForId = GetBlockedPromisesToLoadForId(loadedPromiseId);

                await SkipFrameIfOverBudgetAsync();

                cancellationToken.ThrowIfCancellationRequested();

                if (loadedPromise.state != AssetPromiseState.FINISHED)
                    await ForceFailPromiseListAsync(promisesToLoadForId);
                else
                    await LoadPromisesListAsync(promisesToLoadForId);

                await SkipFrameIfOverBudgetAsync();
            }

            if (masterToBlockedPromises.ContainsKey(loadedPromiseId))
                masterToBlockedPromises.Remove(loadedPromiseId);
        }

        private async UniTask SkipFrameIfOverBudgetAsync()
        {
            if (useTimeBudget && Time.realtimeSinceStartup - startTime >= AssetPromiseKeeper.PROCESS_PROMISES_TIME_BUDGET)
            {
                await UniTask.Yield();
                startTime = Time.unscaledTime;
            }
        }

        private List<AssetPromiseType> GetBlockedPromisesToLoadForId(object masterPromiseId)
        {
            var blockedPromisesToLoadAux = new List<AssetPromiseType>();

            using (var iterator = masterToBlockedPromises[masterPromiseId].GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var blockedPromise = iterator.Current;

                    blockedPromises.Remove(blockedPromise);

                    if (blockedPromise != null && !blockedPromise.isForgotten) { blockedPromisesToLoadAux.Add(blockedPromise); }
                }
            }

            return blockedPromisesToLoadAux;
        }

        private async UniTask ForceFailPromiseListAsync(List<AssetPromiseType> promises)
        {
            int promisesCount = promises.Count;

            for (int i = 0; i < promisesCount; i++)
            {
                var promise = promises[i];

                if (promise.isForgotten)
                    continue;

                promise.ForceFail(new PromiseForgottenException("Promise is forgotten"));
                Forget(promise);
                CleanPromise(promise);

                await SkipFrameIfOverBudgetAsync();
            }
        }

        private async UniTask LoadPromisesListAsync(List<AssetPromiseType> promises)
        {
            int promisesCount = promises.Count;

            for (int i = 0; i < promisesCount; i++)
            {
                AssetPromiseType promise = promises[i];

                if (promise.isForgotten)
                    continue;

                promise.library = library;
                CleanPromise(promise);
                promise.Load();

                await SkipFrameIfOverBudgetAsync();
            }
        }

        void CleanPromise(AssetPromise<AssetType> promise)
        {
            if (!promise.isDirty)
                return;

            promise.isDirty = false;

            AssetPromiseType finalPromise = promise as AssetPromiseType;

            object id = promise.GetId();

            if (masterToBlockedPromises.ContainsKey(id))
            {
                if (masterToBlockedPromises[id].Contains(finalPromise)) { masterToBlockedPromises[id].Remove(finalPromise); }
            }

            if (masterPromiseById.ContainsKey(id) && masterPromiseById[id] == promise) { masterPromiseById.Remove(id); }

            if (blockedPromises.Contains(finalPromise))
                blockedPromises.Remove(finalPromise);

            if (waitingPromises.Contains(finalPromise))
                waitingPromises.Remove(finalPromise);
        }

        public void Cleanup()
        {
            if (blockedPromiseSolverCancellationTokenSource != null)
            {
                blockedPromiseSolverCancellationTokenSource.Cancel();
                blockedPromiseSolverCancellationTokenSource.Dispose();
                blockedPromiseSolverCancellationTokenSource = null;
            }

            blockedPromises = new HashSet<AssetPromiseType>();
            masterToBlockedPromises = new Dictionary<object, HashSet<AssetPromiseType>>();

            using (var e = waitingPromises.GetEnumerator())
            {
                while (e.MoveNext())
                    e.Current?.Cleanup();
            }

            foreach (var kvp in masterPromiseById) { kvp.Value.Cleanup(); }

            masterPromiseById = new Dictionary<object, AssetPromiseType>();
            waitingPromises = new HashSet<AssetPromiseType>();
            library.Cleanup();
        }

        protected virtual void OnSilentForget(AssetPromiseType promise) { }
    }

    public class PromiseForgottenException : Exception
    {
        public PromiseForgottenException(string message) : base(message) { }
    }
}
