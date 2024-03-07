using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace MyNamespace
{
    public class InventoryManager
    {
        // Bütün inventories içinde tutar
        public Dictionary<string, List<Inventory>> inventories = new Dictionary<string, List<Inventory>>();

        public InventoryManager()
        {
            
        }

        void CleanInventory(Inventory inv)
        {
            if (inv.stackSize == 0)
            {
                if (inventories.ContainsKey(inv.type))
                {
                    inventories[inv.type].Remove(inv);
                }

                if (inv.tile != null)
                {
                    inv.tile.inventory = null;
                    inv.tile = null;
                }

                if (inv.character != null)
                {
                    inv.character.inventory = null;
                    inv.character = null;
                }
            }
        }

        public bool PlaceInventory(Tile tile, Inventory inv)
        {
            bool tileWasEmpty = tile.inventory == null;

            if (!tile.PlaceInventory(inv))
            {
                return false;
            }

            CleanInventory(inv);

            if (tileWasEmpty)
            {
                if (!inventories.ContainsKey(tile.inventory.type))
                {
                    inventories[tile.inventory.type] = new List<Inventory>();
                }

                inventories[tile.inventory.type].Add(tile.inventory);

                World.current.OnInventoryCreated(tile.inventory);
            }

            return true;
        }

        public bool PlaceInventory(Job job, Inventory inv)
        {
            if(!job.inventoryRequirements.ContainsKey(inv.type))
            {
                Debug.Log("! Trying to add inventory to a job that it doesn't want. !");
                return false;
            }

            job.inventoryRequirements[inv.type].stackSize += inv.stackSize;

            if (job.inventoryRequirements[inv.type].maxStackSize < job.inventoryRequirements[inv.type].stackSize)
            {
                inv.stackSize = job.inventoryRequirements[inv.type].stackSize - job.inventoryRequirements[inv.type].maxStackSize;
                job.inventoryRequirements[inv.type].stackSize = job.inventoryRequirements[inv.type].maxStackSize;
            }
            else
            {
                inv.stackSize = 0;
            }

            CleanInventory(inv);

            return true;
        }

        public bool PlaceInventory(Character character, Inventory sourceInventory,int amount = -1)
        {
            if (amount < 0)
            {
                amount = sourceInventory.stackSize;
            }
            else
            {
                amount = Mathf.Min(amount, sourceInventory.stackSize);
            }

            if (character.inventory == null)
            {
                character.inventory = sourceInventory.Clone();
                character.inventory.stackSize = 0;  
                inventories[character.inventory.type].Add(character.inventory);
            }
            else if (character.inventory.type != sourceInventory.type)
            {
                Debug.Log("! Character is trying to pick up a mismatched inventory object type !");
                return false;
            }

            character.inventory.stackSize += amount;

            if (character.inventory.maxStackSize < character.inventory.stackSize)
            {
                sourceInventory.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
                character.inventory.stackSize = character.inventory.maxStackSize;
            }
            else
            {
                sourceInventory.stackSize -= amount;
            }

            CleanInventory(sourceInventory);

            return true;
        }

        public void GetClosestInventoryOfType(string objectType, Character character, int desiredAmount ,bool canTakeFromStockpile)
        {
            if (!inventories.ContainsKey(objectType))
            {
                Debug.Log("! GetClosestInventoryOfType --- no items of desired type !");
                return;
            } 
            List<Inventory> inventoryList = inventories[objectType];

            float bestDistance = -1;
            Inventory bestInventory = null;

            foreach (Inventory inventory in inventoryList)
            {
                //Uzaklığa göre git
                //canTakeFromStockpile bunuda sonra dikate al

                if (inventory.tile != null)
                {
                    if (Vector2.Distance(new Vector2(inventory.tile.x,inventory.tile.y),new Vector2(character.x,character.y)) < bestDistance || bestDistance == -1)
                    {
                        bestDistance = Vector2.Distance(new Vector2(inventory.tile.x,inventory.tile.y),new Vector2(character.x,character.y));
                        bestInventory = inventory;
                    }
                }
            }

            if (bestInventory != null)
            {
                character.destTile = bestInventory.tile;
                PathfinderMaster.Instance.RequestPathfind(character.currentTile,character.destTile,character);
            }
            else
            {
                character.ChechPath();
            }
            
        }

        /*

        public Inventory GetClosestInventoryOfType(string objectType, Tile t, int desiredAmount,bool canTakeFromStockpile)
        {
            Queue<Tile> path = GetPathToClosedInventoryOfType(objectType, t, desiredAmount, canTakeFromStockpile);

            return path.Peek().inventory;
        }

        public Queue<Tile> GetPathToClosedInventoryOfType(string objectType,Tile t,int desiredAmount,bool canTakeFromStockpile)
        {
            if (!inventories.ContainsKey(objectType))
            {
                Debug.Log("! GetClosestInventoryOfType --- no items of desired type !");
                return null;
            }

            Queue<Tile> path = new PathAStar(World.current, t, null, objectType);

            return path;
        }
        */
    }
}
