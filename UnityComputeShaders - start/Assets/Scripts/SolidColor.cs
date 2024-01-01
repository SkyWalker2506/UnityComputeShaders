using UnityEngine;
using System.Collections;

public class SolidColor : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private int textResolution = 256;
    [SerializeField] private string kernelName="SolidRed";
    
    private Renderer rend;
    private RenderTexture outputTexture;
    private int kernelHandle;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int Result = Shader.PropertyToID("Result");
    private static readonly int TextResolution = Shader.PropertyToID("texResolution");

    private void Start()
    {
        outputTexture = new RenderTexture(textResolution, textResolution, 0)
        {
            enableRandomWrite = true
        };
        outputTexture.Create();
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.material.mainTexture = outputTexture;
        InitShader();
    }

    private void InitShader()
    {
        kernelHandle = shader.FindKernel(kernelName);
        shader.SetInt(TextResolution, textResolution);
        shader.SetTexture(kernelHandle, Result, outputTexture);
        rend.material.SetTexture(MainTex, outputTexture);
        DispatchShader(textResolution / 8, textResolution / 8);
    }

    private void DispatchShader(int x, int y)
    {
        shader.Dispatch(kernelHandle, x, y, 1);
    }
  
}

