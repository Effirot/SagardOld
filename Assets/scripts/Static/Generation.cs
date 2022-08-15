using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;
using System;
using Random = UnityEngine.Random;
using UnityAsync;

public abstract class Generation : MonoBehaviour
{
    [SerializeField] GameObject AttackVisual;

    private static GameObject[,] AttackVisualsRealizers;
    private static List<Attack>[,] AllAttackZoneArchive;
    protected void LetsGenerate(Map map){
        ClearMap();

        AttackVisualsRealizers = new GameObject[map.XScale, map.ZScale];
        AllAttackZoneArchive = new List<Attack>[map.XScale, map.ZScale];
        
        for(int x = 0; x < map.XScale - 1; x++)
        for(int z = 0; z < map.ZScale - 1; z++)
        {
            AllAttackZoneArchive[x, z] = new List<Attack>();

            GameObject AttackGizmo = Instantiate<GameObject>(AttackVisual, new Checkers(x, z, 0.04f), AttackVisual.transform.rotation, transform);
            AttackVisualsRealizers[x, z] = AttackGizmo;
            AttackGizmo.name = $"{x}:{z}";
            
            AttackGizmo.SetActive(false);

            AttackGizmo.transform.position = new Checkers(x, z, 0.04f);
    
        }
    }
    private void ClearMap(){
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Map"))
        {   
            Destroy(obj);
        }
    }

    public static void DrawAttack(List<Attack> AttackZone, CharacterCore sender)
    {
        foreach(List<Attack> attacks in AllAttackZoneArchive) { if(attacks != null) attacks.RemoveAll((a) => a.Sender == sender); }
        foreach(Attack attack in AttackZone) {  try{ AllAttackZoneArchive[attack.Position.x, attack.Position.z].Add(attack); } catch { }  }

        CheckAllGizmos();
    }
    private static void CheckAllGizmos()
    {
        for(int x = 0; x < AllAttackZoneArchive.GetLength(0) - 1; x++)
        for(int z = 0; z < AllAttackZoneArchive.GetLength(1) - 1; z++)
        {
            List<Attack> attackList = AllAttackZoneArchive[x, z];
            if(attackList.Count == 0) { AttackVisualsRealizers[x, z].SetActive(false); continue; }

            AttackVisualsRealizers[x, z].SetActive(true);
            AttackVisualsRealizers[x, z].transform.position = new Checkers(AttackVisualsRealizers[x, z].transform.position);
            AttackVisualsRealizers[x, z].GetComponent<SpriteRenderer>().color = AttackPaints(attackList);
        }
        Color AttackPaints(List<Attack> attacks)
        {
            Color result = Color.black;
            foreach (Attack attack in attacks)
            {
                switch (attack.DamageType)
                {
                    default: result += Color.HSVToRGB(0.01f, 1, attack.Damage * 0.08f); break;
                    case DamageType.Repair: goto case DamageType.Heal; 
                    case DamageType.Heal: result += Color.HSVToRGB(0.42f, 1, attack.Damage * 0.08f); break;
                    case DamageType.Rezo: result += Color.HSVToRGB(67f / 360f, 1, attack.Damage * 0.08f); break;
                    case DamageType.Pure: result += Color.HSVToRGB(274f / 360f, 1, attack.Damage * 0.08f); break;
                }
            }
            return result;
        }
    }

    public void StartStep()
    {
        Map.CompleteModeSwitch();
    }
    
}


