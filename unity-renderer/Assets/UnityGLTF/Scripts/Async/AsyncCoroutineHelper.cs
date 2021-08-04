using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

namespace UnityGLTF
{
    public class AsyncCoroutineHelper : MonoBehaviour
    {
        [SerializeField] private Queue<CoroutineInfo> queuedActions = new Queue<CoroutineInfo>();
        [SerializeField] private List<CoroutineInfo> runningActions = new List<CoroutineInfo>();

        public int runningActionsCount { get { return runningActions != null ? runningActions.Count : 0; } }
        public int queuedActionsCount { get { return queuedActions != null ? queuedActions.Count : 0; } }

        private int DEBUG_pendingActions = 0;
        private int DEBUG_runningActions = 0;

        public CoroutineInfo RunAsTask(IEnumerator coroutine, string name)
        {
            queuedActions.Enqueue(
                new CoroutineInfo
                {
                    Coroutine = coroutine,
                    finished = false
                }
            );

            return queuedActions.Peek();
        }

        private static int calls = 0;
        private int call2 = 0;

        private IEnumerator CallMethodOnMainThread(CoroutineInfo coroutineInfo)
        {
            call2++;
            Debug.Log($"PATO: CallMethodOnMainThread start {++calls}");
            yield return coroutineInfo.Coroutine;
            Debug.Log($"PATO: CallMethodOnMainThread end {--calls}");
            call2--;
            coroutineInfo.finished = true;
            runningActions.Remove(coroutineInfo);
            DEBUG_runningActions = runningActions.Count;
        }

        public bool AllCoroutinesAreFinished()
        {
            if (runningActions == null || runningActions.Count == 0)
            {
                return true;
            }

            return false;
        }

        private void Update()
        {
            DEBUG_pendingActions = queuedActions.Count;
            if (queuedActions.Count > 0)
            {
                CoroutineInfo coroutineInfo = null;
                coroutineInfo = queuedActions.Dequeue();

                if (coroutineInfo != null)
                {
                    runningActions.Add(coroutineInfo);
                    DEBUG_runningActions = runningActions.Count;
                    this.StartThrowingCoroutine(CallMethodOnMainThread(coroutineInfo), (ex) => Debug.LogError($"PATO: {name} {ex}"));
                }
            }
        }

        public class CoroutineInfo
        {
            public IEnumerator Coroutine;
            public bool finished = false;
        }
    }
}