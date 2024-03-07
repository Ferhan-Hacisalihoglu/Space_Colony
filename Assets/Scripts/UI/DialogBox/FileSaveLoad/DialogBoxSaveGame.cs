using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class DialogBoxSaveGame : DialogBoxLoadSaveGame
    {
        public void OkeyWasClicked()
        {
            string fileName = gameObject.GetComponentInChildren<InputField>().text;

            string filePath = System.IO.Path.Combine(WorldController.Instance.FileSaveBasePath(), fileName + ".save");

            if (File.Exists(fileName))
            {
                Debug.Log("File already exists --- overwriting the file for now.");
            }

            CloseDialog();   

            SaveWorld(filePath);
        }

        public void SaveWorld(string filepath)
        {
            Debug.Log("LoadWorld button was clicked");

            XmlSerializer serializer = new XmlSerializer(typeof(World));
            TextWriter writer = new StringWriter();
            serializer.Serialize(writer, WorldController.Instance.world);
            writer.Close();

            Debug.Log(writer.ToString());

            if (Directory.Exists(WorldController.Instance.FileSaveBasePath()) == false)
            {
                Directory.CreateDirectory(WorldController.Instance.FileSaveBasePath());
            }

            File.WriteAllText(filepath, writer.ToString());

            //PlayerPrefs.SetString("SaveGame00",writer.ToString());

        }
    }
}
