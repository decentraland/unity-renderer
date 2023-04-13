# Scene Boundaries Checking
This document describes the architecture behind the Scene Bounds Checker for SDK6 scenes, for a more higher level architecture description and details on the SDK7 Scene Bounds Checker read [ADR-192](https://adr.decentraland.org/adr/ADR-192).

### Goal
The main goal of the Scene Boundaries Checking System ([`SceneBoundsChecker` class](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs)) is to make sure that deployed content is only in effect inside the scenes they belong to.

### Approach
SDK entities are sent to the `SceneBoundsChecker` ("SBC" from now onwards) at specific moments in their lifecycle so that the SBC can evaluate if they are out of their scene boundaries, flag them and act according to the components they have attached. Entities that belong to a global scene (as in the player avatars gloabal scene or any portable experience scene) are not checked as their scene has no boundaries.

The values used to define the 3D space boundaries of a scene are its parcels position (16x16 -> widthXdepth) + the height limitation stated in the [Scene Limits Documentation](https://docs.decentraland.org/development-guide/scene-limitations/), those calculations can be seen [here](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L246).

Components on entities that are considered outside their respective scene boundaries are "turned off" according to each component functionality.

#### Entity Evaluation
When the SBC runs its pass to evaluate all the entities in its collection, they are checked one by one and then removed from the collection (they'll be auto-added again later if the needed actions are executed)

- If the evaluated entity doesn't have a `Shape` component attached, then [its Transform position](https://github.com/decentraland/unity-renderer/blob/08284ea1d1d2d58faa087ee93ad9c95fdd5c4e5d/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs#L233) is used for the scene-boundaries evaluation.
- If the evaluated entity has a Shape component that's not an `AvatarShape`, [the whole mesh merged boundaries (including colliders) min-max points](https://github.com/decentraland/unity-renderer/blob/08284ea1d1d2d58faa087ee93ad9c95fdd5c4e5d/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs#L219) are used for the scene-boundaries evaluation.
- If the evaluated entity has an AvatarShape component then a `Bounds` object is created and resized based on the entity's transform lossy scale, and [that `Bounds` object](https://github.com/decentraland/unity-renderer/blob/08284ea1d1d2d58faa087ee93ad9c95fdd5c4e5d/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs#L259) is used as the merged bounds for the scene-boundaries evaluation.

#### Persistent Entities

When added to the SBC for checking, entities can be added as "persistent" in that case they are not removed after being checked. The most common case for persisten entities are entities that are attached to the player in some way (parented to the Camera/Avatar or using the AvatarAttach component).

### Scene boundaries affected components
#### Non-rendered components
These components implement the `IOutOfSceneBoundariesHandler` interface and should be added to the `DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries` dictionary in their initialization code ([example](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/Components/Audio/DCLAudioStream.cs#L29)) using [these SBC extension methods](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/DataStore/DataStore_SceneBoundariesChecker_Extensions.cs#L10) 

- Audio components (`DCLAudioSource`, `DCLAudioStream`, `ECSAudioSourceComponentHandler`)
- Transform Component (`DCLTransform`)
- OnPointerEVent components (`OnPointerEvent`, `OnPointerDown`, `OnClick`, `OnPointerUp`)

#### Rendered components
Rendered components (any 'Shape' component), that are outside their scene boundaries, are affected differently depending on the explorer client running on production or in preview mode (local scene). When running in "debug mode" (e.g. in preview mode) meshes are shown with a red wireframe indicating which submesh is getting outside its scene boundaries, otherwise (as in production) meshes rendering is disabled.
- [Debug Mode (Preview mode) feedback style](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsFeedbackStyle_RedBox.cs)
- [Normal mode (Production) feedback style](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsFeedbackStyle_Simple.cs)

### Entity actions that trigger being added to the SBC checking
- [When an entity is created](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L366)
- [When an entity shape is updated](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L373)
- [When an entity is parented to the camera](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L509)
- [When an entity is reparented to the avatar](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L528)
- [When an entity is normally reparented](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ParcelScene.cs#L557)
- [When any affected component implementing the `IOutOfSceneBoundariesHandler` interface is attached/modified on the entity (if it's a Transform component the preliminary check will be run)](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/ECSComponentManagerLegacy/ECSComponentsManagerLegacy.cs#L435) 
- [When the `AvatarAttachHandler` starts its entity update](https://github.com/decentraland/unity-renderer/blob/05c9abdbf1e55bf33817e890ce56d65fb51dd66a/unity-renderer/Assets/Scripts/MainScripts/DCL/Components/AvatarAttach/AvatarAttachHandler.cs#L141) 

### Types of scenes

#### Irregular-shaped scenes
Scenes whose joint parcels form up a non-square shape, the most common one is the "L-shaped" scene consisting of 2 or 3 parcels in-line and 1 on the side.

**Example of irregular-shaped scene**
![IMAGE](scene-boundaries-checking/irregular-example.png)
![IMAGE](scene-boundaries-checking/irregular-gizmo-example.png)

If it weren't for these kind of scenes, the scene boundaries checking logic could probably be much simpler, and we could even tackle the issue taking advantage of Unity's physics system and its trigger collision events, as already tried with no success in this [spike PR](https://github.com/decentraland/unity-renderer/issues/2433).

#### Regular-shaped scenes

Scenes whose joint parcels form up a square-shape.

### Optimizations
As the scene bounds checking can be pretty stressful for performance (checking parcel by parcel), some optimizations were implemented, having the web client as the main use case.

#### CPU Budget throttling
The SBC doesn't run its entities evaluation pass every frame, instead there's [a specific time wait](https://github.com/decentraland/unity-renderer/blob/1fc852690b414b51e9f70dc1c812f2a9427a2b79/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs#L13) between runs.

Also, to avoid taking all the CPU budget available in a single frame, the entities checking may be [interrupted based on the remaining CPU time counted in the `messagingManager.timeBudgetCounter`](https://github.com/decentraland/unity-renderer/blob/1fc852690b414b51e9f70dc1c812f2a9427a2b79/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/SceneBoundariesController/SceneBoundsChecker.cs#L70). That means that every time the SBC runs, it may not finish evaluating all its accumulated entities and the remaining ones would be checked in future entity evaluation iterations.

#### Outer-boundaries preliminary checks
The idea of this optimization is to have a cheaper check for entities outside their scene boundaries and be able to run that check as a preliminary check when an entity is added to the SBC collection. That way, entities that are really outside the scene boundaries are detected as such on creation (with a cheap check) and not when the SBC decides to run its pass.

##### Inner Boundaries of a scene:
![scene-inner-boundaries](scene-boundaries-checking/scene-inner-boundaries.png)

#### Outer Boundaries of a scene:
![scene-outer-boundaries](scene-boundaries-checking/irregular-gizmo-example-2.png)

At certain calls to add an entity to the SBC collection to be checked, the entities get evaluated with a preliminary check that checks exclusively if the entity is outside its scene outer-bounds.
If its found as outside the outer bounds, the entity is immediately treated as an entity outside scene boundaries and no further checks are run, removing the entity from the SBC collection of entities to be checked later.
