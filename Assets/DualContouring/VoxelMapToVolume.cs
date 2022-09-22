using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public class VoxelMapToVolume : IVolumeGenerater
    {
        public bool[,,] voxelMap;
        public Vector3Int voxelMapSize;
        Texture3D field;

        int resolution = 128;
        void IVolumeGenerater.Generate(ContourGenerater contourGenerater)
        {
            Debug.Log("VoxelMapToVolume, generate");


            field = new Texture3D(resolution, resolution, resolution, TextureFormat.RGBA32, false);

            int idx = 0;
            for(var x = 0; x < resolution; x++)
            {
                for(var y = 0;y < resolution; y++)
                {
                    for(var z = 0; z < resolution; z++, idx++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var value = (insideCubes(pos)) ? 1 : 0;
                        //var value = (float)y / resolution;
                        field.SetPixel(x, y, z, new Color(value, 0, 0, 0));
                    }
                }
            }

            contourGenerater.Field = this.field;
        }

        bool insideCubes(Vector3Int texpos)
        {
            var address = new Vector3Int(Mathf.FloorToInt((float)texpos.x / (float)resolution * 20f), Mathf.FloorToInt((float)texpos.y / (float)resolution * 20f), Mathf.FloorToInt((float)texpos.z / (float)resolution * 20f));
            //Debug.Log(address);
            var val = voxelMap[address.x, address.y, address.z];

            return val;
        }
    }
}
