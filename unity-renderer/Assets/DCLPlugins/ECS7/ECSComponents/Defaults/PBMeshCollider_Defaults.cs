namespace DCL.ECSComponents
{
    public static class PBMeshCollider_Defaults
    {
        public static float GetTopRadius(this PBMeshCollider.Types.CylinderMesh self)
        {
            return self.HasRadiusTop ? self.RadiusTop : 0.5f;
        }

        public static float GetBottomRadius(this PBMeshCollider.Types.CylinderMesh self)
        {
            return self.HasRadiusBottom ? self.RadiusBottom : 0.5f;
        }

        public static int GetColliderLayer(this PBMeshCollider self)
        {
            return self.HasCollisionMask ? self.CollisionMask : ((int)ColliderLayer.ClPhysics | (int)ColliderLayer.ClPointer);
        }
    }
}
