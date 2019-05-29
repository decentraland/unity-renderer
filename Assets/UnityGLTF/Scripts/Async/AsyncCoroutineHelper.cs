using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGLTF
{
    public class AsyncCoroutineHelper : MonoBehaviour
    {
        private Queue<CoroutineInfo> queuedActions = new Queue<CoroutineInfo>();
        private List<CoroutineInfo> runningActions = new List<CoroutineInfo>();

        public int runningActionsCount { get { return runningActions != null ? runningActions.Count : 0; } }
        public int queuedActionsCount { get { return queuedActions != null ? queuedActions.Count : 0; } }

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

        private IEnumerator CallMethodOnMainThread(CoroutineInfo coroutineInfo)
        {
            yield return coroutineInfo.Coroutine;
            coroutineInfo.finished = true;
            runningActions.Remove(coroutineInfo);
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
            if (queuedActions.Count > 0)
            {
                CoroutineInfo coroutineInfo = null;
                coroutineInfo = queuedActions.Dequeue();

                if (coroutineInfo != null)
                {
                    runningActions.Add(coroutineInfo);
                    StartCoroutine(CallMethodOnMainThread(coroutineInfo));
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
