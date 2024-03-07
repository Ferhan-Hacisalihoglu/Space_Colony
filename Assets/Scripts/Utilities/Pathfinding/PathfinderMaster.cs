using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace MyNamespace
{
    public class PathfinderMaster : MonoBehaviour
    {
        public static PathfinderMaster Instance;

        int maxJobs;

        private void Awake() 
        {
            Instance = this;
            maxJobs = System.Environment.ProcessorCount/2;
        }

        public delegate void PathComplateCallBack(Queue<Tile> path,Character character);

        List<Pathfinder> toDoJobs = new List<Pathfinder>();
        List<Pathfinder> currentJobs = new List<Pathfinder>();

        private void Update()
        {
            int i = 0;
            while (i < currentJobs.Count)
            {
                if (currentJobs[i].jobDone)
                {
                    currentJobs[i].NotifyComplate();
                    currentJobs.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (toDoJobs.Count > 0 && currentJobs.Count < maxJobs)
            {
                Pathfinder job = toDoJobs[0];
                toDoJobs.RemoveAt(0);
                currentJobs.Add(job);

                Thread jobThread = new Thread(job.FindPath);
                jobThread.Start();
            }
        }


        public void RequestPathfind(Tile startTile,Tile targetTile,Character character)
        {
            Pathfinder pathJob = new Pathfinder(startTile,targetTile,PathCallBack,character);
            toDoJobs.Add(pathJob);   
        }

        void PathCallBack(Queue<Tile> path,Character character)
        {
            character.path = path;
            character.ChechPath();
        }
    }
}
