using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class MyJobBehaviour : MonoBehaviour
{
    private int dataNum = 100;
    private void Start()
    {
        NativeArray<int> a = new NativeArray<int>(dataNum, Allocator.TempJob);
        NativeArray<int> b = new NativeArray<int>(dataNum, Allocator.TempJob);
        NativeArray<int> result = new NativeArray<int>(dataNum, Allocator.TempJob);

        for(int i = 0; i < dataNum; i++)
        {
            a[i] = i + 1;
            b[i] = i + 1;
        }

        MyJob myJob = new MyJob()
        {
            input1 = a,
            input2 = b,
            result = result
        };

        JobHandle handle = myJob.Schedule(result.Length, 32);

        handle.Complete();

        int total = 0;

        foreach(var tempResult in result)
        {
            total += tempResult;
        }

        Debug.Log(result[0]);
        Debug.Log(result[1]);
        Debug.Log(total);

        a.Dispose();
        b.Dispose();
        result.Dispose();
    }
}
