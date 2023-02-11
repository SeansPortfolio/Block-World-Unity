using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.BlockTerrain
{
    using Meshing;

    public static class BlockExtensions
    {
        public static int GetTextureLayer(this BlockType block, Direction direction)
        {
            if(block == BlockType.Stone)
            {
                return 0;
            }
            if(block == BlockType.Dirt)
            {
                return 1;
            }
            if(block == BlockType.Grass)
            {
                if(direction == Direction.Up)
                {
                    return 3;
                }
                else if(direction == Direction.Down)
                {
                    return 1;
                }
                return 2;
            }

            return 0;
        }
    }
}
