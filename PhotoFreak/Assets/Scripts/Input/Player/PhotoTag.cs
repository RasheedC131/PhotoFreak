using UnityEngine;

public class PhotoTag : MonoBehaviour
{   
    public enum SubjectType
    {
        Monster,
        Elite
    };

    [Header("Subject Description")]
    public SubjectType type;
    public int poseScore;

    public void SetPoseScore(int score)
    {
        poseScore = score;
    }

}
