using UnityEngine;

public class UnitGhost : Unit
{
    [HideInInspector] public ClientGameManager CGM;
    [HideInInspector] public ulong ID;

    void FixedUpdate()
    {
        if (!CGM) return;
        
        Vector3 Position = transform.position;

        if (ID != CGM.MyID)
        {
            if (!CGM.AnotherClientGroup.ContainsKey(ID))
            {
                Destroy(GameManager.Instance.GetUnitGO(this));
                
                return;
            }
            
            Position = CGM.AnotherClientGroup[ID].AuthoritativePosition;
        }
        else Position = CGM.MyAuthoritativePosition;

        transform.position = Position;
    }
}
