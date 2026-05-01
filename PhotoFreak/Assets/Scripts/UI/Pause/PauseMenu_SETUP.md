# Pause Menu — Unity Editor Setup Guide

These scripts are now in `Assets/Scripts/UI/Pause/`:

| Script | Purpose |
|---|---|
| `PauseScreen.cs` | Root pause-menu controller. Listens to `GlobalGameState`, toggles panels, wires buttons. |
| `GameSettings.cs` | Plain class. Loads / saves / applies video settings to PlayerPrefs. |
| `VideoSettingsUI.cs` | Drives the Video tab (monitor / resolution / fullscreen / vsync / fps). |
| `KeyRebindingUI.cs` | Drives the Controls tab. Spawns one rebind row per action. |
| `RebindActionUI.cs` | Component for a single rebind row prefab. |
| `SettingsBootstrap.cs` | Applies saved settings + rebinds at game start. |

> The `PauseScreen.cs` already in `Assets/Scripts/UI/` was replaced — same file, full implementation.

---

## 1. Bootstrap object (do this once)

1. In your **first** scene (e.g. `MainMenu`), create an empty `GameObject` called **SettingsBootstrap**.
2. Add the **`SettingsBootstrap`** component.
3. Drag `Assets/PlayerControls.inputactions` into the `Input Actions` field.
4. Leave **Don't Destroy On Load** ticked.

This loads the player's saved video + key-bind settings before anything else.

---

## 2. Pause Canvas

In your gameplay scene (`MainGame/...`):

1. **GameObject → UI → Canvas** → rename to `PauseCanvas`.
   - Render Mode: `Screen Space - Overlay`.
   - On the Canvas, add component **`PauseScreen`**.
2. Inside the Canvas, create two empty children:
   - `PausePanel`
   - `SettingsPanel`
3. Both should be set inactive — the script enables / disables them.

### 2a. PausePanel children

Add UI buttons under `PausePanel`:

- `ResumeButton` → `Button (TMP)` with label *Resume*.
- `SettingsButton` → label *Settings*.
- `QuitToMenuButton` → label *Quit to Menu*.
- `QuitToDesktopButton` → label *Quit to Desktop*.

### 2b. Wire PauseScreen

Select the `PauseCanvas` and drag references into the `PauseScreen` component:

