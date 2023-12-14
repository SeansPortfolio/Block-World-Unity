using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class WorldManager : MonoBehaviour
{
    public ThreadManager ThreadManager;

    public int WorldWidth = 8;

    public int WorldDepth = 8;

    public int WorldHeight = 32;

    public ChunkGameObject ChunkPrefab;

    public List<Texture2D> BlockTextures;

    public Material ChunkMaterial;

    public Texture2DArray ChunkTextureAtlas;

    private int CurrentDisplayHeight = 32;

    private List<Chunk> ChunksAwaitingBlocks;

    private List<Chunk> ChunksAwaitingMesh;

    private Dictionary<int3, Chunk> ChunkPositionMap;

    private Dictionary<Chunk, ChunkGameObject> ChunkGameObjectMap;

    private Dictionary<int2, Chunk[]> ChunkColumnMap;

    private ObjectPool<ChunkGameObject> ChunkGOPool;

    private void Start()
    {
        Initialize();
        StitchBlockTextures();
        GenerateWorld();
        SetDisplayHeight(WorldHeight - 1);
    }

    private void Update()
    {
        if(ChunksAwaitingBlocks.Count > 0 && ThreadManager.IsBlockBuilderReady)
        {
            List<Chunk> chunks = new List<Chunk>();

            for (int i = 0; i < BlockBuilderJob.MaxChunksToBuild && i < ChunksAwaitingBlocks.Count; i++)
            {
                chunks.Add(ChunksAwaitingBlocks[0]);
                ChunksAwaitingBlocks.RemoveAt(0);
            }

            ThreadManager.CreateBlockBuilderJob(chunks);
        }

        if (ChunksAwaitingMesh.Count > 0 && ThreadManager.IsMeshBuilderReady)
        {
            List<Chunk> chunks = new List<Chunk>();

            for (int i = 0; i < MeshBuilderJob.MaxChunksToBuild && i < ChunksAwaitingMesh.Count; i++)
            {
                chunks.Add(ChunksAwaitingMesh[0]);
                ChunksAwaitingMesh.RemoveAt(0);
            }

            ThreadManager.CreateMeshBuilderJob(chunks);
        }



        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetDisplayHeight(CurrentDisplayHeight - 1);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetDisplayHeight(CurrentDisplayHeight + 1);
        }
    }


    public void Initialize()
    {
        TerrainGenerator.SetSeed(UnityEngine.Random.Range(-100000, 100000));

        ChunkPositionMap = new Dictionary<int3, Chunk>();
        ChunkColumnMap = new Dictionary<int2, Chunk[]>();
        ChunkGameObjectMap = new Dictionary<Chunk, ChunkGameObject>();
        ChunkGOPool = new ObjectPool<ChunkGameObject>(CreateChunkGameObject);

        ChunksAwaitingBlocks = new List<Chunk>();
        ChunksAwaitingMesh = new List<Chunk>();

        ThreadManager.RegisterBlockBuilderComplete(OnBlockBuilderComplete);
        ThreadManager.RegisterMeshBuilderComplete(OnMeshBuilderComplete);
    }



    public void SetDisplayHeight(int height)
    {
        CurrentDisplayHeight = height;

        if(CurrentDisplayHeight < 0)
        {
            CurrentDisplayHeight = 0;
        }

        if(CurrentDisplayHeight > WorldHeight)
        {
            CurrentDisplayHeight = WorldHeight;
        }

        foreach(var pair in ChunkGameObjectMap)
        {
            var chunk = pair.Key;
            var go = pair.Value;

            if(chunk.Position.y > CurrentDisplayHeight)
            {
                go.gameObject.SetActive(false);
            }
            if(chunk.Position.y == CurrentDisplayHeight)
            {
                go.gameObject.SetActive(true);
                go.Refresh(chunk.IsUnderChunk());
            }
            if (chunk.Position.y < CurrentDisplayHeight)
            {
                go.gameObject.SetActive(true);
                go.Refresh(false);
            }

        }
    }



    public bool CreateChunkAt(int x, int z)
    {
        int2 chunkPosition = WorldToChunkPosition(x, z);

        if (ChunkColumnMap.ContainsKey(chunkPosition))
        {
            return false;
        }

        Chunk[] column = new Chunk[WorldHeight];

        for (int y = 0; y < WorldHeight; y++)
        {
            Chunk chunk = new(chunkPosition.x, y * Chunk.Height, chunkPosition.y);
            column[y] = chunk;
            ChunkPositionMap.Add(chunk.Position, chunk);
        }

        ChunkColumnMap.Add(chunkPosition, column);

        for (int y = 0; y < WorldHeight; y++)
        {
            column[y].RefreshNeighbors(this);
            ChunksAwaitingBlocks.Add(column[y]);
        }

        return true;
    }

    public bool GetChunkAt(int x, int y, int z, out Chunk ch)
    {
        var position = WorldToChunkPosition(x, y, z);
        return ChunkPositionMap.TryGetValue(position, out ch);
    }

    public bool GetChunkColumnAt(int x, int z, out Chunk[] column)
    {
        var position = WorldToChunkPosition(x, z);
        return ChunkColumnMap.TryGetValue(position, out column);
    }


    private void StitchBlockTextures()
    {
        ChunkTextureAtlas = new Texture2DArray(32, 32, BlockTextures.Count, TextureFormat.RGBA32, true);

        for (int i = 0; i < BlockTextures.Count; i++)
        {
            var texture = BlockTextures[i];
            BlockExtensions.AddTextureReference(texture.name, i);
            ChunkTextureAtlas.SetPixels(texture.GetPixels(), i);
        }

        ChunkTextureAtlas.Apply();
        ChunkMaterial.SetTexture("_MainTextures", ChunkTextureAtlas);
    }

    private void GenerateWorld()
    {
        for (int x = -WorldWidth; x <= WorldWidth; x++)
        {
            for (int z = -WorldDepth; z <= WorldDepth; z++)
            {
                CreateChunkAt(x * Chunk.Size, z * Chunk.Size);
            }
        }

        ThreadManager.CreateBlockBuilderJob(ChunkPositionMap.Values);
    }

    private void OnBlockBuilderComplete(BlockBuilderJob job)
    {
        for (int i = 0; i < job.ChunksToBuild.Count; i++)
        {
            var chunk = job.ChunksToBuild[i];

            if(!chunk.IsEmpty)
            {
                ChunksAwaitingMesh.Add(chunk);
            }
        }
    }

    private void OnMeshBuilderComplete(MeshBuilderJob job)
    {
        for (int i = 0; i < job.ChunksToBuild.Count; i++)
        {
            RefreshChunkMesh(job.ChunksToBuild[i], job.MeshDatas[i], job.MeshDatasFilledTops[i]);
        }
    }


    private void RefreshChunkMesh(Chunk chunk, MeshData meshData, MeshData filledTop)
    {
        if (meshData.VertexCount == 0 && filledTop.VertexCount == 0)
        {
            if(ChunkGameObjectMap.Remove(chunk, out ChunkGameObject chunkGO))
            {
                chunkGO.enabled = false;
                ChunkGOPool.Return(chunkGO);
            }
        }
        else
        {
            if (ChunkGameObjectMap.TryGetValue(chunk, out ChunkGameObject chunkGO))
            {
                chunkGO.SetMesh(meshData.ToMesh(), filledTop.ToMesh());
                chunkGO.Refresh(false);

                return;
            }
            else
            {
                chunkGO = ChunkGOPool.Get();
                chunkGO.transform.position = new Vector3(
                    chunk.Position.x,
                    chunk.Position.y,
                    chunk.Position.z
                );

                chunkGO.SetMesh(meshData.ToMesh(), filledTop.ToMesh());
                chunkGO.Refresh(false);

                ChunkGameObjectMap.Add(chunk, chunkGO);
            }
        }
    }

    public static int2 WorldToChunkPosition(int x, int z)
    {
        return new int2(
            x < 0 ? (int)((x - 1) / (float)Chunk.Size) * Chunk.Size : (int)(x / (float)Chunk.Size) * Chunk.Size,
            z < 0 ? (int)((z - 1) / (float)Chunk.Size) * Chunk.Size : (int)(z / (float)Chunk.Size) * Chunk.Size
        );
    }

    public static int2 WorldToChunkPosition(float x, float z)
    {
        return new int2(
            x < 0 ? (int)((x - Chunk.Size) / Chunk.Size) * Chunk.Size : (int)(x / Chunk.Size) * Chunk.Size,
            z < 0 ? (int)((z - Chunk.Size) / Chunk.Size) * Chunk.Size : (int)(z / Chunk.Size) * Chunk.Size
        );
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

    private ChunkGameObject CreateChunkGameObject()
    {
        var chunkGO = Instantiate(ChunkPrefab);
        chunkGO.transform.SetParent(this.transform);
        return chunkGO.GetComponent<ChunkGameObject>();
    }

}
