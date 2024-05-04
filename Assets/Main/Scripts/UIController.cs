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
    public CanvasGroup background;
    public RectTransform nextLevelPanel;
    public RectTransform outOfSpacePanel;
    public RectTransform outOfMovePanel;

    [Header("Buttons")] 
    public RectTransform nextLevelBtn;
    public RectTransform tryAgainMoveBtn;
    public RectTransform tryAgainSpaceBtn;
    public RectTransform continueMoveBtn;
    public RectTransform continueSpaceBtn;

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
        if (!isShowingPanel)
        {
            isShowingPanel = true;
            background.DOFade(1f, .45f).SetEase(Ease.OutBack);
            
            outOfSpacePanel.localScale = Vector3.zero;
            outOfSpacePanel.gameObject.SetActive(true);
            outOfSpacePanel.DOScale(Vector3.one, .45f).SetEase(Ease.OutBack);
        }
    }

    public void HideOutOfSpacePanel()
    {
        isShowingPanel = false;
        coinAmountTxt.text = $"{LevelController.Instance.coinAmount}";
        
        background.DOFade(0f, .35f).SetEase(Ease.InBack);
        outOfSpacePanel.DOScale(Vector3.zero, .35f).SetEase(Ease.InBack).OnComplete(() =>
        {
            outOfSpacePanel.gameObject.SetActive(false);
        });
    }

    public void TryAgainOutOfSpace()
    {
        background.DOFade(0f, .35f).SetEase(Ease.InBack);
        outOfSpacePanel.DOScale(Vector3.zero, .35f).SetEase(Ease.InBack).OnComplete(() =>
        {
            outOfSpacePanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });;
    }
    
    
    
    
    public void ShowOutOfMovePanel()
    {
        if (!isShowingPanel)
        {
            isShowingPanel = true;
            background.DOFade(1f, .45f).SetEase(Ease.OutBack);
            
            outOfMovePanel.localScale = Vector3.zero;
            outOfMovePanel.gameObject.SetActive(true);
            outOfMovePanel.DOScale(Vector3.one, .45f).SetEase(Ease.OutBack);
        }
    }
    
    public void HideOutOfMovePanel()
    {
        isShowingPanel = false;
        coinAmountTxt.text = $"{LevelController.Instance.coinAmount}";
        levelMoveCountTxt.text = $"{ShapeController.Instance.levelMoveCount}";
        
        background.DOFade(0f, .35f).SetEase(Ease.InBack);
        outOfMovePanel.DOScale(Vector3.zero, .35f).SetEase(Ease.InBack).OnComplete(() =>
        {
            outOfMovePanel.gameObject.SetActive(false);
        });
    }

    public void TryAgainOutOfMove()
    {
        background.DOFade(0f, .35f).SetEase(Ease.InBack);
        outOfMovePanel.DOScale(Vector3.zero, .35f).SetEase(Ease.InBack).OnComplete(() =>
        {
            outOfMovePanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
    }
    
    
    
    public void ShowNextLevelPanel()
    {
        if (!isShowingPanel)
        {
            isShowingPanel = true;
            background.DOFade(1f, .45f).SetEase(Ease.OutBack);
            
            nextLevelPanel.gameObject.SetActive(true);
            nextLevelPanel.localScale = Vector3.zero;
            nextLevelPanel.DOScale(Vector3.one, .45f).SetEase(Ease.OutBack);
            
            nextLevelBtn.localScale = Vector3.zero;
            nextLevelBtn.DOScale(Vector3.one, .2f).SetEase(Ease.Linear).SetDelay(.2f);
        }
    }
    
    public void NextLevel()
    {
        LevelController.Instance.LevelUp();
        
        background.DOFade(0f, .35f).SetEase(Ease.InBack);
        nextLevelPanel.DOScale(Vector3.zero, .35f).SetEase(Ease.InBack).OnComplete(() =>
        {;
            nextLevelPanel.gameObject.SetActive(false);
            SceneManager.LoadScene(0);
        });
        
        nextLevelBtn.DOScale(Vector3.zero, .15f).SetEase(Ease.Linear);
    }
}
