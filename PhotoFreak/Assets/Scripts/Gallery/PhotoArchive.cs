using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Disk format + I/O for the player's photo gallery.
///
/// PNG + sidecar JSON pairs are written to
/// <c>Application.persistentDataPath/Gallery/</c>. Each photo gets a
/// timestamped basename (<c>photo_YYYY-MM-DD_HH-mm-ss</c>) so collisions
/// are vanishingly unlikely and entries naturally sort newest-last.
/// </summary>
public static class PhotoArchive
{
    private const string GALLERY_FOLDER = "Gallery";

    public static string GalleryPath => Path.Combine(Application.persistentDataPath, GALLERY_FOLDER);

    [Serializable]
    public class GalleryEntry
    {
        // ---- Display ----
        public int   starCount;       // Mathf.RoundToInt(result)
        public float result;          // final 0..N weighted score

        // ---- Score modifiers ----
        public float distance;
        public float facing;
        public float size;
        public float focus;
        public float development;
        public float extras;

        // ---- Metadata ----
        public string capturedAtUtc;  // ISO 8601
        public string photoFile;      // file name (no path)
        public string sidecarFile;    // file name (no path)

        public static GalleryEntry FromScore(ScoreParameters s)
        {
            return new GalleryEntry
            {
                starCount   = Mathf.RoundToInt(s.result),
                result      = s.result,
                distance    = s.distance,
                facing      = s.facing,
                size        = s.size,
                focus       = s.focus,
                development = s.development,
                extras      = s.extras,
                capturedAtUtc = DateTime.UtcNow.ToString("o"),
            };
        }
    }

    // ------------------------------------------------------------------
    // Save
    // ------------------------------------------------------------------

    /// <summary>
    /// Encodes <paramref name="photo"/> as PNG to disk, writes a sidecar JSON
    /// holding the score data, and returns the resulting entry. Returns null
    /// on failure (and logs).
    /// </summary>
    public static GalleryEntry SavePhoto(ScoreParameters score)
    {
        if (score.currentPhoto == null)
        {
            Debug.LogWarning("[PhotoArchive] SavePhoto called but currentPhoto is null.");
            return null;
        }

        try
        {
            Directory.CreateDirectory(GalleryPath);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string baseName  = $"photo_{timestamp}";
            string photoName = baseName + ".png";
            string jsonName  = baseName + ".json";
            string photoFull = Path.Combine(GalleryPath, photoName);
            string jsonFull  = Path.Combine(GalleryPath, jsonName);

            byte[] png = score.currentPhoto.EncodeToPNG();
            if (png == null || png.Length == 0)
            {
                Debug.LogWarning("[PhotoArchive] EncodeToPNG returned no bytes — texture may not be readable. " +
                                 "Make sure the source Texture2D is created with isReadable=true (CaptureScreenshotAsTexture is fine).");
                return null;
            }
            File.WriteAllBytes(photoFull, png);

            var entry = GalleryEntry.FromScore(score);
            entry.photoFile   = photoName;
            entry.sidecarFile = jsonName;
            File.WriteAllText(jsonFull, JsonUtility.ToJson(entry, prettyPrint: true));

            Debug.Log($"[PhotoArchive] Saved photo to {photoFull}");
            return entry;
        }
        catch (Exception e)
        {
            Debug.LogError($"[PhotoArchive] Save failed: {e.Message}");
            return null;
        }
    }

    // ------------------------------------------------------------------
    // Load
    // ------------------------------------------------------------------

    /// <summary>Returns every gallery entry on disk, oldest first.</summary>
    public static List<GalleryEntry> LoadAll()
    {
        var result = new List<GalleryEntry>();
        if (!Directory.Exists(GalleryPath)) return result;

        var jsonFiles = Directory.GetFiles(GalleryPath, "*.json");
        Array.Sort(jsonFiles); // timestamped filenames sort chronologically

        foreach (var jsonPath in jsonFiles)
        {
            try
            {
                var entry = JsonUtility.FromJson<GalleryEntry>(File.ReadAllText(jsonPath));
                if (entry != null) result.Add(entry);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PhotoArchive] Skipping unreadable entry '{jsonPath}': {e.Message}");
            }
        }
        return result;
    }

    /// <summary>Returns the most-recently-saved entry, or null if the gallery is empty.</summary>
    public static GalleryEntry LoadMostRecent()
    {
        if (!Directory.Exists(GalleryPath)) return null;

        var jsonFiles = Directory.GetFiles(GalleryPath, "*.json");
        if (jsonFiles.Length == 0) return null;

        Array.Sort(jsonFiles);
        var latest = jsonFiles[jsonFiles.Length - 1];

        try
        {
            return JsonUtility.FromJson<GalleryEntry>(File.ReadAllText(latest));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PhotoArchive] Could not read most-recent entry: {e.Message}");
            return null;
        }
    }

    /// <summary>Loads the PNG for a saved entry as a Texture2D. Returns null on failure.</summary>
    public static Texture2D LoadTexture(GalleryEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.photoFile)) return null;
        var fullPath = Path.Combine(GalleryPath, entry.photoFile);
        if (!File.Exists(fullPath)) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            // Size is replaced by LoadImage; default 2x2 is fine.
            var tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (!tex.LoadImage(bytes))
            {
                UnityEngine.Object.Destroy(tex);
                return null;
            }
            return tex;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PhotoArchive] Failed to load texture: {e.Message}");
            return null;
        }
    }
}
