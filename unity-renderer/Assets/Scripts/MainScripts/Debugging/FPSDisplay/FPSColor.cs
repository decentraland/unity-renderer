using UnityEngine;

namespace DCL.FPSDisplay
{
    [System.Serializable]
    public struct FPSColor
    {
        public Color color;
        public int fps;

        public FPSColor(Color col, int fps) : this()
        {
            this.color = col;
            this.fps = fps;
        }
    }
}