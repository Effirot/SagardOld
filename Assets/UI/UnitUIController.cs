using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SagardCL;
using System.Threading.Tasks;
using UnityEngine.UI;

public class UnitUIController : MonoBehaviour
{
    public LifeParameters LifeParams { get; set; }

    
    [Space(2), SerializeField] GameObject BarSerializationPrefab;
    [SerializeField] private GameObject BarMenu;
    public List<CustomBar> SerializableCustomBarList;


    [Space(2), SerializeField] GameObject SkillSerializationPrefab;
    [SerializeField] private GameObject SkillMenu;
    List<BaseSkill> SerializableSkillList => LifeParams.SkillRealizer.AvailbleBaseSkills;


    [Space(2), SerializeField] GameObject ItemSerializationPrefab;
    private GameObject ItemMenu => transform.Find("Inventory").gameObject;
    // List<Item> SerializableItemList => ;
    

    private async void OnEnable() {
        await Task.Delay(1);

        int number = 0;
        foreach(BaseSkill skill in SerializableSkillList)
        {
            GameObject obj = Instantiate(SkillSerializationPrefab, SkillMenu.transform) as GameObject;

            obj.name = number.ToString();
            obj.transform.Find("Ico").GetComponent<Image>().sprite = skill.image;
            obj.GetComponent<Button>().onClick.AddListener(() => LifeParams.SkillRealizer.SkillIndex = int.Parse(obj.name));
            number++;
        }
    }







}
