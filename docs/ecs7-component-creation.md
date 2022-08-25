---
title: "How to create new components for ECS7"
slug: "/contributor/unity-renderer/components/how-to-create-components"
---

# How to create new components for ECS7

In order to create new components, we need to do a couple of things before start coding them in the renderer
There is a step by step guide that you need to follow. I will list all the steps and then you can follow each step with a longer explanation

1. Create the proto definition
2. Generate the new proto c# code in the project
3. Code the new component
4. Ensure that the component follows the convention


## Create the proto definition

To create a definition you must go to this repo (implement link to final repo)

1. Create the proto definition in this folder (implement folder link)
2. Make a build of the project with 
```sh
    make build
```
3. Implement unit test to serialize/deserialize the component
4. Create a PR with the news changes

Things to have into account
- We are using proto 3 so all the definition of the proto must compile with their [syntax](https://developers.google.com/protocol-buffers/docs/proto3/)
- We have some common types that cannot be recreated
- The proto should have the basic definition

More information at (link to the complete guide when the PR is merged)

## Generate the new proto c# code in the project

After the proto definition has been merged, we need to generate the code in the renderer to use the model.
For that we have an auto-generator of the code that automatically will get the last version and generate all the code, however, in order to update the code we must call this function manually from the unity renderer.

To generate the code you must open the unity-renderer project, then in the top of unity ( with the File, Edit, Assets...etc) you will find another one that is called `Decentraland`. Then you must click on `Decentraland > Protobuf > UpdateModels`

And it will automatically generate the last version of the proto in the project.

## Code the new component

Now it is time to implement the functionality of the new component. 

The components has 5 important parts 

- ComponentID
This is the ID that will the component will use. It must be unique and it has been generated from the the proto definition

- Model
This is the model of the component. This is the data that we will use to handle the component. It has been auto-generated with the proto generation and it has the same name of the proto file with a PB in front. For example, if you have a `BoxShape.proto` definition the generated class of the model will be `PBBoxShape`

- Component Handler
The component handler will manage all the functionality of the component. In this class you must implement the `IECSComponentHandler<ModelClass>` (ModelClass is the model. It is a generated class from the proto, the name will be PB + name of the file .proto).
This interface has 3 method that are important to implement in order to create a component 

```
        void OnComponentCreated(IParcelScene scene, IDCLEntity entity);
        void OnComponentRemoved(IParcelScene scene, IDCLEntity entity);
        void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, ModelType model);
```

- Serializer
Each component is responsible to implement his serialization and deserialization of the component. This serializer must be able to serialize/deserialize to byte array 
- Register
This will register the component into the system, connecting it to the system. This register will register the component in the factory and the component writer

The design of the components is to avoid inheritance so we encourage to use pure functions as much as possible

In order to create them, We must follow the next steps

1. Create the component folder and assembly. We have all the components unders the follow folder `DCLPlugins/ECS7/ECSComponents`.
You need to create a folder and a new assembly that will hold the component
2. In the new assembly, you must reference the following one `DCL.ECSComponents.Data`. This will reference the new model of the component that you just updated
3. You must create the component handler with all the logic (Take a look at `ECSBoxShapeComponentHandler.cs` as an example)
4. You must create the serializer class (probably you can copy it from another class and adapt to your)
5. You must create the register class
```sh
   public static class AudioSourceSerializer
    {
        public static byte[] Serialize(PBAudioSource model)
        {
            int size = model.CalculateSize();
            byte[] buffer = new byte[size];
            CodedOutputStream output = new CodedOutputStream(buffer);
            model.WriteTo(output);
            return buffer;
        }

        public static PBAudioSource Deserialize(object data)
        {
            return PBAudioSource.Parser.ParseFrom((byte[])data);
        }
    }
```
6. Add the new register to the `ECS7ComponentsComposer` class with his corresponding ID
```sh
   public class ECS7ComponentsComposer : IDisposable
    {
        private readonly TransformRegister transformRegister;
        private readonly SphereShapeRegister sphereShapeRegister;
        private readonly BoxShapeRegister boxShapeRegister;
        private readonly PlaneShapeRegister planeShapeRegister;
        private readonly CylinderShapeRegister cylinderShapeRegister;
        private readonly AudioStreamRegister audioStreamRegister;
        private readonly AudioSourceRegister audioSourceRegister;

        public ECS7ComponentsComposer(ECSComponentsFactory componentsFactory, IECSComponentWriter componentsWriter)
        {
            transformRegister = new TransformRegister(ComponentID.TRANSFORM, componentsFactory, componentsWriter);
            sphereShapeRegister = new SphereShapeRegister(ComponentID.SPHERE_SHAPE, componentsFactory, componentsWriter);
            boxShapeRegister = new BoxShapeRegister(ComponentID.BOX_SHAPE, componentsFactory, componentsWriter);
            planeShapeRegister = new PlaneShapeRegister(ComponentID.PLANE_SHAPE, componentsFactory, componentsWriter);
            cylinderShapeRegister = new CylinderShapeRegister(ComponentID.CYLINDER_SHAPE, componentsFactory, componentsWriter);
            audioStreamRegister = new AudioStreamRegister(ComponentID.AUDIO_STREAM, componentsFactory, componentsWriter);
            audioSourceRegister = new AudioSourceRegister(ComponentID.AUDIO_SOURCE, componentsFactory, componentsWriter);
        }

        public void Dispose()
        {
            transformRegister.Dispose();
            sphereShapeRegister.Dispose();
            boxShapeRegister.Dispose();
            planeShapeRegister.Dispose();
            cylinderShapeRegister.Dispose();
            audioStreamRegister.Dispose();
            audioSourceRegister.Dispose();
        }
    }
```
And now you have your component added and working!

## Ensure that the component follows the convention
There is some checklist that we need to have into account while developing new components, this part tries to summarize them.

- Unit test
All the components must include unit test that cover its functionality, dispose and the serialization/deserialization at least to ensure that the component will work
- Take into account what happens when the component is not inside the scene (Check `SceneBoundariesChecker` class for more info)
- If the component renders something into the world, It must add the rendereable info to the data store, this way we add the information of the renderer to the scene so it can counts toward the limit
- If the component renders something into the world, It must add the `MeshesInfo` to the entity
- It must be as perfomant as possible. This code will be executed lots of time so we need to ensure that everything work as smooth as possible
- It must work with `Hot reload` in the preview mode. If you has coded the `OnComponentRemoved` correctly, this will work out of the box, but the hot reload it a way to test that everything work fine with the dispose of the component
- If the component uses a resource, you must implement the resource management with an `AssetPromiseKeeper`. The component should notify the `AssetPromiseKeeper` when the resource is used and when it is not longer used



