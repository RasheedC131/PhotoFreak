using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialPage : MonoBehaviour
{
    [Header("Modal Root")]
    [SerializeField] private GameObject modalRoot;

    [Header("Pages")]
    [SerializeField] private Transform pages;
    [SerializeField] private Transform forward;
    [SerializeField] private Transform backward;
    [SerializeField] private TMP_Text pageNumber;
    [SerializeField] private Transform tutorialUI;

    private int currentPage = 0;
    private Transform[] pageArr;

    private void Awake()
    {
        if (modalRoot == null) modalRoot = gameObject;
        pageArr = new Transform[pages.childCount];
        for (int i = 0; i < pageArr.Length; i++) pageArr[i] = pages.GetChild(i);
        modalRoot.SetActive(false);
    }


    public void Open()
    {
        // Reset to the first page every time the tutorial is opened.
        currentPage = 0;
        for (int i = 0; i < pageArr.Length; i++)
            pageArr[i].gameObject.SetActive(i == 0);

        showPage(pageArr[currentPage]);
        hide(backward);
        show(forward);

        modalRoot.SetActive(true);
    }

    public void Close()
    {
        modalRoot.SetActive(false);
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
