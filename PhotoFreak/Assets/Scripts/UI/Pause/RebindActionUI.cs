using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// One row in the controls list: an action label, a button showing the current
/// binding and an interactive "press a key" rebind flow.
///
/// Drop on a prefab that has:
///   - TMP_Text with the action name
///   - Button whose label is the binding display string
///   - Optional Reset button
/// then call <see cref="Setup"/> from <see cref="KeyRebindingUI"/>.
/// </summary>
public class RebindActionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text actionLabel;
    [SerializeField] private Button   rebindButton;
    [SerializeField] private TMP_Text rebindLabel;
    [SerializeField] private Button   resetButton;
    [SerializeField] private GameObject waitingOverlay;   // optional "Press any key..."
    [SerializeField] private TMP_Text waitingLabel;       // optional text in the overlay

    private InputAction action;
    private int         bindingIndex;
    private string      controlScheme = "";
    private InputActionRebindingExtensions.RebindingOperation rebindOp;

    /// <summary> Fires when this row finishes rebinding. </summary>
    public event Action OnRebound;

    // ---------------------------------------------------------------------

    public void Setup(InputAction action, int bindingIndex, string displayName = null, string controlScheme = "")
    {
        this.action        = action;
        this.bindingIndex  = bindingIndex;
        this.controlScheme = controlScheme ?? "";

        if (actionLabel != null) actionLabel.text = string.IsNullOrEmpty(displayName) ? action.name : displayName;

        if (rebindButton != null)
        {
            rebindButton.onClick.RemoveAllListeners();
            rebindButton.onClick.AddListener(StartRebind);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetBinding);
        }

        if (waitingOverlay != null) waitingOverlay.SetActive(false);

        RefreshLabel();
    }

    // ---- Rebind flow ----------------------------------------------------

    public void StartRebind()
    {
        if (action == null) return;
        if (rebindOp != null) { rebindOp.Cancel(); rebindOp.Dispose(); rebindOp = null; }

        action.Disable();

        if (waitingOverlay != null) waitingOverlay.SetActive(true);
        if (waitingLabel   != null) waitingLabel.text = $"Press a key for '{actionLabel?.text ?? action.name}'...";
        if (rebindLabel    != null) rebindLabel.text  = "...";

        rebindOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.05f)
            .OnComplete(op => Finish(op))
            .OnCancel (op => Finish(op))
            .Start();
    }

    private void Finish(InputActionRebindingExtensions.RebindingOperation op)
    {
        op.Dispose();
        rebindOp = null;

        action.Enable();
        if (waitingOverlay != null) waitingOverlay.SetActive(false);

        RefreshLabel();
        OnRebound?.Invoke();
    }

    public void ResetBinding()
    {
        if (action == null) return;
        action.RemoveBindingOverride(bindingIndex);
        RefreshLabel();
        OnRebound?.Invoke();
    }

    // ---- Display --------------------------------------------------------

    private void RefreshLabel()
    {
        if (rebindLabel == null || action == null) return;

        rebindLabel.text = action.GetBindingDisplayString(
            bindingIndex,
            InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
    }

    void OnDestroy()
    {
        if (rebindOp != null)
        {
            rebindOp.Dispose();
            rebindOp = null;
        }
    }
}
