using System;
using UnityEngine;

namespace DCL.UIElements.Structures
{
    public struct DCLUVs : IEquatable<DCLUVs>
    {
        public static readonly DCLUVs Default = new (Vector2.zero, Vector2.up, Vector2.one, Vector2.right);

        public Vector2 BottomLeft;
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomRight;

        public DCLUVs(Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight)
        {
            this.BottomLeft = bottomLeft;
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomRight = bottomRight;
        }

        public bool Equals(DCLUVs other) =>
            BottomLeft.Equals(other.BottomLeft) && TopLeft.Equals(other.TopLeft) && TopRight.Equals(other.TopRight) && BottomRight.Equals(other.BottomRight);

        public override bool Equals(object obj) =>
            obj is DCLUVs other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(BottomLeft, TopLeft, TopRight, BottomRight);
    }
}
