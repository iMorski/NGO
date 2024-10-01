using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientGameManager : NetworkBehaviour
{
    public Joystick Joystick;
    public ServerGameManager ServerGameManager;
    
    [HideInInspector] public ulong MyID;
    [HideInInspector] public Unit MyUnit;
    [HideInInspector] public Vector3 MyAuthoritativePosition;
    
    public Dictionary<ulong, Container> AnotherClientGroup = new Dictionary<ulong, Container>();
    public struct Container
    {
        public Unit Unit;
        public Vector3 AuthoritativePosition;
    }
    
    private List<long> TimeGroup = new List<long>();
    private List<Vector2> InputGroup = new List<Vector2>();
    
    public struct State : INetworkSerializable
    {
        public ulong ID;
        public long Time;
        public Vector3 Position;
        
        public void NetworkSerialize<T>(BufferSerializer<T> S) where T : IReaderWriter
        {
            S.SerializeValue(ref ID);
            S.SerializeValue(ref Time);
            S.SerializeValue(ref Position);
        }
    }

    [Rpc(SendTo.NotServer)]
    public void SetStateClientRPC(State[] StateGroup)
    {
        foreach (State State in StateGroup)
        {
            if (State.ID != MyID)
            {
                Container Container = AnotherClientGroup[State.ID];

                if (GameManager.Instance.Zero(Container.AuthoritativePosition))
                    Container.Unit.transform.position = State.Position;
                
                Container.AuthoritativePosition = State.Position;

                AnotherClientGroup[State.ID] = Container;
            }
            else
            {
                for (int i = TimeGroup.Count - 1; i >= 0; i = i - 1)
                {
                    if (TimeGroup[i] <= State.Time)
                    {
                        TimeGroup.RemoveAt(i);
                        InputGroup.RemoveAt(i);
                    }
                }

                MyAuthoritativePosition = State.Position;
            }
        }
    }

    private Vector3 ReCalculatePosition()
    {
        Vector3 Position = MyAuthoritativePosition;
        
        foreach (Vector2 Input in InputGroup)
        {
            Position = GameManager.Instance.CalculatePosition(
                Position, MyUnit, Input);
        }

        return Position;
    }
    
    private void FixedUpdate()
    {
        if (!MyUnit) return;

        Vector3 Position = ReCalculatePosition();
        
        Joystick.Direction.Normalize();

        Vector2 Input = Joystick.Direction != new Vector2() ? Joystick.Direction :
            new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));
        
        if (Input != new Vector2())
        {
            long Time = GameManager.Instance.GetTime();
            
            TimeGroup.Add(Time);
            InputGroup.Add(Input);
            
            Position = GameManager.Instance.CalculatePosition(
                Position, MyUnit, Input);
            
            ServerGameManager.SetTimeInputServerRPC(MyID, Time, Input);
        }
        
        MyUnit.transform.position = Position;

        foreach (KeyValuePair<ulong, Container> AnotherClient in AnotherClientGroup)
        {
            Container Container = AnotherClient.Value;
            
            if (Vector3.Distance(Container.Unit.transform.position, Container.AuthoritativePosition) > 0.1)
                Container.Unit.transform.position = Container.Unit.transform.position + Container.Unit.Speed *
                    GameManager.Instance.GetTimeSinceUpdate() * Vector3.Normalize(Container.AuthoritativePosition - Container.Unit.transform.position);
        }
    }
}
