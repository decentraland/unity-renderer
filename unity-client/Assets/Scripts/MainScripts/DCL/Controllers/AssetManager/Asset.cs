namespace DCL
{
    /// <summary>
    /// Any Asset is the resource wrapper. It has all the specific logic related to handling the asset visually
    /// and cloning/cleaning it up.
    /// 
    /// Here we shouldn't have any loading related code.
    /// </summary>
    public class Asset : System.ICloneable
    {
        public object id { get; internal set; }

        public Asset()
        {
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public virtual void Cleanup()
        {
        }
    }
}
