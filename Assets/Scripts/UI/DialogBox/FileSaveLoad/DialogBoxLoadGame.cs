using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class DialogBoxLoadGame : DialogBoxLoadSaveGame
    {
        public void OkeyWasClicked()
        {
            string fileName = gameObject.GetComponentInChildren<InputField>().text;

            string filePath = System.IO.Path.Combine(WorldController.Instance.FileSaveBasePath(), fileName + ".save");

            if (!File.Exists(filePath))
            {
                Debug.Log("File doesn't exist. What ?");
                CloseDialog();
                return;
            }

            CloseDialog();   

            LoadWorld(filePath);
        }

        public void LoadWorld(string filepath)
        {
            Debug.Log("SaveWorld button was clicked");

            WorldController.Instance.LoadWorld(filepath);

        }
    }
}
