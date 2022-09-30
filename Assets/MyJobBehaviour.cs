using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class MyJobBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private int numberToInstantiate = 100;
    private int total;
    private TransformAccessArray transforms;
    private JobHandle jobHandle;

    private void OnDisable()
    {
        jobHandle.Complete();
        transforms.Dispose();
    }

    private void Start()
    {
        transforms = new TransformAccessArray(0);
    }

    private void Update()
    {
        jobHandle.Complete();

        if (Input.GetMouseButtonDown(0))
        {
            InstantiateGameObject();
        }

        var myJob = new MyJob
        {
            deltaTime = Time.deltaTime,
            speed = 2f
        };
        jobHandle = myJob.Schedule(transforms);
    }

    void InstantiateGameObject()
    {
        jobHandle.Complete();

        transforms.capacity += numberToInstantiate;
        Unity.Mathematics.Random rand;
        rand = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1f, 100f));

        for(int i = 0; i < numberToInstantiate; i++)
        {
            var ins = Instantiate(prefab, new Vector3(rand.NextFloat(-20f, 20f), 50f, 0f), Quaternion.identity);
            transforms.Add(ins.transform);
        }
        total += numberToInstantiate;
        Debug.Log("totalNum : " + total);
    }
}
