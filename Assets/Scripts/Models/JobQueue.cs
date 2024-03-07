using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MyNamespace
{
    public class JobQueue
    {
        Queue<Job> jobQueue = new Queue<Job>();

        public JobQueue()
        {
            
        }

        // Yeni iş ekle

        public void Enqueue(Job j)
        {
            jobQueue.Enqueue(j);

            if (cbJobCreated != null)
            {
                cbJobCreated(j);
            }
        }

        // İş çıkar

        public Job Dequeue()
        {
            if (jobQueue.Count == 0)
            {
                return null;
            }

            return jobQueue.Dequeue();
        }

        // Actions

        Action<Job> cbJobCreated;

        public void RegisterJobCreationCallback(Action<Job> cb)
        {
            cbJobCreated += cb;
        }
        
        public void UnregisterJobCreationCallback(Action<Job> cb)
        {
            cbJobCreated -= cb;
        }
    }
}