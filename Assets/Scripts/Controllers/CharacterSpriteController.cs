using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class CharacterSpriteController : MonoBehaviour
    {
         private Dictionary<Character, GameObject> characterGameObjectMap = new Dictionary<Character, GameObject>();

        private void Start()
        {
            World.current.RegisterCharacterCreated(OnCharacterCreated);

            foreach (Character c in World.current.characters)
            {
                OnCharacterCreated(c);
            }
            //c.SetDestination(world.GetTileAt(world.WorldSize/2+5,world.worldSize/2));
        } 

        public void OnCharacterCreated(Character c)
        {
            //Debug.Log("OnCharacterCreated");

            // yeni GameObject olu�tur ve sahneye ekle
            GameObject char_go = new GameObject();

            characterGameObjectMap.Add(c,char_go);

            char_go.name = "Character";
            char_go.transform.position = new Vector2(c.x, c.y);
            char_go.transform.SetParent(this.transform,true);
            
            SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteManager.Instance.GetSprite("Character", "C1_Front");
            sr.sortingLayerName = "Character";

            c.RegisterOnChangedCallBack(OnCharacterChanged);
        }

        // Karakter hareket etiğinde çarılır

        void OnCharacterChanged(Character c)
        {
            if (!characterGameObjectMap.ContainsKey(c))
            {
                Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
                return;
            }

            GameObject char_go = characterGameObjectMap[c];

            char_go.transform.position = new Vector3(c.x, c.y, 0);
        }
    }
}