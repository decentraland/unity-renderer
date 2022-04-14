using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public static class CoroutineUtils
    {
        /// <summary>
        /// Start a coroutine that might throw an exception. Call the callback with the exception if it
        /// does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="monoBehaviour">MonoBehaviour to start the coroutine on</param>
        /// <param name="enumerator">Iterator function to run as the coroutine</param>
        /// <param name="onException">Callback to call when the coroutine has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <returns>The started coroutine</returns>
        public static Coroutine StartThrottledCoroutine(
            this MonoBehaviour monoBehaviour,
            IEnumerator enumerator,
            Action<Exception> onException,
            Func<double, bool> timeBudgetCounter
        )
        {
            return CoroutineStarter.Start(DCLCoroutineRunner.Run(enumerator, onException, timeBudgetCounter));
        }


        /// <summary>
        /// Start a coroutine that might throw an exception. Call the callback with the exception if it
        /// does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="monoBehaviour">MonoBehaviour to start the coroutine on</param>
        /// <param name="enumerator">Iterator function to run as the coroutine</param>
        /// <param name="onException">Callback to call when the coroutine has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <returns>The started coroutine</returns>
        public static Coroutine StartThrowingCoroutine(
            this MonoBehaviour monoBehaviour,
            IEnumerator enumerator,
            Action<Exception> onException
        )
        {
            return CoroutineStarter.Start(DCLCoroutineRunner.Run(enumerator, onException, null));
        }


        public static IEnumerator WaitForAllIEnumerators(params IEnumerator[] coroutines)
        {
            List<Coroutine> coroutineGroup = new List<Coroutine>();
            for (int i = 0; i < coroutines.Length; i++)
            {
                coroutineGroup.Add(CoroutineStarter.Start(coroutines[i]));
            }

            int coroutineGroupCount = coroutineGroup.Count;
            for (int index = 0; index < coroutineGroupCount; index++)
            {
                var coroutine = coroutineGroup[index];
                yield return coroutine;
            }
        }

        /// <summary>
        /// There are some cases when we need to run a coroutine syncronously for some editor scripts so we use this
        /// </summary>
        /// <param name="coroutine"></param>
        public static void RunCoroutineSync(IEnumerator coroutine)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(coroutine);
            while (stack.Count > 0)
            {
                var enumerator = stack.Pop();
                if (enumerator.MoveNext())
                {
                    stack.Push(enumerator);
                    if (enumerator.Current is IEnumerator subEnumerator)
                    {
                        stack.Push(subEnumerator);
                    }
                }
            }
        }
    }
}