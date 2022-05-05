using DCL.Builder;
using DCL.Configuration;
using UnityEngine;

public class BIWRaycastController : BIWController, IBIWRaycastController
{
    public event System.Action<IBIWGizmosAxis> OnGizmosAxisPressed;

    private Camera builderCamera;
    private IBIWEntityHandler entityHandler;
    private IBIWModeController modeController;

    private const float RAYCAST_MAX_DISTANCE = 10000f;

    public LayerMask gizmoMask { get; private set; }

    public override void Initialize(IContext context)
    {
        base.Initialize(context);

        entityHandler = context.editorContext.entityHandler;
        modeController = context.editorContext.modeController;
        gizmoMask = BIWSettings.GIZMOS_LAYER;
        BIWInputWrapper.OnMouseDown += OnMouseDown;

        builderCamera = context.sceneReferences.mainCamera;
    }

    public override void Dispose()
    {
        base.Dispose();
        BIWInputWrapper.OnMouseDown -= OnMouseDown;
    }

    private void OnMouseDown(int buttonId, Vector3 mousePosition)
    {
        if (buttonId != 0)
            return;

        CheckGizmosRaycast(mousePosition);
    }

    public BIWEntity GetEntityOnPointer()
    {
        Camera camera = Camera.main;

        if (camera == null)
            return null;

        RaycastHit hit;
        UnityEngine.Ray ray = camera.ScreenPointToRay(modeController.GetMousePosition());
        float distanceToSelect = modeController.GetMaxDistanceToSelectEntities();

        if (Physics.Raycast(ray, out hit, distanceToSelect, BIWSettings.COLLIDER_SELECTION_LAYER))
        {
            long entityID = long.Parse(hit.collider.gameObject.name);

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                return entityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);
            }
        }

        return null;
    }

    public bool RayCastFloor(out Vector3 position)
    {
        RaycastHit hit;

        UnityEngine.Ray ray = GetMouseRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, BIWSettings.GROUND_LAYER))
        {
            position = hit.point;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    public bool Raycast(Vector3 mousePosition, LayerMask mask, out RaycastHit hitInfo, System.Func<RaycastHit[], RaycastHit> hitComparer)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(GetMouseRay(mousePosition), RAYCAST_MAX_DISTANCE, mask);
        if (hits.Length > 0)
        {
            hitInfo = hitComparer(hits);
            return true;
        }

        hitInfo = new RaycastHit();
        return false;
    }

    public VoxelEntityHit GetCloserUnselectedVoxelEntityOnPointer()
    {
        RaycastHit[] hits;
        UnityEngine.Ray ray = GetMouseRay(Input.mousePosition);

        float currentDistance = 9999;
        VoxelEntityHit voxelEntityHit = null;

        hits = Physics.RaycastAll(ray, BIWSettings.RAYCAST_MAX_DISTANCE, BIWSettings.COLLIDER_SELECTION_LAYER);

        foreach (RaycastHit hit in hits)
        {
            long entityID = long.Parse(hit.collider.gameObject.name);

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                BIWEntity entityToCheck = entityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);

                if (entityToCheck == null)
                    continue;

                if (entityToCheck.isSelected || !entityToCheck.gameObject.CompareTag(BIWSettings.VOXEL_TAG))
                    continue;

                Camera camera = Camera.main;
                if (Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position) >= currentDistance)
                    continue;

                voxelEntityHit = new VoxelEntityHit(entityToCheck, hit);
                currentDistance = Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
            }
        }

        return voxelEntityHit;
    }

    public Vector3 GetFloorPointAtMouse(Vector3 mousePosition)
    {
        Camera camera = context.sceneReferences.mainCamera;

        if ( camera == null )
            return Vector3.zero;

        RaycastHit hit;
        UnityEngine.Ray ray = camera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, BIWSettings.GROUND_LAYER))
            return hit.point;

        return Vector3.zero;
    }

    public Ray GetMouseRay(Vector3 mousePosition) { return builderCamera.ScreenPointToRay(mousePosition); }

    #region Gizmos

    private void CheckGizmosRaycast(Vector3 mousePosition)
    {
        RaycastHit hit;
        if (Raycast(mousePosition, gizmoMask, out hit, CompareSelectionHit))
        {
            BIWGizmosAxis gizmosAxis = hit.collider.gameObject.GetComponent<BIWGizmosAxis>();
            if (gizmosAxis != null)
                OnGizmosAxisPressed?.Invoke(gizmosAxis);
        }
    }

    public bool RaycastToGizmos(Ray ray, out RaycastHit hitInfo) { return Physics.Raycast(ray, out hitInfo, RAYCAST_MAX_DISTANCE, gizmoMask); }

    public bool RaycastToGizmos(Vector3 mousePosition, out RaycastHit hitInfo) { return RaycastToGizmos(GetMouseRay(mousePosition), out hitInfo); }

    private RaycastHit CompareSelectionHit(RaycastHit[] hits)
    {
        RaycastHit closestHit = hits[0];

        if (IsGizmoHit(closestHit)) // Gizmos has always priority
            return closestHit;

        return closestHit;
    }

    private bool IsGizmoHit(RaycastHit hit) { return hit.collider.gameObject.GetComponent<BIWGizmosAxis>() != null; }

    #endregion
}