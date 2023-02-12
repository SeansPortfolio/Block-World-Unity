using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using MyGame.BlockTerrain;

public class WorldLoader : MonoBehaviour
{
    public WorldController World;

    public int RenderDistance = 6;

    private int3 CurrentWorldPosition;

    private int3 PreviousWorldPosition;


    // Start is called before the first frame update
    void Start()
    {
        var position = transform.position;
        CurrentWorldPosition = PreviousWorldPosition = WorldController.WorldToChunkPosition(position.x, position.y, position.z);

        World.GenerateTerrain(CurrentWorldPosition, RenderDistance);
    }

    // Update is called once per frame
    void Update()
    {
        var position = transform.position;
        CurrentWorldPosition = WorldController.WorldToChunkPosition(position.x, position.y, position.z);

        if(!IsPositionEqual(CurrentWorldPosition, PreviousWorldPosition))
        {
            World.GenerateTerrain(CurrentWorldPosition, RenderDistance);
            PreviousWorldPosition = CurrentWorldPosition;
        }
    }


    private bool IsPositionEqual(int3 lhs, int3 rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }
}
