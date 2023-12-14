using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ThreadManager : MonoBehaviour
{
    public bool IsBlockBuilderReady { get { return BlockBuilder == null; } }

    public bool IsMeshBuilderReady { get { return MeshBuilder == null; } }

    private BlockBuilderJob BlockBuilder;

    private System.Action<BlockBuilderJob> OnBlockBuilderComplete;

    private MeshBuilderJob MeshBuilder;

    private System.Action<MeshBuilderJob> OnMeshBuilderComplete;

    private void Start()
    {

    }


    private void Update()
    {
        if(BlockBuilder != null && BlockBuilder.Update())
        {
            OnBlockBuilderComplete(BlockBuilder);
            BlockBuilder = null;
        }

        if (MeshBuilder != null && MeshBuilder.Update())
        {
            OnMeshBuilderComplete(MeshBuilder);
            MeshBuilderJob.ReturnMeshData(MeshBuilder.MeshDatas);
            MeshBuilderJob.ReturnMeshData(MeshBuilder.MeshDatasFilledTops);
            MeshBuilder = null;
        }
    }

    public void CreateBlockBuilderJob(IEnumerable<Chunk> chunksToBuild)
    {
        if(IsBlockBuilderReady)
        {
            BlockBuilder = new BlockBuilderJob();
            BlockBuilder.ChunksToBuild = new List<Chunk>(chunksToBuild);
            BlockBuilder.Start();
        }
    }

    public void CreateMeshBuilderJob(IEnumerable<Chunk> chunksToBuild)
    {
        if(IsMeshBuilderReady)
        {
            MeshBuilder = new MeshBuilderJob();
            MeshBuilder.ChunksToBuild = new List<Chunk>(chunksToBuild);
            MeshBuilder.Start();
        }
    }

    public void RegisterBlockBuilderComplete(System.Action<BlockBuilderJob> onComplete) 
    { 
        OnBlockBuilderComplete += onComplete; 
    }

    public void RegisterMeshBuilderComplete(System.Action<MeshBuilderJob> onComplete) 
    { 
        OnMeshBuilderComplete += onComplete; 
    }

    public void UnregisterBlockBuilderComplete(System.Action<BlockBuilderJob> onComplete) 
    { 
        OnBlockBuilderComplete -= onComplete; 
    }

    public void UnregisterMeshBuilderComplete(System.Action<MeshBuilderJob> onComplete) 
    { 
        OnMeshBuilderComplete -= onComplete; 
    }

}
