// good browni 51381BFF
// good red 552710FF

using UnityEngine;
using System.Collections;

using ImprovedPerlinNoiseProject;
using System;
using System.Collections.Generic;

namespace InterativeErosionProject
{
    public enum NOISE_STYLE { FRACTAL = 0, TURBULENCE = 1, RIDGE_MULTI_FRACTAL = 2, WARPED = 3 };

    public class ErosionSim : MonoBehaviour
    {
        public ComputeShader shader;
        public GameObject sun;
        ///<summary> Used for rendering</summary>
        public Material m_landMat, m_waterMat, arrowsMat, lavaMat;
        ///<summary> Used for rendering</summary>
        //public Material[] overlays;

        public Material m_initTerrainMat, m_noiseMat;
        public Material m_outFlowMat;
        ///<summary> Updates field according to outflow</summary>
        public Material m_fieldUpdateMat;
        public Material m_waterVelocityMat, m_diffuseVelocityMat;

        /// <summary> Calculates angle for each cell </summary>
        public Material m_tiltAngleMat;
        ///<summary> Calculates layer erosion basing on the forces that are caused by the running water</summary>
        public Material m_processMacCormackMat;
        public Material m_erosionAndDepositionMat;
        ///<summary> Creates new texture based on smoothed sediment data and size of texture(?)</summary>
        public Material m_advectSedimentMat;
        public Material m_slippageHeightMat, m_slippageOutflowMat, m_slippageUpdateMat;
        public Material m_disintegrateAndDepositMat, m_applyFreeSlipMat;
        public Material moveByLiquidMat;



        /// <summary> Movement speed of point of water source</summary>
        //private float m_waterInputSpeed = 0.01f;
        private Vector2 waterInputPoint = new Vector2(-1f, 1f);
        private float waterInputAmount = 0f;
        private float waterInputRadius = 0.008f;

        private Vector2 waterDrainagePoint = new Vector2(-1f, 1f);
        private float waterDrainageAmount = 0f;
        private float waterDrainageRadius = 0.008f;

        public int m_seed = 0;

        //The number of layers used in the simulation. Must be 1, 2, 3 or, 4
        private const int TERRAIN_LAYERS = 4;

        //This will allow you to set a noise style for each terrain layer
        private NOISE_STYLE[] m_layerStyle = new NOISE_STYLE[]
        {
            NOISE_STYLE.FRACTAL,
            NOISE_STYLE.FRACTAL,
            NOISE_STYLE.FRACTAL,
            NOISE_STYLE.FRACTAL
        };
        //This will take the abs value of the final noise is set to true
        //This will make the fractal or warped noise look different.
        //It will have no effect on turbulence or ridged noise as they are all ready abs
        private bool[] m_finalNosieIsAbs = new bool[]
        {
            true,
            false,
            false,
            false
        };
        //Noise settings. Each Component of vector is the setting for a layer
        //ie x is setting for layer 0, y is setting for layer 1 etc

        private Vector4 m_octaves = new Vector4(8, 6, 4, 8); //Higher octaves give more finer detail
        private Vector4 m_frequency = new Vector4(4f, 2f, 2f, 2f); //A lower value gives larger scale details

        private Vector4 m_lacunarity = new Vector4(2.5f, 2.3f, 2.0f, 2.0f); //Rate of change of the noise amplitude. Should be between 1 and 3 for fractal noise
        private Vector4 m_gain = new Vector4(0.5f, 0.5f, 0.5f, 0.5f); //Rate of change of the noise frequency
        static private float terrainAmountScale = 0.5f;
        //private Vector4 m_amp = new Vector4(6.0f * terrainAmountScale, 3f * terrainAmountScale, 6f * terrainAmountScale, 0.15f * terrainAmountScale); //Amount of terrain in a layer        
        private Vector4 m_amp = new Vector4(2f * terrainAmountScale, 1f * terrainAmountScale, 2f * terrainAmountScale, 1f * terrainAmountScale); //Amount of terrain in a layer        

        //original sets by Scrawk
        //public Vector4 m_octaves = new Vector4(8, 8, 8, 8); //Higher octaves give more finer detail
        //public Vector4 m_frequency = new Vector4(2.0f, 100.0f, 200.0f, 200.0f); //A lower value gives larger scale details
        //public Vector4 m_lacunarity = new Vector4(2.0f, 3.0f, 3.0f, 2.0f); //Rate of change of the noise amplitude. Should be between 1 and 3 for fractal noise
        //public Vector4 m_gain = new Vector4(0.5f, 0.5f, 0.5f, 0.5f); //Rate of chage of the noise frequency
        //public Vector4 m_amp = new Vector4(2.0f, 0.01f, 0.01f, 0.001f); //Amount of terrain in a layer


        private Vector4 m_offset = new Vector4(0.0f, 10.0f, 20.0f, 30.0f);

        /// <summary>
        /// The settings for the erosion. If the value is a vector4 each component is for a layer
        /// How easily the layer dissolves
        /// </summary>
        ///  rain
        public Vector4 dissolvingConstant = new Vector4(0.001f, 0.002f, 0.008f, 0.012f);
        //private Vector4 m_dissolvingConstant = new Vector4(0.01f, 0.015f, 0.8f, 1.2f);// stream


        /// <summary>The angle that slippage will occur </summary>        
        //private Vector4 m_talusAngle = new Vector4(80f, 35f, 60f, 10f); looked good
        public Vector4 talusAngle = new Vector4(70f, 45f, 60f, 30f);

        /// <summary>
        /// A higher value will increase erosion on flat areas
        /// Used as limit for surface tilt
        /// Meaning that even flat area will erode as slightly tilted area
        /// Not used now
        /// </summary>
        public float minTiltAngle = 5f;//0.1f;

        /// <summary> How much sediment the water can carry per 1 unit of water </summary>
        [SerializeField]
        private float sedimentCapacity = 0.2f;

        /// <summary> Rate the sediment is deposited on top layer </summary>
        [SerializeField]
        private float depositionConstant = 0.015f;

        /// <summary> Terrain wouldn't dissolve if water level in cell is lower than this</summary>
        public float dissolveLimit = 0.001f;

        /// <summary>Evaporation rate of water</summary>
        private float evaporationConstant = 0.001f;

        ///<summary>Used to draw arrows</summary>
        private float arrowMultiplier = 0.2f;

        /// <summary> Rain power</summary>
        private float rainInputAmount = 0.001f;

        /// <summary>Viscosity of regolith</summary>
        public float regolithDamping = 0.85f;

        /// <summary> Viscosity of water</summary>        
        public float waterDamping = 0.0f;

        /// <summary>Higher number will increase dissolution rate. Or not</summary>
        public float maxRegolith = 0.008f;

        public float oceanDestroySedimentsLevel = 0f;
        public float oceanDepth = 4f;
        public float oceanWaterLevel = 20f;
        public int oceanWidth = 110;

