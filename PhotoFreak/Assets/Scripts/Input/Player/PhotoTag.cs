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

}
