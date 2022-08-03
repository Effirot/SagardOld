using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System;
using System.Linq;
using SagardCL;
using SagardCL.ParameterManipulate;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private TMP_InputField _textField;
    [SerializeField] private TMP_InputField _InputField;
    [Space]
    [SerializeField] private Transform _Figures;
    [SerializeField] private List<Transform> _FiguresList;
    [Space]
    [SerializeField] private List<string> _commands = new List<string> { "-destroy", "-corpse", "-setMovePlan", "-setAttackPlan", "-teleportTo", "-showAllParameters" };
    [SerializeField] private List<string> _allNames;
    [Space]
    [SerializeField] private Splitt splitt;
    
    private void Start()
    {
        _Figures = GameObject.Find("Figures").transform;
        GetFigures();
    }
    private void GetFigures()
    {
        int gcc = _Figures.childCount;
        for(int i = 0; i < gcc; i++)
        {
            _FiguresList.Add(_Figures.GetChild(i));
            _allNames.Add(_Figures.GetChild(i).name);
        }
    }
    public void AddText(string text)
    {
        splitt = SplittingString(text);

        string commandAndName = splitt.commandAndNameBoll ? $" name : {splitt.name} \n command : {splitt.command} \n" : "";
        string skillNumber = splitt.skillNumberBoll ? $" skillNumber : {splitt.skillNumber} \n" : "";        
        string coord = splitt.coordBoll ? $" coord : {splitt.coord} \n" : "";
        string coordEnd = splitt.coordEndBoll ? $" coordEnd :{splitt.coordEnd} \n " : "" ;

        splitt.commandAndNameBoll = false;
        splitt.skillNumberBoll = false;
        splitt.coordBoll = false;
        splitt.coordEndBoll = false;

        if (splitt.name != "No name")
            CallFunction();

        _textField.text += commandAndName + skillNumber + coord  + coordEnd;
        _textField.verticalScrollbar.value = 1;
    }
    private async void CallFunction()
    {

       CharacterCore core = _FiguresList.Where(p => p.name == splitt.name).FirstOrDefault().Find($"Controller({splitt.name})").GetComponent<CharacterCore>();
        switch (splitt.command)
        {
            case ("-destroy"):
                Destroy(_FiguresList.Where(p => p.name == splitt.name).FirstOrDefault().gameObject);
                break;
            case ("-corpse"):
                core.Corpse = !core.Corpse;
                break;
            case ("-setMovePlan"):
                await core.MovePlannerSet(new SagardCL.Checkers(splitt.coord));
                await core.AttackPlannerSet(new SagardCL.Checkers(splitt.coord));
                break;
            case ("-setAttackPlan"):
                core.SkillIndex = splitt.skillNumber;
                await core.AttackPlannerSet(new SagardCL.Checkers(splitt.coord));
                break;
            case ("-teleportTo"):
                core.transform.position = new SagardCL.Checkers(splitt.coord);
                await core.AttackPlannerSet(new SagardCL.Checkers(splitt.coord));
                break;
            case ("-showAllParameters"):
                break;
        }
    }
    private Splitt SplittingString(string text)
    {
        text = text.Trim();
        //List<string> words = text.Split(new char[] { ' ' }).ToList();
        string[] words = text.Split(new char[] { ' ' });
        splitt = new Splitt();

        switch (words.Length)
        {
            case 2:
                string command = String.IsNullOrEmpty(words[0]) == false ? words[0] : splitt.command;
                command= _commands.Where(a => a == command).ToList().FirstOrDefault();
                splitt.command = command == null ? splitt.command : command;

                string name = String.IsNullOrEmpty(words[1]) == false ? words[1] : splitt.name;
                name = _allNames.Where(a => a == name).ToList().FirstOrDefault();
                splitt.name = name == null ? splitt.name : name;
                splitt.commandAndNameBoll = true;
                return splitt;
            case 3:
                FillCoordsAndSkillNumber(2, false);
                splitt.skillNumberBoll = true;
                goto case 2;
            case 4:
                FillCoordsAndSkillNumber(3 ,false);
                splitt.coordBoll = true;
                goto case 3;
            case 5:
                FillCoordsAndSkillNumber(4, true);
                splitt.coordEndBoll = true;
                goto case 4;
            default:
                return splitt;

        }
        void FillCoordsAndSkillNumber(int i, bool end)
        {
            if (words[i].Contains(":"))
            {
                bool x = float.TryParse(words[i].Split(new char[] { ':' })[0], out float xCoord);
                bool z = float.TryParse(words[i].Split(new char[] { ':' })[1], out float zCoord);
                splitt.coord.x = xCoord;
                splitt.coord.y = zCoord;
                if (end)
                {
                    splitt.coordEnd.x = xCoord;
                    splitt.coordEnd.y = zCoord;
                }
            }
            else
            {
                bool skill = int.TryParse(words[i], out int skillNumber);
                splitt.skillNumber = skillNumber;
            }
        }

    }
    [Serializable]
    private class Splitt
    {
        public string command = "No command";
        public string name = "No name";
        public bool commandAndNameBoll = false;
        [Space]
        public int skillNumber = 1;
        public bool skillNumberBoll = false;
        [Space]
        public Vector2 coord = Vector2.zero;
        public bool coordBoll = false;
        [Space]
        public Vector2 coordEnd = Vector2.zero;
        public bool coordEndBoll = false;
    }

}
