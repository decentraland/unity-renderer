using UnityEngine;
using System.Collections.Generic;

namespace Builder.MeshLoadIndicator
{
    public class DCLBuilderMeshLoadIndicatorController : MonoBehaviour
    {
        [SerializeField] private DCLBuilderMeshLoadIndicator baseIndicator = null;

        private Queue<DCLBuilderMeshLoadIndicator> indicatorsAvailable;
        private List<DCLBuilderMeshLoadIndicator> indicatorsInUse;

        private bool isGameObjectActive = false;
        private bool isPreviewMode = false;

        private void Awake()
        {
            indicatorsAvailable = new Queue<DCLBuilderMeshLoadIndicator>();
            indicatorsInUse = new List<DCLBuilderMeshLoadIndicator>();

            indicatorsAvailable.Enqueue(baseIndicator);
        }

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderEntity.OnEntityAddedWithTransform += OnEntityAdded;
                DCLBuilderEntity.OnEntityShapeUpdated += OnShapeUpdated;
                DCLBuilderBridge.OnResetBuilderScene += OnResetBuilderScene;
                DCLBuilderBridge.OnPreviewModeChanged += OnPreviewModeChanged;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderEntity.OnEntityAddedWithTransform -= OnEntityAdded;
            DCLBuilderEntity.OnEntityShapeUpdated -= OnShapeUpdated;
            DCLBuilderBridge.OnResetBuilderScene -= OnResetBuilderScene;
            DCLBuilderBridge.OnPreviewModeChanged -= OnPreviewModeChanged;
        }

        private void OnEntityAdded(DCLBuilderEntity entity)
        {
            if (!entity.HasShape() && !isPreviewMode)
            {
                ShowIndicator(entity.transform.position, entity.rootEntity.entityId);
            }
        }

        private void OnShapeUpdated(DCLBuilderEntity entity)
        {
            if (!isPreviewMode)
            {
                HideIndicator(entity.rootEntity.entityId);
            }
        }

        private void OnResetBuilderScene()
        {
            HideAllIndicators();
        }

        private void OnPreviewModeChanged(bool isPreview)
        {
            isPreviewMode = isPreview;
            if (isPreview)
            {
                HideAllIndicators();
            }
        }

        public DCLBuilderMeshLoadIndicator ShowIndicator(Vector3 position, string entityId)
        {
            DCLBuilderMeshLoadIndicator ret;

            if (indicatorsAvailable == null)
                return null;

            if (indicatorsAvailable.Count > 0)
            {
                ret = indicatorsAvailable.Dequeue();
                ret.transform.position = position;
            }
            else
            {
                ret = Object.Instantiate(baseIndicator, position, Quaternion.identity, transform);
            }

            ret.loadingEntityId = entityId;
            ret.gameObject.SetActive(true);
            indicatorsInUse.Add(ret);
            return ret;
        }

        public void HideIndicator(string entityId)
        {
            for (int i = 0; i < indicatorsInUse.Count; i++)
            {
                if (indicatorsInUse[i].loadingEntityId == entityId)
                {
                    indicatorsInUse[i].gameObject.SetActive(false);
                    indicatorsAvailable.Enqueue(indicatorsInUse[i]);
                    indicatorsInUse.RemoveAt(i);
                    break;
                }
            }
        }

        private void HideAllIndicators()
        {
            for (int i = 0; i < indicatorsInUse.Count; i++)
            {
                indicatorsInUse[i].gameObject.SetActive(false);
                indicatorsAvailable.Enqueue(indicatorsInUse[i]);
            }
            indicatorsInUse.Clear();
        }
    }
}