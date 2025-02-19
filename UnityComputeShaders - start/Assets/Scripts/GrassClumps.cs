﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassClumps : MonoBehaviour
{
    struct GrassClump
    {
        public Vector3 position;
        public float lean;
        public float noise;

        public GrassClump( Vector3 pos)
        {
            position.x = pos.x;
            position.y = pos.y;
            position.z = pos.z;
            lean = 0;
            noise = Random.Range(0.5f, 1);
            if (Random.value < 0.5f) noise = -noise;
        }
    }
    int SIZE_GRASS_CLUMP = 5 * sizeof(float);

    public Mesh mesh;
    public Material material;
    public ComputeShader shader;
    [Range(0,1)]
    public float density = 0.8f;
    [Range(0.1f,3)]
    public float scale = 0.2f;
    [Range(10, 45)]
    public float maxLean = 25;

    ComputeBuffer clumpsBuffer;
    ComputeBuffer argsBuffer;
    GrassClump[] clumpsArray;
    uint[] argsArray = new uint[] { 0, 0, 0, 0, 0 };
    Bounds bounds;
    int timeID;
    int groupSize;
    int kernelLeanGrass;

    // Start is called before the first frame update
    void Start()
    {
        bounds = new Bounds(Vector3.zero, new Vector3(30, 30, 30));
        InitShader();
    }

    void InitShader()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        Bounds bound = filter.sharedMesh.bounds;

        Vector3 clumps = bound.extents;
        Vector3 vec = transform.localScale * 10 * density;
        clumps.x *= vec.x;
        clumps.z *= vec.z;
        int total = Mathf.RoundToInt(clumps.x * clumps.z);
        kernelLeanGrass = shader.FindKernel("LeanGrass");
        uint threadGroupSize;
        shader.GetKernelThreadGroupSizes(kernelLeanGrass, out threadGroupSize, out _, out _);
        groupSize = Mathf.CeilToInt((float)total / threadGroupSize);
        int count= groupSize * (int)threadGroupSize;
        clumpsArray = new GrassClump[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3((Random.value*2-1)*bound.extents.x+bound.center.x,
                0, (Random.value * 2 - 1) * bound.extents.z + bound.center.z);
            pos=transform.TransformPoint(pos);
            clumpsArray[i] = new GrassClump(pos);
        }
        
        clumpsBuffer = new ComputeBuffer(count, SIZE_GRASS_CLUMP);
        clumpsBuffer.SetData(clumpsArray);
        
        shader.SetBuffer(kernelLeanGrass, "clumpsBuffer", clumpsBuffer);
        shader.SetFloat("maxLean", maxLean*Mathf.PI/180);
        timeID = Shader.PropertyToID("time");
        material.SetBuffer("clumpsBuffer", clumpsBuffer);
        material.SetFloat("_Scale", scale);
        argsArray[0] = mesh.GetIndexCount(0);
        argsArray[1] = (uint)count;
        argsBuffer = new ComputeBuffer(1, argsArray.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(argsArray);
    }

    // Update is called once per frame
    void Update()
    {
        shader.SetFloat(timeID, Time.time);
        shader.Dispatch(kernelLeanGrass, groupSize, 1, 1);

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDestroy()
    {
        if (clumpsBuffer != null) clumpsBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}
