using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Numerics;

using SagardCL.SessionController;
using SagardCL;
using SagardCL.MapObjectInfo;
using System.Threading.Tasks;

namespace SagardCL.SessionController
{
    public delegate float FormulaUp(int x, int y, ulong key);
    public delegate CellMode FormulaMod(int x, int y, ulong key);
    public delegate IObjectOnMap FormulaLet(int x, int y, ulong key);

    public struct LayerTransform
    {
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        public LayerTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
    
    public enum CellMode
    {
        Void,
        Stone,
        Grassy,
        Sandy,
    }

    public class MapLayer
    {
        public Dictionary<string, IObjectOnMap> ObjectRegister = new Dictionary<string, IObjectOnMap>();
        public Dictionary<string, CharacterCore> CharacterRegister = new Dictionary<string, CharacterCore>(); 

        public ulong Key { get; private set; }
        
        public ushort ScaleX, ScaleZ;
        MapCell[,] PlatformMatrix;
        public byte ID;
        
        static byte LastLayerNumber = 0;

        public MapLayer(ushort xCoords, ushort zCoords, LayerTransform transform, FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet)
        {
            ScaleX = xCoords; ScaleZ = zCoords;
            PlatformMatrix = new MapCell[xCoords,zCoords];

            Key = KeyGen();
            ID = LastLayerNumber++;
        }
        public static ulong KeyGen(ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
        {
            byte[] buf = new byte[8];
            new System.Random().NextBytes(buf);

            return BitConverter.ToUInt64(buf, 0);
        }
    
        void GenerateRelief(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet)
        {
            
        }
    }
    struct MapCell
    {
        public bool Exist { get; set; }
        public Checkers position { get; set; }
        public List<MapEffect> Effects;
        public bool Protected;

        public CellMode CellMode { get; set; }
        
        public MapCell(Checkers Position, CellMode Mod, bool DeformProtection, uint MeshVariant = 0){ 
            this.Exist = Mod != CellMode.Void;

            this.position = Position;
            this.Effects = new List<MapEffect>();
            this.CellMode = Mod;
            this.Protected = DeformProtection | Mod != CellMode.Void;
        }
    }

    public class Session
    {
        
        // public Transform MapFolders => GameObject.Find("MapFolders").transform ?? new GameObject("MapFolders").transform;
        // public Transform AttackVisualizer => GameObject.Find("AttackVisualizer").transform;
        // public Transform AttackVisualizerFolder(byte layer) {
        //     GameObject result;
        //     result = GameObject.Find("AttackFolder " + layer);

        //     if(!result) {
        //         result = new GameObject("AttackFolder " + layer);
        //         result.transform.parent = GameObject.Find($"MapFolders/MapLayer ({layer})").transform;
        //     }

        //     return result.transform;
        // }

        public static Dictionary<string, CharacterCoreController> thisPlayerFigures = new Dictionary<string, CharacterCoreController>();
        public static Session Current;

        #region // Save Info
            
            public uint CurrentStep { get; private set; } = 0;

            public Dictionary<byte, MapLayer> Layers = new Dictionary<byte, MapLayer>();
            public MapPattern Pattern;

            public Session(params MapLayer[] layers)
            {
                foreach(var layer in layers)
                    Layers.Add(layer.ID, layer);

                Current = this;
            }
        
        #endregion
    }
    
    public class MapPattern
    {

    }
    // [System.Serializable] public class Session
    // {
    //     public static Dictionary<string, IObjectOnMap> ObjectRegister = new Dictionary<string, IObjectOnMap>();
    //     public static Dictionary<string, CharacterCore> CharacterRegister = new Dictionary<string, CharacterCore>(); 
    //     public static Dictionary<string, CharacterCoreController> thisPlayerFigures = new Dictionary<string, CharacterCoreController>();
        
    //     public static Session Current;

    //     public static GameObject MapFolders => GameObject.Find("MapFolders") ?? new GameObject("MapFolders");
    //     public static GameObject AttackVisualizer => GameObject.Find("AttackVisualizer");
    //     public static GameObject AttackVisualizerFolder(byte layer) {
    //         GameObject result;
    //         result = GameObject.Find("AttackFolder " + layer);

    //         if(!result) {
    //             result = new GameObject("AttackFolder " + layer);
    //             result.transform.parent = GameObject.Find($"MapFolders/MapLayer ({layer})").transform;
    //         }

