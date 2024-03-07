using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class TileSpriteController : MonoBehaviour
    {
        private Dictionary<Tile, GameObject> tileGameObjectMap = new Dictionary<Tile, GameObject>();


        void Start()
        {
            LoadSprites();
            
            for (int x = 0; x < World.current.worldSize; x++)
            {
                for (int y = 0; y < World.current.worldSize; y++)
                {
                    Tile tile_data = World.current.GetTileAt(x, y);
                    GameObject tile_go = new GameObject();
                    tileGameObjectMap.Add(tile_data,tile_go);
                    
                    tile_go.name = "Tile_" + x + "_" + y;
                    tile_go.transform.position = new Vector2(tile_data.x, tile_data.y);
                    tile_go.transform.SetParent(this.transform,true);

                    SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                    sr.sprite = tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.GetSprite("Tile",tile_data.type.ToString());;
                    sr.sortingLayerName = "Tile";
                    OnTileChanged(tile_data);

                    tile_data.RegisterTileTypeChangedCallBack(OnTileChanged);
                }
            }
        }

        void LoadSprites()
        {
            
        }

        void OnTileChanged(Tile tile_data)
        {
            if (!tileGameObjectMap.ContainsKey(tile_data))
            {
                Debug.Log("! TileGameObject doesn't contain the tile_data !");
                return;
            }

            GameObject tile_go = tileGameObjectMap[tile_data];

            if (tile_go == null)
            {
                Debug.Log("! TileGameObject's return GameObject is null !");
                return;
            }

            tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.GetSprite("Tile",tile_data.type.ToString());
        }
    }
}