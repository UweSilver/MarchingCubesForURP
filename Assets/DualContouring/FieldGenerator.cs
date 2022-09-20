using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DualContouring
{
    public class FieldGenerator : MonoBehaviour
    {
        DualContouring dualContouring;

        Texture3D Field;

        void OnEnable()
        {
            
        }

        public void Generate()
        {
            Debug.Log("Generate");

            int resolution = 16;
            Field = new Texture3D(resolution, resolution, resolution, TextureFormat.RGBA32, false);

            //var data = new Color[resolution * resolution * resolution];
            int idx = 0;
            for(var x = 0; x < resolution; x++)
            {
                for(var y = 0; y < resolution; y++)
                {
                    for(var z = 0; z < resolution; z++, idx++)
                    {
                        var val = new Vector3(x - resolution / 2, y - resolution / 2, z - resolution / 2).magnitude / (resolution * 0.8f);
                        Field.SetPixel(x, y, z, new Color(val, 0, 0, 0));
                    }
                }
            }

            //Field.SetPixels(data);
            
            dualContouring = GetComponent<DualContouring>();
            dualContouring.Field = this.Field;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FieldGenerator))]
    public class FieldGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }

        void Generate()
        {
            var generator = (target as FieldGenerator);

            generator.Generate();
        }
    }
#endif
}