    //         return result;
    //     }
        
    //     #region // Saving

    //         public int StepNumber = 0;

    //         public ulong key { get; private set; }
    //         ushort MapScaleX;
    //         ushort MapScaleZ;
    //         byte LayerCount;

    //         GameObject[] MapLayers;
    //         List<Material>[] MaterialsList;
    //         MapCell[][,] PlatformMatrix;

    //     #endregion

    //     #region // Controlling
        
    //         // public void ChangeHeigh(byte layer, params Checkers[] poses) 
    //         // {
    //         //     foreach(var pos in poses)
    //         //     {
    //         //         PlatformMatrix[layer][pos.x, pos.z].Mesh.transform = Matrix4x4.TRS(PlatformMatrix[layer][pos.x, pos.z].Mesh.transform.GetPosition() + new Vector3(0, pos.clearUp, 0), 
    //         //                                                                     PlatformMatrix[layer][pos.x, pos.z].Mesh.transform.rotation, 
    //         //                                                                     Vector3.one);

    //         //         PlatformMatrix[layer][pos.x, pos.z].Collider.transform = Matrix4x4.TRS(PlatformMatrix[layer][pos.x, pos.z].Collider.transform.GetPosition() + new Vector3(0, pos.clearUp, 0), 
    //         //                                                                         PlatformMatrix[layer][pos.x, pos.z].Collider.transform.rotation, 
    //         //                                                                         Vector3.one);
    //         //     }
    //         // }
    //         // public void ChangeModifier(byte layer, PlatformPreset Modifier, params Checkers[] Where)
    //         // {
    //         //     foreach (Checkers position in Where)
    //         //     {
    //         //         PlatformMatrix[layer][position.x, position.z] = new MapCell(
    //         //             position.Up(PlatformMatrix[layer][position.x, position.z].position.clearUp), 
    //         //             Modifier, 
    //         //             PlatformMatrix[layer][position.x, position.z].DeformProtection);
    //         //     }
    //         //     foreach(Material material in Modifier.MaterialVariants)
    //         //     if(!MaterialsList.Contains(material))
    //         //         MaterialsList.Add(material);  

    //         //     colliderMesh();  
    //         //     visibleMesh();
    //         // }

    //     #endregion
    //     #region // Overloads

    //         public Session(ushort ScaleX, ushort ScaleZ, byte floorCount, FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet)
    //         {
    //             MaterialsList = new List<Material>[floorCount];
    //             MapLayers = new GameObject[floorCount];

    //             MapScaleX = ScaleX;
    //             MapScaleZ = ScaleZ;
    //             LayerCount = floorCount;

    //             key = KeyGen();

    //             PlatformMatrix = new MapCell[floorCount][,];
    //             for(byte i = 0; i < LayerCount; i++)
    //                 LayerToScene(formulaUp, formulaMod, formulaLet, i);

    //             Current = this;
    //         }

    //         private MapCell[,] GenerateRelief(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, byte Layer)
    //         {
    //             MapCell[,] result = new MapCell[MapScaleX, MapScaleZ];

    //             if(MaterialsList[Layer] == null) MaterialsList[Layer] = new List<Material>();
    //             else MaterialsList[Layer].Clear();

    //             for(int x = 0; x < MapScaleX; x++)
    //             for(int z = 0; z < MapScaleZ; z++){
    //                 result[x, z] = new MapCell(
    //                     new Checkers(x, z, Layer, formulaUp(x, z, Layer, key)), 
    //                     formulaMod(x, z, Layer, key));

    //                 if(!MaterialsList[Layer].Contains(result[x, z].Material))
    //                     MaterialsList[Layer].Add(result[x, z].Material);}
                
    //             return result;
    //         }

    //         void LayerToScene(FormulaUp formulaUp, FormulaMod formulaMod, FormulaLet formulaLet, byte LayerNumber)
    //         {
    //             GameObject MapLayer = new GameObject($"MapLayer ({LayerNumber})",typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
    //             MapLayer.transform.position = new Vector3(0, LayerNumber * 6, 0);
    //             MapLayer.layer = LayerMask.NameToLayer("Map"+LayerNumber);
    //             MapLayer.transform.parent = MapFolders.transform;
                
    //             MapLayers[LayerNumber] = MapLayer;

    //             PlatformMatrix[LayerNumber] = GenerateRelief(formulaUp, formulaMod, formulaLet, LayerNumber);

