using UnityEngine;
using UnityEngine.InputSystem;



public class ClickableAnt : MonoBehaviour
{
    private static InputAction leftClickAction;
    private static Camera cam;
    private static bool isSubscribed = false;

    public string antName;
    public float speed;
    public AntTypeEnum antType;
    public int HP;
    public int XP;
    public int Damage;


    private void Awake()
    {
        if (leftClickAction == null)
        {
            leftClickAction = new InputAction("LeftClick", binding: "<Mouse>/leftButton");
        }
    }

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void OnEnable()
    {
        if (!isSubscribed && leftClickAction != null)
        {
            leftClickAction.performed += WhenClicked;
            leftClickAction.Enable();
            isSubscribed = true;
        }
    }

    void OnDisable()
    {
        if (isSubscribed && leftClickAction != null)
        {
            leftClickAction.performed -= WhenClicked;
            leftClickAction.Disable();
            isSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (isSubscribed && leftClickAction != null)
        {
            leftClickAction.performed -= WhenClicked;
            leftClickAction.Dispose();
            leftClickAction = null;
            isSubscribed = false;
        }
    }

    private void WhenClicked(InputAction.CallbackContext context)
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mouseScreenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ClickableAnt clickedAnt = hit.collider.GetComponent<ClickableAnt>();
            if (clickedAnt != null)
            {
                Debug.Log("🎯 Trafiono mrówkę:\n" + clickedAnt.GetInfo());
            }
            else
            {
                Debug.Log("❌ Kliknięto coś, ale to nie była mrówka.");
            }
        }
        else
        {
            Debug.Log("❌ Nie trafiono żadnego obiektu.");
        }
    }

    public string GetInfo()
    {
        return $"🟢 Mrówka: {antName}\n" +
               $"🔰 Typ: {antType}\n" +
               $"⚡ Szybkość: {speed}\n" +
               $"❤️ HP: {HP}\n" +
               $"⭐ XP: {XP}\n" +
               $"💥 Obrażenia: {Damage}";

    }
}
