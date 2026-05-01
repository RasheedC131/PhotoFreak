using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyRebindingUI : MonoBehaviour
{
    [System.Serializable]
    public class RebindEntry
    {
        public string actionName;
        public string displayName;
        public int bindingIndex = 0;
    }

    [Header("Asset")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("List")]
    [SerializeField] private RebindActionUI rowPrefab;
    [SerializeField] private Transform rowParent;

    [Header("Bindings to expose")]
    [SerializeField] private bool autoPopulate = false;
    [SerializeField] private List<string> autoPopulateSkipMaps = new() { "UIMap" };
    [SerializeField] private bool autoPopulateSkipMouseButtons = false;

    [SerializeField] private List<RebindEntry> entries = new();

    [Header("Buttons")]
    [SerializeField] private Button resetAllButton;

    private const string PrefKey = "PhotoFreak.Rebinds.v1";
    private readonly List<RebindActionUI> rows = new();

    // 

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


    private void Build()
    {
        ClearRows();

        if (inputActions == null || rowPrefab == null || rowParent == null)
        {
            Debug.LogWarning("[KeyRebindingUI] Missing required references nothing to build.");
            return;
        }

        if (autoPopulate) BuildAutoPopulated();
        else              BuildFromManualEntries();
    }

    private void BuildAutoPopulated()
    {
        foreach (var map in inputActions.actionMaps)
        {
            if (map == null) continue;
            if (autoPopulateSkipMaps != null && autoPopulateSkipMaps.Contains(map.name)) continue;

            foreach (var action in map.actions)
            {
                if (action == null) continue;

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];

                    if (binding.isComposite) continue;

                    if (autoPopulateSkipMouseButtons && IsMouseButton(binding.effectivePath)) continue;

                    string display = binding.isPartOfComposite ? $"{action.name} ({binding.name})" : action.name;

                    var row = Instantiate(rowPrefab, rowParent);
                    row.Setup(action, i, display);
                    row.OnRebound += SaveOverrides;
                    rows.Add(row);
                }
            }
        }
    }

    private void BuildFromManualEntries()
    {
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

    private static bool IsMouseButton(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return path.StartsWith("<Mouse>/") && path.EndsWith("Button");
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
