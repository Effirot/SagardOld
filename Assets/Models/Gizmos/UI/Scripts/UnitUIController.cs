using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using SagardCL.ParameterManipulate;
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
    public GameObject InventorySlot;
    public GameObject ArtifacerInventorySlot;
    public GameObject ItemPreset;
    [Space] 
    public MoveOnUi MoveOnCanvas;

    public static UnityEvent<string, GameObject, CharacterCoreController> UiEvent = new UnityEvent<string, GameObject, CharacterCoreController>();

    // void Start() { UiEvent.AddListener((a, b, c) => {
    //     switch (a)
    //     {
    //         case "OpenForPlayer": Open(b, c); break;
    //         case "CloseForPlayer": Close(); break;
    //     }
    // }); }

    // private void Open(GameObject Summoner, CharacterController lifeParameters)
    // {
    //     UI.SetActive(true);
    //     MoveOnCanvas.Target = Summoner.transform;
    //     UpdateUi(lifeParameters);
    // }

    // private static List<GameObject> UIelements = new List<GameObject>();
    
    // private void UpdateUi(CharacterController lifeParameters)
    // {
    //     foreach (GameObject element in UIelements) { Destroy(element); }

    //     #region // Skill Vision
    //     {
    //         int count = 0;
    //         List<Skill> skills = lifeParameters.SkillRealizer.AvailbleSkills;
            
    //         foreach (Skill skill in skills) 
    //         { 
    //             if(!skill.isEmpty){     
    //                 GameObject obj = Instantiate(SkillPreset, UI.transform.Find("Skills").transform);
    //                 UIelements.Add(obj); 
                    
    //                 obj.transform.Find("Ico").GetComponent<Image>().sprite = skill.image;
    //                 obj.transform.Find("StaminaIco/StaminaUseVisual").GetComponent<TextMeshProUGUI>().text = skill.UsingStamina + "";
    //                 obj.name = count.ToString();
    //                 obj.GetComponent<Button>().interactable = lifeParameters.Stamina.Value >= skill.UsingStamina;
    //                 obj.GetComponent<Button>().onClick.AddListener(() => 
    //                 {
    //                     lifeParameters.CurrentSkillIndex = int.Parse(obj.name);
                        
    //                     foreach(GameObject element in UIelements)
    //                     {
    //                         Button butt = null;
    //                         if(element) element.TryGetComponent<Button>(out butt);
    //                         if(butt) butt.interactable = lifeParameters.SkillRealizer.AvailbleSkills
    //                                     [int.Parse(element.transform.Find("StaminaIco/StaminaUseVisual").GetComponent<TextMeshProUGUI>().text)].UsingStamina <= lifeParameters.Stamina.Value;
    //                     }
    //                     obj.GetComponent<Button>().interactable = false;
    //                 });
    //             }
    //             count++;
    //         }
    //     }
    //     #endregion
    //     #region // State Bar Vision
    //         InstantiateBars(lifeParameters.Health);
    //         InstantiateBars(lifeParameters.Sanity);
    //         InstantiateBars(lifeParameters.Stamina);

    //         if(lifeParameters.OtherStates != null) foreach(IStateBar stats in lifeParameters.OtherStates) { InstantiateBars(stats); }    

    //         void InstantiateBars(IStateBar stateBar) {
    //             GameObject obj = Instantiate(StateBarPreset, UI.transform.Find("Bars").transform);
    //             UIelements.Add(obj);

    //             obj.name = stateBar.GetType().Name;

    //             obj.GetComponent<Slider>().maxValue = stateBar.Max;
    //             obj.GetComponent<Slider>().value = stateBar.Value;
    //             obj.transform.Find("Value").GetComponent<Image>().color = stateBar.BarColor;
    //             obj.transform.Find("Value/ValueNum").GetComponent<TextMeshProUGUI>().text = stateBar.Value + "";
    //             obj.transform.Find("MaxNum").GetComponent<TextMeshProUGUI>().text = stateBar.Max + "";
    //         }
    //     #endregion
    //     #region // Inventory Vision
    //     {
    //         int count = 0;
    //         while(count < lifeParameters.InventorySize + lifeParameters.ArtifacerInventorySize)
    //         {
                
    //             if(count < lifeParameters.ArtifacerInventorySize)
    //             {
    //                 GameObject obj = Instantiate(ArtifacerInventorySlot, UI.transform.Find("Inventory").transform);
    //                 UIelements.Add(obj);
    //             }
    //             else
    //             {
    //                 GameObject obj = Instantiate(InventorySlot, UI.transform.Find("Inventory").transform);
    //                 UIelements.Add(obj);
    //             }
    //             count++;

    //         }
    //     }
    //     #endregion
    // }

    // private void Close()
    // {
    //     UI.SetActive(false);
    // }






}
