using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InterativeErosionProject
{
    
    public class Overlay
    {
        static private readonly List<Overlay> list = new List<Overlay>();
        
        static public readonly Overlay Default = new Overlay("Default", "LandShader", false);
        static public readonly Overlay Deposition = new Overlay("Deposition", "DepositionOverlay", false);
        static public readonly Overlay Dissolution = new Overlay("Dissolution", "DissolutionOverlay", false);
        static public readonly Overlay WaterVelocity = new Overlay("Water velocity", null, true);
        static public readonly Overlay Plates = new Overlay("Plates", "Plates", false);
        static public readonly Overlay PlatesVelocity = new Overlay("Plates velocity", null, true);
        static public readonly Overlay Rain = new Overlay("Rain", "RainOverlay", false);
        static public readonly Overlay AtmosphereVelocity = new Overlay("Atmosphere velocity", null, true);
        static public readonly Overlay AtmosphereTemperature= new Overlay("Atmosphere temperature", "AtmosphereTemperatureOverlay", false);
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
}

