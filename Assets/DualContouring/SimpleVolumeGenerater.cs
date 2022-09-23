using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public class SimpleVolumeGenerater : IVolumeGenerater
    {
        ContourGenerater contourGenerater;
        Texture3D field;

        public fieldType Type = fieldType.sin_wave;

        public enum fieldType
        {
            double_sphere,
            plane,
            small_sphere,
            sin_wave
        }

        void IVolumeGenerater.Generate(ContourGenerater contourGenerater)
        {
            Debug.Log("Generate");

            this.contourGenerater = contourGenerater;

            int resolution = 128;
            field = new Texture3D(resolution, resolution, resolution, TextureFormat.RGBA32, false);

            //var data = new Color[resolution * resolution * resolution];
            int idx = 0;
            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    for (var z = 0; z < resolution; z++, idx++)
                    {
                        float distanceToPosition(Vector3 pos)
                        {
                            return (new Vector3(x, y, z) - pos).magnitude;
                        }
                        var distanceToCenter = distanceToPosition(resolution / 2f * new Vector3(1, 1, 1));
                        var distanceToSphere = distanceToPosition(resolution / 4f * new Vector3(1, 1, 1));

                        switch (Type)
                        {
                            case fieldType.double_sphere:
                                field.SetPixel(x, y, z, new Color(Mathf.Min(distanceToCenter * 1.01f, distanceToSphere * 2f) / resolution, 0, 0, 0));
                                break;
                            case fieldType.small_sphere:
                                field.SetPixel(x, y, z, new Color(distanceToSphere * 4f / resolution, 0, 0, 0));
                                break;
                            case fieldType.plane:
                                field.SetPixel(x, y, z, new Color((float)y / resolution, 0, 0, 0));
                                break;
                            case fieldType.sin_wave:
                                field.SetPixel(x, y, z, new Color((Mathf.Sin((float)x / resolution * 2f * Mathf.PI) * 2f + (float)y) / resolution, 0, 0, 0));
                                break;
                        }

                    }
                }
            }

            contourGenerater.Field = this.field;
        }
    }
}
