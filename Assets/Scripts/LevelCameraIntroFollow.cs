using Unity.Cinemachine;
using UnityEngine;

public class LevelCameraIntroFollow : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float introHoldSeconds = 1.5f;
    [SerializeField] private float blendDurationSeconds = 1.5f;

    [Header("Player Camera")]
    [SerializeField] private float playerOrthographicSize = 6f;
    [SerializeField] private Vector3 playerFollowOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private Vector3 playerPositionDamping = new Vector3(0.35f, 0.35f, 0.1f);

    [Header("Priorities")]
    [SerializeField] private int introPriority = 20;
    [SerializeField] private int playerPriority = 30;

    [Header("Scene Cameras")]
    [SerializeField] private CinemachineCamera introCamera;
    [SerializeField] private CinemachineCamera playerCamera;

    private CinemachineBrain brain;
    private Transform playerTarget;
    private float elapsedTime;
    private bool switchedToPlayerCamera;

    void Awake()
    {
        brain = GetComponent<CinemachineBrain>();

        if (brain == null)
        {
            brain = gameObject.AddComponent<CinemachineBrain>();
        }

        brain.DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Styles.EaseInOut,
            blendDurationSeconds);

        EnsureSceneCameras();
        playerTarget = FindPlayerTarget();
        bool loadedFromSave = UiManager.ConsumeLoadingFromSaveFlag();

        if (loadedFromSave)
        {
            switchedToPlayerCamera = true;
            elapsedTime = introHoldSeconds;

            if (playerCamera != null)
            {
                playerCamera.Priority.Value = playerPriority;
            }

            if (introCamera != null)
            {
                introCamera.Priority.Value = introPriority - 10;
            }

            if (playerCamera != null && playerTarget != null)
            {
                playerCamera.Follow = playerTarget;
            }

            return;
        }

        if (introCamera != null)
        {
            introCamera.Priority.Value = introPriority;
        }

        if (playerCamera != null)
        {
            playerCamera.Priority.Value = introPriority - 20;
        }
    }

    void Update()
    {
        EnsureSceneCameras();

        if (!switchedToPlayerCamera)
        {
            playerTarget = FindPlayerTarget();

            if (elapsedTime >= introHoldSeconds && playerCamera != null && introCamera != null)
            {
                switchedToPlayerCamera = true;
                playerCamera.Priority.Value = playerPriority;
                introCamera.Priority.Value = introPriority - 10;
            }

            elapsedTime += Time.deltaTime;
        }

        if (playerTarget != null && playerCamera != null && playerCamera.Follow == null)
        {
            playerCamera.Follow = playerTarget;
        }
    }

    private void EnsureSceneCameras()
    {
        if (introCamera == null)
        {
            introCamera = FindCameraByName("Intro Cinemachine Camera");
            if (introCamera == null)
            {
                introCamera = CreateIntroCamera();
            }
        }

        if (playerCamera == null)
        {
            playerCamera = FindCameraByName("Player Cinemachine Camera");
            if (playerCamera == null)
            {
                playerCamera = CreatePlayerCamera();
            }
        }
    }

    private Transform FindPlayerTarget()
    {
        GameObject taggedPlayer = GameObject.FindWithTag("Player");
        if (taggedPlayer != null)
        {
            return taggedPlayer.transform;
        }

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            return playerMovement.transform;
        }

        return null;
    }

    private CinemachineCamera CreateIntroCamera()
    {
        GameObject introObject = new GameObject("Intro Cinemachine Camera");
        introObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
        introObject.transform.SetParent(transform.parent, true);

        CinemachineCamera camera = introObject.AddComponent<CinemachineCamera>();
        camera.Priority.Value = introPriority;
        camera.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
        camera.Lens.OrthographicSize = GetIntroOrthoSize();

        return camera;
    }

    private CinemachineCamera CreatePlayerCamera()
    {
        GameObject playerCameraObject = new GameObject("Player Cinemachine Camera");
        playerCameraObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
        playerCameraObject.transform.SetParent(transform.parent, true);

        CinemachineCamera camera = playerCameraObject.AddComponent<CinemachineCamera>();
        camera.Priority.Value = introPriority - 20;
        camera.Lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
        camera.Lens.OrthographicSize = playerOrthographicSize;

        CinemachineFollow follow = playerCameraObject.AddComponent<CinemachineFollow>();
        follow.FollowOffset = playerFollowOffset;
        follow.TrackerSettings.PositionDamping = playerPositionDamping;

        return camera;
    }

    private float GetIntroOrthoSize()
    {
        Camera baseCamera = GetComponent<Camera>();
        return baseCamera != null ? baseCamera.orthographicSize : 12f;
    }

    private CinemachineCamera FindCameraByName(string cameraName)
    {
        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].name == cameraName)
            {
                return cameras[i];
            }
        }

        return null;
    }
}
