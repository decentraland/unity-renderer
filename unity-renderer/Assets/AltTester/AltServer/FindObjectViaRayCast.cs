using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Altom.AltTester
{
    public class FindObjectViaRayCast
    {
        private static AltMockUpPointerInputModule _mockUpPointerInputModule;
        public static AltMockUpPointerInputModule AltMockUpPointerInputModule
        {
            get
            {
                if (_mockUpPointerInputModule == null)
                {
                    if (EventSystem.current != null)
                    {
                        _mockUpPointerInputModule = EventSystem.current.gameObject.AddComponent<AltMockUpPointerInputModule>();
                    }
                    else
                    {
                        var newEventSystem = new GameObject("EventSystem");
                        _mockUpPointerInputModule = newEventSystem.AddComponent<AltMockUpPointerInputModule>();
                    }
                }
                return _mockUpPointerInputModule;
            }

        }
        /// <summary>
        /// Finds element at given pointerEventData for which we raise EventSystem input events
        /// </summary>
        /// <param name="pointerEventData"></param>
        /// <returns>the found gameObject</returns>
        private static UnityEngine.GameObject findEventSystemObject(UnityEngine.EventSystems.PointerEventData pointerEventData)
        {
            UnityEngine.EventSystems.RaycastResult firstRaycastResult;
            AltMockUpPointerInputModule.GetFirstRaycastResult(pointerEventData, out firstRaycastResult);
            pointerEventData.pointerCurrentRaycast = firstRaycastResult;
            pointerEventData.pointerPressRaycast = firstRaycastResult;
            return firstRaycastResult.gameObject;
        }
        public static UnityEngine.GameObject FindObjectAtCoordinates(UnityEngine.Vector2 screenPosition)
        {
            var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = screenPosition,
                button = UnityEngine.EventSystems.PointerEventData.InputButton.Left,
                eligibleForClick = true,
                pressPosition = screenPosition
            };
            var eventSystemTarget = findEventSystemObject(pointerEventData);
            if (eventSystemTarget != null) return eventSystemTarget;
            var monoBehaviourTarget = FindMonoBehaviourObject(screenPosition);
            return monoBehaviourTarget;
        }
        /// <summary>
        /// Finds element(s) at given coordinates for which we raise MonoBehaviour input events
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns>the found gameObject</returns>
        public static UnityEngine.GameObject FindMonoBehaviourObject(UnityEngine.Vector2 coordinates)
        {
            var target = GetGameObjectHitMonoBehaviour(coordinates);
            if (target == null)
                return null;

            var rigidBody = target.GetComponentInParent<UnityEngine.Rigidbody>();
            if (rigidBody != null)
                return rigidBody.gameObject;
            // var rigidBody2D = target.GetComponentInParent<UnityEngine.Rigidbody2D>();
            // if (rigidBody2D != null)
            //     return rigidBody2D.gameObject;
            return target;
        }
        public static UnityEngine.GameObject GetGameObjectHitMonoBehaviour(UnityEngine.Vector2 coordinates)
        {
            // foreach (var camera in UnityEngine.Camera.allCameras.OrderByDescending(c => c.depth))
            // {
            //     UnityEngine.RaycastHit hit;
            //     UnityEngine.Ray ray = camera.ScreenPointToRay(coordinates);
            //     UnityEngine.GameObject gameObject3d = null;
            //     UnityEngine.GameObject gameObject2d = null;
            //     UnityEngine.Vector3 hitPosition3d = UnityEngine.Vector3.zero;
            //     UnityEngine.Vector3 hitPosition2d = UnityEngine.Vector3.zero;
            //     if (UnityEngine.Physics.Raycast(ray, out hit))
            //     {
            //         hitPosition3d = hit.point;
            //         gameObject3d = hit.transform.gameObject;
            //     }
            //     // UnityEngine.RaycastHit2D hit2d;
            //     // if (hit2d = UnityEngine.Physics2D.Raycast(coordinates, UnityEngine.Vector2.zero))
            //     // {
            //     //     hitPosition2d = hit2d.point;
            //     //     gameObject2d = hit2d.transform.gameObject;
            //     // }


            //     if (gameObject2d != null && gameObject3d != null)
            //     {
            //         if (UnityEngine.Vector3.Distance(camera.transform.position, hitPosition2d) < UnityEngine.Vector3.Distance(camera.transform.position, hitPosition3d))
            //             return gameObject2d;
            //         else
            //             return gameObject3d;
            //     }
            //     if (gameObject2d != null) return gameObject2d;
            //     if (gameObject3d != null) return gameObject3d;
            // }
            return null;
        }

    }

}