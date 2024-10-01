using UnityEngine;

public class UiChase : MonoBehaviour
{
    [HideInInspector] public Transform Anchor;

    Vector3 Position;
    
    void Start()
    {
        Position = transform.position;
    }

    void FixedUpdate()
    {
        if (!Anchor) return; transform.position = Position + Anchor.position;
    }
}
