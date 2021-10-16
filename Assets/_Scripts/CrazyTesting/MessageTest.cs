using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FreeDraw;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;

using UnityEngine;

public class MessageTest : MonoBehaviour {

    public Texture2D DrawTexture;
    public Texture2D TextureClone;
    public int MessageFrequency = 300;

    protected string MESSAGE_NAME = "FOOO";

    public int MTU_Size = 65000;
    public int ChunkSize;
    public int SendIterations;

    private byte[] textureToSend;
    private byte[] receivedTexture;

    public void Start() {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        CustomMessagingManager.RegisterNamedMessageHandler(MESSAGE_NAME, OnMessageReceived);

        DrawTexture = FindObjectOfType<Drawable>().DrawableTexture;

        textureToSend = DrawTexture.GetRawTextureData();
        Debug.Log(textureToSend.Length);
        receivedTexture = new byte[textureToSend.Length];
        SendIterations = (int)Math.Ceiling((float)textureToSend.Length / (float)MTU_Size);
        ChunkSize = (int)Math.Ceiling((float)textureToSend.Length / (float)SendIterations);

    }

    [ContextMenu("Dammit")]
    public void Dammit() => ClientConnected(ulong.MinValue);

    private async void ClientConnected(ulong id)
    {
        textureToSend = DrawTexture.GetRawTextureData();
        for (int i = 0; i < SendIterations; i++)
        {
            byte[] data = textureToSend.Skip(i * ChunkSize).Take(ChunkSize).ToArray();
            bool finalChunk = i == (SendIterations - 1);
            SendThatMessage(id, data, i * ChunkSize, data.Length, finalChunk);
            await Task.Delay(MessageFrequency);
        }
    }

    protected void OnMessageReceived(ulong sender, Stream payload) {
        bool finalChunk = ReadColors(payload);
        Debug.Log(finalChunk);

        if (!finalChunk) return;

        TextureClone = new Texture2D(DrawTexture.width, DrawTexture.height, TextureFormat.RGBA32, false);
        TextureClone.LoadRawTextureData(receivedTexture);
        TextureClone.Apply();

        Debug.Log(TextureClone.height);

        transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = TextureClone;
    }

    public void SendThatMessage(ulong clientID, byte[] chunk, int startIndex, int elementAmount, bool finalChunk = false)
    {
        byte[] array = textureToSend;
        var segment = new ArraySegment<byte>(chunk);

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
        using (var reader = PooledNetworkReader.Get(stream))
        {
            int startIndex = reader.ReadInt32();
            int elementAmount = reader.ReadInt32();
            bool finalChunk = reader.ReadBool();
            byte[] chunk = reader.ReadByteArray();

            for (int i = startIndex, j = 0; i < elementAmount + startIndex; i++, j++)
            {
                receivedTexture[i] = chunk[j];
            }

            return finalChunk;
        }
    }

}
