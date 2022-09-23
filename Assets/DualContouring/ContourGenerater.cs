using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DualContouring
{
    public class ContourGenerater : MonoBehaviour
    {
        public Texture3D Field;

        [SerializeField]
        Material mat;

        public void Execute()
        {
            //var cpuMC = new CPUMarchingCubes();
            //cpuMC.Execute(Field, mat);

            var gpuMC = new CPUMarchingCubes();
            gpuMC.Execute(Field, mat);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ContourGenerater))]
    public class ContourGeneraterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Execute"))
            {
                var dc = target as ContourGenerater;
                dc.Execute();
            }
        }
    }
#endif
}