using UnityEngine;
using SagardCL;
using System.Collections.Generic;
using System;
using SagardCL.ParameterManipulate;

/* 
    [Serializable]public class Bleed : IEffect
    {
        public string Name { get; set; }
        public Sprite Icon { get; set; }
        public string Description { get; set; }

        public IObjectOnMap Target { get; set; }

        public BalanceChanger Stats { get; set; }
        
        


        void WhenAdded()
        void WhenRemoved()
        
        void Update()
        Attack DamageReaction(Attack attack) { return null; }
        void LostHealth()
        void Rest()

    }
 */


#region // Classic Effects

    [Serializable] public struct Bleed : Effect
    {
        [SerializeField] string _Name; public string Name { get{ return _Name; } }
        [SerializeField] Sprite _Sprite; public Sprite Icon { get{ return _Sprite; } }
        [SerializeField] string _Description; public string Description { get{ return _Description; } }

        public IObjectOnMap Target { get; set; }

        Balancer _Stats; public Balancer Stats { get{ return _Stats; } set { _Stats = value; } }

        public bool Workable() { return Timer > 0 & Damage > 0 & !Target.Corpse; }

        int StartTimer;
        [SerializeField] Balancer Stat;
        [SerializeField][Range(1, 20)] int Timer;
        [SerializeField][Range(0, 10)] int Damage;

        void WhenAdded() { StartTimer = Timer; Stats = Stat; }
        void Update() 
        {
            Target.AddDamage(new Attack(Damage, DamageType.Effect));
            
            Timer -= 1;
            if(Timer <= 0) 
            { Damage -= 1; Timer = StartTimer; }
        }
        void DamageReaction() 
        {
            if(Target.TakeDamageList.Combine().Exists((a) => a.DamageType == DamageType.Melee && a.Damage > 3)) Damage += 1;
        }
    }
    [Serializable] public struct Spikes : Effect
    {
        [SerializeField] string _Name; public string Name { get{ return _Name; } }
        [SerializeField] Sprite _Sprite; public Sprite Icon { get{ return _Sprite; } }
        [field: SerializeField] public string Description { get; set; }

        public IObjectOnMap Target { get; set; }

        [field: SerializeField] public Balancer Stats { get; set; }

        public bool Workable() { return Timer > 0 & !Target.Corpse; }

        int StartTimer;
        [SerializeField][Range(1, 20)] int Timer;
        [SerializeField] Attack CounterEffect;

        void WhenAdded() { StartTimer = Timer; }
        void DamageReaction() 
        {
            foreach(IEffector targets in Target.TakeDamageList.Senders)
            {
                targets.AddDamage(CounterEffect);
            }
        }
    }
    [Serializable] public struct ImpotenceLol : Effect
    {
        [SerializeField] string _Name; public string Name { get{ return _Name; } }
        [SerializeField] Sprite _Sprite; public Sprite Icon { get{ return _Sprite; } }
        [SerializeField] string _Description; public string Description { get{ return _Description; } }

        public IObjectOnMap Target { get; set; }

        [SerializeField] Balancer _Stats; public Balancer Stats { get{ return _Stats; } set { _Stats = value; } }

        public bool Workable() { return !Target.Corpse; }

        int StartTimer;
        [SerializeField][Range(1, 20)] int Timer;

        void WhenAdded() { ((IGetableCrazy)Target).AddSanity(-3); StartTimer = Timer; }
        void DamageReaction() 
        {
            Timer--;
            if(Timer > 0) return;
            
            ((IGetableCrazy)Target).AddSanity(-2);
            Timer = StartTimer;
        }
    }

    public struct Decomposition : HiddenEffect
    {
        public static Decomposition Base(IEffector target) => new Decomposition() { Target = target };

        [SerializeField] string _Name; public string Name { get{ return _Name; } }
        [SerializeField] Sprite _Sprite; public Sprite Icon { get{ return _Sprite; } }
        [SerializeField] string _Description; public string Description { get{ return _Description; } }

        public IObjectOnMap Target { get; set; }

        public Balancer Stats { get{ return new Balancer(); } set{  } }

        public bool Workable() { return Target.Corpse; }

        int Timer;
        void WhenAdded() { Timer = 5; }

        void Update() 
        {
            Timer--;
            if(Timer > 0)Target.TakeDamageList.Add(new Attack(1, DamageType.Pure));
            else Timer = 3;
        }
    }
#endregion