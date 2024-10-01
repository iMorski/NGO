using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerConnectionManager : NetworkBehaviour
{
    public Unit Unit;
    public ServerGameManager ServerGameManager;
    
    public void OnClientConnect(ulong ID)
    {
        Debug.Log("S. OnClientConnect. ID: " + ID);
        
        Unit Unit = Instantiate(this.Unit);

        ServerGameManager.Container Container = new ServerGameManager.Container
        {
            Unit = Unit
        };

        ServerGameManager.ClientGroup.Add(ID, Container);
    }
    
    public void OnClientDisconnect(ulong ID)
    {
        Debug.Log("S. OnClientDisconnect. ID: " + ID);
        
        Destroy(GameManager.Instance.GetUnitGO(ServerGameManager.ClientGroup[ID].Unit));
        ServerGameManager.ClientGroup.Remove(ID);
    }
}