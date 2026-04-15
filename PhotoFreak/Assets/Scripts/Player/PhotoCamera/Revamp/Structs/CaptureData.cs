using UnityEngine;

[System.Serializable]
public struct CaptureData
{
    public Vector3 playerPos;
    public Vector3 subjectPos;
    public Vector3 subjectForward;

    public int extras; //subjects in the background
};