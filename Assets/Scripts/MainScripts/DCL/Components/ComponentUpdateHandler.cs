using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class ComponentUpdateHandler
    {
        protected Queue<string> queue = new Queue<string>();
        public Coroutine routine { get; protected set; }

        public WaitForComponentUpdate yieldInstruction;
        public string oldSerialization { get; protected set; }

        public IComponent owner;

        public bool isRoutineRunning
        {
            get { return routine != null; }
        }

#if UNITY_EDITOR
        bool applyChangesRunning = false;
#endif
        public ComponentUpdateHandler(IComponent owner)
        {
            this.owner = owner;
            this.routine = null;
            yieldInstruction = new WaitForComponentUpdate(owner);
        }

        public void ApplyChangesIfModified(string newSerialization)
        {
            HandleUpdate(newSerialization);
        }

        protected void HandleUpdate(string newSerialization)
        {
            if (newSerialization == oldSerialization) return;

            queue.Enqueue(newSerialization);

            if (!isRoutineRunning)
            {
                var enumerator = HandleUpdateCoroutines();

                if (enumerator != null)
                {
                    routine = CoroutineStarter.Start(enumerator);
                }
            }

            oldSerialization = newSerialization;
        }

        public void Stop()
        {
            if (routine != null)
                CoroutineStarter.Stop(routine);

            routine = null;
#if UNITY_EDITOR
            applyChangesRunning = false;
#endif
        }

        public void Cleanup()
        {
            Stop();

            oldSerialization = null;
            queue.Clear();
        }

        protected IEnumerator HandleUpdateCoroutines()
        {
            while (queue.Count > 0)
            {
                string json = queue.Dequeue();

                IEnumerator updateRoutine = ApplyChangesWrapper(json);

                if (updateRoutine != null)
                {
                    // we don't want to start coroutines if we have early finalization in IEnumerators
                    // ergo, we return null without yielding any result
                    yield return updateRoutine;
                }
            }

            routine = null;
        }


        public virtual IEnumerator ApplyChangesWrapper(string newJson)
        {
#if UNITY_EDITOR
            Assert.IsFalse(applyChangesRunning, "ApplyChanges routine was interrupted when it shouldn't!");
            applyChangesRunning = true;
#endif
            if (owner.IsValid())
            {
                var enumerator = owner.ApplyChanges(newJson);

                if (enumerator != null)
                {
                    yield return enumerator;
                }
            }
#if UNITY_EDITOR
            applyChangesRunning = false;
#endif
            owner.RaiseOnAppliedChanges();
        }
    }
}