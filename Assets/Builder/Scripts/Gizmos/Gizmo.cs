using UnityEngine;

namespace Builder
{
    public class Gizmo : MonoBehaviour
    {
        public string gizmoType;
        public bool transformWithObject;
        public GizmoAxis[] axes;

        protected float snapFactor = 0;

        public void SetObject(GameObject selectedObject)
        {
            if (selectedObject != null)
            {
                if (transformWithObject)
                {
                    transform.SetParent(selectedObject.transform);
                    transform.localPosition = Vector3.zero;

                }
                else
                {
                    transform.position = selectedObject.transform.position;
                }

                gameObject.SetActive(true);
            }
            else
            {
                transform.SetParent(null);
                gameObject.SetActive(false);
            }
        }
        public void SetSnapFactor(float position, float rotation, float scale)
        {
            for (int i = 0; i < axes.Length; i++)
            {
                axes[i].SetSnapFactor(position, rotation, scale);
            }
        }
    }
}