[System.Serializable] public class Map
{
    public List<IObjectOnMap> ObjectRegister = new List<IObjectOnMap>();
    public static Map Current;
    public static int StepNumber = 0;

    #region // Saving
        public Mesh MapMesh;
        public Mesh MapCollider;

        public int XScale { get; private set; }
        public int ZScale { get; private set; }

        uint key;

        MapCell[,] PlatformMatrix;
        MapEffect[,] EffectMatrix;

        public List<Material> MaterialsList;
    #endregion

    #region // Controlling
    
        public void ChangeHeigh(params Checkers[] poses) 
        {
            foreach(var pos in poses)
            {
                PlatformMatrix[pos.x, pos.z].Mesh.transform = Matrix4x4.TRS(PlatformMatrix[pos.x, pos.z].Mesh.transform.GetPosition() + new Vector3(0, pos.clearUp, 0), 
                                                                            PlatformMatrix[pos.x, pos.z].Mesh.transform.rotation, 
                                                                            Vector3.one);

                PlatformMatrix[pos.x, pos.z].Collider.transform = Matrix4x4.TRS(PlatformMatrix[pos.x, pos.z].Collider.transform.GetPosition() + new Vector3(0, pos.clearUp, 0), 
                                                                                PlatformMatrix[pos.x, pos.z].Collider.transform.rotation, 
                                                                                Vector3.one);
            }

            colliderMesh();  
            visibleMesh();
        }
        public void ChangeModifier(PlatformPreset Modifier, params Checkers[] Where)
        {
            foreach (Checkers position in Where)
            {
                PlatformMatrix[position.x, position.z] = new MapCell(
                    position.Up(PlatformMatrix[position.x, position.z].position.clearUp), 
                    Modifier, 
                    PlatformMatrix[position.x, position.z].DeformProtection, 
                    PlatformMatrix[position.x, position.z].Let);
            }
            foreach(Material material in Modifier.MaterialVariants)
            if(!MaterialsList.Contains(material))
                MaterialsList.Add(material);  

            colliderMesh();  
            visibleMesh();
        }

    #endregion
    #region // Overloads

        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, Vector2 Scale)
        {
            XScale = (int)Scale.x; ZScale = (int)Scale.y; key = Key; 
            PlatformMatrix = new MapCell[XScale, ZScale];
            EffectMatrix = new MapEffect[XScale, ZScale];
            
            MaterialsList = new List<Material>();

            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet);

            visibleMesh();
            colliderMesh();

            Current = this;
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, int PlayerNum)
        {
            key = Key;
            XScale = PlayerNum * 9 + (((int)key / 23)%7); ZScale = PlayerNum * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];
            EffectMatrix = new MapEffect[XScale, ZScale];
            
            MaterialsList = new List<Material>();

            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet);
            
            visibleMesh();
            colliderMesh();

            Current = this;
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, int PlayerNum)
        {
            key = (uint)Random.Range(0, 99999999);
            XScale = PlayerNum * 15 + (((int)key / 23)%7); 
            ZScale = PlayerNum * 15 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];
            EffectMatrix = new MapEffect[XScale, ZScale];
            
            MaterialsList = new List<Material>();
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 
            
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet);
            
            visibleMesh();
            colliderMesh();

            Current = this;
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, Vector2 Scale)
        {
            key = (uint)Random.Range(0, 99999999);
            XScale = (int)Scale.x; ZScale = (int)Scale.y; 
            PlatformMatrix = new MapCell[XScale, ZScale];
            EffectMatrix = new MapEffect[XScale, ZScale];
            
            MaterialsList = new List<Material>();
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet);
            
            visibleMesh();
            colliderMesh();
        
            Current = this;
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key)
        {
            key = Key;
            XScale = 2 * 9 + (((int)key / 23)%7); ZScale = 2 * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];
            EffectMatrix = new MapEffect[XScale, ZScale];

            MaterialsList = new List<Material>();
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet);
            
            visibleMesh();
            colliderMesh();

            Current = this;
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet)
        {
            key = key = (uint)Random.Range(0, 99999999);
            XScale = 2 * 9 + (((int)key / 23)%7); ZScale = 2 * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];
            EffectMatrix = new MapEffect[XScale, ZScale];

            MaterialsList = new List<Material>();
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet);
            
            visibleMesh();
            colliderMesh();

            Current = this;
        }
        
    #endregion
    #region // Map Cell information
        public delegate float FormulaUp(int x, int y, uint key);
        public delegate PlatformPreset FormulaMod(int x, int y, uint key);
        public delegate int FormulaLet(int x, int y, uint key);

        [System.Serializable] struct MapCell
        {
            public Let Let;

            public Checkers position { get; private set; }

            public CombineInstance Mesh;
            public CombineInstance Collider;
            public Material Material;
            public float DeformProtection;
            
            public MapCell(Checkers Position, PlatformPreset Mod, float DeformProtection = 0, Let let = null){ 
                Let = let; 
                position = Position;
                this.DeformProtection = DeformProtection;

                Mesh = Mod.GetCombineMesh(Matrix4x4.TRS(new Vector3(position.x, Position.clearUp, position.z), Quaternion.Euler(0, Random.Range(0, 360), 0), Vector3.one)); 
                Collider = Mod.GetCombineCollider(Matrix4x4.TRS(new Vector3(position.x, Position.clearUp, position.z), Quaternion.Euler(0, 0, 0), Vector3.one));
                Material = Mod.GetMaterial(); 
            }
            
            public void AddVerticalPosition(float y)
            {

            }
        }
        private MapCell[,] GenerateRelief(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet)
        {
            MapCell[,] result = new MapCell[XScale, ZScale];

            for(int x = 0; x < XScale; x++) 
            for(int z = 0; z < ZScale; z++){
                result[x, z] = new MapCell(new Checkers(x, z, formulaUp(x, z, key)), formulaMod(x, z, key), formulaMod(x, z, key).DeformProtection);

                foreach(Material material in formulaMod(x, z, key).MaterialVariants)
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);  
            }
            
            return result;
        }
    #endregion
    #region // Let
        public class Let
        {

        }
    #endregion

    #region // Map Mesh

        void colliderMesh()
        {
            List<CombineInstance> instances = new List<CombineInstance>();
            for(int x = 0; x < XScale - 1; x++)
            for(int z = 0; z < ZScale - 1; z++)
            {
                instances.Add(PlatformMatrix[x, z].Collider);
            }
            
            MapCollider.Clear();
            MapCollider.subMeshCount = 0;
            MapCollider.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            MapCollider.CombineMeshes(instances.ToArray(), true);
            Physics.BakeMesh(MapCollider.GetInstanceID(), false);
        }
        void visibleMesh()
        {
            Dictionary<Material, List<CombineInstance>> SubMeshes = new Dictionary<Material, List<CombineInstance>>();
            foreach(Material material in MaterialsList) { SubMeshes.Add(material, new List<CombineInstance>()); }

            for(int x = 0; x < XScale - 1; x++) 
            for(int z = 0; z < ZScale - 1; z++)
            {
                CombineInstance subMesh = PlatformMatrix[x, z].Mesh;

                Material material = PlatformMatrix[x, z].Material;

                SubMeshes[material].Add(subMesh);
            }
            List<CombineInstance> instanceByMaterial = new List<CombineInstance>();
            foreach(var value in SubMeshes)
            {
                CombineInstance item = new CombineInstance();
                Mesh mesh = new Mesh();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.CombineMeshes(value.Value.ToArray(), true);

                item.mesh = mesh;
                item.transform = Matrix4x4.identity;
                instanceByMaterial.Add(item);
            }

            MapMesh.Clear();
            MapMesh.subMeshCount = MaterialsList.Count;
            MapMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            MapMesh.CombineMeshes(instanceByMaterial.ToArray(), false);
        }

    #endregion

    #region // Static Map Methods

        internal static List<StepAction> StepSystem = new List<StepAction>();
        public delegate Task StepAction(string StepStage);

        enum Step : int
        {
            BotLogic,

            Walking,
            Attacking,
            EffectUpdate,
            LateWalking,
            LandscapeDeform,
            DamageMath,
            Dead,
            Rest
        }

        public static async void CompleteModeSwitch()
        {
            if(!MouseControlEvents.Controllable) return;
            MouseControlEvents.Controllable = false;   

            Debug.ClearDeveloperConsole();
            
            for(int i = 0; i < Enum.GetNames(typeof(Step)).Length; i++){
                Debug.Log($"Now step: {(Step)i}");
                MapUpdate.Invoke();
                List<Task> task = new List<Task>();

                Step step = (Step)i;

                foreach(StepAction summon in StepSystem) { task.Add(summon(step.ToString())); }
                try{ await Task.WhenAll(task.ToArray()); } catch(Exception e) { Debug.LogError(e); }
            }
            StepEnd.Invoke();
            
            WhoAttackToWho.Clear();
            StepNumber++;
            
            MouseControlEvents.Controllable = true;
        }

        public static UnityEvent StepEnd = new UnityEvent();

        public static UnityEvent MapUpdate = new UnityEvent();        
        public static UnityEvent<List<SagardCL.Attack>> AttackTransporter = new UnityEvent<List<SagardCL.Attack>>();
        
        public static Dictionary<IObjectOnMap, List<IObjectOnMap>> WhoAttackToWho = new Dictionary<IObjectOnMap, List<IObjectOnMap>>();
    
    #endregion
}

