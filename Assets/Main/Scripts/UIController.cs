using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }
    
    [Header("Panels")]
    public CanvasGroup nextLevelPanel;
    public CanvasGroup gameOverPanel;
    
    [Header("Texts")]
    public TextMeshProUGUI candyAmountText;
    public TextMeshProUGUI levelMoveCountTxt;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void ShowGameOverPanel()
    {
        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.DOFade(1f, .35f).SetEase(Ease.Linear);
    }
    
    public void TryAgain()
    {
        gameOverPanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            gameOverPanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
    
    public void ShowNextLevelPanel()
    {
        nextLevelPanel.gameObject.SetActive(true);
        nextLevelPanel.DOFade(1f, .35f).SetEase(Ease.Linear);
    }
    
    public void NextLevel()
    {
        nextLevelPanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            nextLevelPanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
}
