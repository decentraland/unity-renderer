using NUnit.Framework;
using UnityEngine;

public class SimpleOverlappingTrackerShould
{

    [Test]
    [TestCase(0.5f)]
    [TestCase(0.7f)]
    public void BeConstructedProperly(float overlappingTolerance)
    {
        var overlappingTracker = new SimpleOverlappingTracker(overlappingTolerance);

        Assert.IsEmpty(overlappingTracker.positions);
        Assert.AreEqual(overlappingTolerance * overlappingTolerance, overlappingTracker.sqrTooCloseDistance);
    }

    [Test]
    public void AddPosIfNotTooCloseToOther()
    {
        var overlappingTracker = new SimpleOverlappingTracker(1);

        Assert.IsTrue(overlappingTracker.RegisterPosition(Vector2.zero));
        Assert.AreEqual(1, overlappingTracker.positions.Count);

        Assert.IsTrue(overlappingTracker.RegisterPosition(new Vector2(0, 1.5f)));
        Assert.AreEqual(2, overlappingTracker.positions.Count);
    }

    [Test]
    public void NotAddPosIfTooCloseToOther()
    {
        var overlappingTracker = new SimpleOverlappingTracker(1);

        Assert.IsTrue(overlappingTracker.RegisterPosition(Vector2.zero));
        Assert.AreEqual(1, overlappingTracker.positions.Count);

        Assert.IsFalse(overlappingTracker.RegisterPosition(new Vector2(0, 0.5f)));
        Assert.AreEqual(1, overlappingTracker.positions.Count);
    }

    [Test]
    public void ResetProperly()
    {
        var overlappingTracker = new SimpleOverlappingTracker(1);
        overlappingTracker.RegisterPosition(Vector2.one);
        overlappingTracker.RegisterPosition(Vector2.one * 3);

        overlappingTracker.Reset();

        Assert.IsEmpty(overlappingTracker.positions);
    }
}