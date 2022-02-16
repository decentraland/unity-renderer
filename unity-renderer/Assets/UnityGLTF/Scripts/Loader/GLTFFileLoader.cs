using System;
using System.Collections;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using UnityEngine;

// using System.Threading.Tasks;

namespace UnityGLTF.Loader
{
    public class GLTFFileLoader : ILoader
    {
        private string _rootDirectoryPath;
        public Stream LoadedStream { get; private set; }

        public bool HasSyncLoadMethod { get; private set; }
        public AssetIdConverter assetIdConverter => null;

        public GLTFFileLoader(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
            HasSyncLoadMethod = true;
        }

        public async UniTask LoadStream(string gltfFilePath, CancellationToken token)
        {
            if (gltfFilePath == null)
            {
                Debug.Log("GLTFSceneImporter - Error - gltfFilePath is null!");
                return;
            }

            await LoadFileStream(_rootDirectoryPath, gltfFilePath, token);
        }

        private async UniTask LoadFileStream(string rootPath, string fileToLoad, CancellationToken token)
        {
            string pathToLoad = Path.Combine(rootPath, fileToLoad);

            if (!File.Exists(pathToLoad))
            {
                Debug.LogError($"GLTFSceneImporter - Error - Buffer file not found ({pathToLoad}) -- {fileToLoad}");
                return;
            }

            await UniTask.Yield();
            LoadedStream = File.OpenRead(pathToLoad);
        }

        public void LoadStreamSync(string gltfFilePath)
        {
            if (gltfFilePath == null)
            {
                Debug.Log("GLTFSceneImporter - Error - gltfFilePath is null!");
                return;
            }

            LoadFileStreamSync(_rootDirectoryPath, gltfFilePath);
        }

        private void LoadFileStreamSync(string rootPath, string fileToLoad)
        {
            string pathToLoad = Path.Combine(rootPath, fileToLoad);

            if (!File.Exists(pathToLoad))
            {
                Debug.Log("GLTFSceneImporter - Error - Buffer file not found -- " + fileToLoad);

                return;
            }

            LoadedStream = File.OpenRead(pathToLoad);
        }
    }
}