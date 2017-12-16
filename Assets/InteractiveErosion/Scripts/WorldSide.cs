using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InterativeErosionProject
{
    [System.Serializable]
    public class WorldSide
    {
        static private readonly float u = 1f / (float)ErosionSim.TEX_SIZE;
        [SerializeField]
        static public WorldSide EntireMap = new WorldSide("EntireMap", new Rect(0f, 0f, 1f, 1f)),

            North = new WorldSide("North", new Rect(0.0f, 1f - u, 1.0f, u)),
            South = new WorldSide("South", new Rect(0.0f, 0.0f, 1.0f, u)),
            West = new WorldSide("West", new Rect(0.0f, 0.0f, u, 1.0f)),
            East = new WorldSide("East", new Rect(1.0f - u, 0f, u, 1f));
        private readonly Rect side;
        private readonly string name;        
        private WorldSide(string name, Rect rect)
        {
            this.name = name;
            this.side = rect;
        }
        /// <summary>
        /// get rect-part of world texture according to world side
        /// </summary>
        public Rect getPartOfMap(int width)
        {
            float offest = width / (float)ErosionSim.TEX_SIZE;
            Rect rect = this.side;
            if (this == WorldSide.North)
            {                
                rect.height += offest;// *-1f;                
                rect.y -= offest;
            }
            else if (this == WorldSide.South)
            {             
                rect.height += offest;
            }
            else if (this == WorldSide.East)
            {             
                rect.x -= offest;
                rect.width += offest;
            }
            else if (this == WorldSide.West)
            {             
                rect.width += offest;// * -1f;
            }
            return rect;
        }
        /// <summary>
        /// returns which side of map is closer to point - north, south, etc
        /// </summary>
        public static WorldSide getSideOfWorld(Vector2 point)
        {
            // find to which border it's closer 
            WorldSide side = null;
            float distToWest = Math.Abs(0f - point.x);
            float distToEast = Math.Abs(1f - point.x);
            float distToSouth = Math.Abs(0f - point.y);
            float distToNorth = Math.Abs(1f - point.y);

            if (distToEast == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSide.East;
            else if (distToWest == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSide.West;
            else if (distToSouth == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSide.South;
            else if (distToNorth == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSide.North;

            return side;
        }
        internal Rect getArea()
        {
            return side;
        }
    }
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
