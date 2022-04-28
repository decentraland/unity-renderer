using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using UnityEditor;
using UnityEngine;

namespace DCL.Builder
{
    public static class EntityComponentsUtils
    {
        public static void AddTransformComponent(IParcelScene scene, IDCLEntity entity, DCLTransform.Model model)
        {
            scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(entity.entityId, CLASS_ID_COMPONENT.TRANSFORM, model);
        }

        public static NFTShape AddNFTShapeComponent(IParcelScene scene, IDCLEntity entity, NFTShape.Model model, string id = "")
        {
            id = EnsureId(id);
            
            NFTShape nftShape = (NFTShape) scene.componentsManagerLegacy.SceneSharedComponentCreate(id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
            nftShape.model = model;
            scene.componentsManagerLegacy.SceneSharedComponentAttach(entity.entityId, nftShape.id);
            return nftShape;
        }
        
        public static GLTFShape AddGLTFComponent(IParcelScene scene, IDCLEntity entity, GLTFShape.Model model, string id = "")
        {
            id = EnsureId(id);
            
            GLTFShape gltfComponent = (GLTFShape) scene.componentsManagerLegacy.SceneSharedComponentCreate(id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
            gltfComponent.model = model;
            scene.componentsManagerLegacy.SceneSharedComponentAttach(entity.entityId, gltfComponent.id);
            return gltfComponent;
        }

        public static DCLName AddNameComponent(IParcelScene scene, IDCLEntity entity, DCLName.Model model, string id = "")
        {
            id = EnsureId(id);
            
            DCLName name = (DCLName) scene.componentsManagerLegacy.SceneSharedComponentCreate(id, Convert.ToInt32(CLASS_ID.NAME));
            name.UpdateFromModel(model);
            scene.componentsManagerLegacy.SceneSharedComponentAttach(entity.entityId, name.id);
            return name;
        }
        
        public static DCLLockedOnEdit AddLockedOnEditComponent(IParcelScene scene, IDCLEntity entity, DCLLockedOnEdit.Model model, string id = "")
        {
            id = EnsureId(id);
            
            DCLLockedOnEdit lockedOnEditComponent = (DCLLockedOnEdit) scene.componentsManagerLegacy.SceneSharedComponentCreate(id, Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
            lockedOnEditComponent.UpdateFromModel(model);
            scene.componentsManagerLegacy.SceneSharedComponentAttach(entity.entityId, lockedOnEditComponent.id);
            return lockedOnEditComponent;
        }

        private static string EnsureId(string currentId)
        {
            if (string.IsNullOrEmpty(currentId))
                return Guid.NewGuid().ToString();
            return currentId;
        }
    }
}