        ///<summary> Meshes</summary>
        private GameObject[] gridLand, gridWater, arrowsObjects, gridLava;

        ///<summary> Contains all 4 layers in ARGB</summary>

        [SerializeField]
        private DoubleDataTexture terrainField;

        [SerializeField]
        ///<summary></summary>        
        private DoubleDataTexture advectSediment;

        [SerializeField]
        ///<summary> Actual amount of dissolved sediment in water</summary>
        private DoubleDataTexture sedimentField;

        [SerializeField]
        ///<summary> Actual amount of dissolved sediment in water</summary>
        private DoubleDataTexture sedimentDeposition;

        [SerializeField]
        ///<summary> Contains regolith amount.Regolith is quasi-liquid at the bottom of water flow</summary>
        private DoubleDataTexture regolithField;

        [SerializeField]
        ///<summary> Moved regolith amount in format ARGB : A - flowLeft, R - flowR, G -  flowT, B - flowB</summary>
        private DoubleDataTexture regolithOutFlow;

        [SerializeField]
        ///<summary> Contains water amount. Can't be negative!!</summary>
        private DoubleDataTexture waterField;

        [SerializeField]
        ///<summary> Moved water amount in format ARGB : A - flowLeft, R - flowR, G -  flowT, B - flowB. Keeps only positive numbers</summary>
        private DoubleDataTexture waterOutFlow;

        ///<summary> Water speed (2 channels). Used for sediment movement and dissolution</summary>
        [SerializeField]
        private DoubleDataTexture waterVelocity;

        ///<summary> Contains surface angels for each point. Used in water erosion only (Why?)</summary>
        [SerializeField]
        private RenderTexture tiltAngle;


        /// <summary> Used for non-water erosion aka slippering of material</summary>
        [SerializeField]
        private RenderTexture slippageHeight;

        ///<summary> Used for non-water erosion aka slippering of material. ARGB in format: A - flowLeft, R - flowR, G -  flowT, B - flowB</summary>
        [SerializeField]
        private RenderTexture slippageOutflow;

        [SerializeField]
        private RenderTexture sedimentOutFlow;

        [SerializeField]
        private DoubleDataTexture magmaVelocity;

        [SerializeField]
        public Layer lava;

        private Rect m_rectLeft, m_rectRight, m_rectTop, m_rectBottom, withoutEdges, entireMap;

        //The resolution of the textures used for the simulation. You can change this to any number
        //Does not have to be a pow2 number. You will run out of GPU memory if made to high.
        public const int TEX_SIZE = 1024;//1024;//2048;//4096;
        public const int MAX_TEX_INDEX = TEX_SIZE - 1;//2047;

        ///<summary>The height of the terrain. You can change this</summary>
        private const int TERRAIN_HEIGHT = 128;
        //This is the size and resolution of the terrain mesh you see (in vertexes)
        //You can change this but must be a pow2 number, ie 256, 512, 1024 etc
        public const int TOTAL_GRID_SIZE = 512;  // TEX_SIZE /2;//512;//1024;
        //You can make this smaller but not larger
        private const float TIME_STEP = 0.1f;

        ///<summary>Size of 1 mesh in vertexes</summary>
        private const int GRID_SIZE = 129; // don/t change it. It allows about 33k triangles in mesh, while maximum 65k
        private const float PIPE_LENGTH = 1.0f;
        private const float CELL_LENGTH = 1.0f;
        private const float CELL_AREA = 1.0f; //CELL_LENGTH*CELL_LENGTH
        public const float GRAVITY = 9.81f;
        private const int READ = 0;
        private const int WRITE = 1;

        private Overlay currentOverlay = Overlay.Default;

        //private readonly
        public List<WorldSides> oceans = new List<WorldSides>();
        private readonly Color[] layersColors = new Color[4] {
            new Vector4(123,125,152,155).normalized,
            new Vector4(91f, 91f, 99f, 355f).normalized,
            new Vector4(113,52,21,355).normalized,
            new Vector4(157,156,0, 255).normalized };


        private void Start()
        {
            lava = new Layer("Lava", TEX_SIZE, 0.5f, this);

            layersColors[0].a = 0.98f;
            layersColors[1].a = 0.98f;
            layersColors[2].a = 0.99f;
            layersColors[3].a = 0.9f;
            Application.runInBackground = true;
            m_seed = UnityEngine.Random.Range(0, int.MaxValue);

            waterDamping = Mathf.Clamp01(waterDamping);
            regolithDamping = Mathf.Clamp01(regolithDamping);

            float u = 1.0f / (float)TEX_SIZE;

            m_rectLeft = new Rect(0.0f, 0.0f, u, 1.0f);
            m_rectRight = new Rect(1.0f - u, 0f, u, 1f);
            //m_rectRight = new Rect(1.0f , 0.0f, 1, 1.0f);


            m_rectBottom = new Rect(0.0f, 0.0f, 1.0f, u);
            m_rectTop = new Rect(0.0f, 1f - u, 1.0f, u);
            //m_rectTop = new Rect(0.0f, 1.0f, 1.0f, 1-u);


            withoutEdges = new Rect(0.0f + u, 0.0f + u, 1.0f - u, 1.0f - u);
            entireMap = new Rect(0f, 0f, 1f, 1f);


            InitLayers();
            MakeGrids();
            InitMaps();
        }

        private void InitLayers()
        {
            terrainField = new DoubleDataTexture("Terrain Height Field", TEX_SIZE, RenderTextureFormat.ARGBFloat, FilterMode.Point);

            waterField = new DoubleDataTexture("Water Field", TEX_SIZE, RenderTextureFormat.RFloat, FilterMode.Point);
            waterOutFlow = new DoubleDataTexture("Water outflow", TEX_SIZE, RenderTextureFormat.ARGBHalf, FilterMode.Point);
            waterVelocity = new DoubleDataTexture("Water Velocity", TEX_SIZE, RenderTextureFormat.RGHalf, FilterMode.Bilinear);// was RGHalf

            sedimentField = new DoubleDataTexture("Sediment Field", TEX_SIZE, RenderTextureFormat.RHalf, FilterMode.Bilinear);// was RHalf
            advectSediment = new DoubleDataTexture("Sediment Advection", TEX_SIZE, RenderTextureFormat.RHalf, FilterMode.Bilinear);// was RHalf
            sedimentDeposition = new DoubleDataTexture("Sediment Deposition", TEX_SIZE, RenderTextureFormat.RHalf, FilterMode.Point);// was RHalf
            sedimentOutFlow = DoubleDataTexture.Create("sedimentOutFlow", TEX_SIZE, RenderTextureFormat.ARGBHalf, FilterMode.Point);// was RHalf

            regolithField = new DoubleDataTexture("Regolith Field", TEX_SIZE, RenderTextureFormat.RFloat, FilterMode.Point);
            regolithOutFlow = new DoubleDataTexture("Regolith outflow", TEX_SIZE, RenderTextureFormat.ARGBHalf, FilterMode.Point);


            tiltAngle = DoubleDataTexture.Create("Tilt Angle", TEX_SIZE, RenderTextureFormat.RHalf, FilterMode.Point);// was RHalf
            slippageHeight = DoubleDataTexture.Create("Slippage Height", TEX_SIZE, RenderTextureFormat.RHalf, FilterMode.Point);// was RHalf
            slippageOutflow = DoubleDataTexture.Create("Slippage Outflow", TEX_SIZE, RenderTextureFormat.ARGBHalf, FilterMode.Point);// was ARGBHalf

            magmaVelocity = new DoubleDataTexture("Magma Velocity", TEX_SIZE, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);// was RGHalf          
        }

