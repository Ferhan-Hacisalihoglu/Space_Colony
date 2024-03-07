using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public enum BuildMode
    {
        Floor,
        Furniture,
        Deconstruct
    }

    public class BuildModeController : MonoBehaviour
    {
        // İnşa edilcek zemin tipini tutar
        private string buildModeFloorType = "Ground";
        // İnşa edilcek eşya tipini tutar
        public string buildModeFurnitureType = "Wall";

        // İnşat modu
        public BuildMode currentMode = BuildMode.Floor;

        // İnşat görevi burdan , World classına gönderilir
        public void DoBuild(Tile tile)
        {
            if (currentMode == BuildMode.Floor)
            {
                World.current.PlaceFloor(buildModeFloorType, tile);
            }
            else if (currentMode == BuildMode.Furniture)
            {
                string furnitureType = buildModeFurnitureType;

                if (World.current.IsFurniturePlacementValid(buildModeFurnitureType, tile) && tile.pendingFurnitureJob == null)
                {
                    //World.current.PlaceFurniture(buildModeFurnitureType,tile);

                    Job job = World.current.furnitureJobPrototypes[furnitureType].Clone();

                    job.targetTile = tile;

                    for (int x = 0; x < tile.x + World.current.furniturePrototypes[furnitureType].width; x++)
                    {
                        for (int y = 0; y < tile.y + World.current.furniturePrototypes[furnitureType].height; y++)
                        {
                            job.tiles.Add(World.current.GetTileAt(x, y));
                            World.current.GetTileAt(x, y).pendingFurnitureJob = job;
                        }
                    }

                    job.RegisterJobCompleteCallBack((theJob) =>
                    {
                        World.current.PlaceFurniture(furnitureType, tile);

                        foreach (Tile tile in theJob.tiles)
                        {
                            tile.pendingFurnitureJob = null;
                        }

                        theJob.tiles = new List<Tile>();
                    });

                    job.RegisterJobCancelCallBack((theJob) =>
                    {
                        foreach (Tile tile in theJob.tiles)
                        {
                            tile.pendingFurnitureJob = null;
                        }

                        theJob.tiles = new List<Tile>();
                    });

                    World.current.jobQueue.Enqueue(job);
                }
            }
            else if (currentMode == BuildMode.Deconstruct)
            {
                if (tile.furniture != null)
                {
                    tile.furniture.Deconstruct();
                }
            }
        }

        public bool IsObjectDraggable()
        {
            if (currentMode != BuildMode.Furniture)
            {
                return true;
            }

            Furniture proto = WorldController.Instance.world.furniturePrototypes[buildModeFurnitureType];

            return proto.width == 1 && proto.height == 1;
        }


        // İnşat modlarını atanır
        #region SetBuildMode

        public void SetMode_BuildFloor(string floorType)
        {
            PlayerController.Instance.oldMode = 1;
            currentMode = BuildMode.Floor;
            buildModeFloorType = floorType;
        }


        public void SetMode_BuildFurniture(string objectType)
        {
            PlayerController.Instance.oldMode = 1;
            currentMode = BuildMode.Furniture;
            buildModeFurnitureType = objectType;
        }

        public void SetMode_Deconstruct()
        {
            PlayerController.Instance.oldMode = 1;
            currentMode = BuildMode.Deconstruct;
        }

        #endregion

        // Debug için dünya oluştur

        public void DoPathfindingTest()
        {
            World.current.SetupPathfindingExample();
        }

    }
}