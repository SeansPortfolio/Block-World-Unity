using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace MyGame.BlockTerrain
{
    public class Chunk
    {
        public const int Size = 16;

        public const int Height = 16;

        public BlockType[] Blocks;

        public Chunk[] Neighbors;

        public int3 Position;

        public Chunk(int3 position)
        {
            Position = position;
            Neighbors = new Chunk[6];
        }

        public void GenerateBlocks()
        {
            Blocks = new BlockType[Size * Height * Size];

            for (int x = 0; x < Size; x++)
            {
                for (int z = 0; z < Size; z++)
                {
                    float height = Mathf.PerlinNoise((x + Position.x) / 64f, (z + Position.z) / 60f) * 16f +
                        Mathf.PerlinNoise((x + Position.x) / 36f, (z + Position.z) / 30f) * 8f +
                        Mathf.PerlinNoise((x + Position.x) / 16f, (z + Position.z) / 25f) * 4f +
                        Mathf.PerlinNoise((x + Position.x) / 8f, (z + Position.z) / 8f) * 2f
                        + 96f;

                    for (int y = 0; y < Height; y++)
                    {
                        if(y + Position.y < height)
                        {
                            Blocks[PointToIndex(x, y, z)] = BlockType.Filled;
                        }
                        else
                        {
                            Blocks[PointToIndex(x, y, z)] = BlockType.Void;
                        }
                    }
                }
            }
        }

        public void RefreshNeighbors(WorldController world)
        {
            if(world.GetChunkAt(Position.x, Position.y, Position.z + Size, out Neighbors[0]))
            {
                Neighbors[0].Neighbors[2] = this;
            }
            if (world.GetChunkAt(Position.x + Size, Position.y, Position.z, out Neighbors[1]))
            {
                Neighbors[1].Neighbors[3] = this;
            }
            if (world.GetChunkAt(Position.x, Position.y, Position.z - Size, out Neighbors[2]))
            {
                Neighbors[2].Neighbors[0] = this;
            }
            if (world.GetChunkAt(Position.x - Size, Position.y, Position.z, out Neighbors[3]))
            {
                Neighbors[3].Neighbors[1] = this;
            }
            if (world.GetChunkAt(Position.x, Position.y + Height, Position.z, out Neighbors[4]))
            {
                Neighbors[4].Neighbors[5] = this;
            }
            if (world.GetChunkAt(Position.x, Position.y - Height, Position.z, out Neighbors[5]))
            {
                Neighbors[5].Neighbors[4] = this;
            }

        }

        public BlockType GetBlockAt(int x, int y, int z)
        {
            if(IsPointWithinBounds(x, y, z))
            {
                return Blocks[PointToIndex(x, y, z)];
            }

            if(z >= Size && Neighbors[0] != null)
            {
                return Neighbors[0].GetBlockAt(x, y, z - Size);
            }
            if(x >= Size && Neighbors[1] != null)
            {
                return Neighbors[1].GetBlockAt(x - Size, y, z);
            }
            if(z < 0 && Neighbors[2] != null)
            {
                return Neighbors[2].GetBlockAt(x, y, z + Size);
            }
            if(x < 0 && Neighbors[3] != null)
            {
                return Neighbors[3].GetBlockAt(x + Size, y, z);
            }
            if(y >= Height && Neighbors[4] != null)
            {
                return Neighbors[4].GetBlockAt(x, y - Height, z);
            }
            if(y < 0 && Neighbors[5] != null)
            {
                return Neighbors[5].GetBlockAt(x, y + Height, z);
            }

            return BlockType.Void;
        }

        public static bool IsPointWithinBounds(int x, int y, int z)
        {
            return x >= 0 && y >= 0 && z >= 0 && x < Size && y < Height && z < Size;
        }

        public static int PointToIndex(int x, int y, int z)
        {
            return x * Size * Height + y * Height + z;
        }
    }
}

