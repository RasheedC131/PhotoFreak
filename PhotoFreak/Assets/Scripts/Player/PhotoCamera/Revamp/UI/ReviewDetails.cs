using UnityEngine;
using TMPro;

public class ReviewDetails : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] scoreTexts;

    public void FillDetails(ScoreParameters data)
    {
        float[] scores = new float[]
        {
            data.distance,
            data.facing,
            data.focus,
            data.size,
            data.development
        };

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (i == scoreTexts.Length - 1)
            {
                scoreTexts[i].text = Mathf.RoundToInt(data.extras).ToString();
            } 
            else
            {
                int rounded = Mathf.RoundToInt(scores[i]);
                scoreTexts[i].text = $"{rounded}/5";  
            }
            
        }

        

    }


}
