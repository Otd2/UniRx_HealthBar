using System;
using NaughtyAttributes;
using UniRx;
using UniRx.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private HealthBarView healthBarView;
    [SerializeField] private int totalComboTime;
    [SerializeField] private int recoveryStartTimeAfterLastHit;
    
    [SerializeField] private IntReactiveProperty totalHp = new IntReactiveProperty(100);
    [SerializeField] private IntReactiveProperty hp = new IntReactiveProperty(100);

    private IntReactiveProperty delayedHp = new IntReactiveProperty(100);
    private Subject<int> damageSubject = new Subject<int>();

    public Action OnRecoveryStarted;
    public Action OnRecoveryEnded;
    public Action OnComboStarted;
    public Action<int> ComboFinished;
    public Action<int> HitReceived;

    private void Awake()
    {
        healthBarView.InitView(this);
    }
    
    void Start()
    {
        //hp text
        hp.SubscribeToText(healthBarView.HpText);
        
        //hp bar
        hp.Subscribe(currentHp =>
        {
            healthBarView.ActiveBar.fillAmount = (float)currentHp / totalHp.Value;
        });

        //damage receiver
        damageSubject.Where(_ => _ > 0)
            .Select(_ => Mathf.Max(0, hp.Value - _))
            .Subscribe(newHp =>
        {
            hp.Value = newHp;
            HitReceived?.Invoke(newHp);
        });

        //combo delay
        var throttledDamage = damageSubject.Throttle(TimeSpan.FromSeconds(totalComboTime));
        
        //delayed HP listener
        throttledDamage
            .Select(_=> hp.Value)
            .Subscribe(
                _ =>
                {
                    delayedHp.Value = _;
                }
            );
        
        //delayed HPBar
        delayedHp.Subscribe(_ => healthBarView.DelayedBar.fillAmount = (float)_ / totalHp.Value);

        //combo logic
        //damageSubject.(throttledDamage).Subscribe(_ => Debug.Log("OLDU MU?"));
        
        damageSubject.Buffer(throttledDamage).Subscribe(x =>
        {
            int comboDamage = 0;
            foreach (int damage in x)
            {
                comboDamage += damage;
            }
            ComboFinished?.Invoke(comboDamage);
        }).AddTo(this);
        

        //recovery logic
        var recoveryThrottle = damageSubject
            .Throttle(TimeSpan.FromSeconds(recoveryStartTimeAfterLastHit));
        
        var hp_recoveryStream = Observable.Interval(TimeSpan.FromSeconds(0.1f), Scheduler.MainThread)
            .Select(_ => hp.Value + 1)
            .TakeWhile(_ => _<=100)
            .TakeUntil(damageSubject)
            .DoOnSubscribe(() => OnRecoveryStarted?.Invoke())
            .DoOnCompleted(() => OnRecoveryEnded?.Invoke());
        
        recoveryThrottle.SelectMany(_ => hp_recoveryStream)
            .Subscribe(newHp=>
            {
                healthBarView.DelayedBar.fillAmount = (float)newHp / totalHp.Value;
                hp.Value = newHp;
            });
        

    }
    
    [Button]
    public void Damage()
    {
        var randomDamage = Random.Range(2, 7);
        damageSubject.OnNext(5);
    }
}
