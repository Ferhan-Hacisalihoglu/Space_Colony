using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public enum PlayerMode
    {
        Movement,
        Build,
        UI,
    }

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed;

        BuildModeController buildModeController;
        FurnitureSpriteController furnitureSpriteController;

        public static PlayerController Instance;

        bool isTouching;

        private void Awake()
        {
            buildModeController = FindObjectOfType<BuildModeController>();
            furnitureSpriteController = FindObjectOfType<FurnitureSpriteController>();
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Touch"))
            {
                isTouching = true;
                OnStartTouch(WorldPosition());
            }
            else if (Input.GetButton("Touch"))
            {
                OnSwipe(WorldPosition());
            }
            else if (Input.GetButtonUp("Touch"))
            {
                isTouching = false;
                OnEndTouch(WorldPosition());
            }
        }

        Vector2 WorldPosition()
        {
            Vector2 screenPosition = Input.mousePosition;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        void MovePlayer(Vector2 direction)
        {
            Vector3 moveDirection = direction * moveSpeed * Time.deltaTime;

            if (Vector2.Distance(Vector2.zero, moveDirection) <= minimumDistance)
            {
                return;
            }

            if (transform.position.x + moveDirection.x >= World.current.worldSize || transform.position.x + moveDirection.x <= 0)
            {
                moveDirection.x = 0;
            }

            if (transform.position.y + moveDirection.y >= World.current.worldSize || transform.position.y + moveDirection.y <= 0)
            {
                moveDirection.y = 0;
            }

            transform.position += moveDirection;
        }

        #region MouseEvents

        Vector2 startPosition;
        Vector2 endPosition;

        [SerializeField] GameObject circleCrousorPrefabs;

        List<GameObject> dragPreviewGameObjects = new List<GameObject>();

        void OnStartTouch(Vector2 position)
        {
            startPosition = position;
        }

        
        [SerializeField] float minimumDistance;

        void OnSwipe(Vector2 position)
        {
            endPosition = position;

            if (currentMode == PlayerMode.Movement)
            {
                Vector2 direction = startPosition - endPosition;
                MovePlayer(direction);
            }
            else if (currentMode == PlayerMode.Build)
            {
                ClearDragPreviewGameObjects();

                int start_x = Mathf.FloorToInt(startPosition.x);
                int end_x = Mathf.FloorToInt(endPosition.x);
                int start_y = Mathf.FloorToInt(startPosition.y);
                int end_y = Mathf.FloorToInt(endPosition.y);

                if (!buildModeController.IsObjectDraggable())
                {
                    end_x = start_x;
                    end_y = start_y;
                }

                if (end_x < start_x)
                {
                    int tmp = end_x;
                    end_x = start_x;
                    start_x = tmp;
                }

                if (end_y < start_y)
                {
                    int tmp = end_y;
                    end_y = start_y;
                    start_y = tmp;
                }

                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        Tile t = World.current.GetTileAt(x, y);

                        if (t != null)
                        {
                            if (buildModeController.currentMode == BuildMode.Furniture)
                            {
                                // Furniture inşat edilcek ise onun gölgesini oluştur
                                ShowFurnitureSpriteAtTile(buildModeController.buildModeFurnitureType, t);
                            }
                            else
                            {
                                // Tile inşat edilcek ise CursorCircle oluştur
                                GameObject go = SimplePool.Spawn(circleCrousorPrefabs, new Vector2(x, y));
                                go.transform.SetParent(this.transform, true);
                                dragPreviewGameObjects.Add(go);
                            }
                        }
                    }
                }
            }
        }

        void ShowFurnitureSpriteAtTile(string furnitureType, Tile t)
        {
            GameObject go = new GameObject(furnitureType);
            go.transform.SetParent(this.transform, true);
            dragPreviewGameObjects.Add(go);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Job";
            sr.sprite = furnitureSpriteController.GetSpriteForFurniture(furnitureType);

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t))
            {
                sr.color = new Color(0.5f, 0.9f, 0.5f, 0.5f);
            }
            else
            {
                sr.color = new Color(0.9f, 0.5f, 0.5f, 0.5f);
            }

            go.transform.position = new Vector2(t.x, t.y);
        }

        void OnEndTouch(Vector2 position)
        {
            endPosition = position;

            ClearDragPreviewGameObjects();

            if (currentMode == PlayerMode.Build)
            {
                int start_x = Mathf.FloorToInt(startPosition.x);
                int end_x = Mathf.FloorToInt(endPosition.x);
                int start_y = Mathf.FloorToInt(startPosition.y);
                int end_y = Mathf.FloorToInt(endPosition.y);

                if (!buildModeController.IsObjectDraggable())
                {
                    end_x = start_x;
                    end_y = start_y;
                }

                if (end_x < start_x)
                {
                    int tmp = end_x;
                    end_x = start_x;
                    start_x = tmp;
                }

                if (end_y < start_y)
                {
                    int tmp = end_y;
                    end_y = start_y;
                    start_y = tmp;
                }

                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        Tile t = World.current.GetTileAt(x, y);

                        if (t != null)
                        {
                            buildModeController.DoBuild(t);
                        }
                    }
                }
            }

        }

        void ClearDragPreviewGameObjects()
        {
            while (dragPreviewGameObjects.Count > 0)
            {
                GameObject go = dragPreviewGameObjects[0];
                dragPreviewGameObjects.RemoveAt(0);

                if (buildModeController.currentMode == BuildMode.Furniture)
                {
                    Destroy(go);
                }
                else
                {
                    SimplePool.DeSpawn(go);
                }
            }
        }

        #endregion

        #region PlayerMode

        PlayerMode currentMode = PlayerMode.Movement;

        [HideInInspector] public int oldMode;

        public void SetPlayerMode(int sellectMouseMode)
        {
            // Böyle olması lazım neden böyle bende bilmiyom (değiştirme)
            
            switch (sellectMouseMode)
            {
                case 0:
                    currentMode = PlayerMode.Movement;
                    oldMode = 0;
                    break;
                case 1:
                    currentMode = PlayerMode.Build;
                    break;
                case 2:
                    currentMode = PlayerMode.UI;
                    break;
                default:
                    Debug.Log("! SetMouseMode ---" + sellectMouseMode +"    could not be identified !");
                    break;
            }
        }

        public void SetPlayerOldMode()
        {
            SetPlayerMode(oldMode);
        }

        #endregion
    }
}