    //             MapLayer.GetComponent<MeshFilter>().sharedMesh = VisibleMesh(LayerNumber);
    //             MapLayer.GetComponent<MeshRenderer>().materials = MaterialsList[LayerNumber].ToArray();
    //             MapLayer.GetComponent<MeshCollider>().sharedMesh = ColliderMesh(LayerNumber);
    //         }

            
    //     #endregion

    //     #region // Map Mesh

    //         Mesh ColliderMesh(byte Layer)
    //         {
    //             List<CombineInstance> instances = new List<CombineInstance>();
    //             for(int x = 0; x < MapScaleX; x++)
    //             for(int z = 0; z < MapScaleZ; z++)
    //                 instances.Add(PlatformMatrix[Layer][x, z].Collider);
                
                
    //             Mesh result = new Mesh();
    //             result.Clear();
    //             result.subMeshCount = 0;
    //             result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    //             result.CombineMeshes(instances.ToArray(), true);

    //             result.name = $"Combined collider layer {Layer}";

    //             return result;
    //         }
    //         Mesh VisibleMesh(byte Layer)
    //         {
    //             Dictionary<Material, List<CombineInstance>> SubMeshes = new Dictionary<Material, List<CombineInstance>>();

    //             foreach(Material material in MaterialsList[Layer]) SubMeshes.Add(material, new List<CombineInstance>()); 

    //             for(int x = 0; x < MapScaleX; x++) 
    //             for(int z = 0; z < MapScaleZ; z++)
    //                 SubMeshes[PlatformMatrix[Layer][x, z].Material].Add(PlatformMatrix[Layer][x, z].Mesh);
                
    //             List<CombineInstance> instanceByMaterial = new List<CombineInstance>();
    //             foreach(var value in SubMeshes)
    //             {
    //                 CombineInstance item = new CombineInstance();

    //                 item.mesh = new Mesh();
    //                 item.mesh.indexFormat = IndexFormat.UInt32;
    //                 item.mesh.CombineMeshes(value.Value.ToArray(), true);
    //                 item.transform = Matrix4x4.identity;
    //                 instanceByMaterial.Add(item);
    //             }

    //             Mesh result = new Mesh(); 
                
    //             result.Clear();
    //             result.subMeshCount = MaterialsList[Layer].Count;
    //             result.indexFormat = IndexFormat.UInt32;
    //             result.CombineMeshes(instanceByMaterial.ToArray(), false);

    //             result.RecalculateNormals(); 
                
    //             result.name = $"Combined mesh layer {Layer}";
    //             return result;
    //         }

    //     #endregion

    //     #region // Static Step System

    //         internal static List<StepAction> StepSystem = new List<StepAction>();
    //         public delegate Task StepAction(string StepStage);

    //         enum Step : int
    //         {
    //             BotLogic,

    //             Walking,
    //             Action,
    //             EffectUpdate,
    //             LateWalking,
    //             LandscapeDeform,
    //             DamageMath,
    //             Dead,
    //             Rest
    //         }

    //         public static async void StartStepTasks(int StepCycles = 0)
    //         {
    //             if(!MouseControlEvents.Controllable) return;
    //             MouseControlEvents.Controllable = false;   
                
    //             do{
    //                 Debug.ClearDeveloperConsole();
                    
    //                 for(int i = 0; i < Enum.GetNames(typeof(Step)).Length; i++){
    //                     //Debug.Log($"Now step: {(Step)i}");
    //                     MapUpdate.Invoke();
    //                     List<Task> task = new List<Task>();

    //                     Step step = (Step)i;

    //                     foreach(StepAction summon in StepSystem) { 
    //                         Task action = summon(step.ToString()); 
    //                         if(action != null) 
    //                             task.Add(action); }
                        
    //                     await Task.WhenAll(task.ToArray());
    //                 }
    //                 StepEnd.Invoke();
    //                 Current.StepNumber++;
    //                 StepCycles--;
    //             }while(StepCycles > 0);
                
    //             MouseControlEvents.Controllable = true;
    //         }

    //         public static UnityEvent StepEnd = new UnityEvent();

    //         public static UnityEvent<byte, List<Attack>> AttackTransporter = new UnityEvent<byte, List<Attack>>();
        
    //     #endregion
    //     #region // Controller
            
    //         public static UnityEvent MapUpdate = new UnityEvent();
    //         public static UnityEvent<bool> UsingControllers = new UnityEvent<bool>();

