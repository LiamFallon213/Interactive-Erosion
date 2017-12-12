using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InterativeErosionProject
{
    /// <summary>
    /// Represents 1 layer
    /// </summary>
    [System.Serializable]
    public class Layer
    {
        ///<summary>r - height; g, b - velocity; a - temperature</summary>        
        [SerializeField]//readonly
        public DoubleDataTexture main;
        [SerializeField]//readonly
        public DoubleDataTexture outFlow;
        
        private float viscosity;
        private readonly ErosionSim link;
        public Layer(string name, int size, float viscosity, ErosionSim link)
        {
            main = new DoubleDataTexture(name, size, RenderTextureFormat.ARGBFloat, FilterMode.Point);
            outFlow = new DoubleDataTexture(name, size, RenderTextureFormat.ARGBHalf, FilterMode.Point);
            this.viscosity = viscosity;
            this.link = link;
        }
        /// <summary>
        ///  Calculates flow of field 
        /// </summary>
        public void Flow(RenderTexture onWhat)
        {
            main.SetFilterMode(FilterMode.Point);
            link.m_outFlowMat.SetFloat("_TexSize", (float)ErosionSim.TEX_SIZE);
            link.m_outFlowMat.SetFloat("T", 0.1f);
            link.m_outFlowMat.SetFloat("L", 1.0f);
            link.m_outFlowMat.SetFloat("A", 1.0f);
            link.m_outFlowMat.SetFloat("G", ErosionSim.GRAVITY);
            link.m_outFlowMat.SetFloat("_Layers", 4);
            link.m_outFlowMat.SetFloat("_Damping", 1.0f - viscosity);
            link.m_outFlowMat.SetTexture("_TerrainField", onWhat);
            link.m_outFlowMat.SetTexture("_Field", main.READ);

            Graphics.Blit(outFlow.READ, outFlow.WRITE, link.m_outFlowMat);

            outFlow.Swap(); ;

            link.m_fieldUpdateMat.SetFloat("_TexSize", (float)ErosionSim.TEX_SIZE);
            link.m_fieldUpdateMat.SetFloat("T", 0.1f);
            link.m_fieldUpdateMat.SetFloat("L", 1f);
            link.m_fieldUpdateMat.SetTexture("_OutFlowField", outFlow.READ);

            Graphics.Blit(main.READ, main.WRITE, link.m_fieldUpdateMat);
            main.Swap();
            main.SetFilterMode(FilterMode.Bilinear);
        }

    }
}
