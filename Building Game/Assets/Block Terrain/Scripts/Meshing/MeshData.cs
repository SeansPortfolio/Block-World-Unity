using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;


public class MeshData
{
    public NativeArray<float3> Vertices;

    public NativeArray<float3> Normals;

    public NativeArray<float3> UVs;

    public NativeArray<Color32> AO;

    public NativeArray<int> Triangles;

    public int VertexCount;

    public int TriangleCount0;

    private static readonly Color32 Light = new Color32(255, 255, 255, 255);

    private static readonly Color32 Dark = new Color32(40, 40, 40, 255);

    public void Initialize(int maxVertices)
    {
        Vertices = new NativeArray<float3>(maxVertices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        Normals = new NativeArray<float3>(maxVertices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        UVs = new NativeArray<float3>(maxVertices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        AO = new NativeArray<Color32>(maxVertices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        Triangles = new NativeArray<int>(maxVertices * 3 / 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        VertexCount = TriangleCount0 = 0;
    }

    public void Reset()
    {
        VertexCount = TriangleCount0 = 0;
    }

    public void Dispose()
    {
        Vertices.Dispose();
        Normals.Dispose();
        UVs.Dispose();
        AO.Dispose();
        Triangles.Dispose();

        VertexCount = TriangleCount0 = 0;
    }

    public void AddQuad(float3 v1, float3 v2, float3 v3, float3 v4, float3 normal, int uvLayer, int ao)
    {
        Vertices[VertexCount] = v1;
        Vertices[VertexCount + 1] = v2;
        Vertices[VertexCount + 2] = v3;
        Vertices[VertexCount + 3] = v4;

        Normals[VertexCount] = normal;
        Normals[VertexCount + 1] = normal;
        Normals[VertexCount + 2] = normal;
        Normals[VertexCount + 3] = normal;

        UVs[VertexCount] = new float3(0f, 0f, uvLayer);
        UVs[VertexCount + 1] = new float3(1f, 0f, uvLayer);
        UVs[VertexCount + 2] = new float3(0f, 1f, uvLayer);
        UVs[VertexCount + 3] = new float3(1f, 1f, uvLayer);

        AO[VertexCount] = (ao & 0x01) == 0x01 ? Dark : Light;
        AO[VertexCount + 1] = (ao & 0x02) == 0x02 ? Dark : Light;
        AO[VertexCount + 2] = (ao & 0x04) == 0x04 ? Dark : Light;
        AO[VertexCount + 3] = (ao & 0x08) == 0x08 ? Dark : Light;

        var flipTriangles = ao == 0x02 || ao == 0x04 || ao == 0x9 || ao == 0xB || ao == 0xD;
        if (!flipTriangles)
        {
            Triangles[TriangleCount0] = VertexCount;
            Triangles[TriangleCount0 + 1] = VertexCount + 2;
            Triangles[TriangleCount0 + 2] = VertexCount + 1;
            Triangles[TriangleCount0 + 3] = VertexCount + 2;
            Triangles[TriangleCount0 + 4] = VertexCount + 3;
            Triangles[TriangleCount0 + 5] = VertexCount + 1;
        }
        else
        {
            Triangles[TriangleCount0] = VertexCount;
            Triangles[TriangleCount0 + 1] = VertexCount + 3;
            Triangles[TriangleCount0 + 2] = VertexCount + 1;
            Triangles[TriangleCount0 + 3] = VertexCount;
            Triangles[TriangleCount0 + 4] = VertexCount + 2;
            Triangles[TriangleCount0 + 5] = VertexCount + 3;
        }

        VertexCount += 4;
        TriangleCount0 += 6;
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = VertexCount > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;

        mesh.SetVertices(Vertices, 0, VertexCount);
        mesh.SetNormals(Normals, 0, mesh.vertexCount);
        mesh.SetUVs(0, UVs, 0, mesh.vertexCount);
        mesh.SetColors(AO, 0, mesh.vertexCount);

        mesh.SetIndices(Triangles, 0, mesh.vertexCount * 3 / 2, MeshTopology.Triangles, 0);

        mesh.RecalculateTangents();

        return mesh;
    }
}



