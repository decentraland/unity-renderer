using DCL.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;

namespace DCL
{
    public class Asset_GLTF : Asset_WithPoolableContainer
    {
        public string name;
        public bool visible = true;

        public override GameObject container { get; set; }

        public HashSet<Mesh> meshes = new HashSet<Mesh>();
        public Dictionary<Mesh, int> meshToTriangleCount = new Dictionary<Mesh, int>();
        public HashSet<Renderer> renderers = new HashSet<Renderer>();
        public HashSet<Material> materials = new HashSet<Material>();
        public HashSet<Texture> textures = new HashSet<Texture>();
        public int totalTriangleCount = 0;

        Coroutine showCoroutine;

        public Asset_GLTF()
        {
            container = new GameObject("Asset_GLTF Container");
            visible = true;
        }

        public override object Clone()
        {
            Asset_GLTF result = this.MemberwiseClone() as Asset_GLTF;
            result.visible = true;
            result.meshes = new HashSet<Mesh>(meshes);
            return result;
        }

        public override void Cleanup()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            
            Object.Destroy(container);
        }

        public void Hide()
        {
            if (container != null)
            {
                container.transform.parent = null;
                container.transform.position = EnvironmentSettings.MORDOR;
            }

            visible = false;
        }

        public void CancelShow()
        {
            if (showCoroutine != null)
                CoroutineStarter.Stop(showCoroutine);
        }

        public void Show(bool useMaterialTransition, System.Action OnFinish)
        {
            if (showCoroutine != null)
                CoroutineStarter.Stop(showCoroutine);

            if (!visible)
            {
                OnFinish?.Invoke();
                return;
            }

            bool renderingEnabled = CommonScriptableObjects.rendererState.Get();

            if (!renderingEnabled || !useMaterialTransition)
            {
                container.SetActive(true);
                OnFinish?.Invoke();
                return;
            }

            showCoroutine = CoroutineStarter.Start(ShowCoroutine(OnFinish));
        }

        public IEnumerator ShowCoroutine(System.Action OnFinish)
        {
            // NOTE(Brian): This fixes seeing the object in the scene 0,0 for a frame
            yield return new WaitForSeconds(Random.Range(0, 0.05f));

            // NOTE(Brian): This GameObject can be removed by distance after the delay
            if (container == null)
            {
                OnFinish?.Invoke();
                yield break;
            }

            container?.SetActive(true);
            OnFinish?.Invoke();
        }
    }
}