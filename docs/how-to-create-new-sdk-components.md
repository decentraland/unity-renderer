### How to create new SDK components

Before implementing a component on unity's side, it's recommended to check Kernel for the same component and verify whether it is a Disposable/Shared component or not, as their implementation pipeline differs.

Every component declaration should be under the `/packages/decentraland-ecs/src/decentraland/**` directory. Be aware that your IDE may find the component also in a ".ts" script, dismiss those declarations as they correspond to interfaces for the components in typescript.

#### Entity component:

- Make sure the corresponding CLASS_ID_COMPONENT value exists in MainScripts/DCL/Models/Protocol.cs, otherwise add it.
- Create the component script and prefab in MainScripts/DCL/Components/[Corresponding Folder]/
- From the Unity editor, update the MainScripts/DCL/Factory/DCLComponentFactory scriptable object adding the new element in the Factory List with the correct CLASS_ID_COMPONENT value and its prefab reference

#### Shared/Disposable component:

- Make sure the corresponding CLASS_ID value exists in MainScripts/DCL/Models/Protocol.cs, otherwise add it.
- Create the component script in MainScripts/DCL/Components/[Corresponding Folder]/
- Edit the SharedComponentCreate() method in MainScripts/DCL/Controllers/Scene/ParcelScene.cs to make sure it instantiates the new shared component.