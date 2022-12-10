using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [FormerlySerializedAs("anahtarText")] public Text keyText;
    [FormerlySerializedAs("canIkonlar")] public GameObject[] lifeIcons;
    
    private GameObject rightArrow;
    private GameObject gate;
    private Text levelText;
    private Fade blankScreen;
    private AudioSource music;

    void Awake()
    {
        Variables.lives = 3;
        Variables.keysCollected = 0;
        Variables.keyCount = GameObject.FindGameObjectsWithTag("Key").Length;
        Variables.playerDied = false;
        Variables.levelFinished = false;

        string strLevel = SceneManager.GetActiveScene().name.Remove(0, 5);
        Int32.TryParse(strLevel, out Variables.currentLevel);

        rightArrow = GameObject.FindGameObjectWithTag("Arrow");
        rightArrow.SetActive(false);

        gate = GameObject.FindGameObjectWithTag("Gate");

        levelText = GameObject.FindGameObjectWithTag("LevelText")?.GetComponent<Text>();
        
        blankScreen = GameObject.FindGameObjectWithTag("Blank")?.GetComponent<Fade>();
        blankScreen?.gameObject?.SetActive(false);

        music = GameObject.Find("Music")?.GetComponent<AudioSource>();
    }

    void Start()
    {
        UpdateUI();
        ShowLevelText($"Level {Variables.currentLevel}");
    }

    public void UpdateUI()
    {
        keyText.text = $"{Variables.keysCollected} / {Variables.keyCount}";

        for(int i = 0; i < lifeIcons.Length; i++)
        {
            if(i < Variables.lives)
                lifeIcons[i].SetActive(true);
            else
                lifeIcons[i].SetActive(false);
        }

        if(Variables.keysCollected >= Variables.keyCount)
        {
            rightArrow.SetActive(true);
            gate.SetActive(false);
        }
    }

    void ShowLevelText(string text)
    {
        if(levelText)
        {
            levelText.gameObject.SetActive(true);
            levelText.text = text;

            Sequence levelTextPopup = DOTween.Sequence().SetAutoKill().OnComplete(() => levelText.gameObject.SetActive(false));

            levelTextPopup.Append(levelText.DOColor(Color.black, 1f).SetOptions(true).SetDelay(0.5f));
            levelTextPopup.Join(levelText.transform.DOScale(0.5f, 1.5f).From());
            levelTextPopup.Append(levelText.DOColor(Color.clear, 1f).SetOptions(true));
            levelTextPopup.Join(levelText.transform.DOScale(1.5f, 1.5f));
        }
    }

    public void ResetEnemies()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in enemies)
        {
            enemy.Reset();
        }
    }

    public void FinishLevel()
    {
        if(!Variables.levelFinished)
        {
            Variables.levelFinished = true;
            Variables.currentLevel++;

            if (Variables.currentLevel <= Variables.levelCount)
            {
                blankScreen.FadeScreen(Variables.fadeDuration, LevelUp, 1f);
            }
            else
            {
                ShowLevelText("You completed all levels!");
                blankScreen.FadeScreen(Variables.fadeDuration, ReturnToMenu, 2f);
            }
        }
    }

    public void Failed()
    {
        if (!Variables.levelFinished)
        {
            ShowLevelText("Character died");
            blankScreen.FadeScreen(Variables.fadeDuration, ReturnToMenu, 1f);
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void LevelUp()
    {
        levelText.gameObject.SetActive(true);
        SceneManager.LoadScene($"Level{Variables.currentLevel}", LoadSceneMode.Single);
    }
}
