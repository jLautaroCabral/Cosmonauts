using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatMarchingSquares
{
    public class VoxelStencil
    {
        protected bool fillType;
        protected int centerX, centerY, radius;

        public int XStart { get { return centerX - radius; } }
        public int XEnd { get { return centerX + radius; } }
        public int YStart { get { return centerY - radius; } }
        public int YEnd { get { return centerY + radius; } }

        public virtual void Initialize(bool fillType, int radius)
        {
            this.fillType = fillType;
            this.radius = radius;
        }
        public virtual bool Apply(int x, int y, bool voxel)
        {
            return fillType;
        }

        public void SetCenter(int x, int y)
        {
            centerX = x;
            centerY = y;
        }
    }
}