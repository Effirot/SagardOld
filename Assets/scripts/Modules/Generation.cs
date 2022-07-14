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
            //map.PutCell(x, z, transform);

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
        [SerializeField] int scaleX, scaleZ;
        public int XScale { get{ return scaleX; } }
        public int ZScale { get{ return scaleZ; } }

        uint key { get; }

        MapCell[,] PlatformMatrix;

        private List<Material> uniqueMaterialsList;
    #endregion
    #region // Overloads

        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, Vector2 Scale, params PlatformVisual[] visual)
        {
            scaleX = (int)Scale.x; scaleZ = (int)Scale.y; key = Key; 
            PlatformMatrix = new MapCell[scaleX, scaleZ];
            
            uniqueMaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!uniqueMaterialsList.Contains(material))
                    uniqueMaterialsList.Add(material);
            }
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, int PlayerNum, params PlatformVisual[] visual)
        {
            key = Key;
            scaleX = PlayerNum * 9 + (((int)key / 23)%7); scaleZ = PlayerNum * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[scaleX, scaleZ];
            
            uniqueMaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!uniqueMaterialsList.Contains(material))
                    uniqueMaterialsList.Add(material);
            }
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, int PlayerNum, params PlatformVisual[] visual)
        {
            key = (uint)Random.Range(0, 99999999);
            scaleX = PlayerNum * 9 + (((int)key / 23)%7); scaleZ = PlayerNum * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[scaleX, scaleZ];
            
            uniqueMaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!uniqueMaterialsList.Contains(material))
                    uniqueMaterialsList.Add(material);
            }
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, Vector2 Scale, params PlatformVisual[] visual)
        {
            key = (uint)Random.Range(0, 99999999);
            scaleX = (int)Scale.x; scaleZ = (int)Scale.y; 
            PlatformMatrix = new MapCell[scaleX, scaleZ];
            
            uniqueMaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!uniqueMaterialsList.Contains(material))
                    uniqueMaterialsList.Add(material);
            }
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, uint Key, params PlatformVisual[] visual)
        {
            key = Key;
            scaleX = 2 * 9 + (((int)key / 23)%7); scaleZ = 2 * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[scaleX, scaleZ];

            uniqueMaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!uniqueMaterialsList.Contains(material))
                    uniqueMaterialsList.Add(material);
            }
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
        }
        public Map(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, params PlatformVisual[] visual)
        {
            key = key = (uint)Random.Range(0, 99999999);
            scaleX = 2 * 9 + (((int)key / 23)%7); scaleZ = 2 * 9 + (((int)key / 14)%7); 
            PlatformMatrix = new MapCell[scaleX, scaleZ];

            uniqueMaterialsList = new List<Material>();
            foreach(PlatformVisual v in visual) foreach(Material material in v.MaterialVariants)
            {
                if(!uniqueMaterialsList.Contains(material))
                    uniqueMaterialsList.Add(material);
            }
            PlatformMatrix = GenerateRelief(formulaUp, formulaMod, formulaLet, visual);
        }
        
    #endregion

    #region // Map Cell information

        [System.Serializable] struct MapCell
        {
            public PlatformVisual Modifier;
            public float Up;
            public Let Let;

            public CombineInstance Mesh;
            public CombineInstance Collider;
            
            public MapCell(PlatformVisual Mod, float up, Let let = null)
            { Modifier = Mod; Up = Mathf.Clamp(up, 0, 4); Let = let; Mesh = new CombineInstance(); Collider = new CombineInstance(); }

            public void AddVerticalPosition(float y)
            {

            }
        }
        private MapCell[,] GenerateRelief(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, params PlatformVisual[] visual)
        {
            MapCell[,] result = new MapCell[scaleX, scaleZ];

            for(int x = 0; x < scaleX; x++) 
            for(int z = 0; z < scaleZ; z++)
            result[x, z] = new MapCell(visual[formulaMod(x, z, key)], formulaUp(x, z, key));
            
            return result;
        }

    #endregion
    #region // Platform visualizer

        [System.Serializable]public struct PlatformVisual
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

                result.mesh = CreateCubeCollider(new Vector3(-0.45f, 0, -0.45f), new Vector3(0.45f, -10, 0.45f));
                
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
                    0, 6, 7, //face bottom
                    0, 1, 6
                };
                
                Mesh result = new Mesh();
                result.name = "Collider";
                result.vertices = vertices;
                result.triangles = triangles;

                

                return result;
            }
        }
        public delegate float FormulaUp(int x, int y, uint key);
        public delegate int FormulaMod(int x, int y, uint key);
        public delegate int FormulaLet(int x, int y, uint key);
        public GameObject PutCell(int x, int z, Transform parent = null)
        {
            try { 
                MapCell cell = PlatformMatrix[x, z]; 
                GameObject result = PlatformMatrix[x, z].Modifier.GetObject();
                
                result.name = $"{x}:{z} - cell.Modifier";

                result.transform.position = new Vector3(x, cell.Up, z);

                result.transform.parent = parent;

                return result;
            }
            catch { return null; }
        }

    #endregion
    #region // Let
        public class Let
        {

        }
    #endregion

    #region // Map Mesh
        public Mesh colliderMesh()
        {
            Mesh result = new Mesh();
            List<CombineInstance> instances = new List<CombineInstance>();
            for(int x = 0; x < scaleX - 1; x++) 
            for(int z = 0; z < scaleZ - 1; z++)
            {
                instances.Add(PlatformMatrix[x, z].Modifier.GetCombineCollider(Matrix4x4.TRS(new Vector3(x, PlatformMatrix[x, z].Up, z), Quaternion.Euler(0, 0, 0), Vector3.one)));
            }
            result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            result.CombineMeshes(instances.ToArray());

            return result;
        }
        public Mesh visibleMesh(out Material[] materials)
        {
            Dictionary<Material, List<CombineInstance>> SubMeshes = new Dictionary<Material, List<CombineInstance>>();
            foreach(Material material in uniqueMaterialsList) { SubMeshes.Add(material, new List<CombineInstance>()); }

            for(int x = 0; x < scaleX - 1; x++) 
            for(int z = 0; z < scaleZ - 1; z++)
            {
                CombineInstance subMesh = PlatformMatrix[x, z].Modifier.GetCombineMesh(Matrix4x4.TRS(new Vector3(x, PlatformMatrix[x, z].Up, z), Quaternion.Euler(0, 0, 0), Vector3.one));

                Material material = PlatformMatrix[x, z].Modifier.GetMaterial();

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

            Mesh result = new Mesh();
            result.subMeshCount = uniqueMaterialsList.Count;
            result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            result.CombineMeshes(instanceByMaterial.ToArray(), false);

            materials = uniqueMaterialsList.ToArray();
            return result;
        }




    #endregion

}