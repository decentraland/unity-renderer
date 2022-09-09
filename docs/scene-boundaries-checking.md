# Scene Boundaries Checking
The main goal of the Scene Boundaries Checking System ([`SceneBoundsChecker` class](https://github.com/decentraland/unity-renderer/blob/d73b9ba97aca772740e8fd437c914a7101d15770/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs)) is to make sure that deployed content is only in effect inside the scenes they belong to.

To do that, SDK entities are sent to the `SceneBoundsChecker` ("SBC" from now onwards) at specific moments in their lifecycle so that the SBC can check them, flag them and act according to the components they have attached.

The values used to define the 3D space boundaries of a scene are its parcels position (16x16 -> widthXdepth) + the height limitation stated in the [Scene Limits Documentation](https://docs.decentraland.org/development-guide/scene-limitations/)

The effect of components on entities that are considered outside their respective scene boundaries are "turned off" according to each component funcitonality.

The "turned off" effect varies depending on the explorer client running on production or in preview mode (local scene). For example for Shape Components, those meshes rendering is disabled in production, but in preview mode they are shown in red with a red wireframe indicating which submesh is trespassing its scene boundaries.
- [Production feedback style](https://github.com/decentraland/unity-renderer/blob/d73b9ba97aca772740e8fd437c914a7101d15770/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsFeedbackStyle_Simple.cs)
- [Preview mode feedback style](https://github.com/decentraland/unity-renderer/blob/d73b9ba97aca772740e8fd437c914a7101d15770/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsFeedbackStyle_RedBox.cs)

#### Scene boundaries affected components
- Shape components
- Audio components
- Modifier Area components
-

#### Entity actions that trigger being added to the SBC checking
- When an entity is created
- When a shape is updated
- 
- When any of the affected components are attached to the entity
- 

#### Regular-shaped scenes

[IMAGE]

#### Irregular-shaped scenes
The most common one is the "L-shaped" scene consisting of 2 or 3 parcels in-line and 1 on the side:
[IMAGE]

If it weren't for these kind of scenes, the scene boundaries checking logic could probably be much simpler, and we could even tackle the issue taking advantage of Unity's physics system and its trigger collision events, as already tried with no success in this [spike PR]().

### Approach
- Entities are sent to the SBC to be checked, at that moment they get stored in a collection of entities to be checked.
- When the SBC runs its pass to evaluate all the entities in its collection, one by one is checked and removed from the collection (they'll be auto-added again later if the needed actions are executed)

#### Entity Evaluation
1. First a check is run to know if the entity has a mesh or not, since later the entity check will be based either on the entity transform position or on its mesh merged bounds.
2.

#### Persistent Entities



### Optimizations
As the scene bounds checking can be pretty stressful for performance, some optimizations were implemented, having the web client as the main use case.

#### CPU Budget throttling


#### Outer-bounds preliminary checks
At certain calls to add an entity to the SBC collection to be checked, the entities get evluated with a preliminary check that checks exclusively if the entity is outside its scene outer-bounds.
If its found as outside the outer bounds, the entity is immediately treated as an entity outside scene boundaries and no further checks are run, removing the entity from the SBC collection of entities to be checked later.