using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using UnityEngine.Rendering;

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
            #region 

                AllAttackZoneArchive[x, z] = new List<Attack>();

                GameObject AttackGizmo = Instantiate<GameObject>(AttackVisual, new Checkers(x, z, 0.04f), AttackVisual.transform.rotation, transform);
                AttackVisualsRealizers[x, z] = AttackGizmo;
                AttackGizmo.name = $"{x}:{z}";
                
                AttackGizmo.SetActive(false);

                AttackGizmo.transform.position = new Checkers(x, z, 0.04f);

            #endregion               
        
        }
    }
    private void ClearMap(){
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Map"))
        {   
            Destroy(obj);
        }
    }

    public static void DrawAttack(List<Attack> AttackZone, UnitController sender)
    {
        foreach(List<Attack> attacks in AllAttackZoneArchive) { if(attacks != null) attacks.RemoveAll((a) => a.WhoAttack == sender); }
        foreach(Attack attack in AttackZone) {  try{ AllAttackZoneArchive[attack.Where.x, attack.Where.z].Add(attack); } catch { }  }

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
                switch (attack.damageType)
                {
                    default: result += Color.HSVToRGB(0.01f, 1, attack.damage * 0.06f); break;
                    case DamageType.MetalHeal: goto case DamageType.Heal; 
                    case DamageType.Heal: result += Color.HSVToRGB(0.42f, 1, attack.damage * 0.06f); break;
                    case DamageType.Rezo: result += Color.HSVToRGB(67f / 360f, 1, attack.damage * 0.06f); break;
                    case DamageType.Pure: result += Color.HSVToRGB(274f / 360f, 1, attack.damage * 0.06f); break;
                }
            }
            return result;
        }


    }
}


[System.Serializable] public struct Map
{
    #region // Saving
        public Mesh MapMesh;
        public Mesh MapCollider;

        public int XScale { get; private set; }
        public int ZScale { get; private set; }

        uint key { get; }

        MapCell[,] PlatformMatrix;
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
        public void ChangeModifier(PlatformVisual Modifier)
        {
            
        }

        public List<Material> MaterialsList;
    #endregion
    #region // Overloads

        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, Vector2 Scale, params PlatformVisual[] visual)
        {
            XScale = (int)Scale.x; ZScale = (int)Scale.y; key = Key; 
            PlatformMatrix = new MapCell[XScale, ZScale];
            
            MaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);
            }

            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);

            visibleMesh();
            colliderMesh();
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, int PlayerNum, params PlatformVisual[] visual)
        {
            key = Key;
            XScale = PlayerNum * 9 + (((int)key / 23)%7); ZScale = PlayerNum * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];
            
            MaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);
            }

            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
            
            visibleMesh();
            colliderMesh();
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, int PlayerNum, params PlatformVisual[] visual)
        {
            key = (uint)Random.Range(0, 99999999);
            XScale = PlayerNum * 9 + (((int)key / 23)%7); ZScale = PlayerNum * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];
            
            MaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);
            }
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 
            
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
            
            visibleMesh();
            colliderMesh();
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, Vector2 Scale, params PlatformVisual[] visual)
        {
            key = (uint)Random.Range(0, 99999999);
            XScale = (int)Scale.x; ZScale = (int)Scale.y; 
            PlatformMatrix = new MapCell[XScale, ZScale];
            
            MaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);
            }
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
            
            visibleMesh();
            colliderMesh();
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, params PlatformVisual[] visual)
        {
            key = Key;
            XScale = 2 * 9 + (((int)key / 23)%7); ZScale = 2 * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];

            MaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);
            }
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
            
            visibleMesh();
            colliderMesh();
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, params PlatformVisual[] visual)
        {
            key = key = (uint)Random.Range(0, 99999999);
            XScale = 2 * 9 + (((int)key / 23)%7); ZScale = 2 * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[XScale, ZScale];

            MaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!MaterialsList.Contains(material))
                    MaterialsList.Add(material);
            }
            
            MapMesh = new Mesh(); 
            MapCollider = new Mesh(); 

            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
            
            visibleMesh();
            colliderMesh();
        }
        
    #endregion
    #region // Map Cell information
        public delegate float FormulaUp(int x, int y, uint key);
        public delegate int FormulaMod(int x, int y, uint key);
        public delegate int FormulaLet(int x, int y, uint key);

        [System.Serializable] struct MapCell
        {
            public Let Let;

            Checkers position;

            public CombineInstance Mesh;
            public CombineInstance Collider;
            public Material Material;
            
            public MapCell(Checkers Position, PlatformVisual Mod, float heigh, Let let = null)
            { Let = let; 
            position = Position;

            Mesh = Mod.GetCombineMesh(Matrix4x4.TRS(new Vector3(position.x, heigh, position.z), Quaternion.Euler(0, Random.Range(0, 3) * 90, 0), Vector3.one)); 
            Collider = Mod.GetCombineCollider(Matrix4x4.TRS(new Vector3(position.x, heigh, position.z), Quaternion.Euler(0, 0, 0), Vector3.one));
            Material = Mod.GetMaterial(); }
            public MapCell(int X, int Z, PlatformVisual Mod, float heigh, Let let = null)
            { Let = let; 
            position = new Checkers(X, Z);

            //.Mod.GetCombineCollider(Matrix4x4.TRS(new Vector3(x, PlatformMatrix[x, z].Up, z), Quaternion.Euler(0, 0, 0), Vector3.one))
            Mesh = Mod.GetCombineMesh(Matrix4x4.TRS(new Vector3(position.x, heigh, position.z), Quaternion.Euler(0, Random.Range(0, 3) * 90, 0), Vector3.one)); 
            Collider = Mod.GetCombineCollider(Matrix4x4.TRS(new Vector3(position.x, heigh, position.z), Quaternion.Euler(0, 0, 0), Vector3.one)); 
            Material = Mod.GetMaterial(); }
            
            public void AddVerticalPosition(float y)
            {

            }
        }
        private MapCell[,] GenerateRelief(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, params PlatformVisual[] visual)
        {
            MapCell[,] result = new MapCell[XScale, ZScale];

            for(int x = 0; x < XScale; x++) 
            for(int z = 0; z < ZScale; z++)
            result[x, z] = new MapCell(x, z, visual[formulaMod(x, z, key)], formulaUp(x, z, key));
            
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
}

[System.Serializable]public class PlatformVisual
{
    [SerializeField]public Mesh[] MeshVariants;
    [SerializeField]public Material[] MaterialVariants;

    public PlatformVisual(Material material, Mesh Mesh) { MaterialVariants = new Material[] { material }; MeshVariants = new Mesh[] { Mesh }; }
    public PlatformVisual(Material material, params Mesh[] Meshes) { MaterialVariants = new Material[] { material }; MeshVariants = Meshes; }
    public PlatformVisual(Material[] material, params Mesh[] Meshes) { MaterialVariants = material; MeshVariants = Meshes; }

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
