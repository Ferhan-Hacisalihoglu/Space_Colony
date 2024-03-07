using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.IO;

namespace MyNamespace
{
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance { get; protected set; }
        public World world { get; protected set; }
        public static string loadWorldFromFile = null;

        public bool IsModal;

        bool _isPaused = false;
        bool IsPaused
        {
            get
            {
                return _isPaused || IsModal;
            }
            set
            {
                _isPaused = value;
            }
        }

        private void Awake() 
        {
            if (Instance != null)
            {
                Debug.LogError("! There should never be two WorldGeneration !");
            }
            else
            {
                Instance = this;
            }

            if (loadWorldFromFile != null)
            {
                CreateWorldFromSaveFile();
                loadWorldFromFile = null;
            }
            else
            {
                CreateEmptyWorld();
            }
        }

        #region UpdateActions
        
        private void Update() 
        {
            if (!IsPaused)
            {
                world.Update(Time.deltaTime);
                //InputManager.Instance.UpdateMove();
            }
        }

        private void FixedUpdate() 
        {
            if (!IsPaused)
            {
                world.FixedUpdate(Time.fixedDeltaTime);
            }
        }

        #endregion

        public void NewWorld()
        {
            Debug.Log("NewWorld button was clicked");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Save klasötlerini çağırmak için kullanılır

        public string FileSaveBasePath()
        {
            string saveFolderPath = System.IO.Path.Combine(Application.persistentDataPath, "Saves");

            if (!System.IO.Directory.Exists(saveFolderPath))
            {
                System.IO.Directory.CreateDirectory(saveFolderPath);
            }

            return saveFolderPath;
        }

        public void LoadWorld(string fileName)
        {
            Debug.Log("LoadWorld button was clicked");
            loadWorldFromFile = fileName;
            Instance = null;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void CreateWorldFromSaveFile()
        {
            Debug.Log("CreateWorldFromSaveFile");
            XmlSerializer serializer = new XmlSerializer(typeof(World));

            string saveGameText = File.ReadAllText(loadWorldFromFile);
            TextReader reader = new StringReader(saveGameText);

            Debug.Log(reader.ToString());
            world = (World)serializer.Deserialize(reader);
            reader.Close();
            Camera.main.transform.position = new Vector3(world.worldSize / 2, world.worldSize / 2, -10);
        }

        void CreateEmptyWorld()
        {
            world = new World(100);
            Camera.main.transform.position = new Vector3(world.worldSize / 2, world.worldSize / 2, -10);
        }
    }
}