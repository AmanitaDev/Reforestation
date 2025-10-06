using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace GameTemplate.Systems.Pooling
{
    public class PoolingService : MonoBehaviour
    {
        [HideInInspector] public Transform poolParent;
        PoolID testPoolId = 0;
        
        // we store pooled objects in this dictionary
        private Dictionary<PoolID, Queue<GameObject>> objectPool = new Dictionary<PoolID, Queue<GameObject>>();
        // wes store pool objects that parents changed in this dictionary. this way we can retrieve them back
        [SerializeField] private List<PoolElement> parentsChangedPoolObjects = new List<PoolElement>();

        PoolingDataSO _poolingDataSoDataSo;

        [Inject]
        public void Construct(PoolingDataSO poolingDataSoDataSo)
        {
            Debug.Log("Construct PoolingService");
            _poolingDataSoDataSo = poolingDataSoDataSo;
            SpawnObjects();
        }

        void SpawnObjects()
        {
            Debug.Log("Initialize PoolingService");
            poolParent = new GameObject("_PoolParent").transform;
            DontDestroyOnLoad(poolParent.gameObject);
            
            for (int i = 0; i < _poolingDataSoDataSo.poolObjects.Length; i++)
            {
                objectPool.Add((PoolID)i, new Queue<GameObject>());
                for (int z = 0; z < _poolingDataSoDataSo.poolObjects[i].objectCount; z++)
                {
                    GameObject newObject = Object.Instantiate(_poolingDataSoDataSo.poolObjects[i].objectPrefab, poolParent);
                    newObject.SetActive(false);
                    newObject.GetComponent<PoolElement>()
                        .Initialize(_poolingDataSoDataSo.poolObjects[i].goBackOnDisable, (PoolID)i);
                    objectPool[(PoolID)i].Enqueue(newObject);
                }
            }
        }

        public void ResetPool()
        {
            for (int i = 0; i < parentsChangedPoolObjects.Count; i++)
            {
                parentsChangedPoolObjects[i].transform.SetParent(poolParent);
            }

            parentsChangedPoolObjects.Clear();
            PoolElement[] children = poolParent.GetComponentsInChildren<PoolElement>();
            for (int i = 0; i < children.Length; i++)
            {
                children[i].gameObject.SetActive(false);
            }
        }

        public void OnPoolElementDestroyed(PoolElement destroyedPoolElement)
        {
            Debug.Log("element Destroyed : " + destroyedPoolElement);
        }

        public void GoBackToPool(GameObject poolObject)
        {
            poolObject.transform.SetParent(poolParent);
            objectPool[poolObject.GetComponent<PoolElement>().PoolId].Enqueue(poolObject);
        }

        public GameObject GetParticleById(PoolID poolId, Transform referance)
        {
            return GetParticleById(poolId, referance.position, Vector3.one);
        }

        public GameObject GetParticleById(PoolID poolId, Transform referance, Vector3 targetScale)
        {
            return GetParticleById(poolId, referance.position, targetScale);
        }

        public GameObject GetParticleById(PoolID poolId, Vector3 position, Vector3 targetScale,
            Transform parentInfo = null)
        {
            GameObject particle = GetGameObjectById(poolId, position, Quaternion.identity);
            particle.transform.localScale = targetScale;
            if (parentInfo != null)
            {
                particle.transform.SetParent(parentInfo.parent);
                parentInfo.transform.localPosition = parentInfo.localPosition;
            }

            particle.GetComponent<ParticleSystem>().Play();
            return particle;
        }

        public GameObject GetGameObjectById(PoolID poolId)
        {
            return GetGameObjectById(poolId, Vector3.zero, Quaternion.identity);
        }

        public GameObject GetGameObjectById(PoolID poolId, Transform objectTransform)
        {
            return GetGameObjectById(poolId, objectTransform.position, objectTransform.rotation);
        }

        public GameObject GetGameObjectById(PoolID poolId, Vector3 position)
        {
            return GetGameObjectById(poolId, position, Quaternion.identity);
        }

        public GameObject GetGameObjectById(PoolID poolId, Vector3 position, Quaternion rotation)
        {
            if (!objectPool.ContainsKey(poolId))
            {
                objectPool.Add(poolId, new Queue<GameObject>());
            }

            if (objectPool[poolId].Count != 0)
            {
                GameObject poolObject = objectPool[poolId].Dequeue();
                poolObject.transform.position = position;
                poolObject.transform.rotation = rotation;
                poolObject.SetActive(true);
                return poolObject;
            }

            PoolObject selectedPoolObject =
                _poolingDataSoDataSo.poolObjects.Where(x => x.poolName.Equals(poolId.ToString())).First();

            if (selectedPoolObject != null)
            {
                GameObject poolObject = Object.Instantiate(selectedPoolObject.objectPrefab, position, rotation);
                poolObject.transform.SetParent(poolParent);
                poolObject.GetComponent<PoolElement>().Initialize(selectedPoolObject.goBackOnDisable, poolId);
                poolObject.SetActive(true);
                return poolObject;
            }

            return null;
        }

        public GameObject GetGameObjectById(PoolID poolId, Vector3 position, Quaternion rotation, Vector3 targetScale)
        {
            if (!objectPool.ContainsKey(poolId))
            {
                objectPool.Add(poolId, new Queue<GameObject>());
            }

            if (objectPool[poolId].Count != 0)
            {
                GameObject poolObject = objectPool[poolId].Dequeue();
                poolObject.transform.position = position;
                poolObject.transform.rotation = rotation;
                poolObject.transform.localScale = targetScale;
                poolObject.SetActive(true);
                return poolObject;
            }

            PoolObject selectedPoolObject =
                _poolingDataSoDataSo.poolObjects.Where(x => x.poolName.Equals(poolId.ToString())).First();

            if (selectedPoolObject != null)
            {
                GameObject poolObject = Object.Instantiate(selectedPoolObject.objectPrefab, position, rotation);
                poolObject.transform.SetParent(poolParent);
                poolObject.GetComponent<PoolElement>().Initialize(selectedPoolObject.goBackOnDisable, poolId);
                poolObject.transform.localScale = targetScale;
                poolObject.SetActive(true);
                return poolObject;
            }

            return null;
        }

        public void GoBackToPool(PoolElement elementToGoBackToPool)
        {
            objectPool[elementToGoBackToPool.PoolId].Enqueue(elementToGoBackToPool.gameObject);
        }

        public void GoBackToPool(PoolID poolId, GameObject objectToAddPool)
        {
            objectToAddPool.SetActive(false);
            objectPool[poolId].Enqueue(objectToAddPool);
        }

        public void PoolElementParentChanged(PoolElement parentChangedObject)
        {
            parentsChangedPoolObjects.Add(parentChangedObject);
        }

        [Button("TestGetObject")]
        public void GetTestGameObject()
        {
            GetGameObjectById(testPoolId);
        }
    }
}