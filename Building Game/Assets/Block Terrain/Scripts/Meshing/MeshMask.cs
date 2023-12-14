using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMask
{
    public Block Block;

    public int Normal;

    public int AO;

    public MeshMask(Block block, int direction, int ambientOcclusion)
    {
        Block = block;
        Normal = direction;
        AO = ambientOcclusion;
    }

    public override bool Equals(object obj)
    {
        return obj is MeshMask mask &&
               Block == mask.Block &&
               Normal == mask.Normal &&
               AO == mask.AO;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Block, Normal, AO);
    }

    public static bool operator==(MeshMask lhs, MeshMask rhs)
    {
        return lhs.Block == rhs.Block && lhs.Normal == rhs.Normal && lhs.AO == rhs.AO;
    }

    public static bool operator!=(MeshMask lhs, MeshMask rhs)
    {
        return lhs.Block != rhs.Block || lhs.Normal != rhs.Normal || lhs.AO != rhs.AO;
    }
}
