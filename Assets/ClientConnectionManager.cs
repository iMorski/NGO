using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ClientConnectionManager : NetworkBehaviour
{
    public MainCamera MainCamera;
    public Unit Unit;
    public UnitGhost UnitGhost;
    /* public RectTransform UiCanvas;
    public UiChase UiUnitBar;
    public UiChase UiAnotherUnitBar; */
    public ClientGameManager ClientGameManager;

    public void OnClientConnect(ulong MyID, NativeArray<ulong> AnotherClientIDGroup)
    {
        Unit Unit = Instantiate(this.Unit);

        ClientGameManager.MyID = MyID;
        ClientGameManager.MyUnit = Unit;

        MainCamera.Anchor = Unit.transform;

        /* UiChase UiUnitBar = Instantiate(this.UiUnitBar, UiCanvas);
        UiUnitBar.Anchor = Unit.transform; */

        SetUnitGhost(MyID);

        foreach (ulong ID in AnotherClientIDGroup)
        {
            SetAnotherClient(ID);
        }
    }
    
    public void OnAnotherClientConnect(ulong ID)
    {
        Debug.Log("C. OnAnotherClientConnect. ID: " + ID);

        SetAnotherClient(ID);
    }

    void SetAnotherClient(ulong ID)
    {
        Unit Unit = Instantiate(this.Unit);
        
        ClientGameManager.Container Container = new ClientGameManager.Container
        {
            Unit = Unit
        };
        ClientGameManager.AnotherClientGroup.Add(ID, Container);
        
        /* UiChase UiAnotherUnitBar = Instantiate(this.UiAnotherUnitBar, UiCanvas);
        UiAnotherUnitBar.Anchor = Unit.transform; */

        SetUnitGhost(ID);
    }

    void SetUnitGhost(ulong ID)
    {
        UnitGhost UnitGhost = Instantiate(this.UnitGhost);

        UnitGhost.CGM = ClientGameManager;
        UnitGhost.ID = ID;
    }

    public void OnClientDisconnect()
    {
        Debug.Log("C. OnClientDisconnect");
    }
    
    public void OnAnotherClientDisconnect(ulong ID)
    {
        Debug.Log("C. OnAnotherClientDisconnect. ID: " + ID);
        
        Destroy(GameManager.Instance.GetUnitGO(ClientGameManager.AnotherClientGroup[ID].Unit));
        
        ClientGameManager.AnotherClientGroup.Remove(ID);
    }
}