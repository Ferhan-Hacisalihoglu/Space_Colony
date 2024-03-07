using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public static class SimplePool
    {
        private const int Default_Pool_Size = 3;

        class Pool
        {
            private int nextId = 1;
            private Stack<GameObject> inactive;
            private GameObject prefabs;

            public Pool(GameObject prefabs, int initialQty)
            {
                this.prefabs = prefabs;
                inactive = new Stack<GameObject>(initialQty);
            }

            public GameObject Spawn(Vector3 pos)
            {
                GameObject obj;
                if (inactive.Count == 0)
                {
                    obj = GameObject.Instantiate(prefabs, pos, Quaternion.identity);
                    obj.name = prefabs.name + " ( " + (nextId++) + " ) ";
                    obj.AddComponent<PoolMember>().myPool = this;
                }
                else
                {
                    obj = inactive.Pop();
                    if (obj == null)
                    {
                        return Spawn(pos);
                    }
                }

                obj.transform.position = pos;
                obj.transform.rotation = Quaternion.identity;
                obj.SetActive(true);
                return obj;
            }

            public void DeSpawn(GameObject obj)
            {
                obj.SetActive(false);
                inactive.Push(obj);
            }
            
        }

        class PoolMember : MonoBehaviour
        {
            public Pool myPool;
        }

        private static Dictionary<GameObject, Pool> pools = new Dictionary<GameObject, Pool>();

        static void Init(GameObject prefabs = null, int qty = Default_Pool_Size)
        {
            if (prefabs != null && pools.ContainsKey(prefabs) == false)
            {
                pools[prefabs] = new Pool(prefabs, qty);
            }
        }

        static public void Preload(GameObject prefabs, int qty = 1)
        {
            Init(prefabs,qty);
            GameObject[] obs = new GameObject[qty];
            for (int i = 0; i < qty; i++)
            {
                obs[i] = Spawn(prefabs, Vector3.zero);
            }

            for (int i = 0; i < qty; i++)
            {
                DeSpawn(obs[i]);
            }
        }

        static public GameObject Spawn(GameObject prefabs, Vector3 pos)
        {
            Init(prefabs);
            return pools[prefabs].Spawn(pos);
        }

        static public void DeSpawn(GameObject obj)
        {
            PoolMember pm = obj.GetComponent<PoolMember>();
            if (pm == null)
            {
                //Debug.Log("Object "+obj.name + "wasn't spawned form a pool. Destroying it instead.");
                GameObject.Destroy(obj);
            }
            else
            {
                pm.myPool.DeSpawn(obj);
            }
        }

    }
}