        /// <summary>
        ///  Calculates flow of field 
        /// </summary>
        private void FlowLiquid(DoubleDataTexture liquidField, DoubleDataTexture outFlow, float damping)
        {
            m_outFlowMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_outFlowMat.SetFloat("T", TIME_STEP);
            m_outFlowMat.SetFloat("L", PIPE_LENGTH);
            m_outFlowMat.SetFloat("A", CELL_AREA);
            m_outFlowMat.SetFloat("G", GRAVITY);
            m_outFlowMat.SetFloat("_Layers", TERRAIN_LAYERS);
            m_outFlowMat.SetFloat("_Damping", 1.0f - damping);
            m_outFlowMat.SetTexture("_TerrainField", terrainField.READ);
            m_outFlowMat.SetTexture("_Field", liquidField.READ);

            Graphics.Blit(outFlow.READ, outFlow.WRITE, m_outFlowMat);

            outFlow.Swap(); ;

            m_fieldUpdateMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_fieldUpdateMat.SetFloat("T", TIME_STEP);
            m_fieldUpdateMat.SetFloat("L", PIPE_LENGTH);
            m_fieldUpdateMat.SetTexture("_OutFlowField", outFlow.READ);

            Graphics.Blit(liquidField.READ, liquidField.WRITE, m_fieldUpdateMat);
            liquidField.Swap();
        }
        /// <summary>
        ///  Calculates how much ground should go in sediment flow aka force-based erosion
        ///  Transfers m_terrainField to m_sedimentField basing on
        ///  m_waterVelocity, m_sedimentCapacity, m_dissolvingConstant,
        ///  m_depositionConstant, m_tiltAngle, m_minTiltAngle
        /// Also calculates m_tiltAngle
        /// </summary>
        private void ErosionAndDeposition()
        {
            m_tiltAngleMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_tiltAngleMat.SetFloat("_Layers", TERRAIN_LAYERS);
            m_tiltAngleMat.SetTexture("_TerrainField", terrainField.READ);

            Graphics.Blit(null, tiltAngle, m_tiltAngleMat);

            m_erosionAndDepositionMat.SetTexture("_TerrainField", terrainField.READ);
            m_erosionAndDepositionMat.SetTexture("_SedimentField", sedimentField.READ);
            m_erosionAndDepositionMat.SetTexture("_VelocityField", waterVelocity.READ);
            m_erosionAndDepositionMat.SetTexture("_WaterField", waterField.READ);
            m_erosionAndDepositionMat.SetTexture("_TiltAngle", tiltAngle);
            m_erosionAndDepositionMat.SetFloat("_MinTiltAngle", minTiltAngle);
            m_erosionAndDepositionMat.SetFloat("_SedimentCapacity", sedimentCapacity);
            m_erosionAndDepositionMat.SetVector("_DissolvingConstant", dissolvingConstant);
            m_erosionAndDepositionMat.SetFloat("_DepositionConstant", depositionConstant);
            m_erosionAndDepositionMat.SetFloat("_Layers", (float)TERRAIN_LAYERS);
            m_erosionAndDepositionMat.SetFloat("_DissolveLimit", dissolveLimit); //nash added it            

            RenderTexture[] terrainAndSediment = new RenderTexture[3] { terrainField.WRITE, sedimentField.WRITE, sedimentDeposition.WRITE };

            RTUtility.MultiTargetBlit(terrainAndSediment, m_erosionAndDepositionMat);
            terrainField.Swap();
            sedimentField.Swap();
            sedimentDeposition.Swap();
        }
        /// <summary>
        /// Transfers ground to regolith basing on water level, regolith level, max_regolith
        /// aka dissolution based erosion
        /// </summary>
        private void DisintegrateAndDeposit()
        {
            m_disintegrateAndDepositMat.SetFloat("_Layers", (float)TERRAIN_LAYERS);
            m_disintegrateAndDepositMat.SetTexture("_TerrainField", terrainField.READ);
            m_disintegrateAndDepositMat.SetTexture("_WaterField", waterField.READ);
            m_disintegrateAndDepositMat.SetTexture("_RegolithField", regolithField.READ);
            m_disintegrateAndDepositMat.SetFloat("_MaxRegolith", maxRegolith);

            RenderTexture[] terrainAndRegolith = new RenderTexture[2] { terrainField.WRITE, regolithField.WRITE };

            RTUtility.MultiTargetBlit(terrainAndRegolith, m_disintegrateAndDepositMat);
            terrainField.Swap();
            regolithField.Swap();
        }
        /// <summary>
        ///  Calculates water velocity
        /// </summary>
        private void CalcWaterVelocity()
        {
            m_waterVelocityMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_waterVelocityMat.SetFloat("L", CELL_LENGTH);
            m_waterVelocityMat.SetTexture("_WaterField", waterField.READ);
            m_waterVelocityMat.SetTexture("_WaterFieldOld", waterField.WRITE);
            m_waterVelocityMat.SetTexture("_OutFlowField", waterOutFlow.READ);

            Graphics.Blit(null, waterVelocity.READ, m_waterVelocityMat);

            const float viscosity = 10.5f;
            const int iterations = 2;

            m_diffuseVelocityMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_diffuseVelocityMat.SetFloat("_Alpha", CELL_AREA / (viscosity * TIME_STEP));

            for (int i = 0; i < iterations; i++)
            {
                Graphics.Blit(waterVelocity.READ, waterVelocity.WRITE, m_diffuseVelocityMat);
                waterVelocity.Swap();
            }
        }
        /// <summary>
        ///  Moves sediment 
        /// </summary>
        private void AlternativeAdvectSediment()
        {
            //m_advectSedimentMat.SetFloat("_TexSize", (float)TEX_SIZE);
            //moveByLiquidMat.SetFloat("T", TIME_STEP/ 10000);
            ////m_advectSedimentMat.SetFloat("_VelocityFactor", 1.0f);
            //moveByLiquidMat.SetTexture("_VelocityField", m_waterVelocity.READ);            

            //Graphics.Blit(m_sedimentField.READ, m_sedimentField.WRITE, moveByLiquidMat);
            //m_sedimentField.Swap();

            ////moveByLiquidMat.SetFloat("T", TIME_STEP * -1f);
            ////Graphics.Blit(m_sedimentField.WRITE, m_sedimentField.READ, moveByLiquidMat);
            ////m_sedimentField.Swap();


            moveByLiquidMat.SetFloat("T", TIME_STEP);
            moveByLiquidMat.SetTexture("_OutFlow", waterOutFlow.READ);
            moveByLiquidMat.SetTexture("_LuquidLevel", waterField.READ);

            Graphics.Blit(sedimentField.READ, sedimentOutFlow, moveByLiquidMat);

            m_fieldUpdateMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_fieldUpdateMat.SetFloat("T", TIME_STEP);
            m_fieldUpdateMat.SetFloat("L", PIPE_LENGTH);
            m_fieldUpdateMat.SetTexture("_OutFlowField", sedimentOutFlow);

            Graphics.Blit(sedimentField.READ, sedimentField.WRITE, m_fieldUpdateMat);
            sedimentField.Swap();

        }
        /// <summary>
        ///  Moves sediment 
        /// </summary>
        private void AdvectSediment()
        {
            m_advectSedimentMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_advectSedimentMat.SetFloat("T", TIME_STEP);
            m_advectSedimentMat.SetFloat("_VelocityFactor", 1.0f);
            m_advectSedimentMat.SetTexture("_VelocityField", waterVelocity.READ);

            //is bug? No its no
            Graphics.Blit(sedimentField.READ, advectSediment.READ, m_advectSedimentMat);

            m_advectSedimentMat.SetFloat("_VelocityFactor", -1.0f);
            Graphics.Blit(advectSediment.READ, advectSediment.WRITE, m_advectSedimentMat);

            m_processMacCormackMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_processMacCormackMat.SetFloat("T", TIME_STEP);
            m_processMacCormackMat.SetTexture("_VelocityField", waterVelocity.READ);
            m_processMacCormackMat.SetTexture("_InterField1", advectSediment.READ);
            m_processMacCormackMat.SetTexture("_InterField2", advectSediment.WRITE);

            Graphics.Blit(sedimentField.READ, sedimentField.WRITE, m_processMacCormackMat);
            sedimentField.Swap();
        }
        /// <summary>
        /// Erodes all ground layers based on it m_talusAngle, water isn't evolved
        /// </summary>
        private void ApplySlippage()
        {
            for (int i = 0; i < TERRAIN_LAYERS; i++)
            {
                if (talusAngle[i] < 90.0f)
                {
                    float talusAngle = (Mathf.PI * this.talusAngle[i]) / 180.0f;
                    float maxHeightDif = Mathf.Tan(talusAngle) * CELL_LENGTH;

                    m_slippageHeightMat.SetFloat("_TexSize", (float)TEX_SIZE);
                    m_slippageHeightMat.SetFloat("_Layers", (float)(i + 1));
                    m_slippageHeightMat.SetFloat("_MaxHeightDif", maxHeightDif);
                    m_slippageHeightMat.SetTexture("_TerrainField", terrainField.READ);

                    Graphics.Blit(null, slippageHeight, m_slippageHeightMat);

                    m_slippageOutflowMat.SetFloat("_TexSize", (float)TEX_SIZE);
                    m_slippageOutflowMat.SetFloat("_Layers", (float)(i + 1));
                    m_slippageOutflowMat.SetFloat("T", TIME_STEP);
                    m_slippageOutflowMat.SetTexture("_MaxSlippageHeights", slippageHeight);
                    m_slippageOutflowMat.SetTexture("_TerrainField", terrainField.READ);

                    Graphics.Blit(null, slippageOutflow, m_slippageOutflowMat);

                    m_slippageUpdateMat.SetFloat("T", TIME_STEP);
                    m_slippageUpdateMat.SetFloat("_TexSize", (float)TEX_SIZE);
                    m_slippageUpdateMat.SetFloat("_Layers", (float)(i + 1));
                    m_slippageUpdateMat.SetTexture("_SlippageOutflow", slippageOutflow);

                    Graphics.Blit(terrainField.READ, terrainField.WRITE, m_slippageUpdateMat);
                    terrainField.Swap();
                }
            }
        }

