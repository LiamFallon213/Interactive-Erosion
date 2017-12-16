using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.InteractiveErosion.Scripts.Utils
{
    public class Point
    {
        public int x, y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return "x = " + x + "; y = " + y;
        }

        internal Vector2 getVector2(int tetureSize)
        {
            return new Vector2(x / (float)tetureSize, y / (float)tetureSize);
        }
    }
}
