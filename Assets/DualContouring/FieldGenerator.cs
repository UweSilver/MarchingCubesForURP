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

        public IVolumeGenerater VolumeGenerater = new VoxelMapToVolume();
        public void Generate()
        {
            VolumeGenerater.Generate(GetComponent<ContourGenerater>());
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
