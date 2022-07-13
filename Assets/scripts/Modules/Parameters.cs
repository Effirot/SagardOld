using UnityEngine;
using SagardCL;
using System.Collections.Generic;

public abstract class Parameters : MonoBehaviour {
        #region // =========================================================== All parameters =================================================================================================

        protected Vector3 position{ get{ return transform.position; } set{ transform.position = value; } }
        
        #region // ================================== controlling
        
            public Color Team { get { return _Team; } set { _Team = value; } }
            [SerializeField] Color _Team;

            [SerializeField] bool _CanControl = true;
            [SerializeField] bool _Corpse = false;
            [SerializeField] int _WalkDistance = 5;
            
            public bool CanControl { get{ return _CanControl & !_Corpse; } set { _CanControl = value; } }
            public bool Corpse { get { return _Corpse; } set{ _Corpse = value; } }
            public int WalkDistance { get { return _WalkDistance; } set { _WalkDistance = value; } }
        #endregion
        #region // ================================== parameters

            public int maxVisibleDistance = 10;
            public bool nowVisible() { return true; }

            IHealthBar _Health;
            ISanityBar _Sanity;
            IStaminaBar _Stamina;

            [SerializeReference, SerializeReferenceButton] protected IHealthBar BaseHealth;
            [SerializeReference, SerializeReferenceButton] protected ISanityBar BaseSanity;
            [SerializeReference, SerializeReferenceButton] protected IStaminaBar BaseStamina;
            [SerializeReference, SerializeReferenceButton] List<IOtherBar> _OtherStates;
            
            public IHealthBar Health { get{ return _Health; } set{ _Health = value; } }
            public ISanityBar Sanity { get { return _Sanity; } set{ _Sanity = value; } } 
            public IStaminaBar Stamina { get{ return _Stamina; } set{ _Stamina = value; } }
            public virtual List<IOtherBar> OtherStates { get { return _OtherStates; } set{ _OtherStates = value; } }

        #endregion
        #region // ================================== effects
            
            [SerializeReference, SerializeReferenceButton] List<Effect> _Debuff;
            [SerializeReference, SerializeReferenceButton] List<Effect> _Resists;

            public List<Effect> Debuff { get { return _Debuff; } set{ _Debuff = value; } }
            public List<Effect> Resists { get { return _Resists; } set{ _Resists = value; }}

        #endregion

    #endregion
}