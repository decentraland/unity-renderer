namespace DCL.ECSComponents
{
    public static class PBMeshCollider_Defaults
    {
        public static float GetTopRadius(this PBMeshCollider.Types.CylinderMesh self)
        {
            return self.HasRadiusTop ? self.RadiusTop : 1;
        }

        public static float GetBottomRadius(this PBMeshCollider.Types.CylinderMesh self)
        {
            return self.HasRadiusBottom ? self.RadiusBottom : 1;
        }

        public static int GetColliderLayer(this PBMeshCollider self)
        {
            return self.HasCollisionMask ? self.CollisionMask : ((int)ColliderLayer.Physics | (int)ColliderLayer.Pointer);
        }
    }
}