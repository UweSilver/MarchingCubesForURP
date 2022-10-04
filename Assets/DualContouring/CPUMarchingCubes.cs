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
    public class CPUMarchingCubes : IContourGenerater
    {
        int resolution = 64;


        private JobHandle jobHandle;
        
        public void Execute(Texture3D field, Material material) 
        {
            Debug.Log("CPUMarchingCubes");

            var root = new GameObject();
            var meshRenderer = root.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            var meshFilter = root.AddComponent<MeshFilter>();

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
            mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

            var voxelCount = Mathf.FloorToInt(Mathf.Pow(resolution, 3));

            NativeArray<Vector3Int> unitCubePositions = new NativeArray<Vector3Int>(voxelCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            NativeArray<VertexVolumeData> vertexVolumeData = new NativeArray<VertexVolumeData>(voxelCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            NativeArray<UnitCubeVertexArray> vertices = new NativeArray<UnitCubeVertexArray>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            NativeArray<UnitCubeIndexArray> indices = new NativeArray<UnitCubeIndexArray>(voxelCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            NativeArray<UnitCube> unitCubes = new NativeArray<UnitCube>(voxelCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
            int idx = 0;
            for (var x = 0; x < resolution; x++)
            {
                for (var y = 0; y < resolution; y++)
                {
                    for (var z = 0; z < resolution; z++, idx++)
                    {
                        var intPosition = new Vector3Int(x, y, z);
                        var position = new Vector3(x, y, z);
                        unitCubePositions[idx] = intPosition;

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
                        vertexVolumeData[idx] = vertexVolume;
                    }
                }
            }
            for(int i = 0; i < voxelCount; i++)
            {
                unitCubes[i] = new UnitCube(
                    i,
                    unitCubePositions[i],
                    new Vector3(1, 1, 1),
                    new Vector3Int(resolution, resolution, resolution)
                );
            }
            var mcJob = new MCJob
            {
                unitCubeCoordinate = unitCubePositions,
                voxelSize = new Vector3(1, 1, 1),
                voxelResolution = new Vector3Int(resolution, resolution, resolution),
                vertexVolumeData = vertexVolumeData,
                threshold = 0.5f,
                unitCubes = unitCubes,
                vertices = vertices,
                indices = indices,
            };
            jobHandle = mcJob.Schedule(voxelCount, voxelCount);

            jobHandle.Complete();

            var verticesVector3 = new NativeArray<Vector3>(voxelCount * 12, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for(var i = 0; i < vertices.Length; i++)
            {
                for(var j = 0; j < 12; j++)
                {
                    verticesVector3[i * 12 + j] = vertices[i][j];
                }
            }

            var indicesInt = new NativeArray<int>(voxelCount * 15, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for(var i = 0; i < indices.Length; i++)
            {
                for(var j = 0; j < 15; j++)
                {
                    indicesInt[i * 15 + j] = indices[i][j];
                }
            }

            mesh.SetVertices(verticesVector3);
            mesh.SetIndices(indicesInt, MeshTopology.Triangles, 0);
            meshFilter.mesh = mesh;
            vertexVolumeData.Dispose();
            unitCubePositions.Dispose();

            unitCubes.Dispose();
            vertices.Dispose();
            indices.Dispose();
            verticesVector3.Dispose();
            indicesInt.Dispose();
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
            int i15 = 0 //�̂Ă�
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

    public struct MCJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3Int> unitCubeCoordinate;
        [ReadOnly]
        public Vector3 voxelSize;
        [ReadOnly]
        public Vector3Int voxelResolution;
        [ReadOnly]
        public NativeArray<VertexVolumeData> vertexVolumeData;
        [ReadOnly]
        public float threshold;

        public NativeArray<UnitCube> unitCubes;
        public NativeArray<UnitCubeVertexArray> vertices;
        public NativeArray<UnitCubeIndexArray> indices;
        
        public void Execute(int index)
        {
            var uc = unitCubes[index];
            uc.GenerateMesh(threshold, vertexVolumeData[index]);
            vertices[index] = uc.vertices;
            indices[index] = uc.indices;
        }
    }
}