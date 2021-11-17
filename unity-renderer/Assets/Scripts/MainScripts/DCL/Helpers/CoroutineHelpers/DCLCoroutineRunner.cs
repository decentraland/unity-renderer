using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Based on work from: http://JacksonDunstan.com/articles/3718
    /// </summary>
    public static class DCLCoroutineRunner
    {
        public static Func<float> realtimeSinceStartup => () => Time.realtimeSinceStartup;

        /// <summary>
        /// Run an iterator function that might throw an exception. Call the callback with the exception
        /// if it does or null if it finishes without throwing an exception.
        /// </summary>
        /// <param name="enumerator">Iterator function to run</param>
        /// <param name="done">Callback to call when the iterator has thrown an exception or finished.
        /// The thrown exception or null is passed as the parameter.</param>
        /// <param name="timeBudgetCounter">A func that takes elapsed time as parameter, and returns a bool
        /// indicating if a frame should be skipped or not. Use this in combination with ThrottlingCounter.EvaluateTimeBudget().
        /// If this is passed as null, no time budget will be used.</param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        public static IEnumerator Run(
            IEnumerator enumerator,
            Action<Exception> done,
            Func<double, bool> timeBudgetCounter = null
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
    }

    /// <summary>
    /// When a coroutine is started with throttling, yielding this object
    /// will make the frame skip ONLY if the time budget is exceeded.
    ///
    /// If the time budget is not exceeded, no frames will be skipped by yielding this object.
    /// </summary>
    public class SkipFrameIfDepletedTimeBudget : CustomYieldInstruction
    {
        public override bool keepWaiting => false;
    }
}