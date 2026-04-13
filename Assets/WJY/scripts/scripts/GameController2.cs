using UnityEngine;

public class GameController2 : MonoBehaviour
{
    public static GameController2 Instance;

    [Header("Player")]
    public Transform player;

    [Header("Default Respawn Point")]
    public Vector3 respawnPosition;
    public Vector3 respawnEulerAngles;

    private CharacterController characterController;
    private Rigidbody rb;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            rb = player.GetComponent<Rigidbody>();

            if (respawnPosition == Vector3.zero)
            {
                respawnPosition = player.position;
                respawnEulerAngles = player.eulerAngles;
            }
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            rb = player.GetComponent<Rigidbody>();
        }
    }

    public void SetRespawnPoint(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }

    public void SetRespawnPoint(Vector3 newPosition, Vector3 newEulerAngles)
    {
        respawnPosition = newPosition;
        respawnEulerAngles = newEulerAngles;
    }

    public void TeleportPlayerToRespawn()
    {
        if (player == null)
        {
            Debug.LogWarning("GameController: Player is not assigned.");
            return;
        }

        if (characterController == null)
            characterController = player.GetComponent<CharacterController>();

        if (rb == null)
            rb = player.GetComponent<Rigidbody>();

        if (characterController != null)
            characterController.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = respawnPosition;
        player.eulerAngles = respawnEulerAngles;

        if (characterController != null)
            characterController.enabled = true;
    }

    public void TeleportPlayerToTarget(Transform targetPoint)
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("GameController: targetPoint is null.");
            return;
        }

        SetRespawnPoint(targetPoint.position, targetPoint.eulerAngles);
        TeleportPlayerToRespawn();
    }

    public void RespawnPlayer()
    {
        TeleportPlayerToRespawn();
    }
}