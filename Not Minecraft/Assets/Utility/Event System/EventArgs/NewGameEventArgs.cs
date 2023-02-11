using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Events
{
    public class NewGameEventArgs : System.EventArgs
    {
        public BlockTerrain.WorldConfig Configs;

        public NewGameEventArgs(BlockTerrain.WorldConfig config)
        {
            Configs = config;
        }
    }
}

