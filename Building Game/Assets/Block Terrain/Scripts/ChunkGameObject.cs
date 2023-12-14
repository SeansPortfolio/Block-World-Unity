using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ChunkGameObject : MonoBehaviour
{

    public MeshFilter Filter;

    public MeshRenderer Renderer;

    public Mesh ChunkMesh;

    public Mesh ChunkMeshFilledTop;

    public void SetMesh(Mesh chunkMesh, Mesh chunkMeshFilledTop)
    {
        ChunkMesh = chunkMesh;
        ChunkMeshFilledTop = chunkMeshFilledTop;
    }

    public void Refresh(bool useFilledTop)
    {
        Filter.sharedMesh = useFilledTop ? ChunkMeshFilledTop : ChunkMesh;
    }




}
