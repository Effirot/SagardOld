using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[System.Serializable]
public struct Checkers
{
    int X, Z;
    float UP;

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

    public enum mode{ NoHeight, Height }
    public static float Distance(Checkers a, Checkers b, mode Mode = mode.NoHeight)
    {
        if(Mode == mode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.ToVector3.y - b.ToVector3.y, 2) + Mathf.Pow(a.z - b.z, 2));
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
    }
    public static float Distance(Vector3 a, Vector3 b, mode Mode = mode.NoHeight)
    {
        if(Mode == mode.Height) return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.y - b.y, 2) + Mathf.Pow(a.z - b.z, 2));
        return Mathf.Sqrt(Mathf.Pow(a.x - b.x, 2) + Mathf.Pow(a.z - b.z, 2));
    }

    public Vector3 ToVector3{ get{ return new Vector3(x, up, z);} }

    public static bool CheckCoords(Checkers Coordinats) 
    {
        return Physics.Raycast(new Vector3(Coordinats.x, 1000, Coordinats.z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
    }
    public static bool CheckCoords(int x, int z) 
    {
        return Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map"));
    }
    




    public class PatchWay
    {
        public static Checkers[] WayTo(Checkers a, Checkers b) { return new Checkers[] { a, b }; }
        public static Vector3[] WayTo(Vector3 a, Vector3 b) { return new Vector3[] { a, b }; }
    
        private class PathNode
        {
            public Checkers Position { get; set; }
            public int PathLengthFromStart { get; set; }
            public PathNode CameFrom { get; set; }
            public int HeuristicEstimatePathLength { get; set; }
            public int EstimateFullPathLength { get { return this.PathLengthFromStart + this.HeuristicEstimatePathLength; } }
        }

        // public static List<Checkers> FindPath(Checkers start, Checkers goal)
        // {
        //     // Шаг 1.
        //     var closedSet = new List<PathNode>();
        //     var openSet = new List<PathNode>();
        //     // Шаг 2.
        //     openSet.Add(new PathNode()
        //     {
        //         Position = start,
        //         CameFrom = null,
        //         PathLengthFromStart = 0,
        //         HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
        //     });
            
        //     while (openSet.Count > 0)
        //     {
        //         // Шаг 3.
        //         var currentNode = openSet.OrderBy(node => 
        //         node.EstimateFullPathLength).First();
        //         // Шаг 4.
        //         if (currentNode.Position == goal)
        //         return GetPathForNode(currentNode);
        //         // Шаг 5.
        //         openSet.Remove(currentNode);
        //         closedSet.Add(currentNode);
        //         // Шаг 6.
        //         foreach (var neighbourNode in GetNeighbours(currentNode, goal))
        //         {
        //         // Шаг 7.
        //         if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
        //             continue;
        //         var openNode = openSet.FirstOrDefault(node =>
        //             node.Position == neighbourNode.Position);
        //         // Шаг 8.
        //         if (openNode == null)
        //             openSet.Add(neighbourNode);
        //         else
        //             if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
        //             {
        //             // Шаг 9.
        //             openNode.CameFrom = currentNode;
        //             openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
        //             }
        //         }
        //     }
        //     // Шаг 10.
        //     return null;
        // }

        private static int GetHeuristicPathLength(Checkers from, Checkers to)
        {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.z - to.z);
        }
        private static int GetDistanceBetweenNeighbours()
        {
            return 1;
        }
        

        GameObject CheckPosition(int x, int z) {
            Physics.Raycast(new Vector3(x, 1000, z), -Vector3.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Map", "Object"));  
            return hit.collider.gameObject;
        }

        // private static List<PathNode> GetNeighbours(PathNode pathNode, Checkers goal)
        // {
        //     var result = new List<PathNode>();
        //     for(int x = 0; x < 3; x++)
        //     {
        //         for(int z = 0; z < 3; z++)
        //         {
        //             bool Checked = 
        //         }
        //     }

            
        //     foreach (Checkers point in neighbourPoints)
        //     {
        //         if ()
        //             continue;
        //         // Заполняем данные для точки маршрута.
        //         var neighbourNode = new PathNode()
        //         {
        //             Position = point,
        //             CameFrom = pathNode,
        //             PathLengthFromStart = pathNode.PathLengthFromStart +
        //                 GetDistanceBetweenNeighbours(),
        //             HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
        //         };
        //         result.Add(neighbourNode);
        //     }
        //     return result;
        // }

        private static List<Checkers> GetPathForNode(PathNode pathNode)
        {
            var result = new List<Checkers>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }


    }
}
