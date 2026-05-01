using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main-menu modal that lets the player browse every saved photo with
/// Previous / Next buttons. Wire <see cref="Open"/> and <see cref="Close"/>
/// to your buttons' OnClick events.
/// </summary>
public class GalleryModal : MonoBehaviour
{
    [Header("Root")]
    [Tooltip("The modal panel itself. Toggled on Open / Close. If null, this GameObject is used.")]
    [SerializeField] private GameObject modalRoot;

    [Header("Photo")]
    [SerializeField] private RawImage  photoImage;
    [Tooltip("Shown when there are no saved photos yet.")]
    [SerializeField] private GameObject emptyState;
    [Tooltip("Container that holds the photo + scores. Hidden when emptyState is shown.")]
    [SerializeField] private GameObject contentRoot;

    [Header("Score Display")]
    [SerializeField] private StarRating    starRating;
    [SerializeField] private ReviewDetails reviewDetails;

    [Header("Optional Info")]
    [Tooltip("Displays the capture timestamp for the current photo.")]
    [SerializeField] private TMP_Text capturedAtText;
    [Tooltip("Displays e.g. '3 / 7'. Leave empty to skip.")]
    [SerializeField] private TMP_Text photoCountText;

    [Header("Navigation")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    // ---- State -----------------------------------------------------------
    private List<PhotoArchive.GalleryEntry> _entries = new();
    private int      _index;
    private Texture2D loadedTexture;

    void Awake()
    {
        if (modalRoot == null) modalRoot = gameObject;
        modalRoot.SetActive(false);
    }

    void Start()
    {
        if (prevButton != null) prevButton.onClick.AddListener(NavigatePrev);
        if (nextButton != null) nextButton.onClick.AddListener(NavigateNext);
    }

    void OnDestroy()
    {
        ReleaseLoadedTexture();
    }

    // ---- Public API ------------------------------------------------------

    public void Open()
    {
        modalRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        modalRoot.SetActive(false);
        ReleaseLoadedTexture();
    }

    /// <summary>Re-reads disk and resets to the most recent photo.</summary>
    public void Refresh()
    {
        _entries = PhotoArchive.LoadAll();
        // Start on the newest photo (last in the list).
        _index = _entries.Count > 0 ? _entries.Count - 1 : 0;
        ShowCurrent();
    }

    // ---- Navigation ------------------------------------------------------

    public void NavigatePrev()
    {
        if (_entries.Count == 0) return;
        _index = Mathf.Max(0, _index - 1);
        ShowCurrent();
    }

    public void NavigateNext()
    {
        if (_entries.Count == 0) return;
        _index = Mathf.Min(_entries.Count - 1, _index + 1);
        ShowCurrent();
    }

    // ---- Display ---------------------------------------------------------

    private void ShowCurrent()
    {
        if (_entries.Count == 0)
        {
            ShowEmptyState();
            return;
        }

        ShowEntry(_entries[_index]);
        UpdateNavButtons();
        UpdateCountText();
    }

    private void ShowEmptyState()
    {
        if (contentRoot != null) contentRoot.SetActive(false);
        if (emptyState  != null) emptyState.SetActive(true);

        if (prevButton != null) prevButton.interactable = false;
        if (nextButton != null) nextButton.interactable = false;
        if (photoCountText != null) photoCountText.text = string.Empty;
    }

    private void ShowEntry(PhotoArchive.GalleryEntry entry)
    {
        if (emptyState  != null) emptyState.SetActive(false);
        if (contentRoot != null) contentRoot.SetActive(true);

        // Photo texture
        ReleaseLoadedTexture();
        loadedTexture = PhotoArchive.LoadTexture(entry);
        if (photoImage != null) photoImage.texture = loadedTexture;

        // Score widgets
        var score = new ScoreParameters
        {
            result      = entry.result,
            distance    = entry.distance,
            facing      = entry.facing,
            size        = entry.size,
            focus       = entry.focus,
            development = entry.development,
            extras      = entry.extras,
            currentPhoto = loadedTexture,
        };

        if (starRating    != null) starRating.DisplayStars(entry.starCount);
        if (reviewDetails != null) reviewDetails.FillDetails(score);

        // Timestamp
        if (capturedAtText != null && !string.IsNullOrEmpty(entry.capturedAtUtc))
        {
            if (System.DateTime.TryParse(entry.capturedAtUtc, out var utc))
                capturedAtText.text = utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            else
                capturedAtText.text = entry.capturedAtUtc;
        }
    }

    private void UpdateNavButtons()
    {
        if (prevButton != null) prevButton.interactable = _index > 0;
        if (nextButton != null) nextButton.interactable = _index < _entries.Count - 1;
    }

    private void UpdateCountText()
    {
        if (photoCountText != null && _entries.Count > 0)
            photoCountText.text = $"{_index + 1} / {_entries.Count}";
    }

    private void ReleaseLoadedTexture()
    {
        if (loadedTexture != null)
        {
            Destroy(loadedTexture);
            loadedTexture = null;
        }
    }
}
