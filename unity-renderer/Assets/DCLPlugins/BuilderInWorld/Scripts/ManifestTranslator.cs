using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder.Manifest;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Builder
{
    public static class ManifestTranslator
    {
        public static ParcelScene TranslateManifestToScene(Manifest.Manifest manifest)
        {
            GameObject parcelGameObject = new GameObject("Builder Scene SceneId: " + manifest.scene.id);
            ParcelScene scene = parcelGameObject.AddComponent<ParcelScene>();

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
                            DCLTransform.Model model = (DCLTransform.Model)component.data;
                            EntityComponentsUtils.AddTransformComponent(scene, entity, model);
                            break;
                        
                        case "GLTFShape":
                            LoadableShape.Model gltfModel = (LoadableShape.Model) component.data;
                            EntityComponentsUtils.AddGLTFComponent(scene, entity, gltfModel, component.id);
                            break;
                        
                        case "NFTShape":
                            //Builder use a different way to load the NFT so we convert it to our system
                            string url = (string) component.data;
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
                            DCLName.Model nameModel = (DCLName.Model) component.data;
                            EntityComponentsUtils.AddNameComponent(scene , entity,nameModel, Guid.NewGuid().ToString());
                            break;
                        
                        case "LockedOnEdit":
                            DCLLockedOnEdit.Model lockedModel = (DCLLockedOnEdit.Model) component.data;
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
            
            //We remove the old assets to they don't collide with the new ones
            BIWUtils.RemoveAssetsFromCurrentScene();
            
            //We add the assets from the scene to the catalog
            AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(manifest.scene.assets.Values.ToArray());
            
            return scene;
        }
    }
}