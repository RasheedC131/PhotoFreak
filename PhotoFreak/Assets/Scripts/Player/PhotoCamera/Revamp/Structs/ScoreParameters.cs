using UnityEngine;

[System.Serializable]
public struct ScoreParameters
{
    public float distance; //How far is the subject
    public float facing; //Is the subject facing the camera
    public float size; //Does the subject fit in the camera
    public float focus; //Taken from either Manual or Auto Focus
    public float development; //Percent developed

    public float extras; //Amount of subjects in the bg of a photo

    public float result; //The final Photo Score
};
