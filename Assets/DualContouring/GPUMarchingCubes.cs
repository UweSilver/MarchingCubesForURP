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

        Vector3Int voxelResolution;
        float threshold;

        //mesh obj
        GameObject root;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        Mesh mesh;

        public GPUMarchingCubes()
        {
            marchingCubes = (ComputeShader)Resources.Load("MarchingCubes");
            MCKernel = marchingCubes.FindKernel("CSMain");

            marchingCubes.SetInts("voxelResolution", new int[] { voxelResolution.x, voxelResolution.y, voxelResolution.z});
            marchingCubes.SetFloat("threshold", threshold);

            root = new GameObject();
            meshRenderer = root.AddComponent<MeshRenderer>();
            meshFilter = root.AddComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Append;

            var vertices = new NativeArray<Vector3>((voxelResolution.x - 1) * (voxelResolution.y - 1) * (voxelResolution.z - 1) * sizeof(float) * 18, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            mesh.SetVertices(vertices);

            gpuVertices = mesh.GetVertexBuffer(0);

            meshFilter.mesh = new Mesh();
        }

        void IContourGenerater.Execute(Texture3D field, Material material)
        {
            Debug.Log("GPUMarchingCubes");

            gpuVertices.SetCounterValue(0);

            marchingCubes.SetTexture(MCKernel, "field", field);

            marchingCubes.Dispatch(MCKernel, voxelResolution.x, voxelResolution.y, voxelResolution.z);

            meshRenderer.material = material;
        }
    }
}
