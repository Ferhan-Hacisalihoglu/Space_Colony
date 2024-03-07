using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MyNamespace
{
    public class Inventory 
    {

        #region Variable

        public string type = "Stone";

        public int maxStackSize = 64;

        protected int _stackSize = 1;

        public int stackSize
        {
            get { return _stackSize; }
            set
            {
                if (_stackSize != value)
                {
                    _stackSize = value;
                    if (cbInventoryChanged != null)
                    {
                        cbInventoryChanged(this);
                    }
                }
            }
        }

        public Tile tile;
        public Character character;

        #endregion

        #region Instance

        public Inventory()
        {

        }

        public Inventory(string type, int maxStackSize,int stackSize = 0)
        {
            this.type = type;   
            this.maxStackSize = maxStackSize;
            this.stackSize = stackSize;
        }

        protected Inventory(Inventory other)
        {
            type = other.type;
            maxStackSize = other.maxStackSize;
            stackSize = other.stackSize;
        }

        public virtual Inventory Clone()
        {
            return new Inventory(this);
        }

        #endregion

        #region Actions

        Action<Inventory> cbInventoryChanged;

        public void RegisterInventoryChangedCallBack(Action<Inventory> cb)
        {
            cbInventoryChanged += cb;   
        }

        public void UnregisterInventoryChangedCallBack(Action<Inventory> cb)
        {
            cbInventoryChanged -= cb;
        }

        #endregion
    }
}
