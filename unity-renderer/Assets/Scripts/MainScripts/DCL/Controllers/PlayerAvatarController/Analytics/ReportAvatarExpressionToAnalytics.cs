using System.Collections.Generic;

public class ReportAvatarExpressionToAnalytics
{
    private readonly IAnalytics analytics;
    private readonly Vector2IntVariable playerCoords;

    public ReportAvatarExpressionToAnalytics(IAnalytics analytics,
        Vector2IntVariable playerCoords)
    {
        this.analytics = analytics;
        this.playerCoords = playerCoords;
    }

    public void Report(string expressionId)
    {
        var location = playerCoords.Get();
        analytics.SendAnalytic("used_emote", new Dictionary<string, string>
        {
            {"id", expressionId},
            {"parcel_location", $"{location.x},{location.y}"}
        });
    }
}