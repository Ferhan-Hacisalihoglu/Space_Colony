using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace MyNamespace
{
    public class Job
    {
        public Tile targetTile;

        public List<Tile> tiles = new List<Tile>();

        public string jobObjectType {get; protected set;}

        public bool accecptsAnyInventoryItem = false;

        public bool canTakeFromStockpile = true;

        public Dictionary<string, Inventory> inventoryRequirements = new Dictionary<string, Inventory>();

        // Job Instance

        public Job(string jobObjectType,float jobTime,List<Inventory> inventoryRequirements)
        {
            this.jobObjectType = jobObjectType;
            this.jobTime = jobTime;

            if (inventoryRequirements != null)
            {
                foreach (Inventory inv in inventoryRequirements)
                {
                    this.inventoryRequirements[inv.type] = inv.Clone();
                }
            }
        } 

        protected Job(Job other)
        {
            this.jobObjectType = other.jobObjectType;
            this.jobTime = other.jobTime;

            if (other.inventoryRequirements != null)
            {
                foreach (Inventory inv in other.inventoryRequirements.Values)
                {
                    this.inventoryRequirements[inv.type] = inv.Clone();
                }
            }
        }

        public Inventory[] GetInventoryRequirementValues()
        {
            return inventoryRequirements.Values.ToArray();
        }

        virtual public Job Clone()
        {
            return new Job(this);
        }

        // Job Update

        float jobTime;

        public void DoWork(float workTime)
        {
            if (HasAllMaterial() == false)
            {
                //Debug.Log("! Tried to do work on a job that doesn't have all the material !");
                return;
            }

            jobTime -= workTime;
            
            if (jobTime <= 0)
            {
                if (cbJobCompleted != null)
                {
                    cbJobCompleted(this);
                }
            }
        }

        // Job iptal

        public void CancelJob()
        {
            if (cbJobCancel != null)
            {
                cbJobCancel(this);
            }
        }

        public bool HasAllMaterial()
        {
            foreach (Inventory inv in inventoryRequirements.Values)
            {
                if (inv.maxStackSize > inv.stackSize)
                {
                    return false;
                }
            }

            return true;
        }

        public int DesiresInventoryType(Inventory inv)
        {
            if (accecptsAnyInventoryItem)
            {
                return inv.maxStackSize;
            }

            if (!inventoryRequirements.ContainsKey(inv.type))
            {
                return 0;
            }

            if (inventoryRequirements[inv.type].stackSize >= inventoryRequirements[inv.type].maxStackSize)
            {
                return 0;
            }

            return inventoryRequirements[inv.type].maxStackSize - inventoryRequirements[inv.type].stackSize;
        }

        public Inventory GetFirstDesiredInventory()
        {
            foreach(Inventory inv in inventoryRequirements.Values)
            {
                if (inv.maxStackSize > inv.stackSize)
                {
                    return inv;
                }
            }

            return null;
        }

        #region Actions

        Action<Job> cbJobCompleted;

        public void RegisterJobCompleteCallBack(Action<Job> cb)
        {
            cbJobCompleted += cb;
        }

        public void UnregisterJobCompleteCallBack(Action<Job> cb)
        {
            cbJobCompleted -= cb;
        }

        Action<Job> cbJobCancel;

        public void RegisterJobCancelCallBack(Action<Job> cb)
        {
            cbJobCompleted += cb;
        }

        public void UnregisterJobCancelCallBack(Action<Job> cb)
        {
            cbJobCompleted -= cb;
        }
        
        #endregion
    }
}