using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private Image delayedBar;
    [SerializeField] private Image activeBar;
    [SerializeField] private Transform hpParent;

    private Tween recoveryTween;

    public void InitView(HealthManager healthManager)
    {
        delayedBar.fillAmount = 1;
        activeBar.fillAmount = 1;
        
        healthManager.ComboFinished += OnComboFinished;
        healthManager.HitReceived += OnHitReceived;
        healthManager.OnRecoveryStarted += RecoveryStarted;
        healthManager.OnRecoveryEnded += RecoveryEnded;
    }

    public Image ActiveBar
    {
        get => activeBar;
    }

    public Image DelayedBar
    {
        get => delayedBar;
    }

    public TextMeshProUGUI HpText
    {
        get => _hpText;
    }

    private void OnHitReceived(int damage)
    {
        hpParent.DOPunchScale(Vector3.one * 0.1f, 0.1f);
        hpParent.DOShakeRotation(0.15f, Vector3.one * 5f, 40);
        Debug.Log("OnHitReceived : " + damage);
    }

    private void OnComboFinished(int totalDamage)
    {
        Debug.Log("OnComboFinished : " + totalDamage);
    }

    private void RecoveryStarted()
    {
        if(recoveryTween != null)
            recoveryTween.Kill();

        recoveryTween = hpParent
            .DOScaleY(1.05f, 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .OnKill(()=> hpParent.localScale = Vector3.one);
        
        Debug.Log("RecoveryStarted");
    }
    
    private void RecoveryEnded()
    {
        recoveryTween.Kill();
        Debug.Log("RecoveryEnded");
    }
}