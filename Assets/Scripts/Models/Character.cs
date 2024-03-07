using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MyNamespace
{
    public class Character : IXmlSerializable
    {

        #region PositionAndMovement

        public float x 
        { 
            get 
            {
                if (nextTile == null)
                    return currentTile.x;

                return Mathf.Lerp(currentTile.x, nextTile.x, movementPercentage); 
            } 
        }

        public float y 
        { 
            get 
            {
                if (nextTile == null)
                    return currentTile.y;

                return Mathf.Lerp(currentTile.y, nextTile.y, movementPercentage); 
            } 
        }


        // Current Tile

        private Tile _currentTile;

        public Tile currentTile { 
            get { return _currentTile; }

            protected set
            {
                if (_currentTile != null)
                {
                    _currentTile.characters.Remove(this);
                }
                _currentTile = value;
                _currentTile.characters.Add(this);
            }
        }

        // Pathfinding

        Tile _destTile;
        public Tile destTile
        {
            get { return _destTile; }
            set
            {
                if (_destTile != value)
                {
                    _destTile = value;
                }
            }
        }

        public Queue<Tile> path;
        
        private Tile nextTile;
        
        // movement

        private float movementPercentage;
        private float speed = 4f;

        // PathfinderMaster

        bool isPathComplete;

        #endregion

        #region CharacterInstance
        
        public Character()
        {
            isPathComplete = true;
        }

        public Character(Tile tile)
        {
            _currentTile = currentTile = destTile = nextTile = tile;
            isPathComplete = true;
        }

        public void ChechPath()
        {
            if (path == null || path.Count == 0)
            {
                AbandonJob();
            }
            else
            {
                isPathComplete = true;
                nextTile = this.path.Dequeue();
            }
        }

        #endregion

        #region Inventory

        public Inventory inventory;

        #endregion

        #region UpdateActions

        // Update

        public void Update(float deltaTime) 
        {
            Update_DoMovement(deltaTime);

            if (cbCharacterChanged != null)
            {
                cbCharacterChanged(this);
            }
        }

        // Karakter hareket etme kodu

        void Update_DoMovement(float deltaTime)
        {
            //Hedefe ulaştığında durdur
            if (currentTile == destTile)
            {
                path = null;
                isPathComplete = true;
                return; 
            }

            //Yol bulma algoritması bitmeden devam etme
            if (!isPathComplete)
            {
                return; 
            }

            //Yolu ata

            if (nextTile == null || nextTile == currentTile)
            {
                if (path == null || path.Count == 0)
                {
                    // Generate a path to our destination
                    PathfinderMaster.Instance.RequestPathfind(currentTile,destTile,this);
                    isPathComplete = false;

                    return;
                }

                nextTile = path.Dequeue();
            }

            //Hareket ettir

            float distToTravel = Mathf.Sqrt(
                Mathf.Pow(currentTile.x - nextTile.x, 2) +
                Mathf.Pow(currentTile.y - nextTile.y, 2)
            );

            if (nextTile.IsEnterable() == Enterability.Never)
            {
                nextTile = null;    
                path = null; 
                return;
            }
            else if (nextTile.IsEnterable() == Enterability.Soon)
            {
                return;
            }

            float distThisFrame = speed / nextTile.movementCost * deltaTime;

            float percThisFrame = distThisFrame / distToTravel;

            movementPercentage += percThisFrame;

            if (movementPercentage >= 1)
            {
                currentTile = nextTile;
                movementPercentage = 0;
            }
        }

        // FixedUpdate

        public void FixedUpdate(float deltaTime) 
        {
            Update_DoJob(deltaTime);
        }

        // Karakterin işini yönetir

        Job myJob;

        float jobSearchCoolDown = 0;

        void Update_DoJob(float deltaTime)
        {
            jobSearchCoolDown -= Time.fixedDeltaTime;

            if (myJob == null)
            {
                if (jobSearchCoolDown > 0)
                {
                    return;
                }

                GetNewJob();

                if (myJob == null)
                {
                    jobSearchCoolDown = 0.5f;
                    destTile = currentTile;
                    return;
                }
            }

            if (!myJob.HasAllMaterial())
            {
                if (inventory != null)
                {
                    if (myJob.DesiresInventoryType(inventory) > 0)
                    {
                        if (currentTile == myJob.targetTile)
                        {
                            World.current.inventoryManager.PlaceInventory(myJob, inventory);
                            myJob.DoWork(0);

                            if (inventory.stackSize == 0)
                            {
                                inventory = null;
                            }
                            else
                            {
                                Debug.Log("! Character is still carrying inventory , wich shouldn't be . Just setting to NULL for now , but this means we are  LEAKING inventory !");
                                inventory = null;
                            }
                        }
                        else
                        {
                            destTile = myJob.targetTile;
                            return;
                        }
                    }
                    else
                    {
                        if (!World.current.inventoryManager.PlaceInventory(currentTile, inventory))
                        {
                            Debug.Log("! Character tried to dump inventory into an invalid tile (maybe there's already something here !");
                            inventory = null;
                        }
                    }
                }
                else
                {
                    if (currentTile.inventory != null && 
                        (myJob.canTakeFromStockpile || currentTile.furniture == null || currentTile.furniture.IsStockpile() == false ) &&
                        myJob.DesiresInventoryType(currentTile.inventory) > 0)
                    {
                        World.current.inventoryManager.PlaceInventory(
                            this, 
                            currentTile.inventory, 
                            myJob.DesiresInventoryType(currentTile.inventory)
                            );
                    }
                    else
                    {
                        Inventory desired = myJob.GetFirstDesiredInventory();

                        if (currentTile != nextTile)
                        {
                            return;
                        }

                        if (isPathComplete && path != null && path.Count > 0 && destTile != null && destTile.inventory != null && destTile.inventory.type == desired.type)
                        {
                            // Zaten istediğimizi içeren bir tile doğru ilerliyoruz!
                            // Yani... hiçbir şey yapmayalım mı?
                        }
                        else
                        {   
                            if (!isPathComplete)
                            {
                                return; 
                            }

                            if (path == null || path.Count == 0)
                            {
                                World.current.inventoryManager.GetClosestInventoryOfType(desired.type,this,desired.maxStackSize-desired.stackSize,myJob.canTakeFromStockpile);
                                isPathComplete = false;
                                return;
                            }
                        }

                        return;
                    }
                }
            }

            destTile = myJob.targetTile;

            if (currentTile == myJob.targetTile)
            {
                myJob.DoWork(deltaTime);
            }
        }

        void GetNewJob()
        {
            myJob = World.current.jobQueue.Dequeue();

            if (myJob == null)
            {
                return;
            }

            destTile = myJob.targetTile;
            myJob.RegisterJobCompleteCallBack(OnJobEnded);
            myJob.RegisterJobCancelCallBack(OnJobEnded);
        }

        // Karakter işe ulaşamadığında işi iptal eder

        void AbandonJob()
        {
            Debug.Log("Job abandoned : " + myJob);
            myJob = null;
            nextTile = destTile = currentTile = _currentTile;
            path = null;
        }

        // İş bittiğinde çağrılır

        void OnJobEnded(Job job)
        {
            if (job != myJob)
            {
                // Çözemedim 
                //Debug.Log("! Character being told about job isn't his . You forget to unregister something. !");
                return;
            }

            myJob = null;
        }


        #endregion
        
        #region Action

        // Karakter hareket etirme aksiyonlarını ata

        private Action<Character> cbCharacterChanged;

        public void RegisterOnChangedCallBack(Action<Character> cb)
        {
            cbCharacterChanged += cb;
        }

        public void UnregisterOnChangedCallBack(Action<Character> cb)
        {
            cbCharacterChanged -= cb;
        }

        #endregion
    
        #region SaveLoad

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("X",currentTile.x.ToString());
            writer.WriteAttributeString("Y", currentTile.y.ToString());
        }

        #endregion

    }
}