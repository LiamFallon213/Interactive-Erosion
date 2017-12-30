
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace InterativeErosionProject
{
    enum Layers
    {
        stone, cobble, clay, sand, lava, water
    }
    public class InfoWindow : DragPanel
    {
        [SerializeField]
        private Text text;
        [SerializeField]
        private ErosionSim sim;

        public override void Refresh()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Selected point: x = {0}, y = {1}", ControlPanel.selectedPoint.x.ToString("F3"), ControlPanel.selectedPoint.y.ToString("F3"));


            var terrain = sim.getTerrainLayers(ControlPanel.selectedPoint);
            var terrainHeight = terrain.x + terrain.y + terrain.z + terrain.w;
            sb.Append("\nTerrain height: ").Append(terrainHeight);
            sb.Append("\n\t").Append((Layers)0).Append(" height: ").Append(terrain.x);
            sb.Append("\n\t").Append((Layers)1).Append(" height: ").Append(terrain.y);
            sb.Append("\n\t").Append((Layers)2).Append(" height: ").Append(terrain.z);
            sb.Append("\n\t").Append((Layers)3).Append(" height: ").Append(terrain.w);

            var lavaHeight = sim.getLavaLevel(ControlPanel.selectedPoint);
            sb.Append("\n\t").Append((Layers)4).Append(" height: ").Append(lavaHeight);

            var waterHeight = sim.getWaterLevel(ControlPanel.selectedPoint);
            sb.Append("\n\t").Append((Layers)5).Append(" height: ").Append(waterHeight);

            float totalHeight = lavaHeight + waterHeight + terrainHeight;
            sb.Append("\nTotal height: ").Append(totalHeight);

            sb.Append("\nSand in water: ").Append(sim.getSedimentInWater(ControlPanel.selectedPoint).x);

            var waterVelocity = sim.getWaterVelocity(ControlPanel.selectedPoint);
            sb.Append("\nWater velocity: ").Append(waterVelocity);
            sb.Append("\nWater speed: ").Append(waterVelocity.magnitude);
            sb.Append("\nWater flow: ").Append(sim.getWaterFlow(ControlPanel.selectedPoint));

            sb.Append("\nTerrain deposition(+)/dissolution(-): ").Append(sim.getDeposition(ControlPanel.selectedPoint));

            //sb.Append("\nLava flow: ").Append(sim.getLavaFlow(ControlPanel.selectedPoint));
            var lavaTemperature = sim.getLavaTemperature(ControlPanel.selectedPoint);
            sb.Append("\nLava temperature, K: ").Append(lavaTemperature);

            //Should be same as in shader!!
            //var fluidity = sim.getLavaFluidity() * Mathf.Pow(lavaTemperature, 3);
            //fluidity = Mathf.Clamp01(fluidity);
            //sb.Append("\nLava fluidity: ").Append(fluidity);

            sb.Append("\nAtmosphere temperature, K: ").Append(sim.getAtmosphereTemperature(ControlPanel.selectedPoint));
            float vapor = sim.getVaporInAtmosphere(ControlPanel.selectedPoint);
            sb.Append("\nVapor in atmosphere: ").Append(vapor);

            float maxVapor = (4236f * 0.018f * (120f-totalHeight)) / (8.314f * 303f);
            sb.Append("\nMax vapor mass: ").Append(maxVapor);

            float pReal = vapor * 8.314f * 303f / (0.018f * (120f - totalHeight));
            sb.Append("\nRelative humidity: ").Append(pReal / 4236f *100f).Append(" %");

            text.text = sb.ToString();

        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            Refresh();
        }
    }
}