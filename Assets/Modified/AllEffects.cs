using UnityEngine;
using SagardCL;
using System.Collections.Generic;
using System;
using SagardCL.IParameterManipulate;

/* 
    [Serializable]public class Bleed : IEffect
    {
        public string Name { get; set; }
        public Sprite Icon { get; set; }
        public string Description { get; set; }

        public CharacterCore Target { get; set; }

        public BalanceChanger Stats { get; set; }
        
        


        void WhenAdded()
        
        void Update() { }
        Attack DamageReaction(Attack attack) { return null; }
        void LostHealth() { }
    }
 */


#region // Classic Effects

    [Serializable]public struct Bleed : Effect
    {
        [SerializeField] string _Name; public string Name { get{ return _Name; } }
        [SerializeField] Sprite _Sprite; public Sprite Icon { get{ return _Sprite; } }
        [SerializeField] string _Description; public string Description { get{ return _Description; } }

        [SerializeField] CharacterCore _Target; public CharacterCore Target { get{ return _Target; } set{ _Target = value; } }

        [SerializeField] BalanceChanger _Stats; public BalanceChanger Stats { get{ return _Stats; } set { _Stats = value; } }

        public bool ExistReasons() { return Timer > 0 & Damage > 0 & !Target.Corpse; }

        int StartTimer;
        [SerializeField][Range(1, 20)] int Timer;
        [SerializeField][Range(0, 10)] int Damage;

        void WhenAdded() { StartTimer = Timer; }
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


    
    public struct Decomposition : HiddenEffect
    {
        public static Decomposition Base(CharacterCore target) => new Decomposition() { Target = target };

        [SerializeField] string _Name; public string Name { get{ return _Name; } }
        [SerializeField] Sprite _Sprite; public Sprite Icon { get{ return _Sprite; } }
        [SerializeField] string _Description; public string Description { get{ return _Description; } }

        [SerializeField] CharacterCore _Target; public CharacterCore Target { get{ return _Target; } set{ _Target = value; } }

        [SerializeField] BalanceChanger _Stats; public BalanceChanger Stats { get{ return _Stats; } set { _Stats = value; } }

        public bool ExistReasons() { return Target.Corpse; }

        int Timer;
        void WhenAdded() { Timer = 3; }

        void Update() 
        {
            Timer--;
            if(Timer > 0)Target.TakeDamageList.Add(new Attack(1, DamageType.Pure));
            else Timer = 3;
        }
    }
#endregion