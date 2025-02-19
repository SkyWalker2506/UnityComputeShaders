﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class QuadParticles : MonoBehaviour
{

    private Vector2 cursorPos;

    // struct
    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
    }
    
    struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
        public float life;
    }

    const int SIZE_PARTICLE = 7 * sizeof(float);
    const int SIZE_VERTEX = 6 * sizeof(float);
    
    public int particleCount = 10000;
    public Material material;
    public ComputeShader shader;
    [Range(0.01f, 1.0f)]
    public float quadSize = 0.1f;

    int numParticles;
    int numVerticesInMesh;
    int kernelID;
    ComputeBuffer particleBuffer;
    ComputeBuffer vertexBuffer;
    
    int groupSizeX; 
    
    // Use this for initialization
    void Start()
    {
        Init();
    }

    void Init()
    {
        // find the id of the kernel
        kernelID = shader.FindKernel("CSMain");

        uint threadsX;
        shader.GetKernelThreadGroupSizes(kernelID, out threadsX, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)particleCount / (float)threadsX);
        numParticles = groupSizeX * (int)threadsX;

        // initialize the particles
        Particle[] particleArray = new Particle[numParticles];
        int numVertices = numParticles * 6;
        Vertex[] vertexArray = new Vertex[numVertices];
        
        Vector3 pos = new Vector3();
        
        int vertexIndex = 0;
        for (int i = 0; i < numParticles; i++)
        {
            pos.Set(Random.value * 2 - 1.0f, Random.value * 2 - 1.0f, Random.value * 2 - 1.0f);
            pos.Normalize();
            pos *= Random.value;
            pos *= 0.5f;

            particleArray[i].position.Set(pos.x, pos.y, pos.z + 3);
            particleArray[i].velocity.Set(0,0,0);
          
            // Initial life value
            particleArray[i].life = Random.value * 5.0f + 1.0f;
            vertexIndex = i * 6;
            // Set uv of the quad for 6 indices
            vertexArray[vertexIndex].uv.Set(0.0f, 0.0f);
            vertexArray[vertexIndex + 1].uv.Set(0.0f, 1.0f);
            vertexArray[vertexIndex + 2].uv.Set(1.0f, 1.0f);
            vertexArray[vertexIndex + 3].uv.Set(0.0f, 0.0f);
            vertexArray[vertexIndex + 4].uv.Set(1.0f, 1.0f);
            vertexArray[vertexIndex + 5].uv.Set(1.0f, 0.0f);
        }

        // create compute buffers
        particleBuffer = new ComputeBuffer(numParticles, SIZE_PARTICLE);
        particleBuffer.SetData(particleArray);
        vertexBuffer = new ComputeBuffer(numVertices, SIZE_VERTEX);
        vertexBuffer.SetData(vertexArray);
        // bind the compute buffers to the shader and the compute shader
        shader.SetBuffer(kernelID, "particleBuffer", particleBuffer);
        shader.SetBuffer(kernelID, "vertexBuffer", vertexBuffer);
      
        shader.SetFloat("halfSize", quadSize * 0.5f);
        
        material.SetBuffer("vertexBuffer", vertexBuffer);
        shader.Dispatch(kernelID, groupSizeX, 1, 1);

    }

    void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, numParticles);
    }

    void OnDestroy()
    {
        if (particleBuffer != null){
            particleBuffer.Release();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }
        float[] mousePosition2D = { cursorPos.x, cursorPos.y };

        // Send datas to the compute shader
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloats("mousePosition", mousePosition2D);

        // Update the Particles
   
        shader.Dispatch(kernelID, groupSizeX, 1, 1);
    }

    void OnGUI()
    {
        Vector3 p = new Vector3();
        Camera c = Camera.main;
        Event e = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = e.mousePosition.x;
        mousePos.y = c.pixelHeight - e.mousePosition.y;

        p = c.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, c.nearClipPlane + 14));

        cursorPos.x = p.x;
        cursorPos.y = p.y;
        
    }
}