        private void Simulate()
        {
            terrainField.SetFilterMode(FilterMode.Point);
            waterField.SetFilterMode(FilterMode.Point); ;
            sedimentDeposition.SetFilterMode(FilterMode.Point);

            if (simulateWaterFlow)
            {
                /// Evaporate water everywhere 
                if (evaporationConstant > 0.0f)
                {
                    waterField.ChangeValueZeroControl(evaporationConstant * -1f, entireMap);
                }
                if (rainInputAmount > 0.0f)
                {
                    waterField.ChangeValue(new Vector4(rainInputAmount, 0f, 0f, 0f), entireMap);
                }


                if (waterInputAmount > 0f)
                    waterField.ChangeValueGaussZeroControl(waterInputPoint, waterInputRadius, waterInputAmount, new Vector4(1f, 0f, 0f, 0f));// WaterInput();
                if (waterDrainageAmount > 0f)
                {
                    waterField.ChangeValueGaussZeroControl(waterDrainagePoint, waterDrainageRadius, waterDrainageAmount * -1f, new Vector4(1f, 0f, 0f, 0f));
                    terrainField.ChangeValueGaussZeroControl(waterDrainagePoint, waterDrainageRadius, waterDrainageAmount * -1f, new Vector4(0f, 0f, 0f, 1f));
                }

                // set specified levels of water and terrain at oceans
                foreach (var item in oceans)
                {
                    Rect rect = getPartOfMap(item, 1);
                    waterField.SetValue(new Vector4(oceanWaterLevel, 0f, 0f, 0f), rect);
                    terrainField.SetValue(new Vector4(oceanDestroySedimentsLevel, 0f, 0f, 0f), rect);
                }


                FlowLiquid(waterField, waterOutFlow, waterDamping);
                CalcWaterVelocity();
            }

            lava.main.SetFilterMode(FilterMode.Point);
            FlowLiquid(lava.main, lava.outFlow, 0.999f);
            //lava.Flow(terrainField.READ);
            lava.main.SetFilterMode(FilterMode.Bilinear);

            if (simulateWaterErosion)
            {
                ErosionAndDeposition();
                AdvectSediment();
                //AlternativeAdvectSediment();
            }
            if (simulateRigolith)
            {
                DisintegrateAndDeposit();
                FlowLiquid(regolithField, regolithOutFlow, regolithDamping);
            }
            if (simulateTectonics)
                terrainField.MoveByVelocity(magmaVelocity.READ, 1f, 0.03f, 1f, shader);
            if (simulateSlippage)
                ApplySlippage();

            

            terrainField.SetFilterMode(FilterMode.Bilinear);
            waterField.SetFilterMode(FilterMode.Bilinear);
            sedimentDeposition.SetFilterMode(FilterMode.Bilinear);
        }
        private void Update()
        {
            Simulate();
            UpdateMesh();
        }
        private void UpdateMesh()
        {
            // updating meshes
            //if the size of the mesh does not match the size of the texture 
            //the y axis needs to be scaled 
            float scaleY = (float)TOTAL_GRID_SIZE / (float)TEX_SIZE;

            if (currentOverlay == Overlay.Default)
            {
                currentOverlay.getMaterial().SetVector("_LayerColor0", layersColors[0]);
                currentOverlay.getMaterial().SetVector("_LayerColor1", layersColors[1]);
                currentOverlay.getMaterial().SetVector("_LayerColor2", layersColors[2]);
                currentOverlay.getMaterial().SetVector("_LayerColor3", layersColors[3]);

                currentOverlay.getMaterial().SetFloat("_ScaleY", scaleY);
                currentOverlay.getMaterial().SetFloat("_TexSize", (float)TEX_SIZE);
                currentOverlay.getMaterial().SetTexture("_MainTex", terrainField.READ);
                currentOverlay.getMaterial().SetFloat("_Layers", (float)TERRAIN_LAYERS);
            }
            else if (currentOverlay == Overlay.Deposition)
            {
                currentOverlay.getMaterial().SetVector("_LayerColor0", layersColors[0]);
                currentOverlay.getMaterial().SetVector("_LayerColor1", layersColors[1]);
                currentOverlay.getMaterial().SetVector("_LayerColor2", layersColors[2]);
                currentOverlay.getMaterial().SetVector("_LayerColor3", layersColors[3]);

                currentOverlay.getMaterial().SetFloat("_ScaleY", scaleY);
                currentOverlay.getMaterial().SetFloat("_TexSize", (float)TEX_SIZE);
                currentOverlay.getMaterial().SetTexture("_MainTex", terrainField.READ);
                currentOverlay.getMaterial().SetTexture("_SedimentDepositionField", sedimentDeposition.READ);
                currentOverlay.getMaterial().SetFloat("_Layers", (float)TERRAIN_LAYERS);
            }
            else if (currentOverlay == Overlay.Dissolution)
            {
                currentOverlay.getMaterial().SetVector("_LayerColor0", layersColors[0]);
                currentOverlay.getMaterial().SetVector("_LayerColor1", layersColors[1]);
                currentOverlay.getMaterial().SetVector("_LayerColor2", layersColors[2]);
                currentOverlay.getMaterial().SetVector("_LayerColor3", layersColors[3]);

                currentOverlay.getMaterial().SetFloat("_ScaleY", scaleY);
                currentOverlay.getMaterial().SetFloat("_TexSize", (float)TEX_SIZE);
                currentOverlay.getMaterial().SetTexture("_MainTex", terrainField.READ);
                currentOverlay.getMaterial().SetTexture("_SedimentDepositionField", sedimentDeposition.READ);
                currentOverlay.getMaterial().SetFloat("_Layers", (float)TERRAIN_LAYERS);
            }
            else if (currentOverlay == Overlay.WaterVelocity)
            {
                arrowsMat.SetFloat("_ScaleY", scaleY);
                arrowsMat.SetFloat("_TexSize", (float)TEX_SIZE);
                arrowsMat.SetTexture("_Terrain", terrainField.READ);
                arrowsMat.SetTexture("_Water", waterField.READ);
                arrowsMat.SetTexture("_WaterVelocity", waterVelocity.READ);
                arrowsMat.SetFloat("_LengthMultiplier", arrowMultiplier);
                arrowsMat.SetFloat("_Width", 0.03f);
            }
            else if (currentOverlay == Overlay.Plates)
            {
                currentOverlay.getMaterial().SetFloat("_ScaleY", scaleY);
                currentOverlay.getMaterial().SetFloat("_TexSize", (float)TEX_SIZE);
                currentOverlay.getMaterial().SetTexture("_MainTex", terrainField.READ);
                currentOverlay.getMaterial().SetTexture("_MagmaVelocity", magmaVelocity.READ);
                currentOverlay.getMaterial().SetFloat("_Layers", (float)TERRAIN_LAYERS);
            }
            else if (currentOverlay == Overlay.PlatesVelocity)
            {
                arrowsMat.SetFloat("_ScaleY", scaleY);
                arrowsMat.SetFloat("_TexSize", (float)TEX_SIZE);
                arrowsMat.SetTexture("_Terrain", terrainField.READ);
                arrowsMat.SetTexture("_Water", waterField.READ);
                arrowsMat.SetTexture("_WaterVelocity", magmaVelocity.READ);
                arrowsMat.SetFloat("_LengthMultiplier", arrowMultiplier);
                arrowsMat.SetFloat("_Width", 0.03f);
            }

            m_waterMat.SetTexture("_SedimentField", sedimentField.READ);
            m_waterMat.SetTexture("_VelocityField", waterVelocity.READ);
            m_waterMat.SetFloat("_ScaleY", scaleY);
            m_waterMat.SetFloat("_TexSize", (float)TEX_SIZE);
            m_waterMat.SetTexture("_WaterField", waterField.READ);
            m_waterMat.SetTexture("_Terrain", terrainField.READ);
            m_waterMat.SetTexture("_Lava", lava.main.READ);
            m_waterMat.SetFloat("_Layers", (float)TERRAIN_LAYERS);
            m_waterMat.SetVector("_SunDir", sun.transform.forward * -1.0f);
            m_waterMat.SetVector("_SedimentColor", new Vector4(1f - 0.808f, 1f - 0.404f, 1f - 0.00f, 1f));

            
            lavaMat.SetFloat("_ScaleY", scaleY);
            lavaMat.SetFloat("_TexSize", (float)TEX_SIZE);
            lavaMat.SetTexture("_WaterField", lava.main.READ);
            lavaMat.SetTexture("_Terrain", terrainField.READ);
            lavaMat.SetFloat("_Layers", (float)TERRAIN_LAYERS);
            lavaMat.SetVector("_SunDir", sun.transform.forward * -1.0f);


            //foreach (var item in m_gridLand)
            //{
            //    item.GetComponent<MeshCollider>().sharedMesh = item.GetComponent<MeshFilter>().mesh;
            //}            
        }
        private void InitMaps()
        {
            terrainField.ClearColor();
            waterOutFlow.ClearColor();
            waterVelocity.ClearColor();
            advectSediment.ClearColor();
            waterField.ClearColor();
            sedimentField.ClearColor();
            regolithField.ClearColor();
            regolithOutFlow.ClearColor();
            sedimentDeposition.ClearColor();
            magmaVelocity.ClearColor();



            DoubleDataTexture noiseTex;

            noiseTex = new DoubleDataTexture("", TEX_SIZE, RenderTextureFormat.RFloat, FilterMode.Bilinear);

            GPUPerlinNoise perlin = new GPUPerlinNoise(m_seed);
            perlin.LoadResourcesFor2DNoise();

            m_noiseMat.SetTexture("_PermTable1D", perlin.PermutationTable1D);
            m_noiseMat.SetTexture("_Gradient2D", perlin.Gradient2D);

            for (int j = 0; j < TERRAIN_LAYERS; j++)
            {
                m_noiseMat.SetFloat("_Offset", m_offset[j]);

                float amp = 0.5f;
                float freq = m_frequency[j];

                //Must clear noise from last pass
                noiseTex.ClearColor();

                //write noise into texture with the settings for this layer
                for (int i = 0; i < m_octaves[j]; i++)
                {
                    m_noiseMat.SetFloat("_Frequency", freq);
                    m_noiseMat.SetFloat("_Amp", amp);
                    m_noiseMat.SetFloat("_Pass", (float)i);

                    Graphics.Blit(noiseTex.READ, noiseTex.WRITE, m_noiseMat, (int)m_layerStyle[j]);
                    noiseTex.Swap();

                    freq *= m_lacunarity[j];
                    amp *= m_gain[j];
                }

                float useAbs = 0.0f;
                if (m_finalNosieIsAbs[j]) useAbs = 1.0f;

                //Mask the layers that we dont want to write into
                Vector4 mask = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                mask[j] = 1.0f;

                m_initTerrainMat.SetFloat("_Amp", m_amp[j]);
                m_initTerrainMat.SetFloat("_UseAbs", useAbs);
                m_initTerrainMat.SetVector("_Mask", mask);
                m_initTerrainMat.SetTexture("_NoiseTex", noiseTex.READ);
                m_initTerrainMat.SetFloat("_Height", TERRAIN_HEIGHT);

                //Apply the noise for this layer to the terrain field
                Graphics.Blit(terrainField.READ, terrainField.WRITE, m_initTerrainMat);
                terrainField.Swap();
            }

            //dont need this tex anymore
            noiseTex.Destroy();

        }

