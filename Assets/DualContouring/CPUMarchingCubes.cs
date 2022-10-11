using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace DualContouring
{
    public class CPUMarchingCubes : IContourGenerater, System.IDisposable
    {
        int resolution = 64;

        private JobHandle jobHandle;
        
        GameObject root = new();
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        Mesh mesh = new();

        int voxelCount;

        NativeArray<Vector3Int> unitCubePositions;
        NativeArray<VertexVolumeData> vertexVolumeData;
        NativeArray<UnitCubeVertexArray> vertices;
        NativeArray<UnitCubeIndexArray> indices;
        NativeArray<UnitCube> unitCubes;

        NativeArray<UnitCubeIndexArray> MCLUT;

        //外部で初期化 disposeはこっち
        public NativeArray<float> field;
        public int fieldRes;

        MCJob mcJob;

        public CPUMarchingCubes()
        {
            Debug.Log("CPUMarchingCubes");

            meshRenderer = root.AddComponent<MeshRenderer>();
            meshFilter = root.AddComponent<MeshFilter>();

            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
            mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

            voxelCount = Mathf.FloorToInt(Mathf.Pow(resolution, 3));

            unitCubePositions = new NativeArray<Vector3Int>(voxelCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            vertexVolumeData = new NativeArray<VertexVolumeData>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            vertices = new NativeArray<UnitCubeVertexArray>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            indices = new NativeArray<UnitCubeIndexArray>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            unitCubes = new NativeArray<UnitCube>(voxelCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            MCLUT = new NativeArray<UnitCubeIndexArray>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            for (int i = 0; i < MCLUT.Length; i++)
                MCLUT[i] = UnitCubeUtils.triTableIndexArray[i];

            SetParameters((address, idx) => {

                var intPosition = address;
                unitCubePositions[idx] = intPosition;
                return true; });

            for (int i = 0; i < voxelCount; i++)
            {
                unitCubes[i] = new UnitCube(
                    i,
                    unitCubePositions[i],
                    new Vector3(1, 1, 1),
                    new Vector3Int(resolution, resolution, resolution)
                );
            }

            mcJob = new MCJob
            {
                vertexVolumeData = vertexVolumeData,
                threshold = 0.5f,
                unitCubes = unitCubes,
                vertices = vertices,
                indices = indices,
                triTable = MCLUT,
                arrayField = field,
                fieldResolution = fieldRes,
            };
        }

        public void Dispose()
        {
            vertexVolumeData.Dispose();
            unitCubePositions.Dispose();

            unitCubes.Dispose();
            vertices.Dispose();
            indices.Dispose();

            MCLUT.Dispose();

            field.Dispose();
        }

        void SetParameters(System.Func<Vector3Int, int, bool> func)
        {
            int idx = 0;
            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    for (var z = 0; z < resolution; z++, idx++)
                    {
                        func(new Vector3Int(x, y, z), idx);
                    }
                }
            }
        }

        public void Execute(Texture3D field, Material material) 
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Reset();
            timer.Start();

            meshRenderer.material = material;

            //reflesh parameter
            bool setParameters(Vector3Int address, int index)
            {
                var position = new Vector3(address.x, address.y, address.z);

                float readField(Vector3 pos)
                {
                    var value = field.GetPixelBilinear(pos.x, pos.y, pos.z, 0).r;
                    return value;
                }

                var data = new float[8];
                data[0] = readField((position + new Vector3(0, 0, 0)) / resolution);
                data[1] = readField((position + new Vector3(1, 0, 0)) / resolution);
                data[2] = readField((position + new Vector3(1, 0, 1)) / resolution);
                data[3] = readField((position + new Vector3(0, 0, 1)) / resolution);
                data[4] = readField((position + new Vector3(0, 1, 0)) / resolution);
                data[5] = readField((position + new Vector3(1, 1, 0)) / resolution);
                data[6] = readField((position + new Vector3(1, 1, 1)) / resolution);
                data[7] = readField((position + new Vector3(0, 1, 1)) / resolution);

                var vertexVolume = new VertexVolumeData(data);
                vertexVolumeData[index] = vertexVolume;

                return true;
            }
            SetParameters(setParameters);

            timer.Stop();
            Debug.Log("parameters : " + timer.ElapsedMilliseconds);
            timer.Restart();

            //job
            jobHandle = mcJob.Schedule(voxelCount, voxelCount);

            jobHandle.Complete();

            timer.Stop();
            Debug.Log("jobs : " + timer.ElapsedMilliseconds);
            timer.Restart();

            var verticesVector3 = vertices.Reinterpret<Vector3>(System.Runtime.InteropServices.Marshal.SizeOf(new UnitCubeVertexArray()));
            mesh.SetVertices(verticesVector3);

            var indicesInt = indices.Reinterpret<int>(System.Runtime.InteropServices.Marshal.SizeOf(new UnitCubeIndexArray()));
            mesh.SetIndices(indicesInt, MeshTopology.Triangles, 0);
            meshFilter.mesh = mesh;

            timer.Stop();
            Debug.Log("mesh : " + timer.ElapsedMilliseconds);
        }

        public void Execute(Material material)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Reset();
            timer.Start();

            meshRenderer.material = material;


            timer.Stop();
            Debug.Log("parameters : " + timer.ElapsedMilliseconds);
            timer.Restart();

            //job
            jobHandle = mcJob.Schedule(voxelCount, voxelCount);

            jobHandle.Complete();

            timer.Stop();
            Debug.Log("jobs : " + timer.ElapsedMilliseconds);
            timer.Restart();

            var verticesVector3 = vertices.Reinterpret<Vector3>(System.Runtime.InteropServices.Marshal.SizeOf(new UnitCubeVertexArray()));
            mesh.SetVertices(verticesVector3);

            var indicesInt = indices.Reinterpret<int>(System.Runtime.InteropServices.Marshal.SizeOf(new UnitCubeIndexArray()));
            mesh.SetIndices(indicesInt, MeshTopology.Triangles, 0);
            meshFilter.mesh = mesh;

            timer.Stop();
            Debug.Log("mesh : " + timer.ElapsedMilliseconds);
        }
    }
    
    public struct VertexVolumeData
    {
        float v0, v1, v2, v3, v4, v5, v6, v7;
        public VertexVolumeData(float[] volumeData)
        {
            v0 = volumeData[0];
            v1 = volumeData[1];
            v2 = volumeData[2];
            v3 = volumeData[3];
            v4 = volumeData[4];
            v5 = volumeData[5];
            v6 = volumeData[6];
            v7 = volumeData[7];
        }

        public float this[int i]
        {
            get {
                return i switch
                {
                    0 => v0,
                    1 => v1,
                    2 => v2,
                    3 => v3,
                    4 => v4,
                    5 => v5,
                    6 => v6,
                    7 => v7,
                    _ => throw new System.Exception()
                };
            }
        }

        public bool HasDiff(VertexVolumeData newData)
        {
            for (int i = 0; i < 8; i++)
                if (this[i] != newData[i]) return true;

            return false;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnitCubeVertexArray
    {
        Vector3 v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11;

        public void set(Vector3[] data)
        {
            v0 = data[0];
            v1 = data[1];
            v2 = data[2];
            v3 = data[3];
            v4 = data[4];
            v5 = data[5];
            v6 = data[6];
            v7 = data[7];
            v8 = data[8];
            v9 = data[9];
            v10 = data[10];
            v11 = data[11];
        }

        public Vector3 this[int i]
        {
            get
            {
                return i switch
                {
                    0 => v0,
                    1 => v1,
                    2 => v2,
                    3 => v3,
                    4 => v4,
                    5 => v5,
                    6 => v6,
                    7 => v7,
                    8 => v8,
                    9 => v9,
                    10 => v10,
                    11 => v11,
                    _ => throw new System.Exception()
                };
            }
            set
            {
                switch (i)
                {
                    case 0: v0 = value;break;
                    case 1: v1 = value;break;
                    case 2: v2 = value;break;
                    case 3: v3 = value;break;
                    case 4: v4 = value;break;
                    case 5: v5 = value; break;
                    case 6: v6 = value; break;
                    case 7: v7 = value; break;
                    case 8: v8 = value; break;
                    case 9: v9 = value; break;
                    case 10: v10 = value; break;
                    case 11: v11 = value; break;
                    default: throw new System.Exception();
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnitCubeIndexArray
    {
        int i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14;

        public UnitCubeIndexArray(
            int i0 = 0, 
            int i1 = 0, 
            int i2 = 0, 
            int i3 = 0,
            int i4 = 0,
            int i5 = 0,
            int i6 = 0,
            int i7 = 0,
            int i8 = 0,
            int i9 = 0,
            int i10 = 0,
            int i11 = 0,
            int i12 = 0,
            int i13 = 0,
            int i14 = 0,
            int i15 = 0 //捨てる
        )
        {
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
            this.i4 = i4;
            this.i5 = i5;
            this.i6 = i6;
            this.i7 = i7;
            this.i8 = i8;
            this.i9 = i9;
            this.i10 = i10;
            this.i11 = i11;
            this.i12 = i12;
            this.i13 = i13;
            this.i14 = i14;
        }

        public void set(int[] data)
        {
            i0 = data[0];
            i1 = data[1];
            i2 = data[2];
            i3 = data[3];
            i4 = data[4];
            i5 = data[5];
            i6 = data[6];
            i7 = data[7];
            i8 = data[8];
            i9 = data[9];
            i10 = data[10];
            i11 = data[11];
            i12 = data[12];
            i13 = data[13];
            i14 = data[14];
        }

        public int this[int i]
        {
            get
            {
                return i switch
                {
                    0 => i0,
                    1 => i1,
                    2 => i2,
                    3 => i3,
                    4 => i4,
                    5 => i5,
                    6 => i6,
                    7 => i7,
                    8 => i8,
                    9 => i9,
                    10 => i10,
                    11 => i11,
                    12 => i12,
                    13 => i13,
                    14 => i14,
                    _ => throw new System.Exception()
                };
            }
            set
            {
                switch(i)
                {
                    case 0: i0 = value; break;
                    case 1: i1 = value; break;
                    case 2: i2 = value; break;
                    case 3: i3 = value; break;
                    case 4: i4 = value; break;
                    case 5: i5 = value; break;
                    case 6: i6 = value; break;
                    case 7: i7 = value; break;
                    case 8: i8 = value; break;
                    case 9: i9 = value; break;
                    case 10: i10 = value; break;
                    case 11: i11 = value; break;
                    case 12: i12 = value; break;
                    case 13: i13 = value; break;
                    case 14: i14 = value; break;
                    default: throw new System.Exception();
                };
            }
        }
    }

    [BurstCompile]
    public struct MCJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<VertexVolumeData> vertexVolumeData;
        [ReadOnly]
        public float threshold;

        [ReadOnly]
        public NativeArray<UnitCube> unitCubes;
        //このNativeArrayを[ReadOnly]にすることで、このJobのindexと異なる要素にアクセスできるようになる

        [ReadOnly]
        public NativeArray<UnitCubeIndexArray> triTable;

        [ReadOnly]
        public NativeArray<float> arrayField;
        [ReadOnly]
        public int fieldResolution;

        public NativeArray<UnitCubeVertexArray> vertices;
        public NativeArray<UnitCubeIndexArray> indices;
        
        public void Execute(int index)
        {
            VertexVolumeData vertVolData;

            var uc = unitCubes[index];
            var lutIdx = uc.GetLUTIdx(threshold, vertexVolumeData[index]);
            if (lutIdx < 0) 
                return;
            var triangles = getTriangles(lutIdx, index);
            uc.GenerateMesh(triangles);
            vertices[index] = uc.vertices;
            indices[index] = uc.indices;
        }

        //triTableを参照するための関数
        //NativeArrayはjob内には持ち込めるが、UnitCube内には持ち込めない
        UnitCubeIndexArray getTriangles(int index, int unitCubeIndex)
        {
            UnitCubeIndexArray triangles = new();
            for (int i = 0; i < 15; i++)
            {
                var rawVal = triTable[index][i];
                if (rawVal != -1)
                {
                    triangles[i] = (rawVal + unitCubeIndex * 12);
                }
                else triangles[i] = 0;
            }
            return triangles;
        }
    }
}