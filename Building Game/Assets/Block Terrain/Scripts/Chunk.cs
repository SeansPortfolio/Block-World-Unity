using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public class Chunk
{
    public const int Size = 64;

    public const int Height = 1;

    public const int Volume = Size * Height * Size;

    public int3 Position { get; private set; }

    public bool IsEmpty { get; private set; }

    public Chunk[] Neighbors;

    public Block[] Blocks;


    public Chunk(int posX, int posY, int posZ)
    {
        Position = new int3(posX, posY, posZ);
        Neighbors = new Chunk[6];
    }

    public Chunk(int3 pos)
    {
        Position = pos;
    }

    public void RefreshNeighbors(WorldManager world)
    {
        if (world.GetChunkAt(Position.x, Position.y, Position.z + Size, out Neighbors[0]))
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

    public bool IsUnderChunk()
    {
        return Neighbors[4] != null && Neighbors[4].IsEmpty == false;
    }

    public void SetBlocks(Block[] blocks, bool isEmpty)
    {
        Blocks = blocks;
        IsEmpty = isEmpty;
    }

    public Block GetBlockAt(int3 pos)
    {
        return GetBlockAt(pos.x, pos.y, pos.z);
    }

    public Block GetBlockAt(int x, int y, int z)
    {
        if (IsPointWithinBounds(x, y, z))
        {
            return Blocks[PointToIndex(x, y, z)];
        }

        if (z >= Size && Neighbors[0] != null)
        {
            return Neighbors[0].GetBlockAt(x, y, z - Size);
        }
        if (x >= Size && Neighbors[1] != null)
        {
            return Neighbors[1].GetBlockAt(x - Size, y, z);
        }
        if (z < 0 && Neighbors[2] != null)
        {
            return Neighbors[2].GetBlockAt(x, y, z + Size);
        }
        if (x < 0 && Neighbors[3] != null)
        {
            return Neighbors[3].GetBlockAt(x + Size, y, z);
        }
        if (y >= Height && Neighbors[4] != null)
        {
            return Neighbors[4].GetBlockAt(x, y - Height, z);
        }
        if (y < 0 && Neighbors[5] != null)
        {
            return Neighbors[5].GetBlockAt(x, y + Height, z);
        }

        return Block.Void;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPointWithinBounds(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 && x < Size && y < Height && z < Size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PointToIndex(int x, int y, int z)
    {
        return x * Size * Height + z * Height + y;
    }
}
