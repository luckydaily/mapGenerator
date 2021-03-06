﻿using UnityEngine;
using Assets.PerlinNoise.Script.PerlinNoise;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Resources.Script.Monobehaviour.Generator
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colourMap;

        public MapData(float[,] heightMap, Color[] colourMap)
        {
            this.heightMap = heightMap;
            this.colourMap = colourMap;
        }
    }

    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColourMap,
            Mesh,
        };

        public DrawMode drawMode;
        public const int mapChunkSize = 241;
        [Range(0,6)]
        public int levelOfDetail;
        public float noiseScale;
        public bool autoUpdate;
        
        public int octaves;
        [Range (0,1)]
        public float persistance;
        public float lacurarity;

        public int seed;
        public Vector2 offset;
        public float meshHeightMultipier;
        public AnimationCurve meshHeightCurve;

        public TerrainType[] regions;
        private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData();

            MapDisplay display = FindObjectOfType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            else if (drawMode == DrawMode.ColourMap)
                display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.Mesh)
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultipier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }

        public void RequestMapData(Action<MapData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MapDataThread(callback);
            };

            new Thread(threadStart).Start();
        }

        private void MapDataThread(Action<MapData> callback)
        {
            MapData mapData = GenerateMapData();
            lock(mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, Action<MeshData> callback)
        {
            ThreadStart threadStart = delegate
            {
                MeshDataThread(mapData, callback);
            };

            new Thread(threadStart).Start();
        }
        private void MeshDataThread(MapData mapData, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultipier, meshHeightCurve, levelOfDetail);
            lock(meshDataThreadInfoQueue)
            {
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            if(mapDataThreadInfoQueue.Count > 0)
            {
                for(int i = 0; i < mapDataThreadInfoQueue.Count; ++i)
                {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }

            if (meshDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < meshDataThreadInfoQueue.Count; ++i)
                {
                    MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }

        private MapData GenerateMapData()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacurarity, offset);
            Color[] colourMap = new Color[mapChunkSize * mapChunkSize];

            for (int y = 0; y < mapChunkSize; ++y)
            {
                for(int x = 0; x < mapChunkSize; ++x)
                {
                    float currentHeight = noiseMap[x, y];

                    for(int i = 0; i < regions.Length; ++i)
                    {
                        if(currentHeight <= regions[i].height)
                        {
                            colourMap[y * mapChunkSize + x] = regions[i].colour;
                            break;
                        }
                    }
                }
            }

            return new MapData(noiseMap, colourMap);
        }

        private void OnValidate()
        {
            if (lacurarity < 1)
                lacurarity = 1;
            if (octaves < 0)
                octaves = 0;
        }

        private struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }
    }
}
