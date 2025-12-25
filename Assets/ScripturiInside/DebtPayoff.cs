using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pay off your debt and be free!
/// Press E when you have $3000 to end the game
/// </summary>
public class DebtPayoff : MonoBehaviour
{
    [Header("Debt Settings")]
    public int debtAmount = 3000;
    public string endingScene = "Sfarsit";

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public string promptNotEnough = "Pay Debt: $3000 (Not enough money)";
    public string promptCanPay = "E to Pay Debt ($3000) and Be Free!";

    // Cache
    private Camera cam;
    private InteractionUI ui;

    void Start()
    {
        cam = Camera.main;
        ui = FindFirstObjectByType<InteractionUI>();

        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }

        Debug.Log("[DebtPayoff] Ready. Need $" + debtAmount + " to be free.");
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;

        // Check distance
        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist > interactionDistance)
        {
            if (ui != null) ui.Hide();
            return;
        }

        // Raycast
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                // Looking at this object
                bool canAfford = PlayerMoney.Money >= debtAmount;

                if (ui != null)
                {
                    string prompt = canAfford ? promptCanPay : promptNotEnough + " (You have: $" + PlayerMoney.Money + ")";
                    ui.SetPrompt(prompt);
                    ui.Show();
                }

                // E to pay
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (canAfford)
                    {
                        PayDebt();
                    }
                    else
                    {
                        Debug.Log("[DebtPayoff] Not enough money! Need $" + debtAmount + ", have $" + PlayerMoney.Money);
                    }
                }
            }
            else
            {
                if (ui != null) ui.Hide();
            }
        }
        else
        {
            if (ui != null) ui.Hide();
        }
    }

    void PayDebt()
    {
        Debug.Log("[DebtPayoff] DEBT PAID! You are FREE!");

        // Remove money
        PlayerMoney.Remove(debtAmount);

        // Go to ending scene
        SceneManager.LoadScene(endingScene);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}