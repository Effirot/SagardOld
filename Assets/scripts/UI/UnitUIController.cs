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

    public static UnityEvent<string, GameObject, UnitController> UiEvent = new UnityEvent<string, GameObject, UnitController>();

    void Start() { UiEvent.AddListener((a, b, c) => {
        switch (a)
        {
            case "OpenForPlayer": Open(b, c); break;
            case "CloseForPlayer": Close(); break;
        }
    }); }

    

    private async void Open(GameObject Summoner, UnitController lifeParameters)
    {
        
        await System.Threading.Tasks.Task.Delay(1);
        UI.SetActive(true);
        MoveOnCanvas.Target = Summoner.transform;
        UpdateUi(lifeParameters);
    }


    private List<GameObject> UIelements = new List<GameObject>();
    private void UpdateUi(UnitController lifeParameters)
    {
        foreach (GameObject element in UIelements) { Destroy(element); }

        int count = 0;
        // Skill Vision
        foreach (BaseSkill skill in lifeParameters.SkillRealizer.AvailbleSkills) 
        { 
            if(skill.Type != HitType.Empty){            
                GameObject obj = Instantiate(SkillPreset, UI.transform.Find("Skills").transform);
                UIelements.Add(obj); 
                
                obj.transform.Find("Ico").GetComponent<Image>().sprite = skill.image;
                obj.transform.Find("StaminaIco/StaminaUseVisual").GetComponent<TextMeshProUGUI>().text = skill.UsingStamina + "";
                obj.name = count.ToString();
                obj.GetComponent<Button>().interactable = lifeParameters.Stamina.Value >= skill.UsingStamina;
                obj.GetComponent<Button>().onClick.AddListener(() => 
                {
                    lifeParameters.CurrentSkillIndex = int.Parse(obj.name);
                    
                    foreach(GameObject element in UIelements)
                    {
                        if(element.GetComponent<Button>()) element.GetComponent<Button>().interactable = lifeParameters.SkillRealizer.AvailbleSkills
                                    [int.Parse(element.transform.Find("StaminaIco/StaminaUseVisual").GetComponent<TextMeshProUGUI>().text)].UsingStamina <= lifeParameters.Stamina.Value;
                    }
                    obj.GetComponent<Button>().interactable = false;
                });
            }
            count++;
        }
    
        // State Bar Vision
        InstantiateBars(lifeParameters.Health);
        InstantiateBars(lifeParameters.Sanity);
        InstantiateBars(lifeParameters.Stamina);

        if(lifeParameters.OtherStates != null) foreach(StateBar stats in lifeParameters.OtherStates) { InstantiateBars(stats); }    
    }

    void InstantiateBars(StateBar stateBar) {
        GameObject obj = Instantiate(StateBarPreset, UI.transform.Find("Bars").transform);
        UIelements.Add(obj);

        obj.GetComponent<Slider>().maxValue = stateBar.Max;
        obj.GetComponent<Slider>().value = stateBar.Value;
        obj.transform.Find("Value").GetComponent<Image>().color = stateBar.BarColor;
        obj.transform.Find("Value/ValueNum").GetComponent<TextMeshProUGUI>().text = stateBar.Value + "";
        obj.transform.Find("MaxNum").GetComponent<TextMeshProUGUI>().text = stateBar.Max + "";

        
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
