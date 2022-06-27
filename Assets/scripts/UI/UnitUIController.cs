using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

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
            case WhatUiDo.Update: UpdateUi (c); break;
        }
    }); }

    private List<GameObject> UIelements = new List<GameObject>();

    private async void Open(GameObject Summoner, UnitController lifeParameters)
    {
        
        await System.Threading.Tasks.Task.Delay(1);
        UI.SetActive(true);
        MoveOnCanvas.Target = Summoner.transform;
        UpdateUi(lifeParameters);
    }

    private void UpdateUi(UnitController lifeParameters)
    {
        foreach (GameObject element in UIelements) { Destroy(element); }
        
        List<BaseSkill> skills = lifeParameters.SkillRealizer.AvailbleSkills;
        int count = 0;
        // Skill Vision
        foreach (BaseSkill skill in skills) 
        { 
            if(skill.Type != HitType.Empty){            
                GameObject obj = Instantiate(SkillPreset, UI.transform.Find("Skills").transform);
                UIelements.Add(obj); 
                
                obj.transform.Find("Ico").GetComponent<Image>().sprite = skill.image;
                obj.transform.Find("StaminaIco/StaminaUseVisual").GetComponent<TextMeshProUGUI>().text = skill.UsingStamina + "";
                obj.name = count.ToString();
                obj.GetComponent<Button>().interactable = lifeParameters.Stamina.State >= skill.UsingStamina;
                obj.GetComponent<Button>().onClick.AddListener(() => 
                {
                    lifeParameters.CurrentSkillIndex = int.Parse(obj.name);
                    
                    foreach(GameObject element in UIelements)
                    {
                        if(element)element.GetComponent<Button>().interactable = lifeParameters.SkillRealizer.AvailbleSkills
                                    [int.Parse(transform.Find("StaminaIco/StaminaUseVisual").GetComponent<TextMeshProUGUI>().text)].UsingStamina <= lifeParameters.Stamina.State;
                    }
                    obj.GetComponent<Button>().interactable = false;
                });
            }
            count++;
        }
    
        // State Bar Vision
        

    
    }

    GameObject InstantiateStateBar(GameObject obj, Transform parent)
    {
        GameObject stateBar = Instantiate(obj, parent);
    
        return stateBar;
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
