using UnityEngine;
using Assets.PerlinNoise.Script.PerlinNoise;
using System.Collections;
using System.Linq;
using System.Text;

namespace Assets.Resources.Script.Monobehaviour
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColourMap,
        };

        public DrawMode drawMode;
        public int mapWidth;
        public int mapHeight;
        public float noiseScale;
        public bool autoUpdate;

        public TerrainType[] regions;

        public int octaves;
        [Range (0,1)]
        public float persistance;
        public float lacurarity;

        public int seed;
        public Vector2 offset;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacurarity, offset);
            Color[] colourMap = new Color[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; ++y)
            {
                for(int x = 0; x < mapWidth; ++x)
                {
                    float currentHeight = noiseMap[x, y];

                    for(int i = 0; i < regions.Length; ++i)
                    {
                        if(currentHeight <= regions[i].height)
                        {
                            colourMap[y * mapWidth + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }

            MapDisplay display = FindObjectOfType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
            else if (drawMode == DrawMode.ColourMap)
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }

        private void OnValidate()
        {
            if (mapWidth < 1)
                mapWidth = 1;
            if (mapHeight < 1)
                mapHeight = 1;
            if (lacurarity < 1)
                lacurarity = 1;
            if (octaves < 0)
                octaves = 0;
        }
    }
}
