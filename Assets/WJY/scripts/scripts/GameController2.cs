using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController2 : MonoBehaviour
{
    public static GameController2 Instance;

    [Header("Player")]
    public Transform player;

    [Header("Default Respawn Point")]
    public Vector3 respawnPosition;
    public Vector3 respawnEulerAngles;

    [Header("Respawn Gravity Reset")]
    [Tooltip("传送回 checkpoint 时，是否把世界重力恢复为普通向下")]
    public bool resetGravityOnRespawn = true;

    [Tooltip("普通重力大小，一般保持 9.81")]
    public float normalGravityStrength = 9.81f;

    [Tooltip("传送回 checkpoint 时，是否把玩家旋转也恢复成正常站立方向")]
    public bool resetPlayerRotationOnRespawn = true;

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
            Debug.LogWarning("GameController2: Player is not assigned.");
            return;
        }

        if (characterController == null)
            characterController = player.GetComponent<CharacterController>();

        if (rb == null)
            rb = player.GetComponent<Rigidbody>();

        if (characterController != null)
            characterController.enabled = false;

        /*
         * 核心新增：
         * 传送回 checkpoint 前，先把全局重力恢复成普通向下。
         */
        if (resetGravityOnRespawn)
        {
            Physics.gravity = Vector3.down * normalGravityStrength;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        player.position = respawnPosition;

        if (resetPlayerRotationOnRespawn)
        {
            player.rotation = Quaternion.Euler(respawnEulerAngles);
        }
        else
        {
            player.eulerAngles = respawnEulerAngles;
        }

        Physics.SyncTransforms();

        if (characterController != null)
            characterController.enabled = true;
    }

    public void TeleportPlayerToTarget(Transform targetPoint)
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("GameController2: targetPoint is null.");
            return;
        }

        SetRespawnPoint(targetPoint.position, targetPoint.eulerAngles);
        TeleportPlayerToRespawn();
    }

    public void RespawnPlayer()
    {
        TeleportPlayerToRespawn();
    }

    public void RestartTheGame(string sceneName)
    {
        Physics.gravity = Vector3.down * normalGravityStrength;
        SceneManager.LoadScene(sceneName);
    }

    public void ResetGravityToNormal()
    {
        Physics.gravity = Vector3.down * normalGravityStrength;

        if (player != null && resetPlayerRotationOnRespawn)
        {
            player.rotation = Quaternion.Euler(respawnEulerAngles);
        }

        if (rb != null)
        {
            rb.useGravity = true;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Physics.SyncTransforms();
    }
}