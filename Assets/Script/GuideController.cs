using UnityEngine;

public class GuideController : MonoBehaviour
{
    [SerializeField] private GameObject guide1;
    [SerializeField] private GameObject setting1;
    [SerializeField] private GameObject quit1;
    [SerializeField] private GameObject[] guidePages1; // 複数のページを一つにまとめる
    [SerializeField] private GameObject guide2;
    [SerializeField] private GameObject setting2;
    [SerializeField] private GameObject quit2;
    [SerializeField] private GameObject[] guidePages2;
    public int channel1;
    public int channel2;

    //--------初期設定処理開始-----------
    void Start()
    {
        guide1.SetActive(false);
        setting1.SetActive(true);
        quit1.SetActive(false);
        guide2.SetActive(false);
        setting2.SetActive(true);
        quit2.SetActive(false);
        channel1 = 0;
        channel2 = 0;
    }
    //--------初期設定処理終わり----------

    //--------ボタン操作処理開始-----------
    public void ClickGuideBtn1()
    {
        setting1.SetActive(false);
        guide1.SetActive(true);
    }
    public void ClickGuideBtn2()
    {
        setting2.SetActive(false);
        guide2.SetActive(true);
    }

    public void ClickSetBtn1()
    {
        setting1.SetActive(true);
        guide1.SetActive(false);
        quit1.SetActive(false);
    }
    public void ClickSetBtn2()
    {
        setting2.SetActive(true);
        guide2.SetActive(false);
        quit2.SetActive(false);
    }

    public void ClickQuitBtn1()
    {
        setting1.SetActive(false);
        quit1.SetActive(true);
    }

    public void ClickQuitBtn2()
    {
        setting2.SetActive(false);
        quit2.SetActive(true);
    }

    public void GuideBtn1(int channel1)
    {
        guide1.SetActive(false);
        foreach (var page in guidePages1)
        {
            page.SetActive(false);
        }
        if (channel1 >= 0 && channel1 < guidePages1.Length)
        {
            guidePages1[channel1].SetActive(true);
        }
    }
    public void GuideBtn2(int channel2)
    {
        guide2.SetActive(false);
        foreach (var page in guidePages2)
        {
            page.SetActive(false);
        }
        if (channel2 >= 0 && channel2 < guidePages2.Length)
        {
            guidePages2[channel2].SetActive(true);
        }
    }
    //--------ボタン操作処理終わり----------
}