        private void OnDestroy()
        {
            DoubleDataTexture.DestroyAll();
            Destroy(tiltAngle);
            Destroy(slippageHeight);
            Destroy(slippageOutflow);
            Destroy(sedimentOutFlow);


            int numGrids = TOTAL_GRID_SIZE / GRID_SIZE + 1;
            for (int x = 0; x < numGrids; x++)
            {
                for (int y = 0; y < numGrids; y++)
                {
                    int idx = x + y * numGrids;

                    Destroy(gridLand[idx]);
                    Destroy(gridWater[idx]);
                    Destroy(gridLava[idx]);

                }
            }

        }

        private void MakeGrids()
        {
            int numGrids = TOTAL_GRID_SIZE / GRID_SIZE + 1;

            gridLand = new GameObject[numGrids * numGrids];
            gridWater = new GameObject[numGrids * numGrids];
            gridLava = new GameObject[numGrids * numGrids];
            arrowsObjects = new GameObject[numGrids * numGrids];

            for (int x = 0; x < numGrids; x++)
            {
                for (int y = 0; y < numGrids; y++)
                {
                    int idx = x + y * numGrids;

                    int posX = x * (GRID_SIZE - 1);
                    int posY = y * (GRID_SIZE - 1);


                    Mesh mesh = MakeGridMesh(GRID_SIZE, TOTAL_GRID_SIZE, posX, posY);

                    mesh.bounds = new Bounds(new Vector3(GRID_SIZE / 2, 0, GRID_SIZE / 2), new Vector3(GRID_SIZE, TERRAIN_HEIGHT * 2, GRID_SIZE));

                    gridLand[idx] = new GameObject("Grid Land " + idx.ToString());
                    gridLand[idx].AddComponent<MeshFilter>();
                    gridLand[idx].AddComponent<MeshRenderer>();
                    gridLand[idx].GetComponent<Renderer>().material = m_landMat;
                    gridLand[idx].GetComponent<MeshFilter>().mesh = mesh;
                    //m_gridLand[idx].AddComponent<MeshCollider>();
                    //m_gridLand[idx].GetComponent<MeshCollider>().gameObject.layer = 8;
                    //m_gridLand[idx].GetComponent<MeshCollider>().sharedMesh = mesh;

                    gridLand[idx].transform.localPosition = new Vector3(-TOTAL_GRID_SIZE / 2 + posX, 0, -TOTAL_GRID_SIZE / 2 + posY);
                    gridLand[idx].transform.SetParent(this.transform);


                    gridLava[idx] = new GameObject("Grid Lava " + idx.ToString());
                    gridLava[idx].AddComponent<MeshFilter>();
                    gridLava[idx].AddComponent<MeshRenderer>();
                    gridLava[idx].GetComponent<Renderer>().material = lavaMat;
                    gridLava[idx].GetComponent<MeshFilter>().mesh = mesh;
                    gridLava[idx].transform.localPosition = new Vector3(-TOTAL_GRID_SIZE / 2 + posX, 0, -TOTAL_GRID_SIZE / 2 + posY);
                    gridLava[idx].transform.SetParent(this.transform);

                    gridWater[idx] = new GameObject("Grid Water " + idx.ToString());
                    gridWater[idx].AddComponent<MeshFilter>();
                    gridWater[idx].AddComponent<MeshRenderer>();
                    gridWater[idx].GetComponent<Renderer>().material = m_waterMat;
                    gridWater[idx].GetComponent<MeshFilter>().mesh = mesh;
                    gridWater[idx].transform.localPosition = new Vector3(-TOTAL_GRID_SIZE / 2 + posX, 0, -TOTAL_GRID_SIZE / 2 + posY);
                    gridWater[idx].transform.SetParent(this.transform);

                    

                    arrowsObjects[idx] = new GameObject("Arrows " + idx.ToString());
                    arrowsObjects[idx].AddComponent<MeshFilter>();
                    arrowsObjects[idx].AddComponent<MeshRenderer>();
                    arrowsObjects[idx].GetComponent<Renderer>().material = arrowsMat;
                    arrowsObjects[idx].GetComponent<MeshFilter>().mesh = MakeArrowsMesh(GRID_SIZE, TOTAL_GRID_SIZE, posX, posY);
                    arrowsObjects[idx].transform.localPosition = new Vector3(-TOTAL_GRID_SIZE / 2 + posX, 0, -TOTAL_GRID_SIZE / 2 + posY);
                    arrowsObjects[idx].transform.SetParent(this.transform);
                }
            }
            foreach (var item in arrowsObjects)
                item.SetActive(false);
        }

