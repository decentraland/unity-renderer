using System.Collections;
using DCL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ThrottlingRunnerShould
{
    private ThrottlingCounter counter;

    [UnityTest]
    public IEnumerator SkipFramesAccordingToBudgetWithMultipleCases()
    {
        counter = new ThrottlingCounter();
        counter.enabled = true;
        counter.evaluationTimeElapsedCap = 5 / 1000.0;

        yield return SkipFramesAccordingToBudget(100, 2, 50, 5);
        yield return SkipFramesAccordingToBudget(100, 10, 10, 3);
        yield return SkipFramesAccordingToBudget(100, 5, 20, 5);
    }

    [UnityTest]
    public IEnumerator NotSkipFramesIfThrottlingCounterIsDisabled()
    {
        counter = new ThrottlingCounter();
        counter.enabled = false;

        int budgetMs = 1;
        int totalMsDuration = 100;

        counter.budgetPerFrame = budgetMs / 1000.0;
        counter.Reset();

        int frameCount = Time.frameCount;
        yield return DCLCoroutineRunner.Run(ThrottlingTest(totalMsDuration), null, counter.EvaluateTimeBudget);

        int elapsedFrames = Time.frameCount - frameCount;
        Assert.That( elapsedFrames, Is.EqualTo(1), $"No frames must be skipped when throttling is disabled!");
    }

    public IEnumerator SkipFramesAccordingToBudget(int totalMsDuration, int budgetMs, int framesThatShouldBeSkipped, int errorThreshold)
    {
        counter.budgetPerFrame = budgetMs / 1000.0;
        counter.Reset();

        int frameCount = Time.frameCount;
        int elapsedFrames = 0;

        yield return DCLCoroutineRunner.Run(ThrottlingTest(totalMsDuration), null, counter.EvaluateTimeBudget);

        elapsedFrames = Time.frameCount - frameCount;

        int elapsedFramesAccuracy = Mathf.Abs(elapsedFrames - framesThatShouldBeSkipped);

        Assert.That( elapsedFramesAccuracy, Is.LessThanOrEqualTo(errorThreshold), $"An approx. value of {framesThatShouldBeSkipped} frames must be skipped when throttling {totalMsDuration} ms with {budgetMs} ms as time budget! (error threshold: {errorThreshold})");
    }

    IEnumerator ThrottlingTest(double msTotalDuration)
    {
        var skipFrameCoroutine = new SkipFrameIfDepletedTimeBudget();
        yield return skipFrameCoroutine;

        double startTime = Time.realtimeSinceStartupAsDouble;
        double secsDuration = msTotalDuration / 1000.0;

        while (Time.realtimeSinceStartupAsDouble - startTime < secsDuration)
        {
            int frameCount = Time.frameCount;
            double frameStart = Time.realtimeSinceStartupAsDouble;
            yield return skipFrameCoroutine;

            // We skipped a frame, so we add this frame time to the total time evaluation
            // This way, we can ensure the skipped frames can be evaluated consistently no matter
            // FPS values.
            if ( frameCount != Time.frameCount )
                secsDuration += Time.realtimeSinceStartupAsDouble - frameStart;
        }
    }
}