using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class FurnitureBuildMenu : MonoBehaviour
    {
        public GameObject buildFloorButtonPrefab;

        private void Start()
        {
            BuildModeController bmc = GameObject.FindAnyObjectByType<BuildModeController>();

            foreach(string s in World.current.furniturePrototypes.Keys)
            {
                //Debug.Log(s);

                GameObject go = (GameObject)Instantiate(buildFloorButtonPrefab);
                go.transform.SetParent(this.transform);

                string objectName = World.current.furniturePrototypes[s].name;

                go.name = "Button - Build - " + s;
                go.transform.GetComponentInChildren<Text>().text = "Build " + objectName;

                Button b = go.GetComponent<Button>();

                string objectId = s;
                b.onClick.AddListener(delegate { bmc.SetMode_BuildFurniture(objectId); });
                //print(objectId);
            }

            gameObject.GetComponent<AutomaicVerticalSize>().AdJustSize();
        }
    }
}
