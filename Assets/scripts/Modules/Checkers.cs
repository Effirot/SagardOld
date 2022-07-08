using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable] public struct Checkers
{
    [SerializeField] int X, Z;
    [SerializeField] float UP;

    public int x { get{ return X; } }
    public int z { get{ return Z; } }
    public float up { get{ return this.UP + YUpPos(); } }
    public float clearUp { get{ return UP; } }

    float YUpPos()
    {
        if (Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map")))
            return hit.point.y;
        return 0;
    }

    #region // =============================== Realizations

    public Checkers(float Xadd, float Zadd, float UPadd = 0) { X = (int)Mathf.Round(Xadd); Z = (int)Mathf.Round(Zadd); UP = UPadd; }
    public Checkers(Vector3 Vector3add, float UPadd = 0) { X = (int)Mathf.Round(Vector3add.x); Z = (int)Mathf.Round(Vector3add.z); UP = UPadd; }
    public Checkers(Vector2 Vector2add, float UPadd = 0) { X = (int)Mathf.Round(Vector2add.x); Z = (int)Mathf.Round(Vector2add.y); UP = UPadd; }
    public Checkers(Transform Transformadd, float UPadd = 0) { X = (int)Mathf.Round(Transformadd.position.x); Z = (int)Mathf.Round(Transformadd.position.z); UP = UPadd; }

    public static implicit operator Vector3(Checkers a) { return new Vector3(a.x, a.up, a.z); }
    public static implicit operator Checkers(Vector3 a) { return new Checkers(a.x, a.z); }

    public static Checkers operator +(Checkers a, Checkers b) { return new Checkers(a.x + b.x, a.z + b.z, a.up); }
    public static Checkers operator -(Checkers a, Checkers b) { return new Checkers(a.x - b.x, a.z - b.z, a.up); }
    public static bool operator ==(Checkers a, Checkers b) { return a.x == b.x & a.z == b.z; }
    public static bool operator !=(Checkers a, Checkers b) { return !(a.x == b.x & a.z == b.z); }
    
    public override int GetHashCode() { return 0; }  
    public override bool Equals(object o) { return true; } 

    #endregion
    #region // =============================== Math

    public enum mode{ NoHeight, Height, OnlyHeight, }
    public static float Distance(Checkers a, Checkers b, mode Mode = mode.NoHeight)
    {
        if(Mode == mode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.ToVector3().y - b.ToVector3().y, 2) + Mathf.Pow(a.z - b.z, 2));
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
    }
    public static float Distance(Vector3 a, Vector3 b, mode Mode = mode.NoHeight)
    {
        if(Mode == mode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
    }

    public Vector3 ToVector3(){ return new Vector3(x, up, z); }
    public static List<Vector3> ToVector3List(List<Checkers> checkers) 
    { 
        List<Vector3> list = new List<Vector3>();
        foreach(Checkers checker in checkers){ list.Add(checker.ToVector3()); }
        return list;        
    }
    public static List<Checkers> ToCheckersList(List<Vector3> vector3, float up = 0) 
    { 
        List<Checkers> list = new List<Checkers>();
        foreach(Checkers vector in vector3){ list.Add(new Checkers(vector, up)); }
        return list;        
    }

    public static bool CheckCoords(Checkers Coordinates) 
    {
        return Physics.Raycast(new Vector3(Coordinates.x, 1000, Coordinates.z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
    }
    public static bool CheckCoords(int x, int z) 
    {
        return Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
    }
    
    #endregion

    public static class PatchWay
    {
        public static async IAsyncEnumerable<Checkers> WayTo(Checkers a, Checkers b, int MaxSteps, float CheckersUp = 0.1f) 
        { 
            await Task.Delay(0); 
            yield return new Checkers(a, CheckersUp); 
            yield return new Checkers(b, CheckersUp);
        }

    }

    public enum EMoveAction { walk, jump, fall, swim };
    
    public class PathPoint
    {
        // текущая точка
        public Checkers point { get; set; }
        // расстояние от старта
        public float pathLenghtFromStart { get; set; }
        // примерное расстояние до цели
        public float heuristicEstimatePathLenght { get; set; }
        // еврестическое расстояние до цели
        public float estimateFullPathLenght
        {
            get
            {
               return this.heuristicEstimatePathLenght + this.pathLenghtFromStart;
            }
        }
        // способ движения
        public EMoveAction moveAction = EMoveAction.walk;
        // точка из которой пришли сюда
        public PathPoint cameFrom;
            private PathPoint NewPathPoint(Checkers point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction)
        {
            PathPoint a = new PathPoint();
            a.point = point;
            a.pathLenghtFromStart = pathLenghtFromStart;
            a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
            a.moveAction = moveAction;
            return a;
        }

        private PathPoint NewPathPoint(Checkers point, float pathLenghtFromStart, float heuristicEstimatePathLenght, EMoveAction moveAction, PathPoint ppoint)
        {
            PathPoint a = new PathPoint();
            a.point = point;
            a.pathLenghtFromStart = pathLenghtFromStart;
            a.heuristicEstimatePathLenght = heuristicEstimatePathLenght;
            a.moveAction = moveAction;
            a.cameFrom = ppoint;
            return a;
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Checkers))]
public class CheckersEditor : Editor
{
    // SerializedProperty x, z, up;

    // private void OnEnable() {
    //     x = serializedObject.FindProperty("X");
    //     z = serializedObject.FindProperty("Z");

    //     up = serializedObject.FindProperty("UP");
    // }

    // public override void OnInspectorGUI()
    // {
    //     EditorGUILayout.BeginHorizontal();

    //     EditorGUILayout.PropertyField(x, new GUIContent ("X"));
    //     EditorGUILayout.PropertyField(z, new GUIContent ("Z"));

    //     EditorGUILayout.EndHorizontal();

    //     EditorGUILayout.Slider(up, -1, 10, new GUIContent ("Up"));
    // }


}
#endif