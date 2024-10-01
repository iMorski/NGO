using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerGameManager : NetworkBehaviour
{
    public ClientGameManager ClientGameManager;
    
    public Dictionary<ulong, Container> ClientGroup = new Dictionary<ulong, Container>();
    public class Container
    {
        public Unit Unit;
        public long Time;
        public List<Vector2> InputGroup = new List<Vector2>();
        public List<Vector3> PositionGroup = new List<Vector3>();
        public List<long> TimeGroup = new List<long>();
    }

    [Rpc(SendTo.Server)]
    public void SetTimeInputServerRPC(ulong ID, long Time, Vector2 Input)
    {
        Container CC = ClientGroup[ID];
        CC.Time = Time;
        CC.InputGroup.Add(Input);
        
        ClientGroup[ID] = CC;
    }

    void FixedUpdate()
    {
        if (ClientGroup.Count < 1) return;
        
        ClientGameManager.State[] StateGroup =
            new ClientGameManager.State[ClientGroup.Count];
        
        int Index = 0;
        
        foreach (KeyValuePair<ulong, Container> Client in ClientGroup)
        {
            ulong ID = Client.Key;
            Container Container = Client.Value;
            
            Vector3 Position = Container.Unit.transform.position;
            
            ClientGameManager.State State = new ClientGameManager.State
            {
                ID = ID,
                Time = Container.Time,
                Position = Position
            };
            
            if (Container.InputGroup.Count > 0)
            {
                foreach (Vector2 Input in Container.InputGroup)
                {
                    Position = GameManager.Instance.CalculatePosition(
                        Position, Container.Unit, Input);
                }
                
                Container.InputGroup.Clear();
                
                Container.PositionGroup.Add(Position);
                if (Container.PositionGroup.Count > 1800)
                    Container.PositionGroup.Remove(Container.PositionGroup[0]);

                long Time = GameManager.Instance.GetTime();

                Container.TimeGroup.Add(Time);
                if (Container.TimeGroup.Count > 1800)
                    Container.TimeGroup.Remove(Container.TimeGroup[0]);
                
                Container.Unit.transform.position = Position;
                
                State.Position = Position;
            }

            StateGroup[Index] = State;
            Index = Index + 1;
        }
        
        ClientGameManager.SetStateClientRPC(StateGroup);
    }
}
