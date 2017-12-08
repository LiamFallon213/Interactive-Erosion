using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InterativeErosionProject
{
    //public struct Ocean
    //{
    //    public float waterLevel, terrainLevel;        
    //    public Ocean(float waterLevel, float terrainLevel)
    //    {
    //        this.waterLevel = waterLevel;
    //        this.terrainLevel = terrainLevel;            
    //    }
    //}
    public enum WorldSides
    {
        None = 0, West = 1, North = 2, East = 4, South = 8
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
    public class Action
    {
        private readonly string name;
        static private readonly List<Action> list = new List<Action>();
        static public Action Nothing = new Action("Nothing");
        static public Action Add = new Action("Add");
        static public Action Remove = new Action("Remove");
        static public Action Info = new Action("Info");
        private Action(string name)
        {
            this.name = name;
            list.Add(this);
        }
        static public IEnumerable<Action> getAllPossible()
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }
        public override string ToString()
        {
            return name;
        }

        internal static Action getById(int value)
        {
            return list[value];
        }
    }
    public class Overlay
    {
        static private readonly List<Overlay> list = new List<Overlay>();
        //order is important!
        static public readonly Overlay Default = new Overlay("Default", "LandShader", false);
        static public readonly Overlay Deposition = new Overlay("Deposition", "DepositOverlay", false);
        static public readonly Overlay WaterVelocity = new Overlay("Water velocity", null, true);
        static public readonly Overlay Plates = new Overlay("Plates", "Plates", false);
        static public readonly Overlay PlatesVelocity = new Overlay("Plates velocity", null, true);
        private readonly string name;
        private Material material;
        private readonly string materialPath;
        private readonly int ID;
        private readonly bool _isArrow;


        private Overlay(string name, string materialPath, bool _isArrow)
        {
            this.name = name;
            this._isArrow = _isArrow;
            this.materialPath = materialPath;
            ID = list.Count;

            list.Add(this);
        }
        static public IEnumerable<Overlay> getAllPossible()
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }
        public bool isArrow()
        {
            return _isArrow;
        }
        public Material getMaterial()
        {
            if (!isArrow() && material == null)
                material = Resources.Load("Materials/Renders/" + materialPath, typeof(Material)) as Material;
            return material;
        }
        public override string ToString()
        {
            return name;
        }

        internal static Overlay getById(int value)
        {
            return list[value];
        }
        public int getID()
        {
            return ID;
        }

    }
    public enum MaterialsForEditing
    {
        stone,
        cobble, clay, sand,
        water, watersource, waterdrain, sediment
        , ocean
    }
}

