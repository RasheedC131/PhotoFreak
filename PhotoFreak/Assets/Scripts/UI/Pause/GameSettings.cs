using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plain serialisable container for the player-tweakable video settings.
/// Saved to PlayerPrefs as JSON and re-applied on load.
/// </summary>
[Serializable]
public class GameSettings
{
    public int  monitorIndex     = 0;
    public int  width            = 1920;
    public int  height           = 1080;
    public uint refreshNumerator = 60000;
    public uint refreshDenominator = 1000;
    public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
    public bool vsync            = true;
    public int  targetFrameRate  = -1;       // -1 = unlimited

    private const string PrefKey = "PhotoFreak.GameSettings.v1";

    // ---- Persistence ------------------------------------------------------

    public static GameSettings Load()
    {
        if (PlayerPrefs.HasKey(PrefKey))
        {
            try
            {
                var loaded = JsonUtility.FromJson<GameSettings>(PlayerPrefs.GetString(PrefKey));
                if (loaded != null) return loaded;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GameSettings] Failed to parse saved settings: {e.Message}. Falling back to defaults.");
            }
        }

        return Defaults();
    }

    public void Save()
    {
        PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(this));
        PlayerPrefs.Save();
    }

    public static GameSettings Defaults()
    {
        var current = Screen.currentResolution;
        return new GameSettings
        {
            monitorIndex       = 0,
            width              = current.width  > 0 ? current.width  : 1920,
            height             = current.height > 0 ? current.height : 1080,
            refreshNumerator   = current.refreshRateRatio.numerator   > 0 ? current.refreshRateRatio.numerator   : 60000u,
            refreshDenominator = current.refreshRateRatio.denominator > 0 ? current.refreshRateRatio.denominator : 1000u,
            fullScreenMode     = Screen.fullScreenMode,
            vsync              = QualitySettings.vSyncCount > 0,
            targetFrameRate    = Application.targetFrameRate,
        };
    }

    // ---- Apply ------------------------------------------------------------

    public void Apply()
    {
        // Move the main window to the chosen display before changing resolution.
        var displays = new List<DisplayInfo>();
        Screen.GetDisplayLayout(displays);
        if (displays.Count > 0 && monitorIndex >= 0 && monitorIndex < displays.Count)
        {
            var target = displays[monitorIndex];
            if (Screen.mainWindowDisplayInfo.name != target.name)
            {
                // Move window to the top-left of the target display's work area.
                var topLeft = new Vector2Int(target.workArea.x, target.workArea.y);
                Screen.MoveMainWindowTo(target, topLeft);
            }
        }

        // Resolution + fullscreen mode.
        var rate = new RefreshRate
        {
            numerator   = refreshNumerator   == 0 ? 60000u : refreshNumerator,
            denominator = refreshDenominator == 0 ? 1000u  : refreshDenominator,
        };
        Screen.SetResolution(Mathf.Max(640, width), Mathf.Max(480, height), fullScreenMode, rate);

        // VSync + frame rate cap.
        QualitySettings.vSyncCount   = vsync ? 1 : 0;
        Application.targetFrameRate  = targetFrameRate;
    }

    // ---- Helpers ----------------------------------------------------------

    public float RefreshHz =>
        refreshDenominator == 0 ? 0f : (float)refreshNumerator / refreshDenominator;

    public void SetRefreshRate(RefreshRate r)
    {
        refreshNumerator   = r.numerator;
        refreshDenominator = r.denominator;
    }
}
