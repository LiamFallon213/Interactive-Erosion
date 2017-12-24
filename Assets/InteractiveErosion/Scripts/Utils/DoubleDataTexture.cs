using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InterativeErosionProject
{
    /// <summary>
    /// Encapsulates operations with 1 data matrix (has 2 textures for write-read operations)
    /// </summary>
    /// 
    [System.Serializable]
    public class DoubleDataTexture
    {
        static private readonly List<DoubleDataTexture> all = new List<DoubleDataTexture>();
        static private readonly RenderTexture tempRTARGB, tempRTRFloat;
        static private Texture2D tempT2DRGBA, tempT2DRFloat;
        static private readonly Materials material;
        ///<summary> Contains data</summary>
        [SerializeField]//readonly
        private RenderTexture[] textures = new RenderTexture[2];

        private readonly int size;
        public RenderTexture READ
        {
            get { return textures[0]; }
        }
        public RenderTexture WRITE
        {
            get { return textures[1]; }
        }
        static DoubleDataTexture()
        {
            tempRTARGB = DoubleDataTexture.Create("tempRTARGB", 1, RenderTextureFormat.ARGBFloat, FilterMode.Point);

            tempT2DRGBA = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            tempT2DRGBA.wrapMode = TextureWrapMode.Clamp;
            tempT2DRGBA.filterMode = FilterMode.Point;

            tempRTRFloat = DoubleDataTexture.Create("tempRTRFloat", 1, RenderTextureFormat.RFloat, FilterMode.Point);

            tempT2DRFloat = new Texture2D(1, 1, TextureFormat.RFloat, false);
            tempT2DRFloat.wrapMode = TextureWrapMode.Clamp;
            tempT2DRFloat.filterMode = FilterMode.Point;
            //LoadMaterials();
            material =  GameObject.Find("Materials").GetComponent<Materials>();
        }

        public DoubleDataTexture(string name, int size, RenderTextureFormat format, FilterMode filterMode)
        {

            for (int i = 0; i < 2; i++)
            {
                textures[i] = new RenderTexture(size, size, 0, format);
                textures[i].wrapMode = TextureWrapMode.Clamp;
                textures[i].filterMode = filterMode;
                textures[i].name = name + " " + i;
                textures[i].enableRandomWrite = true;
            }
            this.size = size;
            all.Add(this);
        }
        //static private void LoadMaterials()
        //{
        //    string path = "Materials/UniversalCS/";
        //    setFloatValueMat = Resources.Load(path + "SetFloatValue", typeof(Material)) as Material;
        //    changeValueMat = Resources.Load(path + "ChangeValue", typeof(Material)) as Material;
        //    changeValueZeroControlMat = Resources.Load(path + "ChangeValueZeroControl", typeof(Material)) as Material;
        //    getValueMat = Resources.Load(path + "GetValue", typeof(Material)) as Material;
        //    changeValueGaussMat = Resources.Load(path + "ChangeValueGauss", typeof(Material)) as Material;
        //    changeValueGaussZeroControlMat = Resources.Load(path + "ChangeValueGaussZeroControl", typeof(Material)) as Material;
        //    setRandomValueMat = Resources.Load(path + "SetRandomValue", typeof(Material)) as Material;
        //    moveByVelocityMat = Resources.Load(path + "MoveByVelocity", typeof(Material)) as Material;
        //    scaleMat = Resources.Load(path + "Scale", typeof(Material)) as Material;
        //    changeValueGaussWithHeatMat = Resources.Load(path + "ChangeValueGaussWithHeat", typeof(Material)) as Material;
        //}
        public static void DestroyAll()
        {
            foreach (var item in all)
            {
                item.Destroy();
            }
            all.Clear();
        }
        //public static DataTexture CreateDouble(string name, int size, RenderTextureFormat format, FilterMode filterMode)
        //{
        //    //RenderTexture[] res = new RenderTexture[2];
        //    //for (int i = 0; i < 2; i++)
        //    //{
        //    //    res[i] = new RenderTexture(size, size, 0, format);
        //    //    res[i].wrapMode = TextureWrapMode.Clamp;
        //    //    res[i].filterMode = filterMode;
        //    //    res[i].name = name + " " + i;
        //    //}
        //    //return res;
        //    return new DataTexture( name,  size,  format,  filterMode);
        //}
        public static RenderTexture Create(string name, int size, RenderTextureFormat format, FilterMode filterMode)
        {
            var res = new RenderTexture(size, size, 0, format);
            res.wrapMode = TextureWrapMode.Clamp;
            res.filterMode = filterMode;
            res.name = name;
            res.Create();

            return res;
        }
        public void Destroy()
        {
            UnityEngine.Object.Destroy(textures[0]);
            UnityEngine.Object.Destroy(textures[1]);
        }
        public int getMaxIndex()
        {
            return size - 1;
        }
        public Vector4 getDataRGBAFloatEF(Vector2 point)
        {
            Graphics.CopyTexture(this.READ, 0, 0, (int)(point.x * getMaxIndex()), (int)(point.y * getMaxIndex()), 1, 1, tempRTARGB, 0, 0, 0, 0);
            tempT2DRGBA = GetRTPixels(tempRTARGB, tempT2DRGBA);
            var res = tempT2DRGBA.GetPixel(0, 0);
            return res;
        }
        public Vector4 getDataRFloatEF(Vector2 point)
        {
            Graphics.CopyTexture(this.READ, 0, 0, (int)(point.x * getMaxIndex()), (int)(point.y * getMaxIndex()), 1, 1, tempRTRFloat, 0, 0, 0, 0);
            //var del = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBHalf);
            //del.wrapMode = TextureWrapMode.Clamp;
            //del.filterMode = FilterMode.Point;
            //del.Create();
            //Graphics.ConvertTexture(tempRTRFloat, del);

            tempT2DRGBA = GetRTPixels(tempRTRFloat, tempT2DRFloat);
            var res = tempT2DRFloat.GetPixel(0, 0);
            //tempT2DRFloat = GetRTPixels(tempRTRFloat, tempT2DRFloat);
            //var res = tempT2DRFloat.GetPixel(0, 0);
            return res;
        }


        /// Not supported by unity
        //public float getDataRGHalf(RenderTexture source, Point point)
        //{
        //    bufferRGHalfTexture = GetRTPixels(source, bufferRGHalfTexture);
        //    var res = bufferRGHalfTexture.GetPixel(point.x, point.y);
        //    return res.a;
        //}
        // Get the contents of a RenderTexture into a Texture2D
        static public Texture2D GetRTPixels(RenderTexture source, Texture2D destination)
        {
            // Remember currently active render texture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = source;

            // Create a new Texture2D and read the RenderTexture image into it
            //Texture2D tex = new Texture2D(source.width, source.height);
            destination.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);

            // Restore previously active render texture
            RenderTexture.active = currentActiveRT;
            return destination;
        }
        public void SetValue(Vector4 value, Rect rect)
        {
            Graphics.Blit(this.READ, this.WRITE);
            material.setFloatValueMat.SetVector("_Value", value);
            RTUtility.Blit(this.READ, this.WRITE, material.setFloatValueMat, rect, 0, false);
            this.Swap();
        }
        public void ChangeValueGauss(Vector2 point, float radius, Vector4 value)
        {
            if (value != Vector4.zero)
            {
                material.changeValueGaussMat.SetVector("_Point", point);
                material.changeValueGaussMat.SetFloat("_Radius", radius);
                material.changeValueGaussMat.SetVector("_Value", value);

                Graphics.Blit(this.READ, this.WRITE, material.changeValueGaussMat);
                this.Swap();
            }
        }
        public void ChangeValueGaussWithHeat(Vector2 point, float radius, Vector4 value)
        {
            if (value != Vector4.zero)
            {
                material.changeValueGaussWithHeatMat.SetVector("_Point", point);
                material.changeValueGaussWithHeatMat.SetFloat("_Radius", radius);
                material.changeValueGaussWithHeatMat.SetVector("_Value", value);

                Graphics.Blit(this.READ, this.WRITE, material.changeValueGaussWithHeatMat);
                this.Swap();
            }
        }
        public void ChangeValueGaussZeroControl(Vector2 point, float radius,  Vector4 value)
        {
            if (value != Vector4.zero)
            {
                material.changeValueGaussZeroControlMat.SetVector("_Point", point);
                material.changeValueGaussZeroControlMat.SetFloat("_Radius", radius);
                material.changeValueGaussZeroControlMat.SetVector("_Value", value);

                Graphics.Blit(this.READ, this.WRITE, material.changeValueGaussZeroControlMat);
                this.Swap();
            }
        }
        public void ChangeValue(Vector4 value, Rect rect)
        {
            Graphics.Blit(this.READ, this.WRITE); // don't know why but need it
            material.changeValueMat.SetVector("_Value", value);
            RTUtility.Blit(this.READ, this.WRITE, material.changeValueMat, rect, 0, false);
            this.Swap();
        }
        public void ChangeValueZeroControl(float value)
        {
            //Graphics.Blit(this.READ, this.WRITE); // don't know why but need it
            material.changeValueZeroControlMat.SetFloat("_Value", value);
            Graphics.Blit(this.READ, this.WRITE, material.changeValueZeroControlMat);
            this.Swap();
        }

        internal void Swap()
        {
            RenderTexture temp = textures[0];
            textures[0] = textures[1];
            textures[1] = temp;
        }

        internal void ClearColor()
        {
            //if (texture == null) return;
            //if (!SystemInfo.SupportsRenderTextureFormat(texture.format)) return;

            //Graphics.SetRenderTarget(texture);
            //GL.Clear(false, true, Color.clear);
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null) continue;
                if (!SystemInfo.SupportsRenderTextureFormat(textures[i].format)) continue;

                Graphics.SetRenderTarget(textures[i]);
                GL.Clear(false, true, Color.clear);
            }
        }
        public void SetFilterMode(FilterMode mode)
        {
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i].filterMode = mode;
            }
        }

        internal void SetRandomValue(Vector4 limits, int chance)
        {
            material.setRandomValueMat.SetVector("_Limits", limits);
            material.setRandomValueMat.SetInt("_Chance", chance);
            Graphics.Blit(this.READ, this.WRITE, material.setRandomValueMat);
            this.Swap();
        }

        internal void Set(RenderTexture tex)
        {
            textures[0] = tex;
        }

        internal void MoveByVelocity(RenderTexture velocity, float T, float coefficient, float limit, ComputeShader shader)
        {
            // how it should be:
            // move value by 1 cell, not every but at random tick
            // movement rate is proportional to speed of plate
            //int kernelHandle = shader.FindKernel("CSMain");            

            //shader.SetTexture(kernelHandle, "_MainTex", this.READ);
            //shader.SetTexture(kernelHandle, "_Velocity", velocity);

            //shader.SetFloat("T", T);
            //shader.SetFloat("_Coefficient", coefficient);
            //shader.SetFloat("_TexSize", (float)size);
            //shader.SetFloat("_Limit", limit);
            //shader.Dispatch(kernelHandle, this.size / 8, this.size / 8, 1);

            ///this.Swap();
            material.moveByVelocityMat.SetFloat("T", T);
            material.moveByVelocityMat.SetFloat("_Coefficient", coefficient);
            material.moveByVelocityMat.SetFloat("_TexSize", (float)size);
            material.moveByVelocityMat.SetFloat("_Limit", limit);
            material.moveByVelocityMat.SetTexture("_Velocity", velocity);
            Graphics.Blit(this.READ, this.WRITE, material.moveByVelocityMat);
            this.Swap();
        }

        internal void Scale(float value)
        {
            material.scaleMat.SetFloat("_Value", value);
            Graphics.Blit(this.READ, this.WRITE, material.scaleMat);
            this.Swap();
        }
    }
}
