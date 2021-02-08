using UnityEngine;

namespace DCL
{
    public abstract class Asset_WithPoolableContainer : Asset
    {
        public abstract GameObject container { get; set; }
    }

    /// <summary>
    /// Any Asset is the resource wrapper. It has all the specific logic related to handling the asset visually
    /// and cloning/cleaning it up.
    /// 
    /// Here we shouldn't have any loading related code.
    /// </summary>
    public abstract class Asset : System.ICloneable
    {
        public object id { get; internal set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public abstract void Cleanup();
    }
}
