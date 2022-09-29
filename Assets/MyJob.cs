using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public struct MyJob : IJobParallelForTransform
{
    [ReadOnly]
    public float speed;
    [ReadOnly]
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = transform.position + Vector3.down * speed * deltaTime;
        transform.rotation = math.mul(math.normalize(transform.rotation), quaternion.AxisAngle(math.up(), speed * deltaTime));

        if(transform.position.y <= 0f)
        {
            transform.position = new float3(transform.position.x, 50f, 0f);
        }
    }
}