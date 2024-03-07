using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Noise;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MyNamespace
{
    public class World : IXmlSerializable
    {
        #region SetupWorld

        Tile[,] tiles;
        public List<Furniture> funitures = new List<Furniture>();
        public int worldSize;

        static public World current { get; protected set; }

        public JobQueue jobQueue = new JobQueue();
        
        public World(int worldSize)
        {
            SetupWorld(worldSize);

            CreateCharacter(GetTileAt(worldSize / 2, worldSize / 2));
        }

        public World()
        {

        }

        //Dünyayı kurarken çağırılır
        void SetupWorld(int worldSize)
        {
            current = this;
            
            this.worldSize = worldSize;

            CreateFloorPrototypes();
            CreateFurniturePrototypes();

            tiles = new Tile[worldSize, worldSize];

            for (int x = 0; x < worldSize; x++)
            {
                for (int y = 0; y < worldSize; y++)
                {
                    tiles[x, y] = new Tile(x, y);
                    tiles[x, y].RegisterTileTypeChangedCallBack(OnTileChanged);
                }
            }

            GameObject.FindGameObjectWithTag("MainCamera").transform.parent.position = new Vector3(worldSize/2,worldSize/2,-10);

            // DEBUG için!  sonra comment outla , sile lazım oluyo.
            // Item oluştur
            Inventory inv = new Inventory("Steel", 64, 64);
            Tile t = GetTileAt(worldSize / 2, worldSize / 2 + 3);
            inventoryManager.PlaceInventory(t, inv);
            if (cbInventoryCreated != null)
            {
                cbInventoryCreated(t.inventory);
            }
        }

        
        //Perlin noise rastgele kullanarak dünya oluştur
        //public void SetupRandomWorld()
        //{
        //    Debug.Log("SetupRandomWorld");
        //    float[,] noiseMap = SimpleNoise.GenerateNoiseMap(worldSize, 0, 50, 5, .5f, 2);

        //    for (int x = 0; x < worldSize; x++)
        //    {
        //        for (int y = 0; y < worldSize; y++)
        //        {
        //            if (noiseMap[x, y] < .3f)
        //            {
        //                tiles[x, y].type = TileType.Water;
        //            }
        //            else if (noiseMap[x, y] < .35f)
        //            {
        //                tiles[x, y].type = TileType.Sand;
        //            }
        //            else if (noiseMap[x, y] < .8f)
        //            {
        //                tiles[x, y].type = TileType.Grass;
        //            }
        //            else
        //            {
        //                tiles[x, y].type = TileType.Stone;

        //                if (noiseMap[x, y] > .85f)
        //                {
        //                    PlaceFurniture("furn_StoneWall",GetTileAt(x,y));
        //                }
        //            }
        //        }
        //    }
        //}

        //Yol bulma için dünya oluştur

         public void SetupPathfindingExample()
        {
            //Test için dünya oluþtur

            Debug.Log("SetupPathfindingExample");
            int a = worldSize / 2 - 7;

            for (int x = a-7; x < a+20; x++)
            {
                for (int y = a-7; y < a+20; y++)
                {
                    tiles[x, y].type = TileType.SteelGround;
                    if (x==a||x==(a+12)||y==a||y==(a+12))
                    {
                        if ( (x == a && y == (a+6)) || (x == a+12 && y == (a + 6)) || (x == a + 6 && y != (a + 6)) )
                        {
                            PlaceFurniture("furn_SteelDoor", tiles[x, y]);
                        }
                        else
                        {
                            PlaceFurniture("furn_SteelWall", tiles[x, y]);
                        }
                    }
                }
            }
        }

        #endregion

        #region Pathfinding

        // İstenilen tile'ın komşuları liste halinde çağırır

        public List<Tile> GetNeighbours(Tile tile)
        {
            List<Tile> neighbours = new List<Tile>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int xChech = tile.x+x;
                    int yChech = tile.y+y;

                    Tile neighbourTile = GetTileAt(xChech,yChech);

                    if (neighbourTile != null)
                    {
                        neighbours.Add(neighbourTile);
                    }
                }
            }

            return neighbours;
        }

        #endregion

        #region CreatePrototypes   

        //floor tiplerini kaydet
        public Dictionary<string, TileType> floorPrototypes = new Dictionary<string, TileType>();

        void CreateFloorPrototypes()
        {
            TileType[] tileTypes = (TileType[])Enum.GetValues(typeof(TileType));
            string[] tileTypesNames = Enum.GetNames(typeof(TileType));

            for (int i = 0; i < tileTypes.Length; i++)
            {
                floorPrototypes.Add(tileTypesNames[i],tileTypes[i]);
            }
        }

        //xml dosyasını Furniture Prototype'a yaz

        public Dictionary<string, Furniture> furniturePrototypes = new Dictionary<string, Furniture>();

        void CreateFurniturePrototypes()
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
            filePath = System.IO.Path.Combine(filePath, "Furniture.xml"); 
            string furnitureXmlText = System.IO.File.ReadAllText(filePath);

            XmlTextReader reader = new XmlTextReader(new StringReader(furnitureXmlText));

            int furnCount = 0;
            if (reader.ReadToDescendant("Furnitures"))
            {
                if (reader.ReadToDescendant("Furniture"))
                {
                    do
                    {
                        furnCount++;

                        Furniture furn = new Furniture();
                        furn.ReadXmlPrototype(reader);

                        furniturePrototypes[furn.type] = furn;
                    }
                    while (reader.ReadToNextSibling("Furniture"));  
                }
                else
                {
                    Debug.Log("! The furniture prototype definition file dosen't haveany 'Furniture' elements !");
                }
            }
            else
            {
                Debug.Log("! Did not find a 'Furnitures' elements in the prototype defination file !");
            }

            //Debug.Log("Furniture prototypes read. " + furnCount.ToString());
        }

        // Set job prototypes

        public Dictionary<string, Job> furnitureJobPrototypes = new Dictionary<string, Job>();

        public void SetFurnitureJobPrototype(Job j,string furnitureName)
        {
            furnitureJobPrototypes[furnitureName] = j;
        }


        #endregion

        #region Floor 

        // tile değiştiği zaman çağrılır
        void OnTileChanged(Tile tile)
        {
            if (cbTileChanged == null)
            {
                return;
            }

            cbTileChanged(tile);
        }

        // x,y konumundaki tile'ı çağır
        public Tile GetTileAt(int x, int y)
        {
            if (x >= worldSize || x < 0 || y >= worldSize || y < 0)
            {
                Debug.Log("Tile (" + x + "," + y + ")" + " is out of range");
                return null;
            }

            return tiles[x, y];
        }

        // Tile tipi değiştiği zaman çağırılır

        public void PlaceFloor(string floorType, Tile tile)
        {
            if (!floorPrototypes.ContainsKey(floorType))
            {
                Debug.Log("! FloorPrototypes dosen't contain a prototype for key : "+floorType+" !");
                return;
            }
            
            tile.type = floorPrototypes[floorType];
        }

        #endregion

        #region Furniture

        // Furniturenin yerleştirildiği yeri kontrol eder

        public bool IsFurniturePlacementValid(string furnitureType, Tile t)
        {
            return furniturePrototypes[furnitureType].IsValidPosition(t);
        }

        // Furniture yerleştirildiği zaman çağırılır

        public Furniture PlaceFurniture(string type, Tile t)
        {
            if (!furniturePrototypes.ContainsKey(type))
            {
                Debug.Log("! InstalledObjectPrototypes dosen't contain a prototype for key : "+type+" !");
                return null;
            }

            Furniture furn = Furniture.PlaceInstance(furniturePrototypes[type], t);

            if (furn == null)
            {
                return null; 
            }

            furn.RegisterOnRemoveCallBack(OnFurnitureRemoved);
            funitures.Add(furn);

            if (cbFurnitureCreated != null)
            {
                cbFurnitureCreated(furn);
            }

            return furn;
        }

        // Furniture'yi yok etmek için yok etme function'u çağır

        public void OnFurnitureRemoved(Furniture furn)
        {
            funitures.Remove(furn);
        }

        // Furniture prototype çağır

        public Furniture GetFurniturePrototype(string objectType)
        {
            if (furniturePrototypes.ContainsKey(objectType) == false)
            {
                Debug.Log("! No Furniture with type :"+objectType+" !");
                return null;
            }

            return furniturePrototypes[objectType];
        }

        #endregion

        #region Character
        public List<Character> characters  = new List<Character>();

        public Character CreateCharacter(Tile t)
        {
            Character c = new Character(t);
            characters.Add(c);
            
            if (cbCharacterCreated != null)
            {
                cbCharacterCreated(c);
            }

            return c;
        }

        #endregion
        
        #region Inventory

        public InventoryManager inventoryManager = new InventoryManager();

        #endregion

        #region UpdateActions

        public void Update(float deltaTime) 
        {
            foreach (Character c in characters)
            {
                c.Update(deltaTime);
            }
        }

        public void FixedUpdate(float deltaTime) 
        {
            foreach (Character c in characters)
            {
                c.FixedUpdate(deltaTime);
            }

            foreach (Furniture f in funitures)
            {
                f.FixedUpdate(deltaTime);
            }
        }

        #endregion

        #region Actions
        
        // Tile changed event

        private Action<Tile> cbTileChanged;

        public void RegisterTileChanged(Action<Tile> callbackfunc)
        {
            cbTileChanged += callbackfunc;
        }

        public void UnregisterTileChanged(Action<Tile> callbackfunc)
        {
            cbTileChanged -= callbackfunc;
        }

        // Furniture oluşturma event

        private Action<Furniture> cbFurnitureCreated;

        public void RegisterFurnitureCreated(Action<Furniture> callbackfunc)
        {
            cbFurnitureCreated += callbackfunc;
        }

        public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc)
        {
            cbFurnitureCreated -= callbackfunc;
        }

        // Karakter oluşturma event

        private Action<Character> cbCharacterCreated;

        public void RegisterCharacterCreated(Action<Character> callBackfunc)
        {
            cbCharacterCreated += callBackfunc;
        }
        
        public void UnregisterCharacterCreated(Action<Character> callBackfunc)
        {
            cbCharacterCreated -= callBackfunc;
        }

        // Inventory oluşturma sistemini yönetir

        private Action<Inventory> cbInventoryCreated;

        public void RegisterInventoryCreated(Action<Inventory> callBackfunc)
        {
            cbInventoryCreated += callBackfunc;
        }

        public void UnregisterInventoryCreated(Action<Inventory> callBackfunc)
        {
            cbInventoryCreated -= callBackfunc;
        }

        public void OnInventoryCreated(Inventory inv)
        {
            if (cbInventoryCreated != null)
            {
                cbInventoryCreated(inv);
            }
        }

        #endregion

        #region SaveLoad

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //Save All

            Debug.Log("World::ReadXml");
            worldSize = int.Parse(reader.GetAttribute("WorldSize"));
            SetupWorld(worldSize);

            while (reader.Read())
            {
                switch (reader.Name)
                {
                    case "Tiles":
                        ReadXml_Tiles(reader);
                        break;
                    case "Furnitures":
                        ReadXml_Furnitures(reader);
                        break;
                    case "Characters":
                        ReadXml_Characters(reader);
                        break;
                }
            }
        }

        void ReadXml_Furnitures(XmlReader reader)
        {
            //Save Furnitures

            Debug.Log("ReadXml_Furnitures");

            if (reader.ReadToDescendant("Furniture"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));

                    Furniture furn = PlaceFurniture(reader.GetAttribute("ObjectType"), tiles[x, y]);
                    furn.ReadXml(reader);
                } 
                while (reader.ReadToNextSibling("Furniture"));

            }
        }

        void ReadXml_Tiles(XmlReader reader)
        {
            //Save Tiles

            Debug.Log("ReadXml_Tiles");

            if (reader.ReadToDescendant("Tile"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    tiles[x, y].ReadXml(reader);

                }
                while (reader.ReadToNextSibling("Tile"));
            }
        }

        void ReadXml_Characters(XmlReader reader)
        {
            //Save Characters

            Debug.Log("ReadXml_Characters");

            if (reader.ReadToDescendant("Character"))
            {
                do
                {
                    int x = int.Parse(reader.GetAttribute("X"));
                    int y = int.Parse(reader.GetAttribute("Y"));
                    Character c = CreateCharacter(tiles[x, y]);
                    c.ReadXml(reader);
                }
                while (reader.ReadToNextSibling("Character")) ;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            //Load All

            writer.WriteAttributeString("WorldSize", worldSize.ToString());

            writer.WriteStartElement("Tiles");
            for (int x = 0; x < worldSize; x++)
            {
                for (int y = 0; y < worldSize; y++)
                {
                    if (tiles[x,y].type != TileType.Null)
                    {
                        writer.WriteStartElement("Tile");
                        tiles[x, y].WriteXml(writer);
                        writer.WriteEndElement();
                    }
                }
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Furnitures");
            foreach (Furniture furn in funitures)
            {
                writer.WriteStartElement("Furniture");
                furn.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Characters");
            foreach (Character c in characters)
            {
                writer.WriteStartElement("Character");
                c.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}
