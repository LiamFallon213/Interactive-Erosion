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
        ///<summary>r - height; g, b - velocity; a - temperature. Can't be negative!!</summary>           
        [SerializeField]//readonly
        public DoubleDataTexture main;

        [SerializeField]//readonly
        public DoubleDataTexture outFlow;

        [SerializeField]
        private float damping;
        protected readonly ErosionSim link;
        protected float size;

        public Layer(string name, int size, float damping, ErosionSim link)
        {
            main = new DoubleDataTexture(name, size, RenderTextureFormat.ARGBFloat, FilterMode.Point); // was RFloat
            main.ClearColor();
            outFlow = new DoubleDataTexture(name, size, RenderTextureFormat.ARGBFloat, FilterMode.Point); //was ARGBHalf
            outFlow.ClearColor();
            this.damping = damping;
            this.link = link;
            this.size = size;
        }
        /// <summary>
        ///  Calculates flow of field 
        /// </summary>
        public void Flow(RenderTexture onWhat)
        {
            //main.SetFilterMode(FilterMode.Point);
            link.m_outFlowMat.SetFloat("_TexSize", (float)ErosionSim.TEX_SIZE);
            link.m_outFlowMat.SetFloat("T", link.timeStep);
            link.m_outFlowMat.SetFloat("L",link.PIPE_LENGTH);
            link.m_outFlowMat.SetFloat("A", link.CELL_AREA);
            link.m_outFlowMat.SetFloat("G", ErosionSim.GRAVITY);
            link.m_outFlowMat.SetFloat("_Layers", 4);
            link.m_outFlowMat.SetFloat("_Damping", damping);
            link.m_outFlowMat.SetTexture("_TerrainField", onWhat);
            link.m_outFlowMat.SetTexture("_Field", main.READ);

            Graphics.Blit(outFlow.READ, outFlow.WRITE, link.m_outFlowMat);

            outFlow.Swap(); ;

            link.m_fieldUpdateMat.SetFloat("_TexSize", size);
            link.m_fieldUpdateMat.SetFloat("T", link.timeStep);
            link.m_fieldUpdateMat.SetFloat("L", 1f);
            link.m_fieldUpdateMat.SetTexture("_OutFlowField", outFlow.READ);

            Graphics.Blit(main.READ, main.WRITE, link.m_fieldUpdateMat);
            main.Swap();
            //main.SetFilterMode(FilterMode.Bilinear);
        }
        virtual public void OnDestroy()
        {
            main.Destroy();
            outFlow.Destroy();
        }
        public void SetFilterMode(FilterMode mode)
        {
            main.SetFilterMode(mode);
        }

        
    }
    [System.Serializable]
    public class LayerWithTemperature : Layer
    {
        ///<summary>Must be in 0..1 range</summary>
        private readonly float emissivity;
        ///<summary> Joule per kelvin, J/K</summary>
        private readonly float heatCapacity;
        private static readonly float StefanBoltzmannConstant = 5.670367e-8f;

        public LayerWithTemperature(string name, int size, float damping, ErosionSim link, float emissivity, float heatCapacity) : base(name, size, damping, link)
        {
            this.emissivity = Mathf.Clamp01(emissivity);
            this.heatCapacity = heatCapacity;
        }
        internal void HeatExchange()
        {
            link.heatExchangeMat.SetFloat("_StefanBoltzmannConstant", StefanBoltzmannConstant);
            link.heatExchangeMat.SetFloat("_Emissivity", emissivity);
            link.heatExchangeMat.SetFloat("_HeatCapacity", heatCapacity);
            link.heatExchangeMat.SetFloat("T", link.timeStep);
            //link.heatExchangeMat.SetTexture("_OutFlowField", outFlow.READ);

            Graphics.Blit(main.READ, main.WRITE, link.heatExchangeMat);
            main.Swap();
        }
    }
    [System.Serializable]
    public class LayerWithVelocity : LayerWithTemperature
    {
        ///<summary> Water speed (2 channels). Used for sediment movement and dissolution</summary>
        [SerializeField]
        public DoubleDataTexture velocity;
        public LayerWithVelocity(string name, int size, float viscosity, ErosionSim link) : base(name, size, viscosity, link, 0.96f, 4181f)
        {
            velocity = new DoubleDataTexture("Water Velocity", size, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);// was RGHalf
            velocity.ClearColor();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            velocity.Destroy();
        }
        /// <summary>
        ///  Calculates water velocity
        /// </summary>
        public void CalcWaterVelocity(float TIME_STEP)
        {
            link.m_waterVelocityMat.SetFloat("_TexSize", size);
            link.m_waterVelocityMat.SetFloat("L", 1f);
            link.m_waterVelocityMat.SetTexture("_WaterField", main.READ);
            link.m_waterVelocityMat.SetTexture("_WaterFieldOld", main.WRITE);
            link.m_waterVelocityMat.SetTexture("_OutFlowField", outFlow.READ);

            Graphics.Blit(null, velocity.READ, link.m_waterVelocityMat);

            const float viscosity = 10.5f;
            const int iterations = 2;

            link.m_diffuseVelocityMat.SetFloat("_TexSize", size);
            link.m_diffuseVelocityMat.SetFloat("_Alpha", 1f / (viscosity * TIME_STEP));// CELL_AREA == 1f

            for (int i = 0; i < iterations; i++)
            {
                Graphics.Blit(velocity.READ, velocity.WRITE, link.m_diffuseVelocityMat);
                velocity.Swap();
            }
        }
    }
    [System.Serializable]
    public class LayerWithErosion : LayerWithVelocity
    {
        [SerializeField]
        ///<summary></summary>        
        private DoubleDataTexture advectSediment;

        [SerializeField]
        ///<summary> Actual amount of dissolved sediment in water</summary>
        public DoubleDataTexture sedimentField;

        [SerializeField]
        ///<summary> Actual amount of dissolved sediment in water</summary>
        public DoubleDataTexture sedimentDeposition;

        ///<summary> Contains surface angels for each point. Used in water erosion only (Why?)</summary>
        [SerializeField]
        private RenderTexture tiltAngle;

        //[SerializeField]
        //private RenderTexture sedimentOutFlow;

        /// <summary> Rate the sediment is deposited on top layer </summary>
        [SerializeField]
        private float depositionConstant = 0.015f;

        /// <summary> Terrain wouldn't dissolve if water level in cell is lower than this</summary>
        [SerializeField]        
        private float dissolveLimit = 0.001f;

        /// <summary> How much sediment the water can carry per 1 unit of water </summary>
        [SerializeField]
        private float sedimentCapacity = 0.2f;

        public LayerWithErosion(string name, int size, float viscosity, ErosionSim link) : base(name, size, viscosity, link)
        {
            //waterField = new DoubleDataTexture("Water Field", TEX_SIZE, RenderTextureFormat.RFloat, FilterMode.Point);
            //waterOutFlow = new DoubleDataTexture("Water outflow", TEX_SIZE, RenderTextureFormat.ARGBHalf, FilterMode.Point);


            sedimentField = new DoubleDataTexture("Sediment Field", size, RenderTextureFormat.ARGBFloat, FilterMode.Bilinear);// was RHalf
            sedimentField.ClearColor();
            advectSediment = new DoubleDataTexture("Sediment Advection", size, RenderTextureFormat.RHalf, FilterMode.Bilinear);// was RHalf
            advectSediment.ClearColor();
            sedimentDeposition = new DoubleDataTexture("Sediment Deposition", size, RenderTextureFormat.ARGBFloat, FilterMode.Point);// was RHalf
            sedimentDeposition.ClearColor();

            tiltAngle = DoubleDataTexture.Create("Tilt Angle", size, RenderTextureFormat.RHalf, FilterMode.Point);// was RHalf

            //sedimentOutFlow = DoubleDataTexture.Create("sedimentOutFlow", size, RenderTextureFormat.ARGBHalf, FilterMode.Point);// was ARGBHalf
            //sedimentOutFlow.ClearColor();
        }
        /// <summary>
        ///  Calculates how much ground should go in sediment flow aka force-based erosion
        ///  Transfers m_terrainField to m_sedimentField basing on
        ///  m_waterVelocity, m_sedimentCapacity, m_dissolvingConstant,
        ///  m_depositionConstant, m_tiltAngle, m_minTiltAngle
        /// Also calculates m_tiltAngle
        /// </summary>
        private void DissolveAndDeposition(DoubleDataTexture terrainField, Vector4 dissolvingConstant, float minTiltAngle, int TERRAIN_LAYERS)
        {
            link.m_tiltAngleMat.SetFloat("_TexSize", size);
            link.m_tiltAngleMat.SetFloat("_Layers", TERRAIN_LAYERS);
            link.m_tiltAngleMat.SetTexture("_TerrainField", terrainField.READ);

            Graphics.Blit(null, tiltAngle, link.m_tiltAngleMat);

            link.dissolutionAndDepositionMat.SetTexture("_TerrainField", terrainField.READ);
            link.dissolutionAndDepositionMat.SetTexture("_SedimentField", sedimentField.READ);
            link.dissolutionAndDepositionMat.SetTexture("_VelocityField", velocity.READ);
            link.dissolutionAndDepositionMat.SetTexture("_WaterField", main.READ);
            link.dissolutionAndDepositionMat.SetTexture("_TiltAngle", tiltAngle);
            link.dissolutionAndDepositionMat.SetFloat("_MinTiltAngle", minTiltAngle);
            link.dissolutionAndDepositionMat.SetFloat("_SedimentCapacity", sedimentCapacity);
            link.dissolutionAndDepositionMat.SetVector("_DissolvingConstant", dissolvingConstant);
            link.dissolutionAndDepositionMat.SetFloat("_DepositionConstant", depositionConstant);
            link.dissolutionAndDepositionMat.SetFloat("_Layers", (float)TERRAIN_LAYERS);
            link.dissolutionAndDepositionMat.SetFloat("_DissolveLimit", dissolveLimit); //nash added it            

            RenderTexture[] terrainAndSediment = new RenderTexture[3] { terrainField.WRITE, sedimentField.WRITE, sedimentDeposition.WRITE };

            RTUtility.MultiTargetBlit(terrainAndSediment, link.dissolutionAndDepositionMat);
            terrainField.Swap();
            sedimentField.Swap();
            sedimentDeposition.Swap();
        }
        ///// <summary>
        /////  Moves sediment 
        ///// </summary>
        //private void AlternativeAdvectSediment()
        //{
        //    moveByLiquidMat.SetFloat("T", TIME_STEP);
        //    moveByLiquidMat.SetTexture("_OutFlow", outFlow.READ);
        //    moveByLiquidMat.SetTexture("_LuquidLevel", main.READ);

        //    Graphics.Blit(sedimentField.READ, sedimentOutFlow, moveByLiquidMat);

        //    m_fieldUpdateMat.SetFloat("_TexSize", (float)TEX_SIZE);
        //    m_fieldUpdateMat.SetFloat("T", TIME_STEP);
        //    m_fieldUpdateMat.SetFloat("L", PIPE_LENGTH);
        //    m_fieldUpdateMat.SetTexture("_OutFlowField", sedimentOutFlow);

        //    Graphics.Blit(sedimentField.READ, sedimentField.WRITE, m_fieldUpdateMat);
        //    sedimentField.Swap();

        //}
        /// <summary>
        ///  Moves sediment 
        /// </summary>
        private void AdvectSediment(float TIME_STEP)
        {
            link.m_advectSedimentMat.SetFloat("_TexSize", size);
            link.m_advectSedimentMat.SetFloat("T", TIME_STEP);
            link.m_advectSedimentMat.SetFloat("_VelocityFactor", 1.0f);
            link.m_advectSedimentMat.SetTexture("_VelocityField", velocity.READ);

            //is bug? No its no
            Graphics.Blit(sedimentField.READ, advectSediment.READ, link.m_advectSedimentMat);

            link.m_advectSedimentMat.SetFloat("_VelocityFactor", -1.0f);
            Graphics.Blit(advectSediment.READ, advectSediment.WRITE, link.m_advectSedimentMat);

            link.m_processMacCormackMat.SetFloat("_TexSize", size);
            link.m_processMacCormackMat.SetFloat("T", TIME_STEP);
            link.m_processMacCormackMat.SetTexture("_VelocityField", velocity.READ);
            link.m_processMacCormackMat.SetTexture("_InterField1", advectSediment.READ);
            link.m_processMacCormackMat.SetTexture("_InterField2", advectSediment.WRITE);

            Graphics.Blit(sedimentField.READ, sedimentField.WRITE, link.m_processMacCormackMat);
            sedimentField.Swap();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            advectSediment.Destroy();
            sedimentField.Destroy();
            sedimentDeposition.Destroy();
            //sedimentOutFlow.Destroy();
            GameObject.Destroy(tiltAngle);
        }

        internal void SimulateErosion(DoubleDataTexture terrainField, Vector4 dissolvingConstant, float minTiltAngle, int TERRAIN_LAYERS, float TIME_STEP)
        {
            DissolveAndDeposition(terrainField, dissolvingConstant, minTiltAngle, TERRAIN_LAYERS);
            AdvectSediment( TIME_STEP);
            //AlternativeAdvectSediment();
        }
        public void SetSedimentDepositionRate(float value)
        {
            depositionConstant = value;
        }
        public void SetSedimentCapacity(float value)
        {
            sedimentCapacity = value;
        }
    }
}
