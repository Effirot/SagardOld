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
        
        void Updated() { }
        Attack DamageReaction(Attack attack) { return null; }
        void LostHealth() { }
    }
 */


#region // Classic Effects
    [Serializable]public struct Bleed : IEffect
    {
        [SerializeField] string _Name;
        public string Name { get{ return _Name; } set { _Name = value; } }
        [SerializeField] Sprite _Sprite;
        public Sprite Icon { get{ return _Sprite; } set { _Sprite = value; } }
        [SerializeField] string _Description;
        public string Description { get{ return _Description; } set{ _Description = value; } }

        public CharacterCore Target { get; set; }

        [SerializeField] BalanceChanger _Stats;
        public BalanceChanger Stats { get{ return _Stats; } set { _Stats = value; } }






    }
#endregion