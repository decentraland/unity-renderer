using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Utility functions to handle exceptions thrown from coroutine and iterator functions
    /// http://JacksonDunstan.com/articles/3718
    /// </summary>
    public static class CoroutineUtils
    {
        /// <summary>
        /// Start a coroutine that might throw an exception. Call the callback with the exception if it
        /// does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="monoBehaviour">MonoBehaviour to start the coroutine on</param>
        /// <param name="enumerator">Iterator function to run as the coroutine</param>
        /// <param name="onFinish">Callback to call when the coroutine has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <returns>The started coroutine</returns>
        public static Coroutine StartThrottledCoroutine(
            this MonoBehaviour monoBehaviour,
            IEnumerator enumerator,
            Action<Exception> onFinish,
            Func<double, bool> timeBudgetCounter
        )
        {
            return monoBehaviour.StartCoroutine(RunThrowingIterator(enumerator, onFinish, timeBudgetCounter));
        }


        /// <summary>
        /// Start a coroutine that might throw an exception. Call the callback with the exception if it
        /// does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="monoBehaviour">MonoBehaviour to start the coroutine on</param>
        /// <param name="enumerator">Iterator function to run as the coroutine</param>
        /// <param name="done">Callback to call when the coroutine has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <returns>The started coroutine</returns>
        public static Coroutine StartThrowingCoroutine(
            this MonoBehaviour monoBehaviour,
            IEnumerator enumerator,
            Action<Exception> done
        )
        {
            return monoBehaviour.StartCoroutine(RunThrowingIterator(enumerator, done, null));
        }

        public static Func<float> realtimeSinceStartup => () => Time.realtimeSinceStartup;

        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="done">Callback to call when the iterator has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        public static IEnumerator RunThrowingIterator(
            IEnumerator enumerator,
            Action<Exception> done,
            Func<double, bool> timeBudgetCounter
        )
        {
            float currentTime = realtimeSinceStartup();
            // The enumerator might yield return enumerators, in which case 
            // we need to enumerate those here rather than yield-returning 
            // them. Otherwise, any exceptions thrown by those "inner enumerators"
            // would actually escape to an outer level of iteration, outside this 
            // code here, and not be passed to the done callback.
            // So, this stack holds any inner enumerators.
            var stack = new Stack<IEnumerator>();
            stack.Push(enumerator);

            while (stack.Count > 0)
            {
                // any inner enumerator will be at the top of the stack
                // otherwise the original one
                var currentEnumerator = stack.Peek();
                // this is what get "yield returned" in the work enumerator
                object currentYieldedObject;
                // the contents of this try block run the work enumerator until
                // it gets to a yield return statement
                try
                {
                    if (currentEnumerator.MoveNext() == false)
                    {
                        // in this case, the enumerator has finished
                        stack.Pop();
                        // if the stack is empty, then everything has finished,
                        // and the while (stack.Count &gt; 0) will pick it up
                        continue;
                    }

                    currentYieldedObject = currentEnumerator.Current;
                }
                catch (Exception ex)
                {
                    // this part is the whole point of this method!
                    done(ex);
                    yield break;
                }

                float elapsedTime = realtimeSinceStartup() - currentTime;
                currentTime = realtimeSinceStartup();

                // NOTE: SkipFrameIfDepletedTimeBudget object type is used as a special token here and will not
                // yield unless the time budget is exceeded for this frame.
                //
                // Handling the time budget frame skip for yield return null; calls was also considered.
                // But this means that yield return null; will no longer skip a frame unless the time budget
                // is exceeded. If the user wanted to skip the frame explicitly this detail would change
                // the intended behaviour and introduce bugs.
                bool handleTimeBudget = timeBudgetCounter != null && currentYieldedObject is SkipFrameIfDepletedTimeBudget;

                if ( handleTimeBudget )
                {
                    if ( timeBudgetCounter( elapsedTime ) )
                    {
                        yield return null;
                        currentTime = realtimeSinceStartup();
                    }

                    continue;
                }

                // in unity you can yield return whatever the hell you want,
                // so this will pick up whether it's something to enumerate 
                // here, or pass through by yield returning it
                if (currentYieldedObject is IEnumerator)
                {
                    stack.Push(currentYieldedObject as IEnumerator);
                }
                else
                {
                    yield return currentYieldedObject;
                    currentTime = realtimeSinceStartup();

                    // Force reset of time budget if a frame is skipped on purpose
                    if ( timeBudgetCounter != null )
                        timeBudgetCounter( double.MaxValue );
                }
            }
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
    }


    /// <summary>
    /// When a coroutine is started by using StartThrottledCoroutine, yielding this object
    /// will make the frame skip if the time budget is exceeded only.
    ///
    /// If the time budget is not exceeded, no frames will be skipped by yielding this object.
    /// </summary>
    public class SkipFrameIfDepletedTimeBudget : CustomYieldInstruction
    {
        public override bool keepWaiting => false;
    }

    // Suspends the coroutine execution until the supplied delegate evaluates to true or the timeout is reached.
    public class WaitUntil : CustomYieldInstruction
    {
        Func<bool> predicate;
        float waitTime;
        bool waitEnabled = false;

        public WaitUntil(Func<bool> predicate, float? timeoutInSeconds = null)
        {
            this.predicate = predicate;

            if (timeoutInSeconds.HasValue)
            {
                waitEnabled = true;
                waitTime = Time.realtimeSinceStartup + timeoutInSeconds.Value;
            }
        }

        public override bool keepWaiting
        {
            get
            {
                if (waitEnabled)
                    return !predicate() && Time.realtimeSinceStartup < waitTime;
                else
                    return !predicate();
            }
        }
    }
}