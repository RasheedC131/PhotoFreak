using UnityEngine;

[System.Serializable]
public struct CaptureData
{
    public Vector3 playerPos;
    public Vector3 subjectPos;
    public Vector3 subjectForward;

    public float fov;
    public float focus;
    public bool manualFocus;

    public int extras; //subjects in the background
};