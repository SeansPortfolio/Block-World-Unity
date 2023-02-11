

namespace MyGame.BlockTerrain
{
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Mathematics;
    using Utility;
    using BlockTerrain.Meshing;
    using Events;

    public class WorldController : MonoBehaviour
    {
        public Dictionary<int3, Chunk> ChunkPositionMap;

        public ChunkGameObject ChunkPrefab;

        public Material ChunkMaterial;

        public MeshData MeshData;

        public WorldConfig Configs;

        private ObjectPool<ChunkGameObject> ChunkGOPool;

        private void Start()
        {
            EventSystem.Instance.RegisterEvent(EventChannel.CreateNewGame, OnCreateWorld);

            Initialize(Configs);
        }

        private void OnCreateWorld(object sender, System.EventArgs args)
        {
            if(args is NewGameEventArgs)
            {
                var eventArgs = args as NewGameEventArgs;
                Initialize(eventArgs.Configs);
            }

        }

        public void Initialize(WorldConfig config)
        {
            Configs = config;

            ChunkGOPool = new ObjectPool<ChunkGameObject>(CreateChunkGameObject);
            ChunkPositionMap = new Dictionary<int3, Chunk>();


            for (int x = 0; x < Configs.Width; x++)
            {
                for (int z = 0; z < Configs.Depth; z++)
                {
                    CreateChunkAt(x * Chunk.Size, z * Chunk.Size);
                }
            }

            MeshData meshData = new MeshData();
            meshData.Initialize(100000);

            foreach(var chunk in ChunkPositionMap.Values)
            {
                MeshBuilder.CreateSimpleMesh(chunk, meshData);
                var mesh = meshData.ToMesh();
                var chunkGO = ChunkGOPool.Get();
                chunkGO.gameObject.SetActive(true);

                chunkGO.transform.position = new Vector3(chunk.Position.x, chunk.Position.y, chunk.Position.z);
                chunkGO.SetMesh(mesh);

                if(meshData.VertexCount == 0)
                {
                    chunkGO.gameObject.SetActive(false);
                    ChunkGOPool.Return(chunkGO);
                }

                meshData.Reset();
            }

            meshData.Dispose();
        }

        public bool CreateChunkAt(int x, int z)
        {
            int3 chunkPosition = WorldToChunkPosition(x, 0, z);

            if(ChunkPositionMap.ContainsKey(chunkPosition))
            {
                return false;
            }

            for (int y = 0; y < Configs.Height; y++)
            {
                Chunk chunk = new(chunkPosition);
                chunk.GenerateBlocks();
                ChunkPositionMap.Add(chunkPosition, chunk);
                chunk.RefreshNeighbors(this);

                chunkPosition.y += Chunk.Height;
            }

            return true;
        }

        public bool GetChunkAt(int x, int y, int z, out Chunk ch)
        {
            var position = WorldToChunkPosition(x, y, z);
            return ChunkPositionMap.TryGetValue(position, out ch);
        }

        private ChunkGameObject CreateChunkGameObject()
        {
            var chunkGO = Instantiate(ChunkPrefab);

            chunkGO.transform.SetParent(this.transform);
            chunkGO.gameObject.SetActive(false);

            return chunkGO.GetComponent<ChunkGameObject>();
        }

        public static int3 WorldToChunkPosition(int x, int y, int z)
        {
            return new int3(
                x < 0 ? (int)((x - Chunk.Size) / (float)Chunk.Size) * Chunk.Size : (int)(x / (float)Chunk.Size) * Chunk.Size,
                y < 0 ? (int)((y - Chunk.Height) / (float)Chunk.Height) * Chunk.Height : (int)(y / (float)Chunk.Height) * Chunk.Height,
                z < 0 ? (int)((z - Chunk.Size) / (float)Chunk.Size) * Chunk.Size : (int)(z / (float)Chunk.Size) * Chunk.Size
            );
        }
    }
}


