using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BlockBuilderJob : ThreadedProcess
{
    public List<Chunk> ChunksToBuild;

    public const int MaxChunksToBuild = 1024;

    protected override void ThreadFunction()
    {
        foreach(var chunk in ChunksToBuild)
        {
            var pos = chunk.Position;

            var chunkData = CreateChunkBlocks(pos.x, pos.y, pos.z);
            chunk.SetBlocks(chunkData.Item1, chunkData.Item2);
        }
    }

    protected override void OnFinished()
    {

    }

    private (Block[], bool) CreateChunkBlocks(int chunkX, int chunkY, int chunkZ)
    {
        Block[] blocks = new Block[Chunk.Volume];
        bool isEmpty = true;

        var idx = -1;
        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int z = 0; z < Chunk.Size; z++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    idx++;

                    var block = TerrainGenerator.GetBlockAt(x + chunkX, y + chunkY, z + chunkZ);
                    blocks[idx] = block;

                    if(block != Block.Void)
                    {
                        isEmpty = false;
                    }
                }
            }
        }

        return (blocks, isEmpty);
    }
}
