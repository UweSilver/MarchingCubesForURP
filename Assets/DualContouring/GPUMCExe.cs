using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public class GPUMCExe : MonoBehaviour
    {
        IContourGenerater mc;
        void Start()
        {
            mc = new GPUMarchingCubes();
        }

        void Update()
        {
            mc.Execute(new Texture3D(1, 1, 1, TextureFormat.RGB24, false), (Material)Resources.Load("white_lit"));
        }
    }

}