using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Applies saved video settings and key-binding overrides as soon as the game
/// loads. Place ONE of these in your bootstrap scene (or mark it
/// DontDestroyOnLoad) so settings persist across scene changes.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class SettingsBootstrap : MonoBehaviour
{
    [Tooltip("InputActionAsset to load binding overrides into. Drag the .inputactions asset here.")]
    [SerializeField] private InputActionAsset inputActions;

    [Tooltip("If true, this object survives scene loads.")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private const string RebindPrefKey = "PhotoFreak.Rebinds.v1";

    void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        // 1. Video / display settings
        var settings = GameSettings.Load();
        settings.Apply();

        // 2. Key-binding overrides
        if (inputActions != null && PlayerPrefs.HasKey(RebindPrefKey))
        {
            var json = PlayerPrefs.GetString(RebindPrefKey);
            if (!string.IsNullOrEmpty(json))
            {
                inputActions.LoadBindingOverridesFromJson(json);
            }
        }
    }
}
