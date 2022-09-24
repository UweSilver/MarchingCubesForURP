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

        enum ContouringType
        {
            CPUMC,GPUMC,
        }
        [SerializeField]
        ContouringType contouringType;

        [SerializeField]
        Material mat;

        public void Execute()
        {
            IContourGenerater cg = contouringType switch
            {
                ContouringType.CPUMC => new CPUMarchingCubes(),
                ContouringType.GPUMC => new GPUMarchingCubes(),
                _=> throw new System.Exception("contouring type is not valid")
            };
            cg?.Execute(Field, mat);
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