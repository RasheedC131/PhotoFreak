using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Drives the Controls tab of the settings panel.
/// Spawns one <see cref="RebindActionUI"/> row per rebindable action and
/// persists overrides as JSON in PlayerPrefs.
/// </summary>
public class KeyRebindingUI : MonoBehaviour
{
    [System.Serializable]
    public class RebindEntry
    {
        [Tooltip("Action name as it appears in the .inputactions asset (e.g. 'Movement/up' for composite parts).")]
        public string actionName;
        [Tooltip("Display name shown to the player. Leave blank to use the action name.")]
        public string displayName;
        [Tooltip("Index into the binding list for this action. Use 0 for single bindings, or the composite part index for WASD-style bindings.")]
        public int bindingIndex = 0;
    }

    [Header("Asset")]
    [Tooltip("Reference to the InputActionAsset (drag the .inputactions asset here).")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("List")]
    [Tooltip("Prefab with a RebindActionUI on the root.")]
    [SerializeField] private RebindActionUI rowPrefab;
    [Tooltip("Parent transform that gets one row per entry.")]
    [SerializeField] private Transform rowParent;

    [Header("Bindings to expose")]
    [SerializeField] private List<RebindEntry> entries = new();

    [Header("Buttons")]
    [SerializeField] private Button resetAllButton;

    private const string PrefKey = "PhotoFreak.Rebinds.v1";
    private readonly List<RebindActionUI> rows = new();

    // ---------------------------------------------------------------------

    void Awake()
    {
        LoadOverrides();
    }

    void OnEnable()
    {
        Build();

        if (resetAllButton != null)
        {
            resetAllButton.onClick.RemoveAllListeners();
            resetAllButton.onClick.AddListener(ResetAll);
        }
    }

    void OnDisable()
    {
        if (resetAllButton != null) resetAllButton.onClick.RemoveListener(ResetAll);
    }

    // ---- Build ----------------------------------------------------------

    private void Build()
    {
        ClearRows();

        if (inputActions == null || rowPrefab == null || rowParent == null)
        {
            Debug.LogWarning("[KeyRebindingUI] Missing required references – nothing to build.");
            return;
        }

        foreach (var entry in entries)
        {
            var action = inputActions.FindAction(entry.actionName, throwIfNotFound: false);
            if (action == null)
            {
                Debug.LogWarning($"[KeyRebindingUI] Action '{entry.actionName}' not found in {inputActions.name}.");
                continue;
            }

            int idx = Mathf.Clamp(entry.bindingIndex, 0, Mathf.Max(0, action.bindings.Count - 1));

            var row = Instantiate(rowPrefab, rowParent);
            row.Setup(action, idx, entry.displayName);
            row.OnRebound += SaveOverrides;
            rows.Add(row);
        }
    }

    private void ClearRows()
    {
        foreach (var row in rows)
        {
            if (row == null) continue;
            row.OnRebound -= SaveOverrides;
            Destroy(row.gameObject);
        }
        rows.Clear();
    }

    private void ResetAll()
    {
        if (inputActions == null) return;
        foreach (var map in inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        SaveOverrides();
        // Rebuild so labels refresh.
        Build();
    }

    // ---- Persistence ----------------------------------------------------

    private void SaveOverrides()
    {
        if (inputActions == null) return;
        PlayerPrefs.SetString(PrefKey, inputActions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    private void LoadOverrides()
    {
        if (inputActions == null) return;
        if (!PlayerPrefs.HasKey(PrefKey)) return;
        var json = PlayerPrefs.GetString(PrefKey);
        if (string.IsNullOrEmpty(json)) return;
        inputActions.LoadBindingOverridesFromJson(json);
    }
}
