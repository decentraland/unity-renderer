using System.Collections.Generic;

/// <summary>
/// This will add/set/remove scene cards for the consumer  
/// </summary>
internal interface ISceneListener
{
    /// <summary>
    /// This set the scenes cards
    /// </summary>
    /// <param name="scenes">Dictionary with the cards indexed by ID</param>
    void SetScenes(Dictionary<string, ISceneCardView> scenes);

    /// <summary>
    /// Add a scene card
    /// </summary>
    /// <param name="scene">Scene card to add</param>
    void SceneAdded(ISceneCardView scene);

    /// <summary>
    /// Remove a scene card
    /// </summary>
    /// <param name="scene">Scene card to remove</param>
    void SceneRemoved(ISceneCardView scene);
}