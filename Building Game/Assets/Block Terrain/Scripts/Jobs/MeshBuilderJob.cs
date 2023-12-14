using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MeshBuilderJob : ThreadedProcess
{
    private static ObjectPool<MeshData> MeshDataPool;

    static MeshBuilderJob()
    {
        MeshDataPool = new ObjectPool<MeshData>(CreateMeshData);
    }

    private static MeshData CreateMeshData()
    {
        MeshData data = new MeshData();
        data.Initialize(10000000);
        return data;
    }

    public static void ReturnMeshData(List<MeshData> meshData)
    {
        foreach(var data in meshData)
        {
            data.Reset();
            MeshDataPool.Return(data);
        }
    }


    public List<Chunk> ChunksToBuild;

    public List<MeshData> MeshDatas;

    public List<MeshData> MeshDatasFilledTops;

    public const int MaxChunksToBuild = 32;

    protected override void ThreadFunction()
    {
        if(MeshDatas == null)
        {
            MeshDatas = new List<MeshData>();
        }

        if(MeshDatasFilledTops == null)
        {
            MeshDatasFilledTops = new List<MeshData>();
        }


        for (int i = 0; i < ChunksToBuild.Count; i++)
        {
            MeshDatas.Add(MeshDataPool.Get());
            MeshDatasFilledTops.Add(MeshDataPool.Get());
        }

        for (int i = 0; i < ChunksToBuild.Count; i++)
        {
            var currentChunk = ChunksToBuild[i];

            CreateSimpleMesh(currentChunk, MeshDatas[i], false);
            CreateSimpleMesh(currentChunk, MeshDatasFilledTops[i], true);
        }
    }

    protected override void OnFinished()
    {

    }

    private static void CreateGreedyMesh(Chunk chunk, MeshData data, bool fillTop)
    {
        int i, j, k, l, w, h;
        var x = new int[3];
        var q = new int[3];
        var mask = new MeshMask[Chunk.Size * Chunk.Size];

        for (int index = 0; index < mask.Length; index++)
        {
            mask[index] = new MeshMask(Block.Void, -1, -1);
        }

        // sweep over each axis
        for (var dimension = 0; dimension < 3; ++dimension)
        {
            int u = (dimension + 1) % 3;
            int v = (dimension + 2) % 3;
            x[0] = 0;
            x[1] = 0;
            x[2] = 0;
            q[0] = 0;
            q[1] = 0;
            q[2] = 0;

            for (int index = 0; index < mask.Length; index++)
            {
                mask[index].Block = Block.Void;
                mask[index].Normal = -1;
                mask[index].AO = -1;
            }

            // Check each slice of the chunk one at a time
            for (x[dimension] = -1; x[dimension] < Chunk.Size; )
            {
                var n = 0;
                for (x[v] = 0; x[v] < Chunk.Size; ++x[v])
                {
                    for (x[u] = 0; x[u] < Chunk.Size; ++x[u])
                    {
                        // q determines the direction (X, Y or Z) that we are searching


                    }
                }
            }
        }
    }

    private static void CreateSimpleMesh(Chunk chunk, MeshData data, bool fillTop)
    {
        var blocks = chunk.Blocks;

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int y = 0; y < Chunk.Height; y++)
            {
                for (int z = 0; z < Chunk.Size; z++)
                {
                    var currentBlock = blocks[Chunk.PointToIndex(x, y, z)];
                    if (currentBlock == Block.Void)
                    {
                        continue;
                    }

                    //North
                    if (chunk.GetBlockAt(x, y, z + 1) == Block.Void)
                    {
                        data.AddQuad(
                            new float3(x + 0.5f, y - 0.5f, z + 0.5f),
                            new float3(x - 0.5f, y - 0.5f, z + 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z + 0.5f),
                            new float3(x - 0.5f, y + 0.5f, z + 0.5f),
                            new float3(0f, 0f, 1f),
                            currentBlock.GetTextureLayer(Direction.North),
                            CalculateAO(chunk, x, y, z, Direction.North)
                        );
                    }

                    //East
                    if (chunk.GetBlockAt(x + 1, y, z) == Block.Void)
                    {
                        data.AddQuad(
                            new float3(x + 0.5f, y - 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y - 0.5f, z + 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z + 0.5f),
                            new float3(1f, 0f, 0f),
                            currentBlock.GetTextureLayer(Direction.East),
                            CalculateAO(chunk, x, y, z, Direction.East)
                        );
                    }

                    //South
                    if (chunk.GetBlockAt(x, y, z - 1) == Block.Void)
                    {
                        data.AddQuad(
                            new float3(x - 0.5f, y - 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y - 0.5f, z - 0.5f),
                            new float3(x - 0.5f, y + 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z - 0.5f),
                            new float3(0f, 0f, -1f),
                            currentBlock.GetTextureLayer(Direction.South),
                            CalculateAO(chunk, x, y, z, Direction.South)
                        );
                    }

                    //West
                    if (chunk.GetBlockAt(x - 1, y, z) == Block.Void)
                    {
                        data.AddQuad(
                            new float3(x - 0.5f, y - 0.5f, z + 0.5f),
                            new float3(x - 0.5f, y - 0.5f, z - 0.5f),
                            new float3(x - 0.5f, y + 0.5f, z + 0.5f),
                            new float3(x - 0.5f, y + 0.5f, z - 0.5f),
                            new float3(-1f, 0f, 0f),
                            currentBlock.GetTextureLayer(Direction.West),
                            CalculateAO(chunk, x, y, z, Direction.West)
                        );
                    }

                    //Up
                    if (chunk.GetBlockAt(x, y + 1, z) == Block.Void)
                    {
                        data.AddQuad(
                            new float3(x - 0.5f, y + 0.5f, z + 0.5f),
                            new float3(x - 0.5f, y + 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z + 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z - 0.5f),
                            new float3(0f, 1f, 0f),
                            currentBlock.GetTextureLayer(Direction.Up),
                            CalculateAO(chunk, x, y, z, Direction.Up)
                        );
                    }
                    else if(fillTop)
                    {
                        data.AddQuad(
                            new float3(x - 0.5f, y + 0.5f, z + 0.5f),
                            new float3(x - 0.5f, y + 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z + 0.5f),
                            new float3(x + 0.5f, y + 0.5f, z - 0.5f),
                            new float3(0f, 1f, 0f),
                            currentBlock.GetFilledTopTextureLayer(),
                            CalculateAO(chunk, x, y, z, Direction.Up)
                        );
                    }

                    //Down
                    if (chunk.GetBlockAt(x, y - 1, z) == Block.Void)
                    {
                        data.AddQuad(
                            new float3(x - 0.5f, y - 0.5f, z - 0.5f),
                            new float3(x - 0.5f, y - 0.5f, z + 0.5f),
                            new float3(x + 0.5f, y - 0.5f, z - 0.5f),
                            new float3(x + 0.5f, y - 0.5f, z + 0.5f),
                            new float3(0f, 1f, 0f),
                            currentBlock.GetTextureLayer(Direction.Down),
                            CalculateAO(chunk, x, y, z, Direction.Down)
                        );
                    }
                }
            }
        }
    }

    private static int CalculateAO(Chunk chunk, int x, int y, int z, Direction direction)
    {
        int ao = 0;

        if (direction == Direction.North)
        {
            if (chunk.GetBlockAt(x, y - 1, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y - 1, z + 1) != Block.Void)
                ao |= 0x01;
            if (chunk.GetBlockAt(x, y - 1, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y - 1, z + 1) != Block.Void)
                ao |= 0x02;
            if (chunk.GetBlockAt(x, y + 1, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y + 1, z + 1) != Block.Void)
                ao |= 0x04;
            if (chunk.GetBlockAt(x, y + 1, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y + 1, z + 1) != Block.Void)
                ao |= 0x08;
        }

        if (direction == Direction.East)
        {
            if (chunk.GetBlockAt(x + 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x + 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y - 1, z - 1) != Block.Void)
                ao |= 0x01;
            if (chunk.GetBlockAt(x + 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x + 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y - 1, z + 1) != Block.Void)
                ao |= 0x02;
            if (chunk.GetBlockAt(x + 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x + 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y + 1, z - 1) != Block.Void)
                ao |= 0x04;
            if (chunk.GetBlockAt(x + 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x + 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y + 1, z + 1) != Block.Void)
                ao |= 0x08;
        }

        if (direction == Direction.South)
        {
            if (chunk.GetBlockAt(x, y - 1, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y - 1, z - 1) != Block.Void)
                ao |= 0x01;
            if (chunk.GetBlockAt(x, y - 1, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y - 1, z - 1) != Block.Void)
                ao |= 0x02;
            if (chunk.GetBlockAt(x, y + 1, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y + 1, z - 1) != Block.Void)
                ao |= 0x04;
            if (chunk.GetBlockAt(x, y + 1, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y + 1, z - 1) != Block.Void)
                ao |= 0x08;
        }

        if (direction == Direction.West)
        {
            if (chunk.GetBlockAt(x - 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x - 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y - 1, z + 1) != Block.Void)
                ao |= 0x01;
            if (chunk.GetBlockAt(x - 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x - 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y - 1, z - 1) != Block.Void)
                ao |= 0x02;
            if (chunk.GetBlockAt(x - 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x - 1, y, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y + 1, z + 1) != Block.Void)
                ao |= 0x04;
            if (chunk.GetBlockAt(x - 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x - 1, y, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y + 1, z - 1) != Block.Void)
                ao |= 0x08;
        }

        if (direction == Direction.Up)
        {
            if (chunk.GetBlockAt(x - 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x, y + 1, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y + 1, z + 1) != Block.Void)
                ao |= 0x01;
            if (chunk.GetBlockAt(x - 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x, y + 1, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y + 1, z - 1) != Block.Void)
                ao |= 0x02;
            if (chunk.GetBlockAt(x + 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x, y + 1, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y + 1, z + 1) != Block.Void)
                ao |= 0x04;
            if (chunk.GetBlockAt(x + 1, y + 1, z) != Block.Void || chunk.GetBlockAt(x, y + 1, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y + 1, z - 1) != Block.Void)
                ao |= 0x08;
        }

        if (direction == Direction.Down)
        {
            if (chunk.GetBlockAt(x - 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x, y - 1, z - 1) != Block.Void || chunk.GetBlockAt(x - 1, y - 1, z - 1) != Block.Void)
                ao |= 0x01;
            if (chunk.GetBlockAt(x - 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x, y - 1, z + 1) != Block.Void || chunk.GetBlockAt(x - 1, y - 1, z + 1) != Block.Void)
                ao |= 0x02;
            if (chunk.GetBlockAt(x + 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x, y - 1, z - 1) != Block.Void || chunk.GetBlockAt(x + 1, y - 1, z - 1) != Block.Void)
                ao |= 0x04;
            if (chunk.GetBlockAt(x + 1, y - 1, z) != Block.Void || chunk.GetBlockAt(x, y - 1, z + 1) != Block.Void || chunk.GetBlockAt(x + 1, y - 1, z + 1) != Block.Void)
                ao |= 0x08;
        }

        return ao;
    }
}
