namespace Altom.AltDriver
{
    public struct AltColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
        public AltColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1;

        }
        public AltColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AltColor))
                return false;
            var other = (AltColor)obj;
            return
                other.r == this.r &&
                other.g == this.g &&
                other.b == this.b &&
                other.a == this.a;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("RGBA({0},{1},{2},{3})", this.r, this.g, this.b, this.a);
        }
    }
}
