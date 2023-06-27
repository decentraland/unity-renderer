using System;
using System.Collections.Generic;

[Serializable]
public class QuestRewardResponse
{
    public List<QuestReward> items;
}

[Serializable]
public class QuestReward
{
    public string name;
    public string image_link;
}
