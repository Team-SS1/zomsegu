using UnityEngine;

[DisallowMultipleComponent]
public class VehicleSeatController : MonoBehaviour
{
    [SerializeField] private VehicleController2D vehicle;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform exitPoint;

    private Player currentPlayer;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private Collider2D playerCollider;
    private SpriteController spriteController;
    private Rigidbody2D playerRb;

    public bool HasDriver => currentPlayer != null;
    public Player CurrentPlayer => currentPlayer;

    private void Awake()
    {
        if (vehicle == null)
            vehicle = GetComponent<VehicleController2D>();

        if (seatPoint == null)
            seatPoint = transform;

        if (exitPoint == null)
            exitPoint = transform;
    }

    public bool TryEnter(Player player)
    {
        if (player == null || HasDriver)
            return false;

        currentPlayer = player;

        playerController = player.GetComponent<PlayerController>();
        playerAttack = player.GetComponent<PlayerAttack>();
        playerCollider = player.GetComponent<Collider2D>();
        playerRb = player.GetComponent<Rigidbody2D>();
        spriteController = player.GetComponentInChildren<SpriteController>();

        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.simulated = false;
        }

        if (playerController != null)
            playerController.enabled = false;

        if (playerAttack != null)
            playerAttack.enabled = false;

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (spriteController != null)
            spriteController.SetVisible(false);

        player.transform.SetParent(seatPoint);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;

        vehicle.SetDriverMounted(true);
        vehicle.SetEngine(false);

        return true;
    }

    public void ToggleEngine()
    {
        if (!HasDriver)
            return;

        vehicle.ToggleEngine();
    }

    public void ExitVehicleWithEngineOff()
    {
        if (!HasDriver)
            return;

        vehicle.TurnOffEngine();
        ExitVehicle();
    }

    private void ExitVehicle()
    {
        Player exitingPlayer = currentPlayer;

        exitingPlayer.transform.SetParent(null);
        exitingPlayer.transform.position = exitPoint.position;

        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.velocity = Vector2.zero;
        }

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        if (playerAttack != null)
            playerAttack.enabled = true;

        if (spriteController != null)
            spriteController.SetVisible(true);

        vehicle.SetDriverMounted(false);

        currentPlayer = null;
        playerController = null;
        playerAttack = null;
        playerCollider = null;
        spriteController = null;
        playerRb = null;
    }
}