        private Mesh MakeGridMesh(int size, int totalSize, int posX, int posY)
        {

            Vector3[] vertices = new Vector3[size * size];
            Vector2[] texcoords = new Vector2[size * size];
            Vector3[] normals = new Vector3[size * size];
            int[] indices = new int[size * size * 6];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 uv = new Vector3((posX + x) / (totalSize - 1.0f), (posY + y) / (totalSize - 1.0f));
                    Vector3 pos = new Vector3(x, 0.0f, y);
                    Vector3 norm = new Vector3(0.0f, 1.0f, 0.0f);

                    texcoords[x + y * size] = uv;
                    vertices[x + y * size] = pos;
                    normals[x + y * size] = norm;
                }
            }

            int num = 0;
            for (int x = 0; x < size - 1; x++)
            {
                for (int y = 0; y < size - 1; y++)
                {
                    indices[num++] = x + y * size;
                    indices[num++] = x + (y + 1) * size;
                    indices[num++] = (x + 1) + y * size;

                    indices[num++] = x + (y + 1) * size;
                    indices[num++] = (x + 1) + (y + 1) * size;
                    indices[num++] = (x + 1) + y * size;
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.triangles = indices;
            mesh.normals = normals;

            return mesh;
        }
        private Mesh MakeArrowsMesh(int size, int totalSize, int posX, int posY)
        {

            Vector3[] vertices = new Vector3[size * size * 3];
            Vector2[] texcoords = new Vector2[size * size * 3];
            Vector3[] normals = new Vector3[size * size * 3];
            int[] indices = new int[size * size * 6];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector2 uv = new Vector3((posX + x) / (totalSize - 1.0f), (posY + y) / (totalSize - 1.0f));
                    Vector3 pos = new Vector3(x, 0.0f, y);
                    Vector3 norm = new Vector3(0.0f, 1.0f, 0.0f);
                    texcoords[(x + y * size) * 3 + 0] = uv;
                    vertices[(x + y * size) * 3 + 0] = pos;
                    normals[(x + y * size) * 3 + 0] = norm;

                    pos = new Vector3(x + 0.0f, 0.0f, y);
                    norm = new Vector3(0.0f, 1.0f, 0.0f);

                    texcoords[(x + y * size) * 3 + 1] = uv;
                    vertices[(x + y * size) * 3 + 1] = pos;
                    normals[(x + y * size) * 3 + 1] = norm;

                    pos = new Vector3(x, 0.0f, y + 0.0f);
                    norm = new Vector3(0.0f, 1.0f, 0.0f);

                    texcoords[(x + y * size) * 3 + 2] = uv;
                    vertices[(x + y * size) * 3 + 2] = pos;
                    normals[(x + y * size) * 3 + 2] = norm;
                }
            }

