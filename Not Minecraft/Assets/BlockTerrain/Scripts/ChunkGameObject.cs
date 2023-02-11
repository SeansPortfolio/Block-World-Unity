using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.BlockTerrain
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    public class ChunkGameObject : MonoBehaviour
    {
        public MeshRenderer Renderer;

        public MeshFilter Filter;

        public MeshCollider Collider;

        public void SetMesh(Mesh mesh)
        {
            Filter.mesh = mesh;
        }
    }
}