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
using UnityEngine;

namespace DCL.Builder
{
    public static class ManifestTranslator
    {
        public static Manifest.Manifest TranslateSceneToManifest(ProjectData data, ParcelScene scene)
        {
            Manifest.Manifest manifest = new Manifest.Manifest();
            manifest.project = data;
            
            BuilderScene builderScene = new BuilderScene();
            builderScene.id = Guid.NewGuid().ToString();
            
            BuilderGround ground = new BuilderGround();
            List<string> namesList = new List<string>();

            //We iterate all the entities to create its counterpart in the builder manifest
            foreach (IDCLEntity entity in scene.entities.Values)
            {
                BuilderEntity builderEntity = new BuilderEntity();
                builderEntity.id = entity.entityId;
                string componentType = "";
                string entityName = "";
                
                // Iterate the entity components to transform them to the builder format
                foreach (KeyValuePair<CLASS_ID_COMPONENT,IEntityComponent> entityComponent in entity.components)
                {
                    BuilderComponent builderComponent = new BuilderComponent();
                    switch (entityComponent.Key)
                    {
                        case CLASS_ID_COMPONENT.TRANSFORM:
                            componentType = "Transform";
                            break; 
                        case CLASS_ID_COMPONENT.SMART_ITEM:
                            componentType = "Script";
                            break;
                    }

                    // We generate a new uuid for the component since there is no uuid for components in the stateful scheme
                    builderComponent.id = Guid.NewGuid().ToString();
                    builderComponent.type = componentType;
                    builderComponent.data = JsonConvert.ToString(entityComponent.Value.GetModel());
                    
                    builderEntity.components.Add(builderComponent.id);
                    builderScene.components.Add(builderComponent.id,builderComponent);
                }
                
                // Iterate the entity shared components to transform them to the builder format
                foreach (KeyValuePair<System.Type,ISharedComponent> sharedEntityComponent in entity.sharedComponents)
                {
                    BuilderComponent builderComponent = new BuilderComponent();
                    // We generate a new uuid for the component since there is no uuid for components in the stateful scheme
                    builderComponent.id = Guid.NewGuid().ToString();
                    builderComponent.data = JsonConvert.ToString(sharedEntityComponent.Value.GetModel());
                    
                    if (sharedEntityComponent.Key == typeof(GLTFShape))
                    {
                        componentType = "GLTFShape";
                        
                        var gltfModel = JsonConvert.DeserializeObject<GLTFShape.Model>(builderComponent.data.ToString());
                        
                        //We get the associated asset to the GLFTShape and add it to the scene 
                        var asset = AssetCatalogBridge.i.sceneObjectCatalog.Get(gltfModel.assetId);
                        builderScene.assets.Add(asset.id,asset);
                        
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
                    else if (sharedEntityComponent.Key == typeof(NFTShape))
                    {
                        componentType = "NFTShape";
                        
                        // This is a special case where we are assigning the builder url field for NFTs because builder model data is different
                        NFTShapeBuilderRepresentantion representantion;
                        representantion.url = JsonConvert.DeserializeObject<NFTShape.Model>(builderComponent.data.ToString()).src;
                        builderComponent.data = JsonConvert.ToString(representantion);
                        
                        //This is the name format that is used by builder, we will have a different name in unity due to DCLName component
                        entityName = "nft";
                    }
                    else if (sharedEntityComponent.Key == typeof(DCLName))
                    {
                        componentType = "Name";
                    }
                    else if (sharedEntityComponent.Key == typeof(DCLLockedOnEdit))
                    {
                        componentType = "LockedOnEdit";
                    }
                    
                    builderComponent.type = componentType;
                    
                    builderEntity.components.Add(builderComponent.id);
                    builderScene.components.Add(builderComponent.id,builderComponent);
                }
                
                // We need to give to each entity a unique name so we search for a unique name there
                // Also, since the name of the entity will be used in the code, we need to ensure that the it doesn't have special characters or spaces
                builderEntity.name = GetCleanUniqueName(namesList,entityName);
                manifest.scene.entities.Add(builderEntity.id,builderEntity);
            }

            //We add the limits to the scene, the current metrics are calculated in the builder
            builderScene.limits = BIWUtils.GetSceneMetricsLimits(scene.parcels.Count);
            builderScene.ground = ground;
            
            manifest.scene = builderScene;
            return manifest;
        }

        private static string GetCleanUniqueName(List<string> namesList, string currentName)
        {
            //We clean the name to don't include special characters
            var regex = new Regex(@"/[A-Za-z]+/g");
            var result = regex.Matches(currentName);
            string newName = "";
            foreach (Match match in result)
            {
                newName += match.Value + "_";
            }
            newName = newName.Remove(newName.Length - 1, 1);

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
        
        public static ParcelScene TranslateManifestToScene(Manifest.Manifest manifest)
        {
            GameObject parcelGameObject = new GameObject("Builder Scene SceneId: " + manifest.scene.id);
            ParcelScene scene = parcelGameObject.AddComponent<ParcelScene>();
            
            //We remove the old assets to they don't collide with the new ones
            BIWUtils.RemoveAssetsFromCurrentScene();
            
            //We add the assets from the scene to the catalog
            var assets = manifest.scene.assets.Values.ToArray();
            AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(assets);
            
            //We create and assign the data of the scene
            LoadParcelScenesMessage.UnityParcelScene parcelData = new LoadParcelScenesMessage.UnityParcelScene();
            parcelData.id = manifest.scene.id;
            
            //This scene doesn't exist in the world, so we set it in the 0,0 coordinate
            parcelData.basePosition =  new Vector2Int(0, 0);
            
            //We set the parcels as the first one is in the 0,0, the first one will be in the bottom-left corner 
            parcelData.parcels =  new Vector2Int[manifest.project.rows * manifest.project.cols];
            int x = 0;
            int y = 0;
            for (int index = 0; index == parcelData.parcels.Length; index++)
            {
                parcelData.parcels[index] = new Vector2Int(x, y);
                y++;
                if (y == manifest.project.rows)
                {
                    x++;
                    y = 0;
                }
            }

            //We prepare the mappings to the scenes
            Dictionary<string, string> contentDictionary = new Dictionary<string, string>();
            
            foreach (var sceneObject in assets)
            {
                foreach (var content in sceneObject.contents)
                {
                    if(!contentDictionary.ContainsKey(content.Key))
                        contentDictionary.Add(content.Key,content.Value);    
                }
            }

            //We add the mappings to the scene
            BIWUtils.AddSceneMappings(contentDictionary, BIWUrlUtils.GetUrlSceneObjectContent(),parcelData);
            
            //The data is built so we set the data of the scene 
            scene.SetData(parcelData);

            // We iterate all the entities to create the entity in the scene
            foreach (BuilderEntity builderEntity in manifest.scene.entities.Values)
            {
                var entity = scene.CreateEntity(builderEntity.id);

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
                            string assedId = url.Replace("ethereum://","");
                            int index = assedId.IndexOf("/", StringComparison.Ordinal);
                            string partToremove = assedId.Substring(index);
                            assedId = assedId.Replace(partToremove, "");
                            
                            NFTShape.Model nftModel = new NFTShape.Model();
                            nftModel.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
                            nftModel.src = url;
                            nftModel.assetId = assedId;
                            
                            EntityComponentsUtils.AddNFTShapeComponent(scene, entity, nftModel,component.id);
                            break;
                        
                        case "Name":
                            nameComponentFound = true;
                            DCLName.Model nameModel = JsonConvert.DeserializeObject<DCLName.Model>(component.data.ToString());
                            nameModel.builderValue = builderEntity.name;
                            EntityComponentsUtils.AddNameComponent(scene , entity,nameModel, Guid.NewGuid().ToString());
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
                    EntityComponentsUtils.AddNameComponent(scene , entity,nameModel, Guid.NewGuid().ToString());
                }
            }
            
            //We already have made all the necessary steps to configure the parcel, so we init the scene
            scene.sceneLifecycleHandler.SetInitMessagesDone();
            
            return scene;
        }
    }
}