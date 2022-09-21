using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public class GPUMarchingCubes : IContourGenerater
    {
        ComputeShader marchingCube;
        GraphicsBuffer gpuVertices;

        void IContourGenerater.Execute(Texture3D field, Material material)
        {
            Debug.Log("GPUMarchingCubes");

            var root = new GameObject();
        }
    }
}
