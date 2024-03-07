using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

namespace MyNamespace
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance;

        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("! There should never be two SpriteManager !");
            }
            else
            {
                Instance = this;
            }

            LoadSprites();
        }

        void LoadSprites()
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Images");
            //filePath = System.IO.Path.Combine(filePath, "CursorCircle.png");

            //LoadSprite("CursorCircle", filePath);

            LoadSpritesFromDirectory(filePath);
        }

        void LoadSpritesFromDirectory(string filePath)
        {
            //Debug.Log("LoadSpritesFromDirectory : " + filePath);

            string[] subDirs = Directory.GetDirectories(filePath);
            foreach (string sd in subDirs)
            {   
                LoadSpritesFromDirectory(sd);
            }

            string[] filesInDir = Directory.GetFiles(filePath);
            foreach (string fn in filesInDir)
            {
                string spriteCategory = new DirectoryInfo(filePath).Name;

                LoadImage(spriteCategory, fn);
            }
        }

        void LoadImage(string spriteCategory, string filePath)
        {
            //Debug.Log("Load image: " + filePath);

            if (filePath.Contains(".xml") || filePath.Contains(".meta"))
            {
                return;
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);

            Texture2D imageTexture = new Texture2D(2, 2);
            
            if(imageTexture.LoadImage(imageBytes))
            {
                string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
                string basePath = Path.GetDirectoryName(filePath);

                string xmlPath = System.IO.Path.Combine(basePath, baseSpriteName + ".xml");
                if(System.IO.File.Exists(xmlPath))
                {
                    string xmlText = System.IO.File.ReadAllText(xmlPath);

                    XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));

                    if(reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                    {
                        do
                        {
                            ReadSpriteFromXml(spriteCategory, reader, imageTexture);
                        }
                        while (reader.ReadToNextSibling("Sprite"));
                    }
                    else
                    {
                        Debug.Log("! Could not find a <Sprites> tag. !");
                        return;
                    }
                }
                else
                {
                    LoadSprite(spriteCategory, baseSpriteName, imageTexture,new Rect(0,0,imageTexture.width,imageTexture.height));
                }
            }
        }

        void ReadSpriteFromXml(string spriteCategory, XmlReader reader,Texture2D imageTexture)
        {
            //Debug.Log("ReadSpriteFromXml");
            string name = reader.GetAttribute("name");
            int x = int.Parse(reader.GetAttribute("x"));
            int y = int.Parse(reader.GetAttribute("y"));
            int w = int.Parse(reader.GetAttribute("w"));
            int h = int.Parse(reader.GetAttribute("h"));

            LoadSprite(spriteCategory, name, imageTexture, new Rect(x, y, w, h));
        }

        void LoadSprite(string spriteCategory, string spriteName, Texture2D imageTexture, Rect spriteCoordinates)
        {
            spriteName = spriteCategory + "/" + spriteName;
            //Debug.Log("LoadSprite: " + spriteName);
            Vector2 pivotPoint = new Vector2(0f,0f);   // Ranges from 0..1 -- so 0.5f == center

            Sprite s = Sprite.Create(imageTexture, spriteCoordinates, pivotPoint, 32);

            s.texture.filterMode = FilterMode.Point;

            sprites[spriteName] = s;
        }

        public Sprite GetSprite(string categoryName, string spriteName)
        {
            //Debug.Log(categoryName + "/" + spriteName);

            spriteName = categoryName + "/" + spriteName;

            if (!sprites.ContainsKey(spriteName))
            {
                //Debug.Log("! No sprite with name : " + spriteName + " !");
                return null; //Fixme: belki beyaz kutu konulabilir
            }

            return sprites[spriteName];
        }
    }
}
