using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace  MyNamespace
{
    public class Pathfinder
    {
        public volatile bool jobDone;
        

        Tile startTile;
        Tile targetTile;

        Queue<Tile> path;
        PathfinderMaster.PathComplateCallBack callBack;

        Character character;

        public Pathfinder(Tile start,Tile target, PathfinderMaster.PathComplateCallBack callBack,Character character)
        {
            this.startTile = start;
            this.targetTile = target;
            this.callBack = callBack;
            this.character = character;
        }

        public void FindPath()
        {
            path = FindPathActual(startTile, targetTile);
            jobDone = true;
        }

        public Queue<Tile> FindPathActual(Tile startTile,Tile targetTile)
        {
            Queue<Tile> result = new Queue<Tile>();

            List<Tile> openSet = new List<Tile>();
            HashSet<Tile> closedSet = new HashSet<Tile>();

            openSet.Add(startTile);

            if (targetTile.movementCost > 0) 
            {
                while (openSet.Count > 0)
                {
                    Tile currentTile = openSet[0];
                    
                    for (int i = 0; i < openSet.Count; i++)
                    {
                        if (openSet[i].fCost < currentTile.fCost || openSet[i].fCost == currentTile.fCost && openSet[i].hCost < openSet[i].hCost)
                        {
                            if (!currentTile.Equals(openSet[i]))
                            {
                                currentTile = openSet[i];
                            }
                        }
                    }

                    openSet.Remove(currentTile); 
                    closedSet.Add(currentTile);

                    if (currentTile.Equals(targetTile))
                    {
                        result = RetracePath(startTile,currentTile);
                        break;
                    }

                    foreach (Tile b in World.current.GetNeighbours(currentTile))
                    {
                        if (!closedSet.Contains(b) && b.movementCost > 0)
                        {
                            if (currentTile.x != b.x && currentTile.y != b.y && (
                            World.current.GetTileAt(currentTile.x,b.y).movementCost == 0 || 
                            World.current.GetTileAt(b.x,currentTile.y).movementCost == 0))
                            {
                                continue;
                            }

                            float moveCost = currentTile.gCost + GetDistance(currentTile, b);

                            if (moveCost < b.gCost || !openSet.Contains(b))
                            {
                                b.gCost = moveCost;
                                b.hCost = GetDistance(b,targetTile);
                                b.parent = currentTile;

                                if (!openSet.Contains(b))
                                {
                                    openSet.Add(b);
                                }
                            }
                        }
                    }
                }
            }

            return ReverseQueue(result);
        }

        int GetDistance(Tile t1,Tile t2)
        {
            int distX = Mathf.RoundToInt(Mathf.Abs(t1.x - t2.x));
            int distY = Mathf.RoundToInt(Mathf.Abs(t1.y - t2.y));

            if (distX > distY)
            {
                return 14 * distY + 10 * (distX - distY);
            }

            return 14 * distX + 10 * (distY - distX);

        }

        Queue<Tile> RetracePath(Tile startTile,Tile endTile)
        {
            Queue<Tile> p = new Queue<Tile>();
            Tile currentTile = endTile;

            while (currentTile != startTile)
            {
                p.Enqueue(currentTile);
                currentTile = currentTile.parent;
            }

            return p;
        }

        // Bitiğinde çağırılır

        public void NotifyComplate()
        {
            if (callBack != null)
            {
                callBack.Invoke(path,character);
            }
        }

        Queue<T> ReverseQueue<T>(Queue<T> originalQueue)
        {
            Stack<T> stack = new Stack<T>();

            while (originalQueue.Count > 0)
            {
                stack.Push(originalQueue.Dequeue());
            }

            Queue<T> reversedQueue = new Queue<T>();
            while (stack.Count > 0)
            {
                reversedQueue.Enqueue(stack.Pop());
            }

            return reversedQueue;
        }

    }
}
