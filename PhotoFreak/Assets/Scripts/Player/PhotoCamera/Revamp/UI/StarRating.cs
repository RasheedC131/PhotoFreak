using UnityEngine;
using UnityEngine.UI;

public class StarRating : MonoBehaviour
{
    [SerializeField] private Image[] starImages; 
    [SerializeField] private Color earnedStarColor = Color.yellow; 
    [SerializeField] private Color emptyStarColor = Color.gray;


    public void DisplayStars(int starCount)
    {
        ResetStars();
       
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starCount) starImages[i].color = earnedStarColor; 
            else starImages[i].color = emptyStarColor; 
        }
        
    }

    private void ResetStars()
    { 
        foreach (Image star in starImages)
        {
            star.color = Color.clear; 
        }
    }
}
