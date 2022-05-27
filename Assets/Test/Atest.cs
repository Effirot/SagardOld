using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atest : MonoBehaviour
{
    bool[,] field = new bool[12, 12];
    void Start()
    {
        for(int i = 0; i < 12; i++)
        {
            for(int j = 0; j < 12; j++)
            {
                field[i, j] = true;
            }
        } 
        DrawWay(new Checkers(Random.Range(0, 12), Random.Range(0, 12)));
    }

    void DrawWay(Checkers EndPos)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        
        Checkers start = new Checkers(2, 2);
        
        List<Checkers> way = Checkers.PatchWay.FindPath(field, start, EndPos);

        lineRenderer.positionCount = way.Count;
        for(int i = 0; i < way.Count; i++)
        {
            lineRenderer.SetPosition(i, way[i].ToVector3);
        }
    }



}
