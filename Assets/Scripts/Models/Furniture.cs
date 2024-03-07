using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System;

namespace MyNamespace
{
    public class Furniture : IXmlSerializable
    {

        #region Instrance 

        public string type { get; protected set; }

        private string _name = null;
        public string name 
        {
            get
            {
                if (_name == null || _name.Length == 0)
                {
                    return type;
                }
                return _name;
            }
            set
            {
                _name = value;
            } 
        }
        public Tile tile;

        public int width { get; protected set; }
        public int height { get; protected set; }

        public bool linksToNeighbour { get; protected set; }

        public Furniture()
        {
            this.funcPositionValidation = this.Default__IsValidPosition;
        }

        //Eşyayı kopyalamk için kullanılır

        // Evrensel kod , yerel kodu çağırır

        virtual public Furniture Clone()
        {
            return new Furniture(this);
        }

        // Yerel kod

        protected Furniture(Furniture other)
        {
            this.type = other.type;
            this.name = other.name;
            this.width = other.width;
            this.height = other.height;
            this.linksToNeighbour = other.linksToNeighbour;
            this.movementCost = other.movementCost;
            this.furnParameters = new Dictionary<string, float>(other.furnParameters);

            // functions
            this.cbOnChanged = other.cbOnChanged;
            this.funcPositionValidation = other.funcPositionValidation;

            if(other.updateActions != null)
            {
                this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();
            }

            this.IsEnterable = other.IsEnterable;
        }

        #endregion

        // Xmlden gelen dataları oku

        public void ReadXmlPrototype(XmlReader reader_parent)
        {
            type = reader_parent.GetAttribute("type");

            XmlReader reader = reader_parent.ReadSubtree();

            Job job = null;

            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Name":
                        reader.Read();
                        _name = reader.ReadContentAsString();
                        break;
                    case "Width":
                        reader.Read();
                        width = reader.ReadContentAsInt();
                        break;
                    case "Height":
                        reader.Read();
                        height = reader.ReadContentAsInt();
                        break;
                    case "LinksToNeighbours":
                        reader.Read();
                        linksToNeighbour = reader.ReadContentAsBoolean();
                        break;
                    case "MovementCost":
                        reader.Read();
                        movementCost = reader.ReadContentAsInt();
                        break;
                    case "BuildingJob":
                        float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                        List<Inventory> invs = new List<Inventory>();

                        XmlReader invs_reader = reader.ReadSubtree();

                        while (invs_reader.Read())
                        {
                            if (invs_reader.Name == "Inventory")
                            {
                               
                                invs.Add(new Inventory(
                                invs_reader.GetAttribute("objectType"),
                                int.Parse(invs_reader.GetAttribute("amount")),
                                0
                                ));
                            }
                        }

                        job = new Job(type,jobTime,invs);
                        World.current.SetFurnitureJobPrototype(job,type);

                        break;
                    case "Params":
                        reader.Read();
                        ReadXmlParams(reader);
                        break;
                }
            }

