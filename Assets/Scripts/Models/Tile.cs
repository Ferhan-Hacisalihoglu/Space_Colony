using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MyNamespace
{
    public enum TileType
    {
        Null,
        SteelGround,
    }

    public enum Enterability
    {
        Yes,
        Never,
        Soon,
    }

    public class Tile : IXmlSerializable
    {
        private TileType _type = TileType.Null;

        public TileType type
        {
            get{ return _type; }
            set
            {
                TileType oldType = _type;
                _type = value;
                if (cbTileTypeChanged != null && oldType != _type)
                {
                    cbTileTypeChanged(this);
                }
            }
        }

        public int x { get; protected set; }
        public int y { get; protected set; }

        public Furniture furniture { get; protected set; }

        public Job pendingFurnitureJob;

        public List<Character> characters = new List<Character>();

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        #region Pathfinding

        public Tile parent;

        public float hCost;
        public float gCost;

        public float fCost
        {
            get 
            { 
                return gCost + hCost; 
            }
        }

        public float movementCost
        {
            get
            {
                if (type == TileType.Null)
                {
                    return 0;
                }

                if (furniture ==  null)
                {
                    return 1;
                }

                return furniture.movementCost;
            }
        }

        public Enterability IsEnterable()
        {
            if (movementCost == 0)
            {
                return Enterability.Never;
            }

            if (furniture != null && furniture.IsEnterable != null)
            {
                return furniture.IsEnterable(furniture);
            }

            return Enterability.Yes;
        }

        #endregion

        #region Furniture

        // Furniture yerleştirildiğinde çağırılır

        public bool PlaceFurniture(Furniture furn)
        {
            if (furn == null)
            {
                return UnplaceFurniture();
            }

            if (!furn.IsValidPosition(this))
            {
                Debug.Log("! Trying to assign an furniture to a tile that isn't valid !");
                return false;
            }

            for (int x_off = x; x_off < x+furn.width; x_off++)
            {
                for (int y_off = y; y_off < y + furn.height; y_off++)
                {
                    Tile t = World.current.GetTileAt(x_off, y_off);
                    t.furniture = furn;
                }
            }

            return true;
        }

        // Furniture kaldırıldığında çağırılır

        public bool UnplaceFurniture()
        {
            if (furniture == null)
            {
                return false;
            }

            Furniture f = furniture;

            for (int x_off = x;  x_off < x+f.width;  x_off++)
            {
                for (int y_off = y; y_off < y + f.height; y_off++)
                {
                    Tile t = World.current.GetTileAt(x_off, y_off);
                    t.furniture = null;
                }
            }

            furniture = null;
            return true;
        }

        #endregion

        #region SaveLoad

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            type = (TileType)int.Parse(reader.GetAttribute("Type"));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("X",x.ToString());
            writer.WriteAttributeString("Y",y.ToString());
            writer.WriteAttributeString("Type", ((int)type).ToString());
        }

        #endregion

        #region Actions

        // Tile tipi değiştiğinde çağrılan funtionları tutar

        Action<Tile> cbTileTypeChanged;

        public void RegisterTileTypeChangedCallBack(Action<Tile> callBack)
        {
            cbTileTypeChanged += callBack;
        }
        
        public void UnRegisterTileTypeChangedCallBack(Action<Tile> callBack)
        {
            cbTileTypeChanged -= callBack;
        }

        #endregion

        #region Inventory

        public Inventory inventory;

        public bool PlaceInventory(Inventory inv)
        {
            if (inv == null)
            {
                furniture = null;
                return true;
            }

            if (inventory != null)
            {
                // Stack sistemi 
                
                if (inventory.type != inv.type)
                {
                    Debug.Log("! Trying to assign inventory to a tile that already has some a different type !");
                    return false;
                }

                int numToMove = inv.stackSize;

                if (inventory.stackSize + numToMove > inventory.maxStackSize)
                {
                    Debug.Log("! Trying to assign inventory to a tile that would exceed max stack size !");
                    numToMove = inventory.maxStackSize - inventory.stackSize;
                }

                inventory.stackSize += numToMove;
                inv.stackSize -= numToMove;

                return true;
            }

            inventory = inv.Clone();
            inventory.tile = this;
            inv.stackSize = 0;

            return true;
        }

        #endregion

    }
}

