using System;
using System.Collections.Generic;

[Serializable]
public class EventListFromAPIModel
{
    public int total;
    public bool ok;
    public List<EventFromAPIModel> data;
}

[Serializable]
public class EventFromAPIModel
{
    public string id;
    public string name;
    public string image;
    public string description;
    public string next_start_at;
    public string finish_at;
    public string scene_name;
    public int[] coordinates;
    public string realm;
    public int total_attendees;
    public bool live;
    public string user_name;
    public bool highlighted;
    public bool trending;
    public bool attending;
}

[Serializable]
public class AttendEventRequestModel
{
    public string address;
    public AttendEventMessageModel message;
    public string signature;
}

[Serializable]
public class AttendEventMessageModel
{
    public string type = "attend";
    public string timestamp;
    public string @event;
    public bool attend;
}
