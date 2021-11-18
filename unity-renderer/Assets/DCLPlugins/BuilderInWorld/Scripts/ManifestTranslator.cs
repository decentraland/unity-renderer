using System;
using System.Collections;
using System.Collections.Generic;
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
                            NFTShape.Model nftShapeModel = (NFTShape.Model) component.data;
                            EntityComponentsUtils.AddNFTShapeComponent(scene, entity, nftShapeModel,component.id);
                            break;
                        
                        case "Name":
                            break;
                        case "LockedOnEdit":
                            break;
                    }
                }
            }
            
            return scene;
        }
    }
}