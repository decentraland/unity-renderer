using System;
using System.Collections.Generic;

[Serializable]
public class EventListFromAPIModel
{
    public bool ok;
    public List<EventFromAPIModel> data;
}

[Serializable]
public class EventDetailFromAPIModel
{
    public bool ok;
    public EventFromAPIModel data;
}

[Serializable]
public class EventFromAPIModel
{
    public string id;
    public string name;
    public string image;
    public string description;
    public string start_at;
    public string finish_at;
    public string scene_name;
    public int[] coordinates;
    public string realm;
    public int total_attendees;
    public bool live;
    public string user;
    public bool highlighted;
    public bool trending;
}