using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGenerator
{

    public static FastNoiseLite Noise = new FastNoiseLite();

    private static int SurfaceHeight = 25;

    private static float NoiseMultiplier = 12f;


    public static void SetSeed(int seed)
    {
        Noise.SetSeed(seed);
    }

    public static Block GetBlockAt(int x, int y, int z)
    {
        var noiseValue = Noise.GetNoise(x, z) + 0.5f;
        var surfaceY = SurfaceHeight + noiseValue * NoiseMultiplier;
        surfaceY = 20;
        surfaceY = Mathf.CeilToInt(surfaceY);

        if(y == surfaceY)
        {
            return Block.Grass;
        }

        if(y < surfaceY && y > surfaceY - 3)
        {
            return Block.Dirt;
        }
        if(y == 0 || y <= surfaceY - 3)
        {
            return Block.Stone;
        }

        return Block.Void;
    }
}
