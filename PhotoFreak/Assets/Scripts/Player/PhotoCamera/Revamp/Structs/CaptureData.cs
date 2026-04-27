using UnityEngine;

[System.Serializable]
public struct CaptureData
{
    public Vector3 playerPos;
    public Vector3 subjectPos;
    public Vector3 subjectForward;

    public Camera camera;
    public RaycastHit subject;

    public float focus;
    public bool manualFocus;

    public Texture2D currentPhoto;

    public int extras; //subjects in the background
};