using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Resources.Script.Monobehaviour.Generator;

namespace Assets.Resources.Script.Monobehaviour
{
    public class EndlessTerrain : MonoBehaviour
    {
        public const float maxViewDistance = 450;
        public Transform viewer;
        public Material material;

        public static MapGenerator mapGenerator;

        public static Vector2 viewerPosition;
        private int chunkSize;
        private int chunkVisibleInViewDistance;

        Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
        }

        private void UpdateVisibleChunks()
        {
            for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; ++i)
            {
                terrainChunksVisibleLastUpdate[i].SetVisible(false);
            }
            terrainChunksVisibleLastUpdate.Clear();

            int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewer.position.y / chunkSize);

            for(int offsetY = -chunkVisibleInViewDistance; offsetY <= chunkVisibleInViewDistance; ++offsetY)
            {
                for(int offsetX = -chunkVisibleInViewDistance; offsetX <= chunkVisibleInViewDistance; ++offsetX)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + offsetX, currentChunkCoordY + offsetY);
                    if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        if(terrainChunkDictionary[viewedChunkCoord].IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                        }
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, material));
                    }
                }
            }
        }
    }

    public class TerrainChunk
    {
        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;

            SetVisible(false);

            EndlessTerrain.mapGenerator.RequestMapData(OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            EndlessTerrain.mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= EndlessTerrain.maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
