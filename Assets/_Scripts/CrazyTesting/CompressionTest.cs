using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompressionTest : MonoBehaviour
{
    public Texture2D TextureToSend;
    public Texture2D TextureClone;

    private void Start()
    {
        Debug.Log(TextureToSend.GetRawTextureData().Length);
        byte[] toSend = TextureToSend.GetRawTextureData().Compress();
        Debug.Log(toSend.Length);
        TextureClone = new Texture2D(TextureToSend.width, TextureToSend.height, TextureFormat.RGBA32, false);
        Debug.Log(toSend.Decompress().Length);
        TextureClone.LoadRawTextureData(toSend.Decompress());
        Debug.Log(TextureClone.GetRawTextureData().Length);
    }
}
