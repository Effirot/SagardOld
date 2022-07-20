using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.IParameterManipulate;
using System.Threading.Tasks;
using System;
using System.Reflection;
using Random = UnityEngine.Random;

public class Parameters : MonoBehaviour, Killable, GetableCrazy, Tiredable, Storage, Attacker, HaveName {
        #region // ============================================================= Useful stuff =============================================================================================
            protected Vector3 position{ get{ return this.transform.position; } set{ this.transform.position = value; } }
                    
            public void ChangeFigureColor(Color color, float speed, Material material) { StartCoroutine(ChangeMaterialColor(material, color, speed)); }
            public void ChangeFigureColor(Color color, float speed, Material[] material = null)
            { 
                if(material == null) material = new Material[] { transform.parent.Find("MPlaner/Platform").GetComponent<Renderer>().material, 
                                                                transform.parent.Find("MPlaner/Platform/Figure").GetComponent<Renderer>().material };
                foreach (Material mat in material) StartCoroutine(ChangeMaterialColor(mat, color, speed)); 
            }
            public void ChangeFigureColorWave(Color color, float speed, Material[] material = null)
            { 
                if(material == null) material = new Material[] { transform.parent.Find("MPlaner/Platform").GetComponent<Renderer>().material, 
                                                                transform.parent.Find("MPlaner/Platform/Figure").GetComponent<Renderer>().material };
                
                List<Task> tasks = new List<Task>();
                foreach (Material mat in material) { StartCoroutine(Wave(mat, color, speed)); }

                IEnumerator Wave(Material material, Color color, float speed)
                { 
                    Color Save = material.color;
                    yield return ChangeMaterialColor(material, color, speed); 
                    yield return ChangeMaterialColor(material, Save, speed); 
                }
            }
            static protected IEnumerator ChangeMaterialColor(Material material, Color color, float speed)
            {
                while(material.color != color)
                {
                    material.color = Color.Lerp(material.color, color, speed);
                    speed *= 1.1f;
                    yield return new WaitForFixedUpdate();
                }
            }

            public virtual Type type{ get{ return typeof(Parameters); } }
        #endregion
        #region // =========================================================== All parameters =================================================================================================
        
        protected virtual async void Start()
        {
            transform.parent.name = HaveName.GetName();

            BaseHealth = Health.Clone() as IHealthBar;
            BaseStamina = Stamina.Clone() as IStaminaBar;
            BaseSanity = Sanity.Clone() as ISanityBar;

            InGameEvents.MapUpdate.AddListener(ParametersUpdate);

            InGameEvents.StepSystem.Add(Summon);
            InGameEvents.AttackTransporter.AddListener((a) => { 
                Attack find = a.Find((a) => a.Where == new Checkers(position));
                if(find.Where == new Checkers(position)){
                    DamageReaction(find);
                    GetDamage(find);
                }
            });
            InGameEvents.StepEnd.AddListener(EveryStepEnd);

            await Task.Delay(10);
            position = new Checkers(position);
        }
        
        #region // ================================== parameters

            public const int maxVisibleDistance = 10;
            public float visibleCoefficient = 1;
            [SerializeField] bool AlwaysVisible = false;
            [SerializeField] bool WallIgnoreVisible = false;

            public bool nowVisible(Parameters Object) { return (Checkers.Distance(this.position, Object.position) <= maxVisibleDistance * visibleCoefficient &
                                                                WallIgnoreVisible? Physics.Raycast(this.position, Object.position - this.position, Checkers.Distance(this.position, Object.position), LayerMask.NameToLayer("Object")) : true) | 
                                                                AlwaysVisible; }

            [SerializeReference, SubclassSelector] public IHealthBar Health;
            [SerializeReference, SubclassSelector] public ISanityBar Sanity;
            [SerializeReference, SubclassSelector] public IStaminaBar Stamina;
            [SerializeReference, SubclassSelector] public List<IOtherBar> OtherStates;
            
            protected IHealthBar BaseHealth;
            protected ISanityBar BaseSanity;
            protected IStaminaBar BaseStamina;

            private Attack.AttackCombine GetDamageList;

        #endregion
        #region // ================================== effects
            
            [SerializeReference, SubclassSelector] List<Effect> Debuff;
            [SerializeReference, SubclassSelector] List<Effect> Resists;

        #endregion

        #region // =============================== Update methods
            
            protected virtual void ParametersUpdate() {}
            protected virtual void DamageReaction(Attack attack) {}
            private void GetDamage(Attack attack)
            {
                Health.GetDamage(attack); 
                if(attack.damage > 0) ChangeFigureColorWave(attack.DamageColor(), 0.2f);
            }
            public virtual void LostHealth()
            {
                Destroy(transform.parent.gameObject);
            }
            
        #endregion
        #region // =============================== Step System
            Task Summon(string id){ 
                MethodInfo Method = type.GetMethod(id, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(Method == null) { 
                    return new Task(() => { });
                }
                return (Task)Method?.Invoke(this, parameters: null);
            }   
            async Task Dead() 
            { 
                if(Health.Value > 0) return;
                await Task.Delay(Random.Range(10, 100));                 
                LostHealth();
            }

            protected virtual void EveryStepEnd()
            {
                
            }
        #endregion
    #endregion
}