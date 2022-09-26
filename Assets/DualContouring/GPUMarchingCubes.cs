using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.IO;

namespace DualContouring
{
    public class GPUMarchingCubes : IContourGenerater
    {
        ComputeShader marchingCubes;
        int MCKernel;

        GraphicsBuffer gpuVertices;
        GraphicsBuffer gpuIndices;

        Vector3Int voxelResolution = new Vector3Int(10, 10, 10);
        float threshold = 0.5f;

        //mesh obj
        GameObject root;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        Mesh mesh;


        public GPUMarchingCubes()
        {
            marchingCubes = (ComputeShader)Resources.Load("MarchingCubes");
            MCKernel = marchingCubes.FindKernel("CSMain");

            marchingCubes.SetInts("voxelResolution", new int[] { voxelResolution.x, voxelResolution.y, voxelResolution.z });
            marchingCubes.SetFloat("threshold", threshold);

            root = new GameObject();
            meshRenderer = root.AddComponent<MeshRenderer>();
            meshFilter = root.AddComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
            mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

            var vertices = new NativeArray<Vector3>(9, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            var normals = new NativeArray<Vector3>(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(0, 0, 0);
            vertices[2] = new Vector3(0, 0, 0);

            normals[0] = new Vector3(0, 0, 0);
            normals[1] = new Vector3(0, 0, 0);
            normals[2] = new Vector3(0, 0, 0);

            var indices = new int[3] { 0, 1, 2 };

            mesh.SetVertices(vertices);
            //mesh.SetNormals(normals);
            vertices.Dispose();
            normals.Dispose();

            mesh.triangles = indices;

            gpuVertices = mesh.GetVertexBuffer(0);
            gpuIndices = mesh.GetIndexBuffer();

            meshFilter.mesh = mesh;
        }

        void IContourGenerater.Execute(Texture3D field, Material material)
        {
            Debug.Log("GPUMarchingCubes");

            //gpuVertices.SetCounterValue(0);

            marchingCubes.SetTexture(MCKernel, "field", field);

            marchingCubes.SetBuffer(0, "VertBuffer", gpuVertices);
            marchingCubes.SetBuffer(0, "IdxBuffer", gpuIndices);

            //marchingCubes.Dispatch(MCKernel, voxelResolution.x, voxelResolution.y, voxelResolution.z);

            marchingCubes.Dispatch(MCKernel, 1, 1, 1);

            meshRenderer.material = material;
        }
    }
}
