using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class BlockExtensions
{
    private static Dictionary<string, int> TextureLayerMap;

    public static void AddTextureReference(string name, int index)
    {
        if(TextureLayerMap == null)
        {
            TextureLayerMap = new Dictionary<string, int>();
        }

        if(TextureLayerMap.ContainsKey(name))
        {
            Debug.LogError("Texture with the name " + name + " already exists!");
            return;
        }

        TextureLayerMap.Add(name, index);
    }

    public static int GetFilledTopTextureLayer(this Block block)
    {
        return TextureLayerMap["Filled_Top"];
    }

    public static int GetTextureLayer(this Block block, Direction direction)
    {
        switch(block)
        {
            case Block.Stone:
                return TextureLayerMap["Stone_Brick"];
            case Block.Dirt:
                return TextureLayerMap["Dirt"];
            case Block.Grass:
                if (direction == Direction.Up)
                {
                    return TextureLayerMap["Grass_Top"];
                }

                if (direction == Direction.Down)
                {
                    return TextureLayerMap["Dirt"];
                }

                return TextureLayerMap["Grass_Side"];

            default:
                return TextureLayerMap["Missing"];
        }
    }
}

