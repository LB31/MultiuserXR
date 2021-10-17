using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkDrawSharer : NetworkBehaviour
{
    public Texture2D DrawableTexture;

    private byte[] textureToSend;
    private byte[] receivedTexture;

    private NetworkVariable<ulong> lastClient = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, ulong.MaxValue);

    private void Start()
    {
        DrawableTexture = Drawable.drawable.DrawableTexture;

        // Update new clients with current image
        NetworkManager.Singleton.OnClientConnectedCallback += ((clientID) =>
        {
            Debug.Log("Server welcoming");
            textureToSend = DrawableTexture.GetRawTextureData().Compress();
            SendMessage(clientID, textureToSend);
        });

        //Receiving
        CustomMessagingManager.OnUnnamedMessage += ((senderClientId, stream) =>
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(stream))
            {
                byte[] image = reader.ReadByteArray();
                byte[] imageOriginal = image.Decompress();
                DrawableTexture.LoadRawTextureData(imageOriginal);
                DrawableTexture.Apply();
                Debug.Log("Client getting");

                // Send update to all clients
                if (IsServer)
                {
                    foreach (ulong client in NetworkManager.Singleton.ConnectedClients.Keys)
                    {
                        if (client != lastClient.Value)
                            SendMessage(client, image);
                    }
                }
            }            
        });
    }

    public void ShareUpdate(ulong sendTo, ulong sender)
    {
        textureToSend = DrawableTexture.GetRawTextureData().Compress();
        lastClient.Value = sender;

        SendMessage(sendTo, textureToSend);
    }

    public void SendMessage(ulong sendTo, byte[] image)
    {
        using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get())
        {
            WriteColors(stream, image);
            CustomMessagingManager.SendUnnamedMessage(sendTo, stream);
        }
    }

    private void WriteColors(Stream stream, byte[] image)
    {
        using (var writer = PooledNetworkWriter.Get(stream))
        {
            writer.WriteByteArray(image);
        }
    }

}

