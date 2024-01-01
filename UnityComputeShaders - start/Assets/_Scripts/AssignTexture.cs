using System;
using UnityEngine;

public class AssignTexture : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private int textResolution = 256;
    
    private Renderer rend;
    private RenderTexture outputTexture;
    private int kernelHandle;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int Result = Shader.PropertyToID("Result");

    private void Start()
    {
        outputTexture = new RenderTexture(textResolution, textResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.material.mainTexture = outputTexture;
        InitShader();
    }

    private void InitShader()
    {
        kernelHandle = shader.FindKernel("CSMain");
        shader.SetTexture(kernelHandle, Result, outputTexture);
        rend.material.SetTexture(MainTex, outputTexture);
        DispatchShader(textResolution / 16, textResolution / 16);
    }

    private void DispatchShader(int x, int y)
    {
        shader.Dispatch(kernelHandle, x, y, 1);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.U))
        {
                DispatchShader(textResolution/8, textResolution/8);
        }
    }
}
