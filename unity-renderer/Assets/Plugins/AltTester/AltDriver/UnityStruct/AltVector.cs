namespace Altom.AltDriver
{
    public struct AltVector2
    {
        public float x;
        public float y;

        public AltVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is AltVector2))
                return false;
            var other = (AltVector2)obj;
            return
                other.x == this.x &&
                other.y == this.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
    public struct AltVector3
    {
        public float x;
        public float y;
        public float z;

        public AltVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public AltVector3(float x, float y) : this(x, y, 0)
        {
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AltVector3))
                return false;
            var other = (AltVector3)obj;
            return
                other.x == this.x &&
                other.y == this.y &&
                other.z == this.z;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
