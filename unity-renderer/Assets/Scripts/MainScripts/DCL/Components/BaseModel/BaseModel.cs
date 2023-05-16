using Decentraland.Sdk.Ecs6;

/// <summary>
/// Every component model must inherit from BaseModel, and implement the way he is handling the JSON conversion
/// </summary>
public abstract class BaseModel
{
    public abstract BaseModel GetDataFromJSON(string json);
    public abstract BaseModel GetDataFromPb(ComponentBodyPayload pbModel);
}
