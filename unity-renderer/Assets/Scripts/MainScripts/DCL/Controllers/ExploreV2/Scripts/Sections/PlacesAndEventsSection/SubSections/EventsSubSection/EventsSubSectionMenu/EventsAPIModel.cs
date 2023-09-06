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
    public string server;
    public int total_attendees;
    public bool live;
    public string user_name;
    public bool highlighted;
    public bool trending;
    public bool attending;
    public string[] categories;
    public bool recurrent;
    public double duration;
    public string start_at;
    public string[] recurrent_dates;
    public bool world;
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

[Serializable]
public class CategoryListFromAPIModel
{
    public bool ok;
    public List<CategoryFromAPIModel> data;
}

[Serializable]
public class CategoryFromAPIModel
{
    public string name;
    public bool active;
    public string created_at;
    public string updated_at;
    public CategoryNameTranslationFromAPIModel i18n;
}

[Serializable]
public class CategoryNameTranslationFromAPIModel
{
    public string en;
}
