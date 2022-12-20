using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCL.UIElements.Image
{
    public interface IUITextureConsumer
    {
        Texture2D Texture { set; }
    }

    public enum DCLImageScaleMode
    {
        // Traditional slicing
        NINE_SLICES = 0,

        // Does not scale, draws in a pixel-perfect model relative to the object center
        CENTER = 1,

        // Scales the texture, maintaining aspect ratio, so it completely fits withing the position rectangle passed to GUI.DrawTexture
        // Corresponds to Sprite's ScaleMode.ScaleToFit.
        // Applies custom UVs
        STRETCH = 2
    }

    public struct DCLImageUVs : IEquatable<DCLImageUVs>
    {
        public Vector2 BottomLeft;
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomRight;

        public DCLImageUVs(Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight)
        {
            this.BottomLeft = bottomLeft;
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomRight = bottomRight;
        }

        public bool Equals(DCLImageUVs other) =>
            BottomLeft.Equals(other.BottomLeft) && TopLeft.Equals(other.TopLeft) && TopRight.Equals(other.TopRight) && BottomRight.Equals(other.BottomRight);

        public override bool Equals(object obj) =>
            obj is DCLImageUVs other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(BottomLeft, TopLeft, TopRight, BottomRight);
    }

    public class DCLImage : VisualElement, IUITextureConsumer
    {
        private DCLImageScaleMode scaleMode;
        private Texture2D texture2D;
        private Sprite sprite;
        private Vector4 slices;
        private Color tintColor;
        private DCLImageUVs uvs;

        private bool customMeshGenerationRequired;

        public DCLImageScaleMode ScaleMode
        {
            get => scaleMode;
            set => SetScaleMode(value);
        }

        public Texture2D Texture
        {
            get => texture2D;
            set => SetTexture(value);
        }

        public Vector4 Slices
        {
            get => slices;
            set => SetSlices(value);
        }

        public Color TintColor
        {
            get => tintColor;
            set => SetTintColor(value);
        }

        public DCLImageUVs UVs
        {
            get => uvs;
            set => SetUVs(value);
        }

        public DCLImage()
        {
        }

        public DCLImage(Texture2D texture2D, DCLImageScaleMode scaleMode, Vector4 slices, Color tintColor, DCLImageUVs uvs)
        {
            this.texture2D = texture2D;
            this.scaleMode = scaleMode;
            this.slices = slices;
            this.tintColor = tintColor;
            this.uvs = uvs;

            generateVisualContent += OnGenerateVisualContent;
        }

        private void SetScaleMode(DCLImageScaleMode scaleMode)
        {
            if (this.scaleMode == scaleMode)
                return;

            this.scaleMode = scaleMode;
            MarkDirtyRepaint();
        }

        private void SetTexture(Texture2D texture)
        {
            if (this.texture2D == texture)
                return;

            this.texture2D = texture;
            MarkDirtyRepaint();
        }

        private void SetSlices(Vector4 slices)
        {
            if (this.slices == slices || scaleMode != DCLImageScaleMode.NINE_SLICES)
                return;

            this.slices = slices;
            MarkDirtyRepaint();
        }

        private void SetTintColor(Color color)
        {
            if (this.tintColor == color)
                return;

            this.tintColor = color;
            MarkDirtyRepaint();
        }

        private void SetUVs(DCLImageUVs uvs)
        {
            if (this.uvs.Equals(uvs) || scaleMode != DCLImageScaleMode.STRETCH)
                return;

            this.uvs = uvs;
            MarkDirtyRepaint();
        }

        private void ResolveGenerationWay()
        {
            if (texture2D != null)
            {
                switch (scaleMode)
                {

                }
            }

            MarkDirtyRepaint();
        }

        private void SetSliced()
        {
            // Instead of generating a sliced mesh manually pass it to the existing logic of background
            style.backgroundImage = Background.FromTexture2D(texture2D);
            style.unityBackgroundImageTintColor = new StyleColor(tintColor);
            style.unitySliceLeft = new StyleInt((int) slices[0]);
            style.unitySliceTop = new StyleInt((int) slices[1]);
            style.unitySliceRight = new StyleInt((int) slices[2]);
            style.unitySliceBottom = new StyleInt((int) slices[3]);
            customMeshGenerationRequired = false;
        }

        private void SetCentered()
        {
            style.backgroundImage = new StyleBackground(StyleKeyword.None);
            customMeshGenerationRequired = true;
        }

        private void SetStretched()
        {

        }

        private void SetSolidColor()
        {
            style.backgroundImage = new StyleBackground(StyleKeyword.None);
            style.color = new StyleColor(tintColor);
            customMeshGenerationRequired = false;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (!customMeshGenerationRequired)
                return;


        }

        private static readonly Vertex[] VERTICES = new Vertex[4];
        private static readonly ushort[] INDICES = { 0, 1, 2, 2, 3, 0 };

        private void GenerateStretched(MeshGenerationContext mgc)
        {
            // in local coords
            var r = contentRect;

            var panelScale = worldTransform.lossyScale;
            float targetTextureWidth = texture2D.width * panelScale[0];
            float targetTextureHeight = texture2D.height * panelScale[1];

            float left = 0;
            float right = r.width;
            float top = 0;
            float bottom = r.height;

            VERTICES[0].position = new Vector3(left, bottom, Vertex.nearZ);
            VERTICES[1].position = new Vector3(left, top, Vertex.nearZ);
            VERTICES[2].position = new Vector3(right, top, Vertex.nearZ);
            VERTICES[3].position = new Vector3(right, bottom, Vertex.nearZ);

            var mwd = mgc.Allocate(VERTICES.Length, INDICES.Length, texture2D);

            // uv Rect [0;1] that was assigned by the Dynamic atlas by UI Toolkit
            var uvRegion = mwd.uvRegion;

            VERTICES[0].uv = new Vector2(0, 0) * uvRegion.size + uvRegion.min;
            VERTICES[1].uv = new Vector2(0, 1) * uvRegion.size + uvRegion.min;
            VERTICES[2].uv = new Vector2(1, 1) * uvRegion.size + uvRegion.min;
            VERTICES[3].uv = new Vector2(1, 0) * uvRegion.size + uvRegion.min;

            mwd.SetAllVertices(VERTICES);
            mwd.SetAllIndices(INDICES);
        }

        private void GenerateCenteredTexture(MeshGenerationContext mgc)
        {
            // in local coords
            var r = contentRect;

            var panelScale = worldTransform.lossyScale;
            float targetTextureWidth = texture2D.width * panelScale[0];
            float targetTextureHeight = texture2D.height * panelScale[1];

            // Remain the original center
            var center = r.center;

            float width = Mathf.Min(r.width, targetTextureWidth);
            float height = Mathf.Min(r.height, targetTextureHeight);

            float left = center.x - (width / 2f);
            float right = center.x + (width / 2f);
            float top = center.y - (height / 2f);
            float bottom = center.y + (height / 2f);

            VERTICES[0].position = new Vector3(left, bottom, Vertex.nearZ);
            VERTICES[1].position = new Vector3(left, top, Vertex.nearZ);
            VERTICES[2].position = new Vector3(right, top, Vertex.nearZ);
            VERTICES[3].position = new Vector3(right, bottom, Vertex.nearZ);

            var mwd = mgc.Allocate(VERTICES.Length, INDICES.Length, texture2D);

            // uv Rect [0;1] that was assigned by the Dynamic atlas by UI Toolkit
            var uvRegion = mwd.uvRegion;

            VERTICES[0].uv = uvs.BottomLeft * uvRegion.size + uvRegion.min;
            VERTICES[1].uv = uvs.TopLeft * uvRegion.size + uvRegion.min;
            VERTICES[2].uv = uvs.TopRight * uvRegion.size + uvRegion.min;
            VERTICES[3].uv = uvs.BottomRight * uvRegion.size + uvRegion.min;

            mwd.SetAllVertices(VERTICES);
            mwd.SetAllIndices(INDICES);
        }
    }
}
