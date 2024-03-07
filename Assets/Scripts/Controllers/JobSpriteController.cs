using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class JobSpriteController : MonoBehaviour
    {
        FurnitureSpriteController fsc;

        private Dictionary<Job, GameObject> jobGameObjectMap = new Dictionary<Job, GameObject>();

        private void Start() 
        {
            fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
            World.current.jobQueue.RegisterJobCreationCallback(OnJobCreated);
        }

        void OnJobCreated(Job job)
        {
            if (job == null || job.jobObjectType == null)
            {
                Debug.Log("! OnJobCreated in job could not be identified !");
                return;
            }

            GameObject job_go = new GameObject();
            jobGameObjectMap.Add(job, job_go);

            job_go.name = "Job_" + job.jobObjectType + "_" + job.targetTile.x + "/" + job.targetTile.y;
            job_go.transform.position = new Vector2(job.targetTile.x,job.targetTile.y);
            job_go.transform.SetParent(transform,true);

            SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
            sr.sprite = fsc.GetSpriteForFurniture(job.jobObjectType);
            sr.sortingLayerName = "Job";
            sr.color = new Color(1f,1f,1f,0.5f);

            job.RegisterJobCompleteCallBack(OnJobEnded);
            job.RegisterJobCancelCallBack(OnJobEnded);
        }

        void OnJobEnded(Job job)
        {
            GameObject job_go = jobGameObjectMap[job];
            job.UnregisterJobCompleteCallBack(OnJobEnded);
            job.UnregisterJobCancelCallBack(OnJobEnded);
            Destroy(job_go);
        }
    }
}
