using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class FloorBuildMenu : MonoBehaviour
    {
        public GameObject buildFloorButtonPrefab;

        private void Start()
        {
            BuildModeController bmc = GameObject.FindAnyObjectByType<BuildModeController>();

            foreach(string s in World.current.floorPrototypes.Keys)
            {
                GameObject go = (GameObject)Instantiate(buildFloorButtonPrefab);
                go.transform.SetParent(this.transform);

                string objectName = s;

                go.name = "Button - Build - " + s;
                go.transform.GetComponentInChildren<Text>().text = "Build " + objectName;

                Button b = go.GetComponent<Button>();

                string objectId = s;
                b.onClick.AddListener(delegate { bmc.SetMode_BuildFloor(objectId); });
                //Debug.Log(objectId);
            }

            gameObject.GetComponent<AutomaicVerticalSize>().AdJustSize();
        }
    }
}