| Field | Drag |
|---|---|
| Pause Panel | `PausePanel` |
| Settings Panel | `SettingsPanel` |
| Resume Button | `ResumeButton` |
| Settings Button | `SettingsButton` |
| Quit To Menu Button | `QuitToMenuButton` |
| Quit To Desktop Button | `QuitToDesktopButton` |
| Settings Back Button | (the back button you'll add in step 3) |
| Main Menu Scene Name | `Main Scene` (or whatever your menu is named) |
| Input Manager | drag your scene's `InputManager` GameObject |

> No need to call `Time.timeScale`. The existing `GlobalGameState` already handles freezing the game when paused, and `PauseScreen` listens to its `onGamePaused` / `onGameResumed` events.

---

## 3. SettingsPanel layout

Inside `SettingsPanel`:

1. Add a horizontal tab bar with two buttons (`VideoTabButton`, `ControlsTabButton`).
2. Add two child panels: `VideoTab`, `ControlsTab`.
3. Add a `BackButton` (label *Back*) — drag it into the `Settings Back Button` field on `PauseScreen`.

### 3a. Tab switching

Easiest approach: on each tab button, in the OnClick:
- `VideoTabButton.OnClick` → `VideoTab.SetActive(true)`, `ControlsTab.SetActive(false)`
- `ControlsTabButton.OnClick` → reverse

(Or write a 10-line `TabGroup` script if you want it cleaner.)

---

## 4. Video tab

Inside `VideoTab`, add a vertical layout with these rows:

| Label | Control | Component |
|---|---|---|
| Monitor | `TMP_Dropdown` | `MonitorDropdown` |
| Resolution | `TMP_Dropdown` | `ResolutionDropdown` |
| Window Mode | `TMP_Dropdown` | `WindowModeDropdown` |
| Frame Rate | `TMP_Dropdown` | `FrameRateDropdown` |
| V-Sync | `Toggle` | `VsyncToggle` |

At the bottom, add **Apply** and **Revert** buttons.

Add the **`VideoSettingsUI`** component to `VideoTab` and drag every dropdown / toggle / button into the matching slot.

> Dropdown options are populated **at runtime** — leave the inspector lists empty.

---

## 5. Controls tab

### 5a. Rebind row prefab

1. Create a new prefab: `RebindRow.prefab` in `Assets/prefabs/UI/`.
2. Layout: a horizontal row with
   - `TMP_Text` "ActionLabel" (left)
   - `Button` "RebindButton" with a `TMP_Text` child "RebindLabel" inside it (centre)
   - `Button` "ResetButton" with text "↺" (right)
   - Optional: a `WaitingOverlay` GameObject (full-screen panel, default inactive) with a `TMP_Text` "WaitingLabel" saying "Press a key…"
3. Add the **`RebindActionUI`** component to the prefab root and drag the four+ refs into it.

### 5b. ControlsTab

Inside `ControlsTab`:

1. Add a `Scroll View` → its **Content** transform is where rebind rows will spawn.
2. Add a `ResetAllButton` at the bottom.
3. Add the **`KeyRebindingUI`** component to `ControlsTab`.

Drag in:

| Field | Drag |
|---|---|
| Input Actions | `Assets/PlayerControls.inputactions` |
| Row Prefab | `RebindRow.prefab` |
| Row Parent | the Scroll View's Content transform |
| Reset All Button | `ResetAllButton` |

Now expand the **Entries** list — add one element per binding you want to expose:

| Action Name | Display Name | Binding Index |
|---|---|---|
| `Movement` | Move Forward | 1 |
| `Movement` | Move Back | 2 |
| `Movement` | Move Left | 3 |
| `Movement` | Move Right | 4 |
| `Sprint` | Sprint | 0 |
| `Crouch` | Crouch | 0 |
| `Interact` | Interact | 0 |
| `Aim` | Aim | 0 |
| `Shoot` | Shoot | 0 |
| `Pause` | Pause | 0 |

> WASD lives inside a 2D-vector composite — index `0` is the composite root (no key), indices `1-4` are up / down / left / right. That's why Movement uses 1-4.

---

## 6. Quick smoke test

1. Press Play. The game should start unpaused.
2. Hit **Esc**. `GlobalGameState` flips to `PAUSED`, your `PauseCanvas` shows.
3. Click **Resume** → game resumes (also tested with Esc).
4. Click **Settings → Video** → change resolution → **Apply** → window changes immediately.
5. Restart the game. Saved settings should auto-apply at launch (via `SettingsBootstrap`).
6. **Settings → Controls** → click any rebind button → press a key → label updates.
7. Restart. Rebinds should persist.

---

## 7. Where things are saved

| Data | Key |
|---|---|
| Video settings | `PlayerPrefs["PhotoFreak.GameSettings.v1"]` (JSON) |
| Key rebinds | `PlayerPrefs["PhotoFreak.Rebinds.v1"]` (Input System override JSON) |

To wipe everything during dev: `PlayerPrefs.DeleteAll()` from any Editor script, or just delete the two keys.

---

## 8. Notes / gotchas

- **Display switching** uses `Screen.MoveMainWindowTo` — works on Windows / macOS standalone players, not in the Editor or in WebGL.
- **Resolution list** is filtered to one entry per `width × height` (the highest available refresh rate). Change in `VideoSettingsUI.BuildDropdowns` if you want every refresh-rate variant separately.
- **Frame rate cap** sets `Application.targetFrameRate`. With VSync enabled, the GPU still locks to the monitor's refresh rate — the cap acts as an upper bound on top of that.
- The **Pause action** (Escape) is in `Ground` map. The **Resume action** (Escape) is in `UIMap`. `InputManager.TogglePause()` swaps the active map, so Esc does the right thing in either state.
