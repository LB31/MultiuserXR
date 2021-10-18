using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;

using UnityEngine;

public class MessageTest : MonoBehaviour {

    public Texture2D DrawTexture;
    public Texture2D TextureClone;
    public int MessageFrequency = 300;

    protected string MESSAGE_NAME = "IMAGE";

    public int MTU_Size = 65000;
    public int ChunkSize;
    public int SendIterations;

    private byte[] textureToSend;
    private byte[] receivedTexture;

    public void Start() {
        CustomMessagingManager.RegisterNamedMessageHandler(MESSAGE_NAME, OnMessageReceived);
        DrawTexture = FindObjectOfType<DrawManager>().DrawableTexture;


        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;

        Debug.Log(DrawTexture.GetRawTextureData().Length + " Main tex origin size");
        textureToSend = DrawTexture.GetRawTextureData().Compress();
        Debug.Log(textureToSend.Length + " compressed size");
        receivedTexture = new byte[textureToSend.Length];
        SendIterations = (int)Math.Ceiling((float)textureToSend.Length / (float)MTU_Size);
        ChunkSize = (int)Math.Ceiling((float)textureToSend.Length / (float)SendIterations);

    }

    [ContextMenu("Dammit")]
    public void Dammit() => ClientConnected(ulong.MinValue);

    private async void ClientConnected(ulong id)
    {
        textureToSend = DrawTexture.GetRawTextureData().Compress();
        SendMessage(id, textureToSend, 0, 0, true);
        return;

        for (int i = 0; i < SendIterations; i++)
        {
            byte[] data = textureToSend.Skip(i * ChunkSize).Take(ChunkSize).ToArray();
            bool finalChunk = i == (SendIterations - 1);
            SendMessage(id, data, i * ChunkSize, data.Length, finalChunk);
            await Task.Delay(MessageFrequency);
        }
    }

    protected void OnMessageReceived(ulong sender, Stream payload) {
        bool finalChunk = ReadColors(payload);

        Debug.Log("reading");

        if (!finalChunk) return;
        byte[] decompressedTex = receivedTexture.Decompress();
        Debug.Log(decompressedTex.Length);
        TextureClone = new Texture2D(DrawTexture.width, DrawTexture.height, TextureFormat.RGBA32, false);
        TextureClone.LoadRawTextureData(decompressedTex);
        TextureClone.Apply();
        // TODO
        transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = TextureClone;
    }

    public void SendMessage(ulong clientID, byte[] chunk, int startIndex, int elementAmount, bool finalChunk = false)
    {
        using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get()) {
            WriteColors(stream, chunk, startIndex, elementAmount, finalChunk);
            CustomMessagingManager.SendNamedMessage(MESSAGE_NAME, clientID, stream);
        }
    }

    private void WriteColors(Stream stream, byte[] chunk, int startIndex, int elementAmount, bool finalChunk)
    {
        using (var writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteInt32(startIndex);
            writer.WriteInt32(elementAmount);
            writer.WriteBool(finalChunk);
            writer.WriteByteArray(chunk);
        }
    }

    private bool ReadColors(Stream stream)
    {
        using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
        {
            int startIndex = reader.ReadInt32();
            int elementAmount = reader.ReadInt32();
            bool finalChunk = reader.ReadBool();
            byte[] chunk = reader.ReadByteArray();
            receivedTexture = chunk;
            return finalChunk;

            for (int i = startIndex, j = 0; i < elementAmount + startIndex; i++, j++)
            {
                receivedTexture[i] = chunk[j];
            }

            return finalChunk;
        }
    }

}
