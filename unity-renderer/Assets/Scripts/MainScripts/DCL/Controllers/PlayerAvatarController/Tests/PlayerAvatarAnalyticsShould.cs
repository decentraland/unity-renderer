using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class PlayerAvatarAnalyticsShould
{
    [Test]
    public void ReportExpression()
    {
        const string expressionId = "kiss";
        var parcelLocation = Vector2Int.one;
        var playerCoords = ScriptableObject.CreateInstance<Vector2IntVariable>();
        playerCoords.Set(parcelLocation);
        var analyticsReporter = Substitute.For<IAnalytics>();
        var avatarAnalytics = new PlayerAvatarAnalytics(analyticsReporter, playerCoords);

        avatarAnalytics.ReportExpression(expressionId);
        
        analyticsReporter.Received(1).SendAnalytic("used_emote",
            Arg.Is<Dictionary<string, string>>(@params => ThenExpressionParamsAreCorrect(@params, expressionId, parcelLocation)));
    }

    private static bool ThenExpressionParamsAreCorrect(Dictionary<string, string> @params,
        string expressionId,
        Vector2Int parcelLocation)
    {
        return @params["id"] == expressionId
               && @params["parcel_location"] == $"{parcelLocation.x},{parcelLocation.y}";
    }
}