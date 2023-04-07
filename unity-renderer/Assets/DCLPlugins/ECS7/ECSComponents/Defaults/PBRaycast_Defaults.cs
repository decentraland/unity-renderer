namespace DCL.ECSComponents
{
    public static class PBRaycast_Defaults
    {
        public static uint GetCollisionMask(this PBRaycast self)
        {
            return self.HasCollisionMask ? self.CollisionMask : (int)ColliderLayer.ClPhysics;
        }
    }
}
