using Unity.Collections;
using Unity.Netcode;

public class ConnectionManager : NetworkBehaviour
{
    public ClientConnectionManager ClientConnectionManager;
    public ServerConnectionManager ServerConnectionManager;
    
    private void Start()
    {
        NetworkManager.OnConnectionEvent += OnConnectionEvent;
    }
    
    private void OnConnectionEvent(NetworkManager NetworkManager, ConnectionEventData ConnectionEventData)
    {
        ulong ID = ConnectionEventData.ClientId;
        NativeArray<ulong> IDGroup = ConnectionEventData.PeerClientIds;
        
        switch (ConnectionEventData.EventType)
        {
            case ConnectionEvent.ClientConnected:
                if (IsClient) ClientConnectionManager.OnClientConnect(ID, IDGroup);
                else ServerConnectionManager.OnClientConnect(ID); break;
            case ConnectionEvent.PeerConnected:
                if (IsClient) ClientConnectionManager.OnAnotherClientConnect(ID); break;
            case ConnectionEvent.ClientDisconnected:
                if (IsClient) ClientConnectionManager.OnClientDisconnect();
                else ServerConnectionManager.OnClientDisconnect(ID); break;
            case ConnectionEvent.PeerDisconnected:
                if (IsClient) ClientConnectionManager.OnAnotherClientDisconnect(ID); break;
        }
    }
    
    private void OnDisable()
    {
        if (NetworkManager) NetworkManager.OnConnectionEvent -= OnConnectionEvent;
    }
}
