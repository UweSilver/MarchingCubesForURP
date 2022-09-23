using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        GPUMarchingCubes()
        {
            marchingCubes = (ComputeShader)Resources.Load("MarchingCubes");
            MCKernel = marchingCubes.FindKernel("CSMain");

            marchingCubes.SetInts("voxelResolution", new int[] { voxelResolution.x, voxelResolution.y, voxelResolution.z});
            marchingCubes.SetFloat("threshold", threshold);

            gpuVertices = new GraphicsBuffer(GraphicsBuffer.Target.Append, (voxelResolution.x - 1) * (voxelResolution.y - 1) * (voxelResolution.z - 1) * 5, sizeof(float) * 18);
        }

        void IContourGenerater.Execute(Texture3D field, Material material)
        {
            Debug.Log("GPUMarchingCubes");

            gpuVertices.SetCounterValue(0);

            marchingCubes.SetTexture(MCKernel, "field", field);

            marchingCubes.Dispatch(MCKernel, voxelResolution.x, voxelResolution.y, voxelResolution.z);

            var root = new GameObject();
        }
    }
}
