using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private bool Static;
    [SerializeField] private bool Opaque;
    [Range(0.0f, 0.25f)][SerializeField] private float TapDistance;
    [SerializeField] private Image HandleArea;
    [SerializeField] private Image HandleTapArea;
    [SerializeField] private Image Handle;
    [SerializeField] private GameObject DragArea;
    
    public delegate void OnTap();
    public event OnTap Tap;
    
    public delegate void OnRelease(Vector2 Direction);
    public event OnRelease Release;

    [HideInInspector] public Vector2 Direction;

    private void OnValidate()
    {
        DragArea.SetActive(!Static);
        
        ChangeAlpha(HandleArea,  Opaque ? 0.0f : 0.2f);
        ChangeAlpha(HandleTapArea, Opaque ? 0.0f : 0.2f);
        ChangeAlpha(Handle, Opaque ? 0.0f : 1.0f);
    }

    private Vector2 BeginMousePosition;

    private void Awake()
    {
        BeginMousePosition = new Vector2(HandleArea.transform.position.x,
            HandleArea.transform.position.y);

        HandleTapArea.GetComponent<RectTransform>().sizeDelta =
            HandleArea.GetComponent<RectTransform>().sizeDelta * TapDistance;
    }

    private float HandleAreaRadius;
    
    private void Start()
    {
        Rect HandleAreaRect = HandleArea.GetComponent<RectTransform>().rect;

        HandleAreaRadius = Math.Max(HandleAreaRect.width, HandleAreaRect.height) *
            GetComponentInParent<Canvas>().scaleFactor / 2.0f;
    }

    private bool InNullDistance;

    public void OnPointerDown(PointerEventData Data)
    {
        InNullDistance = true;

        if (!Static)
        {
            BeginMousePosition = MousePosition(Data);
            
            HandleArea.transform.position = BeginMousePosition;
            HandleTapArea.transform.position = BeginMousePosition;
            Handle.transform.position = BeginMousePosition;
        }

        if (Opaque)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeAlphaCoroutine(HandleArea, 0.2f));
            StartCoroutine(ChangeAlphaCoroutine(HandleTapArea, 0.2f));
            StartCoroutine(ChangeAlphaCoroutine(Handle, 1.0f));
        }
    }

    public void OnDrag(PointerEventData Data)
    {
        Vector2 DirectionInPixel = MousePosition(Data) - BeginMousePosition;
        Vector2 DirectionInPixelInCircle = Vector2.ClampMagnitude(DirectionInPixel, HandleAreaRadius);

        if (Vector2.Distance(new Vector2(), DirectionInPixelInCircle / HandleAreaRadius) > TapDistance)
        {
            Direction = DirectionInPixelInCircle / HandleAreaRadius;

            if (InNullDistance) InNullDistance = false;
        }
        else
        {
            Direction = new Vector2();
        }
        
        float DragDistance = Vector2.Distance(MousePosition(Data), BeginMousePosition);

        if (!Static && DragDistance > HandleAreaRadius)
        {
            BeginMousePosition = BeginMousePosition + (DirectionInPixel - DirectionInPixelInCircle);
        }
        
        HandleArea.transform.position = BeginMousePosition;
        HandleTapArea.transform.position = BeginMousePosition;
        Handle.transform.position = DirectionInPixelInCircle + BeginMousePosition;
    }

    public void OnPointerUp(PointerEventData Data)
    {
        if (Opaque)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeAlphaCoroutine(HandleArea, 0.0f));
            StartCoroutine(ChangeAlphaCoroutine(HandleTapArea, 0.0f));
            StartCoroutine(ChangeAlphaCoroutine(Handle, 0.0f));
        }
        else
        {
            HandleArea.transform.localPosition = new Vector3();
            HandleTapArea.transform.localPosition = new Vector3();
            Handle.transform.localPosition = new Vector3();
        }

        BeginMousePosition = new Vector2(HandleArea.transform.position.x,
            HandleArea.transform.position.y);

        if (InNullDistance) Tap?.Invoke();
        if (Vector2.Distance(new Vector2(), Direction) > TapDistance) Release?.Invoke(Direction);

        Direction = new Vector2();
    }

    private void ChangeAlpha(Image Image, float Alpha)
    {
        float R = Image.color.r;
        float G = Image.color.g;
        float B = Image.color.b;

        Image.color = new Color(R, G, B, Alpha);
    }

    private IEnumerator ChangeAlphaCoroutine(Image Image, float Alpha)
    {
        float Count = 0;
        
        float A = Image.color.a;
        
        while (Image.color.a != Alpha)
        {
            ChangeAlpha(Image, Mathf.Lerp(A, Alpha, Count / 0.05f));
            
            Count = Count + Time();

            yield return new WaitForEndOfFrame();
        }
    }

    private Vector2 MousePosition(PointerEventData Data)
    {
        return Data.position;
    }

    private float Time()
    {
        return UnityEngine.Time.deltaTime;
    }
}