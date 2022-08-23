namespace DCL.ECSComponents
{
    public static class PBMeshRenderer_Defaults
    {
        public static float GetTopRadius(this CylinderMesh self)
        {
            return self.HasRadiusTop ? self.RadiusTop : 1;
        }

        public static float GetBottomRadius(this CylinderMesh self)
        {
            return self.HasRadiusBottom ? self.RadiusBottom : 1;
        }
    }
}