            if (type.Contains("Door"))
            {
                IsEnterable += FurnitureActions.Door_IsEnterable;
                updateActions += FurnitureActions.Door_UpdateActions;
            }
        }

        private Func<Tile, bool> funcPositionValidation;

        // Furniturenin yerleştirildiği yeri kontrol eder

        // Evrensel kod , her yerden çağırmak için

        public bool IsValidPosition(Tile t)
        {
            return funcPositionValidation(t);
        }

        // Yerel kod sadece evrensel koda atamak için

        protected bool Default__IsValidPosition(Tile t)
        {
            for (int x_off = t.x; x_off < (t.x+width); x_off++)
            {
                for (int y_off = t.y; y_off < (t.y+height); y_off++)
                {
                    Tile t2 = World.current.GetTileAt(x_off, y_off);

                    if (t2 == null)
                    {
                        return false;
                    }

                    if (t2.type == TileType.Null)
                    {
                        return false;
                    }

                    if (t2.furniture != null)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        public bool IsStockpile()
        {
            return type == "Stockpile";
        }

        #region construct

        // Yerleştirmek için kullan

        static public Furniture PlaceInstance(Furniture proto, Tile tile)
        {
            if (!proto.funcPositionValidation(tile))
            {
                Debug.Log("! PlaceInstance --- Position Validity Function returned False !");
                return null;
            }

            Furniture obj = proto.Clone();
            obj.tile = tile;

            if (!tile.PlaceFurniture(obj))
            {
                return null;
            }

            if (obj.linksToNeighbour)
            {
                CheckNeighbourFurniture(obj, tile);
            }

            return obj;
        }

        //Eşyayı kaldırmak istenildiğinde çağrılır

        public void Deconstruct()
        {
            //Debug.Log("Deconstruct");

            tile.UnplaceFurniture();

            CheckNeighbourFurniture(null, tile);

            if (cbOnRemoved != null)
            {
                cbOnRemoved(this);
            }
        }

        #endregion

        #region CheckNeighbour

        // Komşuları kontrol et

        static void CheckNeighbourFurniture(Furniture furn, Tile tile)
        {
            foreach (Tile t in World.current.GetNeighbours(tile))
            {
                if (t.furniture != null && t.furniture.cbOnChanged != null && (furn == null || t.furniture.type == furn.type))
                {
                    t.furniture.cbOnChanged(t.furniture);
                }
            }

            /*
            Tile t;
            int x = tile.x;
            int y = tile.y;

            t = World.current.GetTileAt(x, y + 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x, y - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x + 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x - 1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x + 1, y + 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x - 1, y + 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x + 1, y - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = World.current.GetTileAt(x - 1, y - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && (obj == null || t.furniture.type == obj.type))
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            */
        }

        #endregion

        // Pathfinding

        public float movementCost { get; protected set; }

        public Func<Furniture,Enterability> IsEnterable;

        #region Params

        protected Dictionary<string, float> furnParameters = new Dictionary<string, float>();

        //Set the parameters values in furniture data
        public void ReadXmlParams(XmlReader reader)
        {
            //movementCost = int.Parse(reader.GetAttribute("MovementCost"));

            if (reader.ReadToDescendant("Param"))
            {
                do
                {
                    string k = reader.GetAttribute("name");
                    float v = float.Parse(reader.GetAttribute("value"));
                    furnParameters[k] = v;
                } while (reader.ReadToNextSibling("Param"));
            }
        }

        //Set the custom furniture parameter value
        public void SetParameter(string key, float value)
        {
            furnParameters[key] = value;
        }

        //Get the custom furniture parameter
        public float GetParameter(string key,float default_value)
        {
            if (furnParameters.ContainsKey(key) == false)
            {
                return default_value;
            }

            return furnParameters[key];
        }

        //Get the furniture parameter with key
        public float GetParameter(string key)
        {
            return GetParameter(key, 0);
        }

        #endregion
        
        #region UpdateActions

        public void FixedUpdate(float deltaTime)
        {
            if(updateActions != null) 
            {
			    updateActions(this, deltaTime);
		    }
        }

        #endregion

        #region Actions

        // Furniture sprite değiştiğinde çağrılan actionları tutar

        public Action<Furniture> cbOnChanged;

        public void RegisterOnChangedCallBack(Action<Furniture> callbackFunction)
        {
            cbOnChanged += callbackFunction;
        }
        
        public void UnregisterOnChangedCallBack(Action<Furniture> callbackFunction)
        {
            cbOnChanged -= callbackFunction;
        }

        // Furniture kaldırıldığı zaman çarılan actionları tutar

        public Action<Furniture> cbOnRemoved;

         public void RegisterOnRemoveCallBack(Action<Furniture> callbackFunction)
        {
            cbOnRemoved += callbackFunction;
        }
        
        public void UnregisterOnRemoveCallBack(Action<Furniture> callbackFunction)
        {
            cbOnRemoved -= callbackFunction;
        }

        public Action<Furniture, float> updateActions;

        #endregion
    
        #region SaveLoad

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //movementCost = int.Parse(reader.GetAttribute("MovementCost"));

            ReadXmlParams(reader);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("X",tile.x.ToString());
            writer.WriteAttributeString("Y",tile.y.ToString());
            writer.WriteAttributeString("ObjectType",type);
            writer.WriteAttributeString("MovementCost",movementCost.ToString());

            foreach (string k in furnParameters.Keys)
            {
                writer.WriteStartElement("Param");
                writer.WriteAttributeString("name",k);
                writer.WriteAttributeString("value", furnParameters[k].ToString());
                writer.WriteEndElement();
            }
        }

        #endregion
    
    }
}
