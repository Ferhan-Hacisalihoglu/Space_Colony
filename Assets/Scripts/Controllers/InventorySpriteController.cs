using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class InventorySpriteController : MonoBehaviour
    {
        public GameObject inventoryUIPrefabs;

        private Dictionary<Inventory, GameObject> inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        private void Start()
        {
            World.current.RegisterInventoryCreated(OnInventoryCreated);

            foreach (string objectType in World.current.inventoryManager.inventories.Keys)
            {
                foreach (Inventory inv in World.current.inventoryManager.inventories[objectType])
                {
                    OnInventoryCreated(inv);
                }
            }
            //c.SetDestination(world.GetTileAt(world.WorldSize/2+5,world.worldSize/2));
        }

        public void OnInventoryCreated(Inventory inv)
        {
            Debug.Log("OnInventoryCreated");
            GameObject inv_go = new GameObject();
            inventoryGameObjectMap.Add(inv,inv_go);
            inv_go.name = inv.type;
            inv_go.transform.position = new Vector2(inv.tile.x, inv.tile.y);
            inv_go.transform.SetParent(this.transform,true);
            
            SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteManager.Instance.GetSprite("Inventory", inv.type);

            if (sr.sprite == null)
            {
                Debug.Log("! No sprite for : " + inv.type + " !");
                return;
            }
            sr.sortingLayerName = "Inventory";

            if (inv.maxStackSize > 1)
            {
                GameObject ui_go = Instantiate(inventoryUIPrefabs);
                ui_go.transform.SetParent(inv_go.transform);
                ui_go.transform.localPosition = Vector2.zero;
                ui_go.GetComponentInChildren<Text>().text = inv.stackSize.ToString();
            }

            inv.RegisterInventoryChangedCallBack(OnCharacterChanged);
        }

        public void OnCharacterChanged(Inventory inv)
        {
            if (!inventoryGameObjectMap.ContainsKey(inv))
            {
                Debug.Log("OnCharacterChanged --- trying to change visual for inventory not in our map.");
                return;
            }

            GameObject inv_go = inventoryGameObjectMap[inv];

            if (inv.stackSize>0)
            {
                Text text = inv_go.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = inv.stackSize.ToString();
                }
            }
            else
            {
                Destroy(inv_go);
                inventoryGameObjectMap.Remove(inv);
                inv.UnregisterInventoryChangedCallBack(OnCharacterChanged);
            }

            
        }
    }
}
