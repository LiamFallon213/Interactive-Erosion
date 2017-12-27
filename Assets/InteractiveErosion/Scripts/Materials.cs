using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Materials : MonoBehaviour
{
    public ComputeShader shader;
    ///<summary> Used for rendering</summary>
    public Material landRender, m_waterMat, arrowsMat, lavaMat, atmosphereRender, atmosphereRenderDownSide, 
        rainOverlay;
    ///<summary> Used for rendering</summary>
    //public Material[] overlays;

    public Material m_initTerrainMat, m_noiseMat;
    public Material m_outFlowMat;
    ///<summary> Updates field according to outflow</summary>
    public Material m_fieldUpdateMat;
    public Material m_waterVelocityMat, m_diffuseVelocityMat;
    public Material heatExchangeMat;
    /// <summary> Calculates angle for each cell </summary>
    public Material m_tiltAngleMat;
    ///<summary> Calculates layer erosion basing on the forces that are caused by the running water</summary>
    public Material m_processMacCormackMat;
    public Material dissolutionAndDepositionMat;
    ///<summary> Creates new texture based on smoothed sediment data and size of texture(?)</summary>
    public Material m_advectSedimentMat;
    public Material m_slippageHeightMat, m_slippageOutflowMat, m_slippageUpdateMat;
    public Material m_disintegrateAndDepositMat, m_applyFreeSlipMat;
    public Material rainFromAtmosphere;

    ///<summary> Evaporate from layer to atmosphere</summary>
    public Material evaporate;


    // from DoubleDataTexture
    public Material setFloatValueMat, changeValueMat, changeValueZeroControlMat, getValueMat,
        changeValueGaussMat, changeValueGaussZeroControlMat, setRandomValueMat, moveByVelocityMat,
        scaleMat, changeValueGaussWithHeatMat;
    
}
