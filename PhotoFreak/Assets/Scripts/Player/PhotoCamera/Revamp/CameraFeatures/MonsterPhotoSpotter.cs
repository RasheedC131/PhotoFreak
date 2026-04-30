using UnityEngine;

/// <summary>
/// While the camera is raised, scans for monsters within
/// <see cref="MonsterSettings.photoDetectRadius"/>. Any monster inside that
/// sphere is flipped into hunt-the-player mode — but only if monsters are the
/// majority of the crowd, per <see cref="CrowdStateManager.MonsterMajority"/>.
///
/// Drop this on the same GameObject as <see cref="CameraController"/> (or any
/// transform you want to use as the detection origin) and wire the controller
/// reference. Replaces the legacy logic that used to live in
/// <c>Legacy/PhotoCamera.Update()</c>.
/// </summary>
[DisallowMultipleComponent]
public class MonsterPhotoSpotter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("New camera system. Used to read the raised/lowered state.")]
    [SerializeField] private CameraController controller;

    [Tooltip("Optional override for the detection origin. If null, this transform is used.")]
    [SerializeField] private Transform detectionOrigin;

    void Awake()
    {
        if (controller == null) controller = GetComponentInParent<CameraController>();
        if (detectionOrigin == null) detectionOrigin = transform;
    }

    void Update()
    {
        if (controller == null || !controller.getCameraState()) return;

        MonsterSettings ms = MonsterSettings.Instance;
        if (ms == null) return;

        bool monsterMajority = CrowdStateManager.Instance != null
            && CrowdStateManager.Instance.MonsterMajority;
        if (!monsterMajority) return;

        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
        Collider[] nearby = Physics.OverlapSphere(origin, ms.photoDetectRadius);

        foreach (Collider col in nearby)
        {
            AIContext monsterCtx = col.GetComponentInParent<AIContext>();
            if (monsterCtx == null || !monsterCtx.isMonster || monsterCtx.isHuntingPlayer) continue;

            monsterCtx.isHuntingPlayer = true;
            Debug.Log($"[{monsterCtx.gameObject.name}] Spotted raised camera (monster majority active) — entering hunt mode.");
        }
    }
}
