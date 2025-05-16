using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [Header("Tutorial UI Objects")]
    [SerializeField] private GameObject case1;
    [SerializeField] private GameObject case2;
    [SerializeField] private GameObject case3;
    [SerializeField] private GameObject case4;
    [SerializeField] private GameObject case5;
    [SerializeField] private GameObject case6;

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;  // 添加调试模式开关

    private GameObject[] tutorialCases;
    private int currentCaseIndex = 0;
    private bool isTutorialActive = false;
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    private void Awake()
    {
        // 初始化教程案例数组
        tutorialCases = new GameObject[] { case1, case2, case3, case4, case5, case6 };
        
        // 在调试模式下，总是显示教程
        if (debugMode)
        {
            StartTutorial();
            return;
        }

        // 检查是否已完成教程
        bool tutorialCompleted = PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;

        if (!tutorialCompleted)
        {
            // 第一次进入游戏
            StartTutorial();
        }
        else
        {
            // 非第一次进入游戏，隐藏所有教程UI
            HideAllCases();
        }
    }

    private void Start()
    {
        // 确保所有案例对象都已分配
        ValidateTutorialCases();
    }

    private void Update()
    {
        if (!isTutorialActive) return;

        // 检测触摸输入
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ShowNextCase();
            }
        }
        // 同时支持鼠标点击（用于编辑器测试）
        else if (Input.GetMouseButtonDown(0))
        {
            ShowNextCase();
        }
    }

    private void StartTutorial()
    {
        isTutorialActive = true;
        currentCaseIndex = 0;
        
        // 隐藏所有案例
        HideAllCases();
        
        // 显示第一个案例
        if (tutorialCases[0] != null)
        {
            tutorialCases[0].SetActive(true);
        }

        Debug.Log("教程开始");
    }

    private void ShowNextCase()
    {
        // 隐藏当前案例
        if (currentCaseIndex < tutorialCases.Length && tutorialCases[currentCaseIndex] != null)
        {
            tutorialCases[currentCaseIndex].SetActive(false);
        }

        currentCaseIndex++;

        // 检查是否完成所有教程
        if (currentCaseIndex >= tutorialCases.Length)
        {
            CompleteTutorial();
            return;
        }

        // 显示下一个案例
        if (tutorialCases[currentCaseIndex] != null)
        {
            tutorialCases[currentCaseIndex].SetActive(true);
            Debug.Log($"显示教程案例 {currentCaseIndex + 1}");
        }
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        HideAllCases();
        
        // 保存教程完成状态
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();
        
        Debug.Log("教程完成");
    }

    private void HideAllCases()
    {
        foreach (GameObject tutorialCase in tutorialCases)
        {
            if (tutorialCase != null)
            {
                tutorialCase.SetActive(false);
            }
        }
    }

    private void ValidateTutorialCases()
    {
        for (int i = 0; i < tutorialCases.Length; i++)
        {
            if (tutorialCases[i] == null)
            {
                Debug.LogError($"教程案例 {i + 1} 未分配！请在Inspector中设置所有教程UI对象。");
            }
        }
    }

    #region Public Methods

    /// <summary>
    /// 重置教程状态（用于测试）
    /// </summary>
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        PlayerPrefs.Save();
        StartTutorial();
    }

    /// <summary>
    /// 跳过教程
    /// </summary>
    public void SkipTutorial()
    {
        CompleteTutorial();
    }

    #endregion
} 