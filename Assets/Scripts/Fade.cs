using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Fade : MonoBehaviour
{
    private Image blankImg;

    private void Awake()
    {
        blankImg = GetComponent<Image>();
    }

    public void FadeScreen(float duration, TweenCallback completeAction, float delay = 0f)
    {
        gameObject.SetActive(true);

        Sequence fadeSequence = DOTween.Sequence().SetAutoKill();
        fadeSequence.Append(blankImg.DOColor(Color.black, duration*0.45f).SetOptions(true).SetDelay(delay).OnComplete(completeAction));
        fadeSequence.Append(blankImg.DOColor(Color.clear, duration*0.45f).SetOptions(true).SetDelay(duration*0.1f));
    }
}
