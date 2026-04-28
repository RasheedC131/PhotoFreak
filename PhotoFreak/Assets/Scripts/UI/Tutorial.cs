using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialPage : MonoBehaviour
{
    [SerializeField] private Transform pages;
    [SerializeField] private Transform forward;
    [SerializeField] private Transform backward;
    [SerializeField] private TMP_Text pageNumber;
    [SerializeField] private Transform tutorialUI;
    private int currentPage = 0;
    private Transform[] pageArr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        pageArr = new Transform[pages.childCount];
        for (int i = 0; i < pageArr.Length; i++)
        {
            pageArr[i] = pages.GetChild(i);
        }
        showPage(pageArr[currentPage]);
        hide(backward);
    }
    private void showPage(Transform page)
    {
        page.gameObject.SetActive(true);
        pageNumber.text = "Page: " + (currentPage + 1);
    }
    
    private void show(Transform button)
    {
        button.gameObject.SetActive(true);
    }
    private void hide(Transform page)
    {
        page.gameObject.SetActive(false);
    }

    public void forwardPage()
    {
        hide(pageArr[currentPage++]);
        showPage(pageArr[currentPage]);
        if (currentPage >= pages.childCount - 1)
        {
            hide(forward);
        }
        else
        {
            show(forward);
        }
        show(backward);
    }
    public void backwardPage()
    {
        hide(pageArr[currentPage--]);
        showPage(pageArr[currentPage]);
        if (currentPage <= 0)
        {
            hide(backward);
        }
        else
        {
            show(backward);
        }
        show(forward);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
