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

        SendIterations = (int)Math.Ceiling((float)textureToSend.Length / (float)MTU_Size);
        ChunkSize = (int)Math.Ceiling((float)textureToSend.Length / (float)SendIterations);
    }

    [ContextMenu("Dammit")]
    private void ClientConnected()
    {
        
        for (int i = 0; i < SendIterations; i++)
        {
            byte[] data = textureToSend.Skip(i * ChunkSize).Take(ChunkSize).ToArray();
            bool finalChunk = i == (SendIterations - 1);
            Debug.Log(data.Length);
            SendChunkClientRpc(data, i * ChunkSize, data.Length, finalChunk);
        }
    }

    [ServerRpc]
    private void SendChunkServerRpc(Color32[] chunk)
    {

    }

    [ClientRpc]
    private void SendChunkClientRpc(byte[] chunk, int startIndex, int elemenAmount, bool finalChunk = false)
    {
        Debug.Log("SendChunkClientRpc");
        for (int i = startIndex, j = 0; i < elemenAmount + startIndex; i++, j++)
        {
            receivedTexture[i] = chunk[j];
        }

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
    private void BlaClientRpc()
    {
        Debug.Log("BlaClientRpc");
    }

}
