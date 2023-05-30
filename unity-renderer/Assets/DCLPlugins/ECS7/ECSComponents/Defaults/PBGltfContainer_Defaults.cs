namespace DCL.ECSComponents
{
    public static class PBGltfContainer_Defaults
    {
        public static uint GetVisibleMeshesCollisionMask(this PBGltfContainer self)
        {
            return self.VisibleMeshesCollisionMask;
        }

        public static uint GetInvisibleMeshesCollisionMask(this PBGltfContainer self)
        {
            return self.HasInvisibleMeshesCollisionMask
                ? self.InvisibleMeshesCollisionMask
                : (int)(ColliderLayer.ClPhysics | ColliderLayer.ClPointer);
        }
    }
}
