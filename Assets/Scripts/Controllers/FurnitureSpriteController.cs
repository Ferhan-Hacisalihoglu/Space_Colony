using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class FurnitureSpriteController : MonoBehaviour
    {
        private Dictionary<Furniture, GameObject> furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        private void Start() 
        {
            World.current.RegisterFurnitureCreated(OnFurnitureCreated);

            foreach (Furniture furn in World.current.funitures)
            {
                OnFurnitureCreated(furn);
            }
        }

        public void OnFurnitureCreated(Furniture furn)
        {
            GameObject furn_go = new GameObject();

            furnitureGameObjectMap.Add(furn,furn_go);

            furn_go.name = furn.type + "_" + furn.tile.x + "_" + furn.tile.y;
            furn_go.transform.position = new Vector2(furn.tile.x,furn.tile.y);
            furn_go.transform.SetParent(this.transform, true);


            //Furniturte'a siprite ata ve ayarlarını yap
            SpriteRenderer sr =  furn_go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForFurniture(furn);
            sr.sortingLayerName = "Furniture";

            OnDoorChanged(furn,furn_go);

            //Furniturte değişime function'u ata
            furn.RegisterOnChangedCallBack(OnFurnitureChanged);
            //Furniture yok eden function'u ata
            furn.RegisterOnRemoveCallBack(OnFurnitureRemoved);
        }

        // Furniture değiştiği zaman çağrılır

        void OnFurnitureChanged(Furniture furn)
        {
            if (!furnitureGameObjectMap.ContainsKey(furn))
            {
                Debug.Log("! OnFurnitureChanged --- trying to change visual for furniture not in our map !");
                return;
            }

            GameObject furn_go = furnitureGameObjectMap[furn];
            furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

            OnDoorChanged(furn,furn_go);
        }

        void OnDoorChanged(Furniture furn,GameObject furn_go)
        {
            if (furn.type.Contains("furn_SteelDoor"))
            {
                Tile nt = World.current.GetTileAt(furn.tile.x, furn.tile.y + 1);
                Tile st = World.current.GetTileAt(furn.tile.x, furn.tile.y - 1);

                if ((nt != null && nt.furniture != null) || (st != null && st.furniture != null))
                {
                    furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
                    //Saçma hata düzeltme , sprite'nin merkezi Button_Left olmasından kaynaklı 
                    furn_go.transform.position = new Vector2(furn.tile.x + 1, furn.tile.y);
                }
            }
        }

        // Furniture yok edildiği zaman çağırılır

        void OnFurnitureRemoved(Furniture furn)
        {
            if (!furnitureGameObjectMap.ContainsKey(furn))
            {
                Debug.Log("! OnFurnitureChanged --- trying to change visual for furniture not in our map !");
                return;
            }

            GameObject furn_go = furnitureGameObjectMap[furn];
            Destroy(furn_go);
            furnitureGameObjectMap.Remove(furn);
        }


        // Furniture'un tipine göre sprite ata

        public Sprite GetSpriteForFurniture(Furniture furn)
        {
            string spriteName = furn.type;

            if (furn.linksToNeighbour == false)
            {
                if (furn.type.Contains("furn_SteelDoor"))
                {
                    if (furn.GetParameter("openness") < 0.1f)
                    {
                        spriteName += "_";
                    }
                    else if (furn.GetParameter("openness") < 0.26f)
                    {
                        spriteName += "_openness_1";
                    }
                    else if (furn.GetParameter("openness") < 0.42f)
                    {
                        spriteName += "_openness_2";
                    }
                    else if (furn.GetParameter("openness") < 0.58f)
                    {
                        spriteName += "_openness_3";
                    }
                    else if (furn.GetParameter("openness") < 0.74f)
                    {
                        spriteName += "_openness_4";
                    }
                    else if (furn.GetParameter("openness") < 0.9f)
                    {
                        spriteName += "_openness_5";
                    }
                    else
                    {
                        spriteName += "_openness_6";
                    }
                    //Debug.Log(spriteName);
                }

                //Debug.Log("No link to neighbour ?");
                return SpriteManager.Instance.GetSprite("Furniture", spriteName); //furnitureSprites[spriteName];
            }

            spriteName += "_";
            int x = furn.tile.x;
            int y = furn.tile.y;
            Tile t1;

            t1 = World.current.GetTileAt(x, y + 1);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type)
            {
                spriteName += "N";
            }
            
            t1 = World.current.GetTileAt(x, y - 1);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type)
            {
                spriteName += "S";
            }
            
            t1 = World.current.GetTileAt(x+1, y);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type)
            {
                spriteName += "E";
            }
            
            t1 = World.current.GetTileAt(x-1, y);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type)
            {
                spriteName += "W";
            }
            
            Tile t2;
            Tile t3;
            
            t1 = World.current.GetTileAt(x+1, y + 1);
            t2 = World.current.GetTileAt(x + 1,y);
            t3 = World.current.GetTileAt(x, y + 1);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type && 
                t2 != null && t2.furniture!=null && t2.furniture.type == furn.type && 
                t3 != null && t3.furniture!=null && t3.furniture.type == furn.type)
            {
                spriteName += "NE";
            }
            
            t1 = World.current.GetTileAt(x-1, y + 1);
            t2 = World.current.GetTileAt(x - 1,y);
            t3 = World.current.GetTileAt(x, y + 1);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type && 
                t2 != null && t2.furniture!=null && t2.furniture.type == furn.type &&
                t3 != null && t3.furniture!=null && t3.furniture.type == furn.type)
            {
                spriteName += "NW";
            }
            
            t1 = World.current.GetTileAt(x+1, y - 1);
            t2 = World.current.GetTileAt(x + 1,y);
            t3 = World.current.GetTileAt(x, y - 1);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type && 
                 t2 != null && t2.furniture!=null  && t2.furniture.type == furn.type  && 
                 t3 != null && t3.furniture!=null && t3.furniture.type == furn.type)
            {
                spriteName += "SE";
            }
            
            t1 = World.current.GetTileAt(x-1, y - 1);
            t2 = World.current.GetTileAt(x - 1,y);
            t3 = World.current.GetTileAt(x, y - 1);
            if (t1 != null && t1.furniture != null && t1.furniture.type == furn.type &&
            t2 != null && t2.furniture!=null && t2.furniture.type == furn.type  && 
            t3 != null && t3.furniture!=null  && t3.furniture.type == furn.type)
            {
                spriteName += "SW";
            }

            return SpriteManager.Instance.GetSprite("Furniture",spriteName);
        }

        public Sprite GetSpriteForFurniture(string objectType)
        {
            Sprite sprite = SpriteManager.Instance.GetSprite("Furniture",objectType);

            if (sprite == null)
            {
                sprite = SpriteManager.Instance.GetSprite("Furniture",objectType + "_");
            }
            
            return sprite;
        }
    }
}
