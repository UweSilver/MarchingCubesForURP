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

        Vector3Int voxelResolution = new Vector3Int(2, 3, 4);
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

            var voxelCount = voxelResolution.x * voxelResolution.y * voxelResolution.z;
            var maxVertexCount = voxelCount * 5; //1Ç¬ÇÃvoxelì‡ÇÃç≈ëÂÇÃí∏ì_êîÇÕ5
            var maxIndexCount = voxelCount * 3 * 3; //ç≈ëÂÇÃí∏ì_êî5->ç≈ëÂÇÃÉ|ÉäÉSÉìêîÇÕ3->ç≈ëÂÇÃindexêî3x3

            var vertices = new NativeArray<Vector3>(maxVertexCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            var indices = new int[maxIndexCount];

            mesh.SetVertices(vertices);
            vertices.Dispose();

            mesh.triangles = indices;

            gpuVertices = mesh.GetVertexBuffer(0);
            gpuIndices = mesh.GetIndexBuffer();

            meshFilter.mesh = mesh;
        }

        void IContourGenerater.Execute(Texture3D field, Material material)
        {
            //Debug.Log("GPUMarchingCubes");

            //gpuVertices.SetCounterValue(0);

            marchingCubes.SetTexture(MCKernel, "field", field);
            marchingCubes.SetFloats("voxelResolution", new float[3] { voxelResolution.x, voxelResolution.y, voxelResolution.z });

            marchingCubes.SetBuffer(0, "VertBuffer", gpuVertices);
            marchingCubes.SetBuffer(0, "IdxBuffer", gpuIndices);

            marchingCubes.Dispatch(MCKernel, voxelResolution.x, voxelResolution.y, voxelResolution.z);

            //marchingCubes.Dispatch(MCKernel, 1, 1, 1);

            meshRenderer.material = material;
        }
    }
}
