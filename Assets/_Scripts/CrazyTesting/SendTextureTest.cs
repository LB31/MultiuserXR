using MLAPI;
using MLAPI.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class SendTextureTest : NetworkBehaviour
{
    public Texture2D Texture;
    public Texture2D TextureClone;
    public int MTU_Size = 65000;

    public int ChunkSize;
    public int SendIterations;

    private byte[] textureToSend;
    private byte[] receivedTexture;


    void Start()
    {
        //NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;

        textureToSend = Texture.GetRawTextureData();
        
        receivedTexture = new byte[textureToSend.Length];
        Debug.Log(Texture.format);

        Debug.Log(string.Join(", ", textureToSend));

        SendIterations = (int)Math.Ceiling((float)textureToSend.Length / (float)MTU_Size);
        ChunkSize = (int)Math.Ceiling((float)textureToSend.Length / (float)SendIterations);

        GetComponent<Renderer>().material.mainTexture = Texture;

    }

    [ContextMenu("Dammit")]
    private void ClientConnected()
    {
        for (int i = 0; i < SendIterations; i++)
        {
            byte[] data = textureToSend.Skip(i * ChunkSize).Take(ChunkSize).ToArray();
            //byte[] datar = textureToSend.Sub
            Debug.Log(data.Length + " data l");
            Debug.Log(string.Join(", ", data) + " the sent one");
            bool finalChunk = i == SendIterations - 1;

            SendChunkClientRpc(data, i * ChunkSize, data.Length, finalChunk);
        }
    }

    //public T[] SubArray<T>(this T[] data, int index, int length)
    //{
    //    T[] result = new T[length];
    //    Array.Copy(data, index, result, 0, length);
    //    return result;
    //}

    void Update()
    {

    }

    private void SendTexture()
    {
        var test = textureToSend.Take(4);
        foreach (var item in test)
        {
            Debug.Log(item);
        }
    }

    private void ReceiveTexture()
    {

    }

    [ServerRpc]
    private void SendChunkServerRpc(Color32[] chunk)
    {

    }

    [ClientRpc]
    private void SendChunkClientRpc(byte[] chunk, int startIndex, int elemenAmount, bool finalChunk = false)
    {
        //Debug.Log("Marker");
        //Debug.Log(string.Join(", ", chunk));

        Debug.Log($"{startIndex}, {elemenAmount}");

        for (int i = startIndex, j = 0; i < elemenAmount + startIndex; i++, j++)
        {
            receivedTexture[i] = chunk[j];
        }

        Debug.Log(string.Join(", ", chunk) + " the received one");
        Debug.Log("Marker Ende");



        if (finalChunk)
        {
            Debug.Log(string.Join(", ", receivedTexture));
            TextureClone = new Texture2D(Texture.width, Texture.height, TextureFormat.RGBA32, false);
            TextureClone.LoadRawTextureData(receivedTexture);
            TextureClone.Apply();
            GetComponent<Renderer>().material.mainTexture = TextureClone;
        }

    }


    [ClientRpc]
    private void TestClientRpc()
    {
        Debug.Log("Obama cares");
    }

}
