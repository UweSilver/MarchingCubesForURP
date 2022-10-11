using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DualContouring
{
    public class VoxelMap : MonoBehaviour
    {
        public bool[,,] map;
        public Vector3Int size = new Vector3Int(10, 20, 30);

        [SerializeField]
        GameObject ContourGeneraterObj;

        IVolumeGenerater volumeGenerater;
        ContourGenerater contourGenerater;

        void Start()
        {
            volumeGenerater = ContourGeneraterObj.GetComponent<FieldGenerator>().VolumeGenerater;
            contourGenerater = ContourGeneraterObj.GetComponent<ContourGenerater>();

            //map init
            map = new bool[size.x, size.y, size.z];
            for(int x = 0; x < size.x; x++)
            {
                for(int y = 0; y < size.y; y++)
                {
                    for(int z = 0; z < size.z; z++)
                    {
                        map[x, y, z] = false;
                    }
                }
            }

            addBox(new Vector3Int(0, 0, 0));

            (volumeGenerater as VoxelMapToVolume).voxelMap = this.map;
        }

        void addBox(Vector3 pos)
        {
            Debug.Log("add : " + pos);
            map[(int)pos.x, (int)pos.y, (int)pos.z] = true;
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.parent = this.gameObject.transform;
            box.transform.localPosition = pos;
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var raycastHit = new RaycastHit();
            bool hit = Physics.Raycast(ray, out raycastHit);
            if (!hit) return;
            Debug.Log(raycastHit.transform.position + " : " + raycastHit.normal);
            addBox(raycastHit.transform.localPosition + raycastHit.normal);

            volumeGenerater.Generate(contourGenerater);
            contourGenerater.Execute();
        }
    }
}