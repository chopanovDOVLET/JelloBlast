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
    public CanvasGroup outOfSpacePanel;
    public CanvasGroup outOfMovePanel;

    [Header("Texts")] 
    public TextMeshProUGUI currentLevelTxt;
    public TextMeshProUGUI candyAmountTxt;
    public TextMeshProUGUI levelMoveCountTxt;
    public TextMeshProUGUI coinAmountTxt;
    public TextMeshProUGUI warningCounterTxt;

    [Header("Game Status")] 
    public bool isShowingPanel;

    private void Awake()
    {
        Instance = this;
        isShowingPanel = false;
    }
    
    public void ShowOutOfSpacePanel()
    {
        isShowingPanel = true;
        outOfSpacePanel.gameObject.SetActive(true);
        outOfSpacePanel.DOFade(1f, .35f).SetEase(Ease.Linear);
    }

    public void HideOutOfSpacePanel()
    {
        isShowingPanel = false;
        coinAmountTxt.text = $"{LevelController.Instance.coinAmount}";
        outOfSpacePanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            outOfSpacePanel.gameObject.SetActive(false);
        });
    }

    public void TryAgainOutOfSpace()
    {
        outOfSpacePanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            outOfSpacePanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
    
    
    
    
    public void ShowOutOfMovePanel()
    {
        isShowingPanel = true;
        outOfMovePanel.gameObject.SetActive(true);
        outOfMovePanel.DOFade(1f, .35f).SetEase(Ease.Linear);
    }
    
    public void HideOutOfMovePanel()
    {
        isShowingPanel = false;
        coinAmountTxt.text = $"{LevelController.Instance.coinAmount}";
        levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";
        outOfMovePanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            outOfMovePanel.gameObject.SetActive(false);
        });
    }

    public void TryAgainOutOfMove()
    {
        outOfMovePanel.DOFade(0f, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            outOfMovePanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
    
    
    
    public void ShowNextLevelPanel()
    {
        isShowingPanel = true;
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
