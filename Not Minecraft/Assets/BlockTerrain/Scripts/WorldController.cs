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
        private const int WorldHeight = 32;

        public Dictionary<int3, Chunk> ChunkPositionMap;

        public ChunkGameObject ChunkPrefab;

        public Material ChunkMaterial;

        public MeshData MeshData;

        public WorldConfig Configs;

        private List<Chunk> ChunksAwaitingMesh;

        private ObjectPool<ChunkGameObject> ChunkGOPool;

        private void Awake()
        {
            ChunkGOPool = new ObjectPool<ChunkGameObject>(CreateChunkGameObject);
            ChunkPositionMap = new Dictionary<int3, Chunk>();
            ChunksAwaitingMesh = new List<Chunk>();

            MeshData = new MeshData();
            MeshData.Initialize(100000);
        }

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
        }

        public bool CreateChunkAt(int x, int z)
        {
            int3 chunkPosition = WorldToChunkPosition(x, 0, z);

            if(ChunkPositionMap.ContainsKey(chunkPosition))
            {
                return false;
            }

            for (int y = 0; y < WorldHeight; y++)
            {
                Chunk chunk = new(chunkPosition);
                chunk.GenerateBlocks();
                ChunkPositionMap.Add(chunkPosition, chunk);
                chunk.RefreshNeighbors(this);

                ChunksAwaitingMesh.Add(chunk);
                chunkPosition.y += Chunk.Height;
            }

            return true;
        }

        public void GenerateTerrain(int3 center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    CreateChunkAt(center.x + (x * Chunk.Size), center.z + (z * Chunk.Size));
                }
            }

            foreach(var chunk in ChunksAwaitingMesh)
            {
                MeshBuilder.CreateSimpleMesh(chunk, MeshData);
                var mesh = MeshData.ToMesh();
                var chunkGO = ChunkGOPool.Get();
                chunkGO.gameObject.SetActive(true);

                chunkGO.transform.position = new Vector3(chunk.Position.x, chunk.Position.y, chunk.Position.z);
                chunkGO.SetMesh(mesh);

                if (MeshData.VertexCount == 0)
                {
                    chunkGO.gameObject.SetActive(false);
                    ChunkGOPool.Return(chunkGO);
                }

                MeshData.Reset();
            }

            ChunksAwaitingMesh.Clear();
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
                x < 0 ? (int)((x - 1) / (float)Chunk.Size) * Chunk.Size : (int)(x / (float)Chunk.Size) * Chunk.Size,
                y < 0 ? (int)((y - 1) / (float)Chunk.Height) * Chunk.Height : (int)(y / (float)Chunk.Height) * Chunk.Height,
                z < 0 ? (int)((z - 1) / (float)Chunk.Size) * Chunk.Size : (int)(z / (float)Chunk.Size) * Chunk.Size
            );
        }

        public static int3 WorldToChunkPosition(float x, float y, float z)
        {
            return new int3(
                x < 0 ? (int)((x - Chunk.Size) / Chunk.Size) * Chunk.Size : (int)(x / Chunk.Size) * Chunk.Size,
                y < 0 ? (int)((y - Chunk.Height) / Chunk.Height) * Chunk.Height : (int)(y / Chunk.Height) * Chunk.Height,
                z < 0 ? (int)((z - Chunk.Size) / Chunk.Size) * Chunk.Size : (int)(z / Chunk.Size) * Chunk.Size
            );
        }

    }
}


