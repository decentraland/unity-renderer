# Scene Boundaries Checking
The main goal of the Scene Boundaries Checking System ([`SceneBoundsChecker` class](https://github.com/decentraland/unity-renderer/blob/d73b9ba97aca772740e8fd437c914a7101d15770/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs)) is to make sure that deployed content is only in effect inside the scenes they belong to.

To do that, SDK entities are sent to the `SceneBoundsChecker` ("SBC" from now onwards) at specific moments in their lifecycle so that the SBC can check them, flag them and act according to the components they have attached.

The values used to define the 3D space boundaries of a scene are its parcels position (16x16 -> widthXdepth) + the height limitation stated in the [Scene Limits Documentation](https://docs.decentraland.org/development-guide/scene-limitations/), those calculations can be seen [here](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L246).

The effect of components on entities that are considered outside their respective scene boundaries are "turned off" according to each component funcitonality.

### Scene boundaries affected components
If the evaluated entity doesn't have a Shape component attached, then its evaluation will be just its Transform position, otherwise the whole mesh merged boundaries (including colliders) is used for the entity out-of-boundaries evaluation.

#### Non-rendered components
These components implement the `IOutOfSceneBoundariesHandler` interface and should be added to the `DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries` dictionary in their initialization code ([example](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/Components/Audio/DCLAudioStream.cs#L29)) using [these SBC extension methods](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/DataStore/DataStore_SceneBoundariesChecker_Extensions.cs#L10) 

- Audio components (`DCLAudioSource`, `DCLAudioStream`, `ECSAudioSourceComponentHandler`)
- Transform Component (`DCLTransform`)
- OnPointerEVent components (`OnPointerEvent`, `OnPointerDown`, `OnClick`, `OnPointerUp`)

#### Rendered components
Rendered components (any 'Shape' component), that are outside their scene boundaries, are affected differently depending on the explorer client running on production or in preview mode (local scene). When running in "debug mode" (e.g. in preview mode) meshes are shown with a red wireframe indicating which submesh is getting outside its scene boundaries, otherwise (as in production) meshes rendering is disabled.
- [Debug Mode (Preview mode) feedback style](https://github.com/decentraland/unity-renderer/blob/d73b9ba97aca772740e8fd437c914a7101d15770/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsFeedbackStyle_RedBox.cs)
- [Normal mode (Production) feedback style](https://github.com/decentraland/unity-renderer/blob/d73b9ba97aca772740e8fd437c914a7101d15770/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsFeedbackStyle_Simple.cs)

### Entity actions that trigger being added to the SBC checking
- [When an entity is created](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L366)
- [When an entity shape is updated](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L373)
- [When an entity is enparented to the camera](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L509)
- [When an entity is reparented to the avatar](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L528)
- [When an entity is normally reparented](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L557)
- [When any affected component implementing the `IOutOfSceneBoundariesHandler` interface is attached to the entity](https://github.com/decentraland/unity-renderer/blob/1fc852690b414b51e9f70dc1c812f2a9427a2b79/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ECSComponentManagerLegacy/ECSComponentsManagerLegacy.cs#L435) 
- [When the `AvatarAttachHandler` starts its entity update](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/Components/AvatarAttach/AvatarAttachHandler.cs#L141) 

#### Regular-shaped scenes

[IMAGE]

#### Irregular-shaped scenes
The most common one is the "L-shaped" scene consisting of 2 or 3 parcels in-line and 1 on the side:
[IMAGE]

If it weren't for these kind of scenes, the scene boundaries checking logic could probably be much simpler, and we could even tackle the issue taking advantage of Unity's physics system and its trigger collision events, as already tried with no success in this [spike PR](https://github.com/decentraland/unity-renderer/issues/2433).

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