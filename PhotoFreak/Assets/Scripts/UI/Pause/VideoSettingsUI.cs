using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Video tab of the settings panel.
/// Exposes monitor / resolution / fullscreen mode / vsync / fps cap.
/// Changes are staged into a "pending" GameSettings and only committed on Apply.
/// </summary>
public class VideoSettingsUI : MonoBehaviour
{
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown monitorDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown frameRateDropdown;

    [Header("Toggle")]
    [SerializeField] private Toggle vsyncToggle;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button revertButton;

    // Cached lookup data.
    private readonly List<DisplayInfo> displays    = new();
    private readonly List<Resolution>  resolutions = new();
    private static readonly int[] frameRateOptions = { -1, 30, 60, 75, 120, 144, 165, 240 };

    // Pending edits – committed on Apply.
    private GameSettings pending;

    // ---------------------------------------------------------------------

    void OnEnable()
    {
        // Re-read what is currently in effect every time the panel opens so we
        // never show stale data.
        pending = GameSettings.Load();
        BuildDropdowns();
        WireListeners();
        SyncUiFromPending();
    }

    void OnDisable()
    {
        UnwireListeners();
    }

    // ---- Build dropdown contents ----------------------------------------

    private void BuildDropdowns()
    {
        // Monitors -----------------------------------------------------------
        displays.Clear();
        Screen.GetDisplayLayout(displays);

        if (monitorDropdown != null)
        {
            monitorDropdown.ClearOptions();
            var labels = new List<string>();
            for (int i = 0; i < displays.Count; i++)
            {
                var d = displays[i];
                labels.Add($"Display {i + 1} – {d.name} ({d.width}×{d.height})");
            }
            if (labels.Count == 0)
            {
                labels.Add($"Display 1 ({Screen.width}×{Screen.height})");
            }
            monitorDropdown.AddOptions(labels);
        }

        // Resolutions --------------------------------------------------------
        // Group identical width/height combos and keep the highest-Hz variant.
        resolutions.Clear();
        resolutions.AddRange(Screen.resolutions
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.OrderByDescending(r => (double)r.refreshRateRatio.value).First())
            .OrderByDescending(r => r.width * r.height));

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resolutions
                .Select(r => $"{r.width} × {r.height}  @ {Mathf.RoundToInt((float)r.refreshRateRatio.value)} Hz")
                .ToList());
        }

        // Window mode --------------------------------------------------------
        if (windowModeDropdown != null)
        {
            windowModeDropdown.ClearOptions();
            windowModeDropdown.AddOptions(new List<string>
            {
                "Exclusive Fullscreen",
                "Borderless Window",
                "Windowed",
            });
        }

        // Frame rate cap -----------------------------------------------------
        if (frameRateDropdown != null)
        {
            frameRateDropdown.ClearOptions();
            frameRateDropdown.AddOptions(frameRateOptions
                .Select(f => f < 0 ? "Unlimited" : $"{f} FPS")
                .ToList());
        }
    }

    private void WireListeners()
    {
        if (monitorDropdown    != null) monitorDropdown.onValueChanged.AddListener(OnMonitorChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (windowModeDropdown != null) windowModeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
        if (frameRateDropdown  != null) frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        if (vsyncToggle        != null) vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        if (applyButton        != null) applyButton.onClick.AddListener(OnApplyClicked);
        if (revertButton       != null) revertButton.onClick.AddListener(OnRevertClicked);
    }

    private void UnwireListeners()
    {
        if (monitorDropdown    != null) monitorDropdown.onValueChanged.RemoveListener(OnMonitorChanged);
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        if (windowModeDropdown != null) windowModeDropdown.onValueChanged.RemoveListener(OnWindowModeChanged);
        if (frameRateDropdown  != null) frameRateDropdown.onValueChanged.RemoveListener(OnFrameRateChanged);
        if (vsyncToggle        != null) vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
        if (applyButton        != null) applyButton.onClick.RemoveListener(OnApplyClicked);
        if (revertButton       != null) revertButton.onClick.RemoveListener(OnRevertClicked);
    }

    // ---- Sync UI <-> pending --------------------------------------------

    private void SyncUiFromPending()
    {
        if (monitorDropdown != null)
        {
            int idx = Mathf.Clamp(pending.monitorIndex, 0, Mathf.Max(0, monitorDropdown.options.Count - 1));
            monitorDropdown.SetValueWithoutNotify(idx);
        }

        if (resolutionDropdown != null)
        {
            int match = resolutions.FindIndex(r => r.width == pending.width && r.height == pending.height);
            if (match < 0) match = 0;
            resolutionDropdown.SetValueWithoutNotify(match);
        }

        if (windowModeDropdown != null)
        {
            windowModeDropdown.SetValueWithoutNotify(ModeToIndex(pending.fullScreenMode));
        }

        if (frameRateDropdown != null)
        {
            int frIdx = System.Array.IndexOf(frameRateOptions, pending.targetFrameRate);
            if (frIdx < 0) frIdx = 0;
            frameRateDropdown.SetValueWithoutNotify(frIdx);
        }

        if (vsyncToggle != null) vsyncToggle.SetIsOnWithoutNotify(pending.vsync);
    }

    // ---- Listeners -------------------------------------------------------

    private void OnMonitorChanged(int i)    => pending.monitorIndex = i;

    private void OnResolutionChanged(int i)
    {
        if (i < 0 || i >= resolutions.Count) return;
        var r = resolutions[i];
        pending.width  = r.width;
        pending.height = r.height;
        pending.SetRefreshRate(r.refreshRateRatio);
    }

    private void OnWindowModeChanged(int i) => pending.fullScreenMode = IndexToMode(i);

    private void OnFrameRateChanged(int i)
    {
        if (i < 0 || i >= frameRateOptions.Length) return;
        pending.targetFrameRate = frameRateOptions[i];
    }

    private void OnVSyncChanged(bool v) => pending.vsync = v;

    private void OnApplyClicked()
    {
        pending.Apply();
        pending.Save();
    }

    private void OnRevertClicked()
    {
        pending = GameSettings.Load();
        SyncUiFromPending();
    }

    // ---- Window mode <-> dropdown index ---------------------------------

    private static int ModeToIndex(FullScreenMode m) => m switch
    {
        FullScreenMode.ExclusiveFullScreen => 0,
        FullScreenMode.FullScreenWindow    => 1,
        FullScreenMode.Windowed            => 2,
        _                                   => 1,
    };

    private static FullScreenMode IndexToMode(int i) => i switch
    {
        0 => FullScreenMode.ExclusiveFullScreen,
        1 => FullScreenMode.FullScreenWindow,
        2 => FullScreenMode.Windowed,
        _ => FullScreenMode.FullScreenWindow,
    };
}
