using System.Threading.Tasks;
using UnityEngine;

public class IDgenerator : MonoBehaviour
{
    public IDgenerator ParentID;
    public uint ID;
    public static uint lastID = 1;
    void Awake()
    {
        if(ParentID != null)
            { IDset(); }
        else { ID = lastID;
        lastID++; }

        InGameEvents.UnitLog.Invoke(ID);
    }

    async void IDset() { await Task.Delay(2);  ID = ParentID.ID; }
    
}