    //         public void DrawAttack(List<Attack> AttackZone, byte Layer, CharacterCore sender)
    //         {
    //             foreach(MapCell cell in PlatformMatrix[Layer]) { 
    //                 cell.AllAttacks.RemoveAll(a=>a.Sender==sender); 

    //                 cell.AllAttacks.Add(AttackZone.FindAll(a=>a.position == cell.position).ToArray());
    //                 if(!cell.AllAttacks.Contains) { cell.AttackVisualize.SetActive(false); continue; }

    //                 cell.AttackVisualize.SetActive(true);
    //                 cell.AttackVisualize.GetComponent<SpriteRenderer>().color = cell.AllAttacks.CombinedColor(); }
    //         }
        
    //     #endregion
    // }
        
    // [System.Serializable]public struct PlatformPreset
    // {
    //     [SerializeField]public Mesh[] MeshVariants;
    //     [SerializeField]public Material[] MaterialVariants;
    //     [SerializeField]public bool DeformProtection;

    //     public GameObject GetObject()
    //     {
    //         GameObject result = new GameObject("Platform", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
    //         result.isStatic = true;

    //         result.tag = "Map";
    //         result.layer = LayerMask.NameToLayer("Map");

    //         if(MeshVariants.Length != 0) result.GetComponent<MeshFilter>().mesh = MeshVariants[Random.Range(0, MeshVariants.Length)];
    //         else                         CreateCubeCollider(new Vector3(-0.45f, 0, -0.45f), new Vector3(0.45f, -10, 0.45f));
    //         if(MaterialVariants.Length != 0) result.GetComponent<MeshRenderer>().material = MaterialVariants[Random.Range(0, MaterialVariants.Length)];
    //         // else                             result.GetComponent<MeshRenderer>().material = Material.Create();       

    //         // result.GetComponent<MeshCollider>().sharedMesh = CreateCubeCollider();
    //         // result.GetComponent<MesНуhCollider>().convex = true;
    //         return result;
    //     }

    //     public CombineInstance GetCombineCollider(Matrix4x4 transform)
    //     {
    //         CombineInstance result = new CombineInstance();

    //         result.mesh = CreateCubeCollider(new Vector3(-0.5f, 0, -0.5f), new Vector3(0.45f, -10, 0.45f));
            
    //         result.transform = transform;
    //         return result;
    //     }
    //     public CombineInstance GetCombineMesh(Matrix4x4 transform)
    //     {
    //         CombineInstance result = new CombineInstance();
    //         Mesh mesh = MeshVariants[Random.Range(0, MeshVariants.Length - 1)];

    //         result.mesh = MeshVariants[Random.Range(0, MeshVariants.Length - 1)];
            
    //         result.transform = transform;
    //         return result;
    //     }
    //     public Material GetMaterial()
    //     {
    //         if(MaterialVariants.Length != 0) return MaterialVariants[Random.Range(0, MaterialVariants.Length - 1)];
    //         else return null;
    //     }

    //     Mesh CreateCubeCollider(Vector3 pos1, Vector3 pos2)
    //     {
    //         Vector3[] vertices = {
    //             new Vector3 (pos1.x, pos2.y, pos1.z),
    //             new Vector3 (pos2.x, pos2.y, pos1.z),
    //             new Vector3 (pos2.x, pos1.y, pos1.z),
    //             new Vector3 (pos1.x, pos1.y, pos1.z),
    //             new Vector3 (pos1.x, pos1.y, pos2.z),
    //             new Vector3 (pos2.x, pos1.y, pos2.z),
    //             new Vector3 (pos2.x, pos2.y, pos2.z),
    //             new Vector3 (pos1.x, pos2.y, pos2.z),
    //         };
    //         int[] triangles = {
    //             0, 2, 1, //face front
    //             0, 3, 2,
    //             2, 3, 4, //face top
    //             2, 4, 5,
    //             1, 2, 5, //face right
    //             1, 5, 6,
    //             0, 7, 4, //face left
    //             0, 4, 3,
    //             5, 4, 7, //face back
    //             5, 7, 6,
    //             // 0, 6, 7, //face bottom
    //             // 0, 1, 6
    //         };
            
    //         Mesh result = new Mesh();
    //         result.name = "Collider";
    //         result.vertices = vertices;
    //         result.triangles = triangles;

            

    //         return result;
    //     }
    // }


}