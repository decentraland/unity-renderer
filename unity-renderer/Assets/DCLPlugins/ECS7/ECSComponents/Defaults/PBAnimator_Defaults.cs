namespace DCL.ECSComponents
{
    public static class PBAnimator_Defaults
    {
        public static float GetWeight(this PBAnimationState self)
        {
            return self.HasWeight ? self.Weight : 1.0f;
        }
        
        public static float GetSpeed(this PBAnimationState self)
        {
            return self.HasSpeed ? self.Speed : 1.0f;
        }
        
        public static bool GetLoop(this PBAnimationState self)
        {
            return !self.HasLoop || self.Loop;
        }
    }
}