using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DCL.Builder.Manifest;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DCL.Builder
{
    public static class ManifestTranslator
    {
        private static readonly Dictionary<string, int> idToHumanReadableDictionary = new Dictionary<string, int>()
        {
            { "Transform", (int) CLASS_ID_COMPONENT.TRANSFORM },

            { "GLTFShape", (int) CLASS_ID.GLTF_SHAPE },

            { "NFTShape", (int) CLASS_ID.NFT_SHAPE },

            { "Name", (int)CLASS_ID.NAME },

            { "LockedOnEdit", (int)CLASS_ID.LOCKED_ON_EDIT },

            { "VisibleOnEdit", (int)CLASS_ID.VISIBLE_ON_EDIT },

            { "Script", (int) CLASS_ID_COMPONENT.SMART_ITEM }
        };

        public static WebBuilderScene StatelessToWebBuilderScene(StatelessManifest manifest, Vector2Int parcelSize)
        {
            WebBuilderScene builderScene = new WebBuilderScene();
            builderScene.id = Guid.NewGuid().ToString();

            BuilderGround ground = new BuilderGround();
            List<string> namesList = new List<string>();
            //We iterate all the entities to create its counterpart in the builder manifest
            foreach (Entity entity in manifest.entities)
            {
                BuilderEntity builderEntity = new BuilderEntity();
                builderEntity.id = entity.id;
                string entityName = builderEntity.id;

                // Iterate the entity components to transform them to the builder format
                foreach (Component entityComponent in entity.components)
                {
                    BuilderComponent builderComponent = new BuilderComponent();
                    builderComponent.id = Guid.NewGuid().ToString();
                    builderComponent.type = entityComponent.type;
                    builderComponent.data = entityComponent.value;
                    builderEntity.components.Add(builderComponent.id);

                    if (entityComponent.type == "NFTShape")
                    {
                        NFTShape.Model model = JsonConvert.DeserializeObject<NFTShape.Model>(entityComponent.value.ToString());
                        builderComponent.data = JsonConvert.SerializeObject(GetWebBuilderRepresentationOfNFT(model));

                        //This is the name format that is used by builder, we will have a different name in unity due to DCLName component
                        entityName = "nft";
                    }

                    if (entityComponent.type ==  "GLTFShape")
                    {
                        var gltfModel = JsonConvert.DeserializeObject<GLTFShape.Model>(builderComponent.data.ToString());

                        //We get the associated asset to the GLFTShape and add it to the scene 
                        var asset = AssetCatalogBridge.i.sceneObjectCatalog.Get(gltfModel.assetId);
                        if (!builderScene.assets.ContainsKey(asset.id))
                            builderScene.assets.Add(asset.id, asset);

                        //If the asset is a floor, we handle this situation for builder 
                        if (asset.category == BIWSettings.FLOOR_CATEGORY)
                        {
                            ground.assetId = asset.id;
                            ground.componentId = builderComponent.id;
                            builderEntity.disableGizmos = true;
                        }

                        entityName = asset.name;
                    }

                    if (!builderScene.components.ContainsKey(builderComponent.id))
                        builderScene.components.Add(builderComponent.id, builderComponent);
                }

                // We need to give to each entity a unique name so we search for a unique name there
                // Also, since the name of the entity will be used in the code, we need to ensure that the it doesn't have special characters or spaces
                builderEntity.name = GetCleanUniqueName(namesList, entityName);

                if (!builderScene.entities.ContainsKey(builderEntity.id))
                    builderScene.entities.Add(builderEntity.id, builderEntity);
            }

            //We add the limits to the scene, the current metrics are calculated in the builder
            builderScene.limits = BIWUtils.GetSceneMetricsLimits(parcelSize.x + parcelSize.y);
            builderScene.ground = ground;

            return builderScene;
        }

        public static StatelessManifest WebBuilderSceneToStatelessManifest(WebBuilderScene scene)
        {
            StatelessManifest manifest = new StatelessManifest();
            manifest.schemaVersion = 1;
            
            foreach (var entity in scene.entities.Values)
            {
                Entity statlesEntity = new Entity();
                statlesEntity.id = entity.id;

                foreach (string componentId in entity.components)
                {
                    foreach (BuilderComponent component in scene.components.Values)
                    {
                        if(component.id != componentId)
                            continue;
                        
                        Component statelesComponent = new Component();
                        statelesComponent.type = component.type;

                        if (statelesComponent.type == "NFTShape")
                        {
                            string url;
                            try
                            {
                                // Builder use a different way to load the NFT so we convert it to our system
                                url = ((NFTShapeBuilderRepresentantion) component.data).url;
                            }
                            catch (Exception e)
                            {
                                // Builder handles the components differently if they come from another site, if we can't do it correctly, we go this way
                                JObject jObject = JObject.Parse(component.data.ToString());
                                url = jObject["url"].ToString();
                            }
                            string assedId = url.Replace(BIWSettings.NFT_ETHEREUM_PROTOCOL, "");
                            int index = assedId.IndexOf("/", StringComparison.Ordinal);
                            string partToremove = assedId.Substring(index);
                            assedId = assedId.Replace(partToremove, "");

                            // We need to use this kind of representation because the color from unity is not serializable to SDK standard
                            NFTShapeStatelessRepresentantion nftModel = new NFTShapeStatelessRepresentantion();
                            nftModel.color = new NFTShapeStatelessRepresentantion.ColorRepresentantion(0.6404918f, 0.611472f, 0.8584906f);
                            nftModel.src = url;
                            nftModel.assetId = assedId;
                            
                            statelesComponent.value = nftModel;
                        }
                        else
                        {
                            statelesComponent.value = component.data;
                        }

                        statlesEntity.components.Add(statelesComponent);
                    }
                }

                manifest.entities.Add(statlesEntity);
            }

            return manifest;
        }
        
        public static StatelessManifest ParcelSceneToStatelessManifest(IParcelScene scene)
        {
            StatelessManifest manifest = new StatelessManifest();
            manifest.schemaVersion = 1;

            foreach (var entity in scene.entities.Values)
            {
                Entity statlesEntity = new Entity();
                statlesEntity.id = entity.entityId.ToString();

                foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> entityComponent in scene.componentsManagerLegacy.GetComponentsDictionary(entity))
                {
                    Component statelesComponent = new Component();
                    statelesComponent.type = idToHumanReadableDictionary.FirstOrDefault( x => x.Value == (int)entityComponent.Key).Key;

                    // Transform component is handle a bit different due to quaternion serializations
                    if (entityComponent.Key == CLASS_ID_COMPONENT.TRANSFORM)
                    {
                        ProtocolV2.TransformComponent entityTransformComponentModel = new ProtocolV2.TransformComponent();
                        entityTransformComponentModel.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                        entityTransformComponentModel.rotation = new ProtocolV2.QuaternionRepresentation(entity.gameObject.transform.rotation);
                        entityTransformComponentModel.scale = entity.gameObject.transform.lossyScale;

                        statelesComponent.value = entityTransformComponentModel;
                    }
                    else
                    {
                        statelesComponent.value = entityComponent.Value.GetModel();
                    }

                    statlesEntity.components.Add(statelesComponent);
                }

                foreach (KeyValuePair<Type, ISharedComponent> entitySharedComponent in scene.componentsManagerLegacy.GetSharedComponentsDictionary(entity))
                {
                    Component statelesComponent = new Component();
                    statelesComponent.type = idToHumanReadableDictionary.FirstOrDefault( x => x.Value == (int)entitySharedComponent.Value.GetClassId()).Key;
                    statelesComponent.value = entitySharedComponent.Value.GetModel();
                    statlesEntity.components.Add(statelesComponent);
                }

                manifest.entities.Add(statlesEntity);
            }

            return manifest;
        }

        public static WebBuilderScene ParcelSceneToWebBuilderScene(ParcelScene scene)
        {
            WebBuilderScene builderScene = new WebBuilderScene();
            builderScene.id = Guid.NewGuid().ToString();

            BuilderGround ground = new BuilderGround();
            List<string> namesList = new List<string>();

            //We iterate all the entities to create its counterpart in the builder manifest
            foreach (IDCLEntity entity in scene.entities.Values)
            {
                BuilderEntity builderEntity = new BuilderEntity();
                builderEntity.id = entity.entityId.ToString();
                string componentType = "";
                string entityName = "";

                // Iterate the entity components to transform them to the builder format
                foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> entityComponent in scene.componentsManagerLegacy.GetComponentsDictionary(entity))
                {
                    BuilderComponent builderComponent = new BuilderComponent();
                    switch (entityComponent.Key)
                    {
                        case CLASS_ID_COMPONENT.TRANSFORM:
                            componentType = "Transform";

                            // We can't serialize the quaternions from Unity since newton serializes have recursive problems so we add this model
                            ProtocolV2.TransformComponent entityTransformComponentModel = new ProtocolV2.TransformComponent();
                            entityTransformComponentModel.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                            entityTransformComponentModel.rotation = new ProtocolV2.QuaternionRepresentation(entity.gameObject.transform.rotation);
                            entityTransformComponentModel.scale = entity.gameObject.transform.lossyScale;

                            builderComponent.data = entityTransformComponentModel;

                            break;
                        case CLASS_ID_COMPONENT.SMART_ITEM:
                            componentType = "Script";
                            break;
                    }

                    // We generate a new uuid for the component since there is no uuid for components in the stateful scheme
                    builderComponent.id = Guid.NewGuid().ToString();
                    builderComponent.type = componentType;

                    // Since the transform model data is different from the others, we set it in the switch instead of here
                    if (builderComponent.type != "Transform")
                        builderComponent.data = entityComponent.Value.GetModel();

                    builderEntity.components.Add(builderComponent.id);

                    if (!builderScene.components.ContainsKey(builderComponent.id))
                        builderScene.components.Add(builderComponent.id, builderComponent);
                }

                // Iterate the entity shared components to transform them to the builder format
                foreach (KeyValuePair<System.Type, ISharedComponent> sharedEntityComponent in scene.componentsManagerLegacy.GetSharedComponentsDictionary(entity))
                {
                    BuilderComponent builderComponent = new BuilderComponent();
                    // We generate a new uuid for the component since there is no uuid for components in the stateful scheme
                    builderComponent.id = Guid.NewGuid().ToString();
                    builderComponent.data = sharedEntityComponent.Value.GetModel();

                    if (sharedEntityComponent.Value is GLTFShape)
                    {
                        componentType = "GLTFShape";
                        
                        var gltfModel = (GLTFShape.Model) builderComponent.data;

                        //We get the associated asset to the GLFTShape and add it to the scene 
                        var asset = AssetCatalogBridge.i.sceneObjectCatalog.Get(gltfModel.assetId);
                        if (!builderScene.assets.ContainsKey(asset.id))
                            builderScene.assets.Add(asset.id, asset);

                        // This is a special case. The builder needs the ground separated from the rest of the components so we search for it.
                        // Since all the grounds have the same asset, we assign it and disable the gizmos in the builder
                        if (asset.category == BIWSettings.FLOOR_CATEGORY)
                        {
                            ground.assetId = asset.id;
                            ground.componentId = builderComponent.id;
                            builderEntity.disableGizmos = true;
                        }

                        entityName = asset.name;
                    }
                    else if (sharedEntityComponent.Value is NFTShape)
                    {
                        componentType = "NFTShape";

                        // This is a special case where we are assigning the builder url field for NFTs because builder model data is different
                        NFTShape.Model model = (NFTShape.Model) builderComponent.data;
                        builderComponent.data = GetWebBuilderRepresentationOfNFT(model);

                        //This is the name format that is used by builder, we will have a different name in unity due to DCLName component
                        entityName = "nft";
                    }
                    else if (sharedEntityComponent.Key == typeof(DCLName))
                    {
                        componentType = "Name";
                        entityName = ((DCLName.Model) sharedEntityComponent.Value.GetModel()).value;
                    }
                    else if (sharedEntityComponent.Key == typeof(DCLLockedOnEdit))
                    {
                        componentType = "LockedOnEdit";
                    }

                    builderComponent.type = componentType;

                    builderEntity.components.Add(builderComponent.id);
                    if (!builderScene.components.ContainsKey(builderComponent.id))
                        builderScene.components.Add(builderComponent.id, builderComponent);
                }

                // We need to give to each entity a unique name so we search for a unique name there
                // Also, since the name of the entity will be used in the code, we need to ensure that the it doesn't have special characters or spaces
                builderEntity.name = GetCleanUniqueName(namesList, entityName);

                if (!builderScene.entities.ContainsKey(builderEntity.id))
                    builderScene.entities.Add(builderEntity.id, builderEntity);
            }
            
            //We add the limits to the scene, the current metrics are calculated in the builder
            builderScene.limits = BIWUtils.GetSceneMetricsLimits(scene.parcels.Count);
            builderScene.metrics = new SceneMetricsModel();
            builderScene.ground = ground;

            return builderScene;
        }

        private static NFTShapeBuilderRepresentantion GetWebBuilderRepresentationOfNFT(NFTShape.Model model)
        {
            NFTShapeBuilderRepresentantion representantion = new NFTShapeBuilderRepresentantion();
            representantion.url = model.src;
            return representantion;
        }

        private static string GetCleanUniqueName(List<string> namesList, string currentName)
        {
            //We clean the name to don't include special characters
            Regex r = new Regex("(?:[^a-z]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            string newName = r.Replace(currentName, String.Empty);
            newName = newName.ToLower();

            //We get a unique name
            bool uniqueName = false;
            int cont = 2;
            while (!uniqueName)
            {
                if (!namesList.Contains(newName))
                    uniqueName = true;
                else
                    newName = newName + cont;

                cont++;
            }

            namesList.Add(newName);
            return newName;
        }

        public static IParcelScene ManifestToParcelSceneWithOnlyData(Manifest.Manifest manifest)
        {
            GameObject parcelGameObject = new GameObject("Builder Scene SceneId: " + manifest.scene.id);
            ParcelScene scene = parcelGameObject.AddComponent<ParcelScene>();

            //We remove the old assets to they don't collide with the new ones
            BIWUtils.RemoveAssetsFromCurrentScene();
            
            //The data is built so we set the data of the scene 
            scene.SetData(CreateSceneDataFromScene(manifest));

            return scene;
        }

        private static LoadParcelScenesMessage.UnityParcelScene CreateSceneDataFromScene(Manifest.Manifest manifest)
        {
            //We add the assets from the scene to the catalog
            var assets = manifest.scene.assets.Values.ToArray();
            AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(assets);

            //We create and assign the data of the scene
            LoadParcelScenesMessage.UnityParcelScene parcelData = new LoadParcelScenesMessage.UnityParcelScene();
            parcelData.id = manifest.scene.id;

            //We set the current scene in the 0,0
            int x = 0;
            int y = 0;

            parcelData.basePosition =  new Vector2Int(x, y);

            //We set the parcels as the first one is in the base position, the first one will be in the bottom-left corner 
            parcelData.parcels =  new Vector2Int[manifest.project.rows * manifest.project.cols];

            //We assign the parcels position
            for (int index = 0; index < parcelData.parcels.Length; index++)
            {
                parcelData.parcels[index] = new Vector2Int(x, y);
                x++;
                if (x == manifest.project.rows)
                {
                    y++;
                    x = 0;
                }
            }

            //We prepare the mappings to the scenes
            Dictionary<string, string> contentDictionary = new Dictionary<string, string>();

            foreach (var sceneObject in assets)
            {
                foreach (var content in sceneObject.contents)
                {
                    if (!contentDictionary.ContainsKey(content.Key))
                        contentDictionary.Add(content.Key, content.Value);
                }
            }

            //We add the mappings to the scene
            BIWUtils.AddSceneMappings(contentDictionary, BIWUrlUtils.GetUrlSceneObjectContent(), parcelData);

            return parcelData;
        }
        
        public static IParcelScene ManifestToParcelScene(Manifest.Manifest manifest)
        {
            GameObject parcelGameObject = new GameObject("Builder Scene SceneId: " + manifest.scene.id);
            ParcelScene scene = parcelGameObject.AddComponent<ParcelScene>();

            //We remove the old assets to they don't collide with the new ones
            BIWUtils.RemoveAssetsFromCurrentScene();
            
            //The data is built so we set the data of the scene 
            scene.SetData(CreateSceneDataFromScene(manifest));

            // We iterate all the entities to create the entity in the scene
            foreach (BuilderEntity builderEntity in manifest.scene.entities.Values)
            {
                var entity = scene.CreateEntity(builderEntity.id.GetHashCode());

                bool nameComponentFound = false;
                // We iterate all the id of components in the entity, to add the component 
                foreach (string idComponent in builderEntity.components)
                {
                    //This shouldn't happen, the component should be always in the scene, but just in case
                    if (!manifest.scene.components.ContainsKey(idComponent))
                        continue;

                    // We get the component from the scene and create it in the entity
                    BuilderComponent component = manifest.scene.components[idComponent];

                    switch (component.type)
                    {
                        case "Transform":
                            DCLTransform.Model model = JsonConvert.DeserializeObject<DCLTransform.Model>(component.data.ToString());
                            EntityComponentsUtils.AddTransformComponent(scene, entity, model);
                            break;

                        case "GLTFShape":
                            LoadableShape.Model gltfModel = JsonConvert.DeserializeObject<LoadableShape.Model>(component.data.ToString());
                            EntityComponentsUtils.AddGLTFComponent(scene, entity, gltfModel, component.id);
                            break;

                        case "NFTShape":
                            //Builder use a different way to load the NFT so we convert it to our system
                            string url = JsonConvert.DeserializeObject<string>(component.data.ToString());
                            string assedId = url.Replace(BIWSettings.NFT_ETHEREUM_PROTOCOL, "");
                            int index = assedId.IndexOf("/", StringComparison.Ordinal);
                            string partToremove = assedId.Substring(index);
                            assedId = assedId.Replace(partToremove, "");

                            NFTShape.Model nftModel = new NFTShape.Model();
                            nftModel.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
                            nftModel.src = url;
                            nftModel.assetId = assedId;

                            EntityComponentsUtils.AddNFTShapeComponent(scene, entity, nftModel, component.id);
                            break;

                        case "Name":
                            nameComponentFound = true;
                            DCLName.Model nameModel = JsonConvert.DeserializeObject<DCLName.Model>(component.data.ToString());
                            nameModel.builderValue = builderEntity.name;
                            EntityComponentsUtils.AddNameComponent(scene , entity, nameModel, Guid.NewGuid().ToString());
                            break;

                        case "LockedOnEdit":
                            DCLLockedOnEdit.Model lockedModel = JsonConvert.DeserializeObject<DCLLockedOnEdit.Model>(component.data.ToString());
                            EntityComponentsUtils.AddLockedOnEditComponent(scene , entity, lockedModel, Guid.NewGuid().ToString());
                            break;
                    }
                }

                // We need to mantain the builder name of the entity, so we create the equivalent part in biw. We do this so we can maintain the smart-item references
                if (!nameComponentFound)
                {
                    DCLName.Model nameModel = new DCLName.Model();
                    nameModel.value = builderEntity.name;
                    nameModel.builderValue = builderEntity.name;
                    EntityComponentsUtils.AddNameComponent(scene , entity, nameModel, Guid.NewGuid().ToString());
                }
            }

            //We already have made all the necessary steps to configure the parcel, so we init the scene
            scene.sceneLifecycleHandler.SetInitMessagesDone();

            return scene;
        }

    }
}