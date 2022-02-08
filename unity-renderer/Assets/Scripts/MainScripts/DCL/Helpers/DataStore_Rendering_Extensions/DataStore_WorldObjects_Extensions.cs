﻿using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public static class DataStore_WorldObjects_Extensions
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public static void AddScene( this DataStore_WorldObjects self, string sceneId )
        {
            if (!self.sceneData.ContainsKey(sceneId))
                self.sceneData.Add(sceneId, new DataStore_WorldObjects.SceneData());
        }

        public static void RemoveScene( this DataStore_WorldObjects self, string sceneId )
        {
            if (self.sceneData.ContainsKey(sceneId))
                self.sceneData.Remove(sceneId);
        }

        /// <summary>
        /// Add owner to the excluded owners list.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sceneId"></param>
        /// <param name="ownerId"></param>
        [Obsolete("This feature is only used by the SmartItem component and will have to be deprecated on the future. Please don't use it.")]
        public static void AddExcludedOwner(this DataStore_WorldObjects self, string sceneId, string ownerId)
        {
            if (!self.sceneData.ContainsKey(sceneId))
                return;

            self.sceneData[sceneId].ignoredOwners.Add(ownerId);
            self.sceneData[sceneId].owners.Remove(ownerId);
        }

        [Obsolete("This feature is only used by the SmartItem component and will have to be deprecated on the future. Please don't use it.")]
        public static void RemoveExcludedOwner(this DataStore_WorldObjects self, string sceneId, string ownerId)
        {
            if (!self.sceneData.ContainsKey(sceneId))
                return;

            self.sceneData[sceneId].ignoredOwners.Remove(ownerId);
        }

        public static void AddTexture(this DataStore_WorldObjects self, string sceneId, string ownerId, Texture texture)
        {
            var r = new Rendereable();
            r.textures.Add(texture);
            r.ownerId = ownerId;
            AddRendereable(self, sceneId, r);
        }

        public static void RemoveTexture(this DataStore_WorldObjects self, string sceneId, string ownerId, Texture texture )
        {
            var r = new Rendereable();
            r.textures.Add(texture);
            r.ownerId = ownerId;
            RemoveRendereable(self, sceneId, r);
        }

        public static void AddMaterial(this DataStore_WorldObjects self, string sceneId, string ownerId, Material material )
        {
            var r = new Rendereable();
            r.materials.Add(material);
            r.ownerId = ownerId;
            AddRendereable(self, sceneId, r);
        }

        public static void RemoveMaterial(this DataStore_WorldObjects self, string sceneId, string ownerId, Material material )
        {
            var r = new Rendereable();
            r.materials.Add(material);
            r.ownerId = ownerId;
            RemoveRendereable(self, sceneId, r);
        }


        public static void AddRendereable( this DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
        {
            if (rendereable == null)
            {
                logger.Log( $"Trying to add null rendereable! (id: {sceneId})");
                return;
            }

            if (string.IsNullOrEmpty(sceneId))
            {
                logger.LogWarning($"AddRendereable", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            if (string.IsNullOrEmpty(rendereable.ownerId))
            {
                logger.LogError($"AddRendereable", $"invalid ownerId! Make sure to assign ownerId to the given rendereable (hint: it's the entityId)");
                return;
            }

            var sceneData = self.sceneData[sceneId];

            if ( sceneData.ignoredOwners.Contains(rendereable.ownerId))
                return;

            sceneData.materials.AddRefCount(rendereable.materials);
            sceneData.meshes.AddRefCount(rendereable.meshes);
            sceneData.textures.AddRefCount(rendereable.textures);
            sceneData.renderers.Add(rendereable.renderers);
            sceneData.owners.Add(rendereable.ownerId);
            sceneData.triangles.Set( sceneData.triangles.Get() + rendereable.totalTriangleCount);
        }

        public static void RemoveRendereable( this DataStore_WorldObjects self, string sceneId, Rendereable rendereable )
        {
            if ( rendereable == null )
            {
                logger.Log( $"Trying to remove null rendereable! (id: {sceneId})");
                return;
            }

            if ( string.IsNullOrEmpty(sceneId) || !self.sceneData.ContainsKey(sceneId) )
            {
                logger.LogWarning($"RemoveRendereable", $"invalid sceneId! (id: {sceneId})");
                return;
            }

            if (string.IsNullOrEmpty(rendereable.ownerId))
            {
                logger.LogError($"AddRendereable", $"invalid ownerId! Make sure to assign ownerId to the given rendereable (hint: it's the entityId)");
                return;
            }

            var sceneData = self.sceneData[sceneId];

            if ( sceneData.ignoredOwners.Contains(rendereable.ownerId))
                return;

            sceneData.materials.RemoveRefCount(rendereable.materials);
            sceneData.meshes.RemoveRefCount(rendereable.meshes);
            sceneData.textures.RemoveRefCount(rendereable.textures);
            sceneData.renderers.Remove(rendereable.renderers);
            sceneData.owners.Remove(rendereable.ownerId);
            sceneData.triangles.Set( sceneData.triangles.Get() - rendereable.totalTriangleCount);
        }
    }
}