using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

[System.Serializable]
public struct Checkers
{
    [SerializeField] static int X, Z;
    [SerializeField] static float UP;

    public int x { get{ return X; } }
    public int z { get{ return Z; } }
    public float up { get{ return UP; } }

    private float YUpPos()
    {
        RaycastHit hit;
        Physics.Raycast(new Vector3(x, 1000, Z), -Vector3.up, out hit, Mathf.Infinity, LayerMask.GetMask("Map"));
        return hit.point.y;
    }

    public Checkers(float Xadd, float Zadd, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Xadd); Z = (int)Mathf.Round(Zadd); UP = YUpPos() + UPadd;
    }
    public Checkers(Vector3 Vector3add, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Vector3add.x); Z = (int)Mathf.Round(Vector3add.z); UP = YUpPos() + UPadd;
    }
    public Checkers(Vector2 Vector2add, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Vector2add.x); Z = (int)Mathf.Round(Vector2add.y); UP = YUpPos() + UPadd;
    }
    public Checkers(Transform Transformadd, float UPadd = 0) 
    { 
        X = (int)Mathf.Round(Transformadd.position.x); Z = (int)Mathf.Round(Transformadd.position.z); UP = YUpPos() + UPadd;
    }

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

    public Vector3 ToVector3{ get{ return new Vector3(X, UP, Z);} }
    




    public static class PatchWay
    {
        public static Checkers[] WayTo(Checkers a, Checkers b)
        {
            return new Checkers[] { a, b };
        }
        public static Vector3[] WayTo(Vector3 a, Vector3 b)
        {
            return new Vector3[] { a, b };
    }
    
        private class PathNode
        {
            // Координаты точки на карте.
            public Checkers Position { get; set; }
            // Длина пути от старта (G).
            public int PathLengthFromStart { get; set; }
            // Точка, из которой пришли в эту точку.
            public PathNode CameFrom { get; set; }
            // Примерное расстояние до цели (H).
            public int HeuristicEstimatePathLength { get; set; }
            // Ожидаемое полное расстояние до цели (F).
            public int EstimateFullPathLength { get { return this.PathLengthFromStart + this.HeuristicEstimatePathLength; } }
        }

        public static List<Checkers> FindPath(bool[,] field, Checkers start, Checkers goal)
        {
            // Шаг 1.
            var closedSet = new List<PathNode>();
            var openSet = new List<PathNode>();
            // Шаг 2.
            PathNode startNode = new PathNode()
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };

            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                // Шаг 3.
                var currentNode = openSet.OrderBy(node => 
                node.EstimateFullPathLength).First();
                // Шаг 4.
                if (currentNode.Position == goal)
                return GetPathForNode(currentNode);
                // Шаг 5.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                // Шаг 6.
                foreach (var neighbourNode in GetNeighbours(currentNode, goal, field))
                {
                // Шаг 7.
                if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
                    continue;
                var openNode = openSet.FirstOrDefault(node =>
                    node.Position == neighbourNode.Position);
                // Шаг 8.
                if (openNode == null)
                    openSet.Add(neighbourNode);
                else
                    if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                    {
                    // Шаг 9.
                    openNode.CameFrom = currentNode;
                    openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
        }

        private static int GetHeuristicPathLength(Checkers from, Checkers to)
        {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.z - to.z);
        }
        private static int GetDistanceBetweenNeighbours()
        {
            return 1;
        }
        
        
        private static List<PathNode> GetNeighbours(PathNode pathNode, 
        Checkers goal, bool[,] field)
        {
        var result = new List<PathNode>();
        
        // Соседними точками являются соседние по стороне клетки.
        Checkers[] neighbourPoints = new Checkers[4];
        neighbourPoints[0] = new Checkers(pathNode.Position.x + 1, pathNode.Position.z);
        neighbourPoints[1] = new Checkers(pathNode.Position.x - 1, pathNode.Position.z);
        neighbourPoints[2] = new Checkers(pathNode.Position.x, pathNode.Position.z + 1);
        neighbourPoints[3] = new Checkers(pathNode.Position.x, pathNode.Position.z - 1);
        
        foreach (Checkers point in neighbourPoints)
        {
            // Проверяем, что не вышли за границы карты.
            if (point.x < 0 || point.x >= field.GetLength(0))
            continue;
            if (point.z < 0 || point.z >= field.GetLength(1))
            continue;
            // Проверяем, что по клетке можно ходить.
            if (field[point.x, point.z])
                continue;
            // Заполняем данные для точки маршрута.
            var neighbourNode = new PathNode()
            {
            Position = point,
            CameFrom = pathNode,
            PathLengthFromStart = pathNode.PathLengthFromStart +
                GetDistanceBetweenNeighbours(),
            HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
            };
            result.Add(neighbourNode);
        }
        return result;
        }

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
