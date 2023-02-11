using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace MyGame.BlockTerrain.Meshing
{
    public static class MeshBuilder
    {
        public static void CreateGreedyMesh(Chunk chunk, MeshData data)
        {



        }

        public static void CreateSimpleMesh(Chunk chunk, MeshData data)
        {
            var blocks = chunk.Blocks;

            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int y = 0; y < Chunk.Height; y++)
                {
                    for (int z = 0; z < Chunk.Size; z++)
                    {
                        var currentBlock = blocks[Chunk.PointToIndex(x, y, z)];
                        if (currentBlock == BlockType.Void)
                        {
                            continue;
                        }

                        //North
                        if (chunk.GetBlockAt(x, y, z + 1) == BlockType.Void)
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
                        if (chunk.GetBlockAt(x + 1, y, z) == BlockType.Void)
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
                        if (chunk.GetBlockAt(x, y, z - 1) == BlockType.Void)
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
                        if (chunk.GetBlockAt(x - 1, y, z) == BlockType.Void)
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
                        if (chunk.GetBlockAt(x, y + 1, z) == BlockType.Void)
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

                        //Down
                        if (chunk.GetBlockAt(x, y - 1, z) == BlockType.Void)
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

            if(direction == Direction.North)
            {
                if (chunk.GetBlockAt(x, y - 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y - 1, z + 1) != BlockType.Void)
                    ao |= 0x01; 
                if (chunk.GetBlockAt(x, y - 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y - 1, z + 1) != BlockType.Void)
                    ao |= 0x02; 
                if (chunk.GetBlockAt(x, y + 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y + 1, z + 1) != BlockType.Void)
                    ao |= 0x04;
                if (chunk.GetBlockAt(x, y + 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y + 1, z + 1) != BlockType.Void)
                    ao |= 0x08;
            }

            if (direction == Direction.East)
            {
                if (chunk.GetBlockAt(x + 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y - 1, z - 1) != BlockType.Void)
                    ao |= 0x01;
                if (chunk.GetBlockAt(x + 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y - 1, z + 1) != BlockType.Void)
                    ao |= 0x02;
                if (chunk.GetBlockAt(x + 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y + 1, z - 1) != BlockType.Void)
                    ao |= 0x04;
                if (chunk.GetBlockAt(x + 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y + 1, z + 1) != BlockType.Void)
                    ao |= 0x08;
            }

            if (direction == Direction.South)
            {
                if (chunk.GetBlockAt(x, y - 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y - 1, z - 1) != BlockType.Void)
                    ao |= 0x01;
                if (chunk.GetBlockAt(x, y - 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y - 1, z - 1) != BlockType.Void)
                    ao |= 0x02;
                if (chunk.GetBlockAt(x, y + 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y + 1, z - 1) != BlockType.Void)
                    ao |= 0x04; 
                if (chunk.GetBlockAt(x, y + 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y + 1, z - 1) != BlockType.Void)
                    ao |= 0x08;
            }

            if (direction == Direction.West)
            {
                if (chunk.GetBlockAt(x - 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y - 1, z + 1) != BlockType.Void)
                    ao |= 0x01;
                if (chunk.GetBlockAt(x - 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y - 1, z - 1) != BlockType.Void)
                    ao |= 0x02;
                if (chunk.GetBlockAt(x - 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y + 1, z + 1) != BlockType.Void)
                    ao |= 0x04;
                if (chunk.GetBlockAt(x - 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x - 1, y, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y + 1, z - 1) != BlockType.Void)
                    ao |= 0x08;
            }

            if (direction == Direction.Up)
            {
                if (chunk.GetBlockAt(x - 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x, y + 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y + 1, z + 1) != BlockType.Void)
                    ao |= 0x01;
                if (chunk.GetBlockAt(x - 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x, y + 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y + 1, z - 1) != BlockType.Void)
                    ao |= 0x02;
                if (chunk.GetBlockAt(x + 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x, y + 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y + 1, z + 1) != BlockType.Void)
                    ao |= 0x04;
                if (chunk.GetBlockAt(x + 1, y + 1, z) != BlockType.Void || chunk.GetBlockAt(x, y + 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y + 1, z - 1) != BlockType.Void)
                    ao |= 0x08;
            }

            if (direction == Direction.Down)
            {
                if (chunk.GetBlockAt(x - 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x, y - 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y - 1, z - 1) != BlockType.Void)
                    ao |= 0x01;
                if (chunk.GetBlockAt(x - 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x, y - 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x - 1, y - 1, z + 1) != BlockType.Void)
                    ao |= 0x02;
                if (chunk.GetBlockAt(x + 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x, y - 1, z - 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y - 1, z - 1) != BlockType.Void)
                    ao |= 0x04;
                if (chunk.GetBlockAt(x + 1, y - 1, z) != BlockType.Void || chunk.GetBlockAt(x, y - 1, z + 1) != BlockType.Void || chunk.GetBlockAt(x + 1, y - 1, z + 1) != BlockType.Void)
                    ao |= 0x08;
            }

            return ao;
        }
    }
}