using System;
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

        public static void AddScene( this DataStore_WorldObjects self, int sceneNumber )
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                self.sceneData.Add(sceneNumber, new DataStore_WorldObjects.SceneData());
        }

        public static void RemoveScene( this DataStore_WorldObjects self, int sceneNumber )
        {
            if (self.sceneData.ContainsKey(sceneNumber))
                self.sceneData.Remove(sceneNumber);
        }

        /// <summary>
        /// Add owner to the excluded owners list.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sceneId"></param>
        /// <param name="ownerId"></param>
        [Obsolete("This feature is only used by the SmartItem component and will have to be deprecated on the future. Please don't use it.")]
        public static void AddExcludedOwner(this DataStore_WorldObjects self, int sceneNumber, long ownerId)
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                return;

            self.sceneData[sceneNumber].ignoredOwners.Add(ownerId);
            self.sceneData[sceneNumber].owners.Remove(ownerId);
        }

        [Obsolete("This feature is only used by the SmartItem component and will have to be deprecated on the future. Please don't use it.")]
        public static void RemoveExcludedOwner(this DataStore_WorldObjects self, int sceneNumber, long ownerId)
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                return;

            self.sceneData[sceneNumber].ignoredOwners.Remove(ownerId);
        }

        public static void AddTexture(this DataStore_WorldObjects self, int sceneNumber, long ownerId, Texture texture)
        {
            var r = new Rendereable();
            r.textures.Add(texture);
            r.ownerId = ownerId;
            AddRendereable(self, sceneNumber, r);
        }

        public static void RemoveTexture(this DataStore_WorldObjects self, int sceneNumber, long ownerId, Texture texture )
        {
            var r = new Rendereable();
            r.textures.Add(texture);
            r.ownerId = ownerId;
            RemoveRendereable(self, sceneNumber, r);
        }

        public static void AddMaterial(this DataStore_WorldObjects self, int sceneNumber, long ownerId,
            Material material)
        {
            var r = new Rendereable();
            r.materials.Add(material);
            r.ownerId = ownerId;
            AddRendereable(self, sceneNumber, r);
        }

        public static void RemoveMaterial(this DataStore_WorldObjects self, int sceneNumber, long ownerId,
            Material material)
        {
            var r = new Rendereable();
            r.materials.Add(material);
            r.ownerId = ownerId;
            RemoveRendereable(self, sceneNumber, r);
        }

        public static void AddAudioClip(this DataStore_WorldObjects self, int sceneNumber, AudioClip clip)
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                return;

            // NOTE(Brian): entityId is not used here, so audio clips do not work with the ignoreOwners
            //              feature. This is done on purpose as ignoreOwners is only used by the smart item component
            //              and should be deprecated. Also, supporting this would complicate the tracking logic and
            //              has a high maintenance cost.

            var sceneData = self.sceneData[sceneNumber];
            sceneData.audioClips.Add(clip);
        }

        public static void RemoveAudioClip(this DataStore_WorldObjects self, int sceneNumber, AudioClip clip)
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                return;

            var sceneData = self.sceneData[sceneNumber];
            sceneData.audioClips.Remove(clip);
        }
        
        public static void AddRendereable( this DataStore_WorldObjects self, int sceneNumber, Rendereable rendereable )
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                return;

            if (rendereable == null)
            {
                logger.Log( $"Trying to add null rendereable! (scene number: {sceneNumber})");
                return;
            }

            if (sceneNumber <= 0 || !self.sceneData.ContainsKey(sceneNumber))
            {
                logger.Log($"AddRendereable", $"invalid sceneNumber! (scene number: {sceneNumber})");
                return;
            }

            var sceneData = self.sceneData[sceneNumber];

            if ( sceneData.ignoredOwners.Contains(rendereable.ownerId))
                return;

            sceneData.materials.AddRefCount(rendereable.materials);
            sceneData.meshes.AddRefCount(rendereable.meshes);
            sceneData.textures.AddRefCount(rendereable.textures);
            sceneData.renderers.Add(rendereable.renderers);
            sceneData.owners.Add(rendereable.ownerId);
            sceneData.animationClips.AddRefCount(rendereable.animationClips);
            sceneData.triangles.Set( sceneData.triangles.Get() + rendereable.totalTriangleCount);
            sceneData.animationClipSize.Set(sceneData.animationClipSize.Get() + rendereable.animationClipSize);
            sceneData.meshDataSize.Set(sceneData.meshDataSize.Get() + rendereable.meshDataSize);
        }

        public static void RemoveRendereable( this DataStore_WorldObjects self, int sceneNumber, Rendereable rendereable )
        {
            if (!self.sceneData.ContainsKey(sceneNumber))
                return;

            if ( rendereable == null )
            {
                logger.Log( $"Trying to remove null rendereable! (scene number: {sceneNumber})");
                return;
            }

            if (sceneNumber <= 0 || !self.sceneData.ContainsKey(sceneNumber) )
            {
                logger.Log($"RemoveRendereable", $"invalid sceneNumber! (scene number: {sceneNumber})");
                return;
            }

            var sceneData = self.sceneData[sceneNumber];

            if ( sceneData.ignoredOwners.Contains(rendereable.ownerId))
                return;

            sceneData.materials.RemoveRefCount(rendereable.materials);
            sceneData.meshes.RemoveRefCount(rendereable.meshes);
            sceneData.textures.RemoveRefCount(rendereable.textures);
            sceneData.renderers.Remove(rendereable.renderers);
            sceneData.owners.Remove(rendereable.ownerId);
            sceneData.animationClips.RemoveRefCount(rendereable.animationClips);
            sceneData.triangles.Set( sceneData.triangles.Get() - rendereable.totalTriangleCount);
            sceneData.animationClipSize.Set(sceneData.animationClipSize.Get() - rendereable.animationClipSize);
            sceneData.meshDataSize.Set(sceneData.meshDataSize.Get() - rendereable.meshDataSize);
        }
    }
}