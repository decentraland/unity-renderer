using NUnit.Framework;
using UnityEngine;

public class RectsOverlappingTrackerShould
{

    [Test]
    [TestCase(0.5f)]
    [TestCase(0.7f)]
    public void BeConstructedProperly(float overlappingTolerance)
    {
        var overlappingTracker = new RectsOverlappingTracker(overlappingTolerance);

        Assert.IsEmpty(overlappingTracker.rects);
        Assert.AreEqual(overlappingTolerance, overlappingTracker.overlappingTolerance);
    }

    [Test]
    public void ClampTolerance_0()
    {
        var overlappingTracker = new RectsOverlappingTracker(-1);
        Assert.AreEqual(0, overlappingTracker.overlappingTolerance);
    }

    [Test]
    public void ClampTolerance_1()
    {
        var overlappingTracker = new RectsOverlappingTracker(3);
        Assert.AreEqual(1, overlappingTracker.overlappingTolerance);
    }

    [Test]
    public void CalculateIntersectionAreaProperly()
    {
        Rect r1;
        Rect r2;

        //Upper right intersection
        r1 = new Rect(0, 0, 2, 2);
        r2 = new Rect(1, 1, 2, 2);
        Assert.IsTrue(RectsOverlappingTracker.TryGetIntersectionArea(r1, r2, out float area0));
        Assert.AreEqual(1, area0);

        //Upper left intersection
        r1 = new Rect(0, 0, 2, 2);
        r2 = new Rect(-1, 1, 2, 2);
        Assert.IsTrue(RectsOverlappingTracker.TryGetIntersectionArea(r1, r2, out float area1));
        Assert.AreEqual(1, area1);

        //Lower right intersection
        r1 = new Rect(0, 0, 2, 2);
        r2 = new Rect(1, -1, 2, 2);
        Assert.IsTrue(RectsOverlappingTracker.TryGetIntersectionArea(r1, r2, out float area2));
        Assert.AreEqual(1, area2);

        //Lower left intersection
        r1 = new Rect(0, 0, 2, 2);
        r2 = new Rect(-1, -1, 2, 2);
        Assert.IsTrue(RectsOverlappingTracker.TryGetIntersectionArea(r1, r2, out float area3));
        Assert.AreEqual(1, area3);
    }

    [Test]
    public void CalculateIntersectionAreaIfNoOverlap()
    {
        Rect r1 = new Rect(0, 0, 1, 1);
        Rect r2 = new Rect(10, 10, 1, 1);

        Assert.IsFalse(RectsOverlappingTracker.TryGetIntersectionArea(r1, r2, out float area));
    }

    [Test]
    public void CalculateIntersectionAreaIfNoOverlap_TouchingBorer()
    {
        Rect r1 = new Rect(0, 0, 1, 1);
        Rect r2 = new Rect(1, 0, 1, 1);

        Assert.IsFalse(RectsOverlappingTracker.TryGetIntersectionArea(r1, r2, out float area));
    }

    [Test]
    public void AddRectIfOverlappingOverTolerance()
    {
        var overlappingTracker = new RectsOverlappingTracker(0.26f);
        Rect r1 = new Rect(0, 0, 2, 2);

        Assert.IsTrue(overlappingTracker.RegisterRect(r1));
        Assert.AreEqual(1, overlappingTracker.rects.Count);

        Rect r2 = new Rect(1, 1, 2, 2);
        Assert.IsTrue(overlappingTracker.RegisterRect(r2));
        Assert.AreEqual(2, overlappingTracker.rects.Count);
    }

    [Test]
    public void AddRectIfOverlappingUnderTolerance()
    {
        var overlappingTracker = new RectsOverlappingTracker(0.24f);
        Rect r1 = new Rect(0, 0, 2, 2);

        Assert.IsTrue(overlappingTracker.RegisterRect(r1));
        Assert.AreEqual(1, overlappingTracker.rects.Count);

        Rect r2 = new Rect(1, 1, 2, 2);
        Assert.IsFalse(overlappingTracker.RegisterRect(r2));
        Assert.AreEqual(1, overlappingTracker.rects.Count);
    }

    [Test]
    public void AddRectIfNoOverlapping()
    {
        var overlappingTracker = new RectsOverlappingTracker(0.24f);
        Rect r1 = new Rect(0, 0, 2, 2);

        Assert.IsTrue(overlappingTracker.RegisterRect(r1));
        Assert.AreEqual(1, overlappingTracker.rects.Count);

        Rect r2 = new Rect(10, 10, 2, 2);
        Assert.IsTrue(overlappingTracker.RegisterRect(r2));
        Assert.AreEqual(2, overlappingTracker.rects.Count);
    }

    [Test]
    public void ResetProperly()
    {
        var overlappingTracker = new RectsOverlappingTracker(1);
        overlappingTracker.RegisterRect(new Rect(0, 0, 2, 2));
        overlappingTracker.RegisterRect(new Rect(10, 10, 2, 2));

        overlappingTracker.Reset();

        Assert.IsEmpty(overlappingTracker.rects);
    }
}