            int num = 0;
            for (int x = 0; x < size - 1; x++)
            {
                for (int y = 0; y < size - 1; y++)
                {
                    indices[num++] = 3 * (x + y * size) + 2;
                    indices[num++] = 3 * (x + y * size) + 1;
                    indices[num++] = 3 * (x + y * size) + 0;


                    //indices[num++] = x + (y + 1) * size;
                    //indices[num++] = (x + 1) + (y + 1) * size;
                    //indices[num++] = (x + 1) + y * size;
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = texcoords;
            mesh.triangles = indices;
            mesh.normals = normals;

            return mesh;
        }
        public void MakeMapFlat()
        {
            //m_terrainField.SetValue(new Vector4(10f, 2f, 2f, 2f), entireMap);
            terrainField.SetValue(new Vector4(10f, 0f, 0f, 0f), entireMap);
        }
        public void SetMagmaVelocity(RenderTexture tex)
        {
            magmaVelocity.Set(tex);
        }
        public void SetTerrain(RenderTexture tex)
        {
            terrainField.Set(tex);
        }
        public void AddToTerrainLayer(MaterialsForEditing layer, Vector2 point)
        {
            Vector4 layerMask = default(Vector4);
            if (layer == MaterialsForEditing.stone)
                layerMask = new Vector4(1f, 0f, 0f, 0f);
            else if (layer == MaterialsForEditing.cobble)
                layerMask = new Vector4(0f, 1f, 0f, 0f);
            else if (layer == MaterialsForEditing.clay)
                layerMask = new Vector4(0f, 0f, 1f, 0f);
            else if (layer == MaterialsForEditing.sand)
                layerMask = new Vector4(0f, 0f, 0f, 1f);
            terrainField.ChangeValueGauss(point, brushSize, brushPower, layerMask);
        }
        public void RemoveFromTerrainLayer(MaterialsForEditing layer, Vector2 point)
        {
            Vector4 layerMask = default(Vector4);
            if (layer == MaterialsForEditing.stone)
            {
                layerMask = new Vector4(1f, 0f, 0f, 0f);
                terrainField.ChangeValueGauss(point, brushSize, brushPower * -1f, layerMask);
                return;
            }
            else if (layer == MaterialsForEditing.cobble)
                layerMask = new Vector4(0f, 1f, 0f, 0f);
            else if (layer == MaterialsForEditing.clay)
                layerMask = new Vector4(0f, 0f, 1f, 0f);
            else if (layer == MaterialsForEditing.sand)
                layerMask = new Vector4(0f, 0f, 0f, 1f);
            terrainField.ChangeValueGaussZeroControl(point, brushSize, brushPower * -1f, layerMask);
        }
        public void AddWater(Vector2 point)
        {
            waterField.ChangeValueGauss(point, brushSize, brushPower, new Vector4(1f, 0f, 0f, 0f));
        }
        public void RemoveWater(Vector2 point)
        {
            waterField.ChangeValueGaussZeroControl(point, brushSize, brushPower * -1f, new Vector4(1f, 0f, 0f, 0f));
        }
        internal void AddLava(Vector2 point)
        {
            lava.main.ChangeValueGauss(point, brushSize, brushPower, new Vector4(1f, 0f, 0f, 0f));
        }
        public void AddSediment(Vector2 point)
        {
            sedimentField.ChangeValueGauss(point, brushSize, brushPower / 50f, new Vector4(1f, 0f, 0f, 0f));
        }
        public void RemoveSediment(Vector2 point)
        {
            sedimentField.ChangeValueGaussZeroControl(point, brushSize, brushPower * -1f / 50f, new Vector4(1f, 0f, 0f, 0f));
        }
        internal void RemoveWaterSource()
        {
            waterInputAmount = 0f;
        }
        internal void MoveWaterSource(Vector2 point)
        {
            waterInputPoint = point;
            //m_waterInputPoint.x = point.x / (float)TEX_SIZE;
            //m_waterInputPoint.y = point.y / (float)TEX_SIZE;
            waterInputRadius = brushSize;
            waterInputAmount = brushPower;

        }
        internal void RemoveWaterDrainage()
        {
            waterDrainageAmount = 0f;
        }
        internal void MoveWaterDrainage(Vector2 point)
        {
            waterDrainagePoint = point;
            //waterDrainagePoint.x = selectedPoint.x / (float)TEX_SIZE;
            //waterDrainagePoint.y = selectedPoint.y / (float)TEX_SIZE;
            waterDrainageRadius = brushSize;
            waterDrainageAmount = brushPower;
        }
        /// <summary>
        /// get rect-part of world texture according to world side
        /// </summary>
        private Rect getPartOfMap(WorldSides side, int width)
        {
            float offest = width / (float)TEX_SIZE;
            Rect rect = default(Rect);
            if (side == WorldSides.North)
            {
                rect = m_rectTop;
                rect.height += offest;// *-1f;                
                rect.y -= offest;
            }
            else if (side == WorldSides.South)
            {
                rect = m_rectBottom;
                rect.height += offest;
            }
            else if (side == WorldSides.East)
            {
                rect = m_rectRight;
                rect.x -= offest;
                rect.width += offest;
            }
            else if (side == WorldSides.West)
            {
                rect = m_rectLeft;
                rect.width += offest;// * -1f;
            }
            return rect;
        }
        /// <summary>
        /// returns which side of map is closer to point - north, south, etc
        /// </summary>
        private WorldSides getSideOfWorld(Vector2 point)
        {
            // find to which border it's closer 
            WorldSides side = default(WorldSides);
            int distToNorth = (int)Math.Abs(0 - point.x);
            int distToSouth = (int)Math.Abs(MAX_TEX_INDEX - point.x);
            int distToWest = (int)Math.Abs(0 - point.y);
            int distToEast = (int)Math.Abs(MAX_TEX_INDEX - point.y);

            if (distToEast == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSides.North;
            else if (distToWest == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSides.South;
            else if (distToSouth == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSides.East;
            else if (distToNorth == Math.Min(Math.Min(Math.Min(distToWest, distToEast), distToNorth), distToSouth))
                side = WorldSides.West;

            return side;
        }

        public void AddOcean(Vector2 point)
        {
            var side = getSideOfWorld(point);
            if (!oceans.Contains(side))
            {
                oceans.Add(side);
                //oceans = oceans & side;
                // clear ocean bottom
                terrainField.ChangeValue(new Vector4(oceanDepth * -1f, 0f, 0, 0f), getPartOfMap(side, oceanWidth));
            }
        }
        public void RemoveOcean(Vector2 point)
        {
            var side = getSideOfWorld(point);
            if (oceans.Contains(side))
            {
                oceans.Remove(side);
                terrainField.ChangeValue(new Vector4(oceanDepth, 0f, 0f, 0f), getPartOfMap(side, oceanWidth));
            }
        }

        public float getTerrainLevel(Vector2 point)
        {
            var vector4 = terrainField.getDataRGBAFloatEF(point);
            return vector4.x + vector4.y + vector4.z + vector4.w;
        }
        public Vector4 getTerrainLayers(Vector2 point)
        {
            //return getData4Float32bits(m_terrainField.READ, point);
            return terrainField.getDataRGBAFloatEF(point);
        }
        internal Vector4 getSedimentInWater(Point selectedPoint)
        {
            return sedimentField.getDataRFloatEF(selectedPoint);
        }
        internal Vector4 getWaterLevel(Point selectedPoint)
        {

            return waterField.getDataRFloatEF(selectedPoint);

            //Vector4 value = new Vector4(0f,1f,2,3f);
            //getValueMat.SetVector("_Coords", selectedPoint.getVector2(TEX_SIZE));
            //getValueMat.SetVector("_Output",  value);
            //Graphics.Blit(m_waterField.READ, null, getValueMat);            

            //return getValueMat.GetVector("_Output");
        }
        //internal Vector4 getWaterVelocity(Point selectedPoint)
        //{
        //    return getDataRGBAFloatEF(m_waterVelocity.READ, selectedPoint);
        //}

        private bool simulateWaterFlow = false;
        public void SetSimulateWater(bool value)
        {
            simulateWaterFlow = value;
        }

        public void SetWaterVisability(bool value)
        {
            if (value)
                //m_waterMat.SetVector("_WaterAbsorption", new Vector4(0.259f, 0.086f, 0.113f, 1000f));
                foreach (var item in gridWater)
                {
                    item.GetComponent<Renderer>().enabled = true;
                }
            else
                //m_waterMat.SetVector("_WaterAbsorption", new Vector4(0f, 0f, 0f, 0f));
                foreach (var item in gridWater)
                {
                    item.GetComponent<Renderer>().enabled = false;
                }
        }
        private bool simulateRigolith;
        public void SetSimulateRegolith(bool value)
        {
            simulateRigolith = value;
        }
        private bool simulateSlippage;
        public void SetSimulateSlippage(bool value)
        {
            simulateSlippage = value;
        }
        private bool simulateWaterErosion;
        public void SetSimulateWaterErosion(bool value)
        {
            simulateWaterErosion = value;
        }
        private bool simulateTectonics;
        public void SetSimulateTectonics(bool value)
        {
            simulateTectonics = value;
        }

        private float brushSize = 0.001f;
        public void SetBrushSize(float value)
        {
            brushSize = value;
        }
        private float brushPower = 0.5f;
        public void SetBrushPower(float value)
        {
            brushPower = value;
            //magmaVelocity.SetRandomValue(new Vector4(1f, 1f, 0.3f, 0f), 100);
            //magmaVelocity.ChangeValueGauss(new Vector2(0.5f, 0.5f), 0.2f, 0.02f, new Vector4(1, 1, 10));
            //magmaVelocity.ChangeValue(new Vector4(0.5f, 0f, 0, 0f), getPartOfMap(WorldSides.North, 200));


        }
        public void SetWaterZFighting(float value)
        {
            //todo save initial value
            m_waterMat.SetFloat("_MinWaterHt", value);
        }
        public void SetRainPower(float value)
        {
            rainInputAmount = value;
        }
        public void SetEvaporationPower(float value)
        {
            evaporationConstant = value;
        }
        public void SetArrowMultiplier(float value)
        {
            arrowMultiplier = value;
        }
        public void SetDepositionRate(float value)
        {
            depositionConstant = value;
        }
        public void SetWaterSedimentCapacity(float value)
        {
            sedimentCapacity = value;
        }
        public void ScaleMapHeight(float value)
        {
            terrainField.Scale(value);
        }
        public void SetOverlay(Overlay overlay)
        {
            this.currentOverlay = overlay;

            if (currentOverlay.isArrow())
                foreach (var item in arrowsObjects)
                    item.SetActive(true);
            else
            {
                foreach (var item in arrowsObjects)
                    item.SetActive(false);
                foreach (var item in gridLand)
                {
                    item.GetComponent<Renderer>().material = currentOverlay.getMaterial();
                }
            }
        }
    }
}
