using System.Collections;
using DCL;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ThrottlingRunnerShould
{
    private ThrottlingCounter counter;
    private float currentMockedTime;
    private int lastFrame;

    float TimeMocker()
    {
        // Fake random time pass each time the time is queried by the coroutine runner
        currentMockedTime += Random.Range(0.0001f, 0.0002f);

        if ( lastFrame != Time.frameCount )
        {
            // Fake new frame elapsed time. 
            currentMockedTime += Random.Range(0.010f, 0.016f);
            lastFrame = Time.frameCount;
        }

        return currentMockedTime;
    }

    [SetUp]
    public void SetUp()
    {
        // The time is just mocked to avoid flaky tests due to erratic running times on CI.
        DCLCoroutineRunner.realtimeSinceStartup = TimeMocker;
    }

    [TearDown]
    public void TearDown()
    {
        DCLCoroutineRunner.realtimeSinceStartup = () => Time.realtimeSinceStartup;
    }

    [UnityTest]
    public IEnumerator SkipFramesAccordingToBudgetWithMultipleCases()
    {
        counter = new ThrottlingCounter();
        counter.enabled = true;

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

        currentMockedTime = 0;
        lastFrame = Time.frameCount;

        int frameCount = Time.frameCount;
        int elapsedFrames = 0;

        yield return DCLCoroutineRunner.Run(ThrottlingTest(totalMsDuration), null, counter.EvaluateTimeBudget);

        elapsedFrames = Time.frameCount - frameCount;

        int elapsedFramesAccuracy = Mathf.Abs(elapsedFrames - framesThatShouldBeSkipped);

        Assert.That( elapsedFramesAccuracy, Is.LessThanOrEqualTo(errorThreshold), $"An approx. value of {framesThatShouldBeSkipped} frames must be skipped when throttling {totalMsDuration} ms with {budgetMs} ms as time budget! (elapsed: {elapsedFrames} - error threshold: {errorThreshold})");
    }

    IEnumerator ThrottlingTest(double msTotalDuration)
    {
        var skipFrameCoroutine = new SkipFrameIfDepletedTimeBudget();
        yield return skipFrameCoroutine;

        double startTime = currentMockedTime;
        double secsDuration = msTotalDuration / 1000.0;

        while (currentMockedTime - startTime < secsDuration)
        {
            int lastFrame = Time.frameCount;
            float mockedStartTime = currentMockedTime;

            yield return skipFrameCoroutine;

            // This test must calculate the skipped frames between a interval, but this
            // interval shouldn't take into account the frame delta.

            // To account for this error, a new frame delta time is added to the total duration.
            if ( lastFrame != Time.frameCount )
                secsDuration += currentMockedTime - mockedStartTime;
        }
    }
}