[System.Serializable]public struct PlatformPreset
{
    [SerializeField]public Mesh[] MeshVariants;
    [SerializeField]public Material[] MaterialVariants;
    [SerializeField]public float DeformProtection;

    public GameObject GetObject()
    {
        GameObject result = new GameObject("Platform", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        result.isStatic = true;

        result.tag = "Map";
        result.layer = LayerMask.NameToLayer("Map");

        if(MeshVariants.Length != 0) result.GetComponent<MeshFilter>().mesh = MeshVariants[Random.Range(0, MeshVariants.Length)];
        else                         CreateCubeCollider(new Vector3(-0.45f, 0, -0.45f), new Vector3(0.45f, -10, 0.45f));
        if(MaterialVariants.Length != 0) result.GetComponent<MeshRenderer>().material = MaterialVariants[Random.Range(0, MaterialVariants.Length)];
        // else                             result.GetComponent<MeshRenderer>().material = Material.Create();       

        // result.GetComponent<MeshCollider>().sharedMesh = CreateCubeCollider();
        // result.GetComponent<MesНуhCollider>().convex = true;
        return result;
    }

    public CombineInstance GetCombineCollider(Matrix4x4 transform)
    {
        CombineInstance result = new CombineInstance();

        result.mesh = CreateCubeCollider(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.45f, -10, 0.45f));
        
        result.transform = transform;
        return result;
    }
    public CombineInstance GetCombineMesh(Matrix4x4 transform)
    {
        CombineInstance result = new CombineInstance();
        Mesh mesh = MeshVariants[Random.Range(0, MeshVariants.Length - 1)];

        if(MeshVariants.Length != 0) result.mesh = MeshVariants[Random.Range(0, MeshVariants.Length - 1)];
        else CreateCubeCollider(new Vector3(-0.45f, 0, -0.45f), new Vector3(0.45f, -10, 0.45f));
        
        result.transform = transform;
        return result;
    }
    public Material GetMaterial()
    {
        if(MaterialVariants.Length != 0) return MaterialVariants[Random.Range(0, MaterialVariants.Length - 1)];
        else return null;
    }

    Mesh CreateCubeCollider(Vector3 pos1, Vector3 pos2)
    {
        Vector3[] vertices = {
            new Vector3 (pos1.x, pos2.y, pos1.z),
            new Vector3 (pos2.x, pos2.y, pos1.z),
            new Vector3 (pos2.x, pos1.y, pos1.z),
            new Vector3 (pos1.x, pos1.y, pos1.z),
            new Vector3 (pos1.x, pos1.y, pos2.z),
            new Vector3 (pos2.x, pos1.y, pos2.z),
            new Vector3 (pos2.x, pos2.y, pos2.z),
            new Vector3 (pos1.x, pos2.y, pos2.z),
        };
        int[] triangles = {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            // 0, 6, 7, //face bottom
            // 0, 1, 6
        };
        
        Mesh result = new Mesh();
        result.name = "Collider";
        result.vertices = vertices;
        result.triangles = triangles;

        

        return result;
    }
}
