using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Events;

public class UnitUIController : MonoBehaviour
{
    public GameObject UI;
    [Space]
    public GameObject StateBarPreset;
    public GameObject SkillPreset;
    public GameObject ItemPreset;
    [Space] 
    public MoveOnUi MoveOnCanvas;

    public static UnityEvent<WhatUiDo, GameObject, UnitController> UiEvent = new UnityEvent<WhatUiDo, GameObject, UnitController>();
    public enum WhatUiDo
    {
        Open,
        Close,
        Update,
    }

    void Start() { UiEvent.AddListener((a, b, c) => { 
        switch (a)
        {
            case WhatUiDo.Open: Open(b, c); break;
            case WhatUiDo.Close: Close(); break;
            case WhatUiDo.Update: UpdateUi (b, c); break;
        }
    }); }

    private List<GameObject> UIelements = new List<GameObject>();

    private async void Open(GameObject Summoner, UnitController lifeParameters)
    {
        await System.Threading.Tasks.Task.Delay(1);
        UI.SetActive(true);
        MoveOnCanvas.Target = Summoner.transform;
        UpdateUi(Summoner, lifeParameters);
    }

    private void UpdateUi(GameObject Summoner, UnitController lifeParameters)
    {
        foreach (GameObject element in UIelements) { Destroy(element); }
        
        List<BaseSkill> skills = lifeParameters.SkillRealizer.AvailbleSkills;

        int count = 0;
        foreach (BaseSkill skill in skills) 
        { 
            GameObject obj = Instantiate(SkillPreset, UI.transform.Find("Skills").transform);
            UIelements.Add(obj); 
            
            obj.transform.Find("Ico").GetComponent<Image>().sprite = skill.image;
            obj.name = count.ToString();
            count++;

            obj.GetComponent<Button>().onClick.AddListener(() => 
            {
                lifeParameters.SkillRealizer.SkillIndex = int.Parse(obj.name);

            });

        
        }
    }


    IEnumerator RotateTo(Vector3 ToVector, float speed = 1)
    {
        while(transform.eulerAngles != ToVector)
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, ToVector, speed);
            yield return null;
        }
        yield break;
    }




    private void Close()
    {
        UI.SetActive(false);
    }






}
