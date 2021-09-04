//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class UI_MultipleObjectives : MonoBehaviour
//{
//    Text ObjectiveText;
//    public string[] task;
//    public GameObject[] CrossObject;
//    string testing = "here";
//    RectTransform rectTransform;

//    // Start is called before the first frame update
//    void Start()
//    {
//        ObjectiveText =  GetComponent<Text>();
//        rectTransform = ObjectiveText.GetComponent<RectTransform>();
//        TaskUpdate();
//        CrossTask(1);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //TaskUpdate();
//    }
//    public void TaskUpdate()
//    {
//        ObjectiveText.text = "";
//        rectTransform.sizeDelta = new Vector2(600, 60 * task.Length);
//        foreach (string item in task)
//        {
//            ObjectiveText.text += item;
//            ObjectiveText.text += "\n";
//            print(item);
//        }
//    }
//    public void CrossTask(int elementNum)
//    {
//        CrossObject[elementNum].SetActive(true);
//    }
//}
