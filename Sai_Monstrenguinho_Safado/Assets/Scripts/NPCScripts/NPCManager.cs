using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    public class NPCManager : MonoBehaviour
    {
        #region ATTRIBUTES
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform npcParent;
        [SerializeField] private Transform targetPositionParent;
        [SerializeField] private List<ClientAttributes> possibleClients = new();
        [Tooltip("As posi��es pra onde os npcs v�o quando spawnam (a filazinha). Eles spawnam e s�o designados uma posi��o pra ir. \nV�o na ordem da lista, ent�o se uma posi��o x t� na frente da y no mundo, mas t� embaixo da y na lista, os npcs v�o primeiro pra y.")]
        [SerializeField] private List<Transform> targetPositions = new();
        [SerializeField] private int maxClients = 5;
        public float spawnInterval = 15f;
        [Tooltip("Se chega no n�mero m�ximo de npcs permitidos, vai checar a cada [esse tanto] de segundos se liberou. Depois de terminar todos os eventos, d� pra tirar isso aqui.")]
        [SerializeField] private float capCheckInterval = .5f;

        private Vector3 spawnOffsetDirection;
        [Tooltip("Se est�o serializadas menos de duas posi��es de alvo para os npcs irem atr�s, \nspawna novas a partir desse offset em rela��o � primeira posi��o.")]
        [SerializeField] private Vector3 defaultOffset = Vector3.forward;
        private WaitForSeconds waitForCapCheck;
        private List<NPCBehaviour> spawnedNPCs = new();
        #endregion

        private void Start()
        {
            if (!TryInitializeTargetPositions()) return;


            waitForCapCheck = new(capCheckInterval);

            StartCoroutine(SpawnRoutine());
        }

        private bool TryInitializeTargetPositions()
        {
            if (targetPositions.Count < 1)
            {
                Debug.LogError("N�o h� posi��o de spawn serializada no NPCManager; Retornando Start()");
                return false;
            }

            if (targetPositions.Count < maxClients)
            {
                //Inicializa cadeia se s� tiver 1 ponto
                if (targetPositions.Count == 1)
                {
                    var secondPosition = new GameObject("TargetPosition (1)").transform;
                    secondPosition.SetParent(targetPositionParent);
                    secondPosition.SetPositionAndRotation
                    (
                        targetPositions[0].position + defaultOffset,
                        targetPositions[0].rotation
                    );

                    targetPositions.Add(secondPosition);
                }

                spawnOffsetDirection = targetPositions[^1].position - targetPositions[^2].position;

                //Pra n�o alterar a lista que t� referenciada no loop
                List<Transform> copyTargetPositions = new(targetPositions);

                for (int i = targetPositions.Count + 1; i <= maxClients; i++)
                {
                    var newPosition = new GameObject($"TargetPosition ({i - 1})").transform;
                    newPosition.SetParent(targetPositionParent);
                    newPosition.SetPositionAndRotation
                    (
                        targetPositions[^1].position + spawnOffsetDirection * (i - targetPositions.Count),
                        newPosition.rotation= targetPositions[^1].rotation
                    );

                    copyTargetPositions.Add(newPosition);
                }
                targetPositions = copyTargetPositions;
            }
            return true;
        }

        private void SpawnClient()
        {
            var client = Instantiate(npcPrefab, transform.position, transform.rotation, npcParent).GetComponent<NPCBehaviour>();
            spawnedNPCs.Add(client);
            client.gameObject.name = $"Cliente_{spawnedNPCs.Count - 1}";
            client.onDestroy.AddListener(RemoveClientFromList);
            client.SetAttributes(possibleClients[Random.Range(0, possibleClients.Count)]);
            client.SetTarget(targetPositions[spawnedNPCs.Count - 1], client.OrderCrops);
            

        }
        
        private void RemoveClientFromList(NPCBehaviour client) => spawnedNPCs.Remove(client);

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                while (spawnedNPCs.Count >= maxClients) yield return waitForCapCheck;

                yield return new WaitForSeconds(spawnInterval);

                SpawnClient();
            }
        }
    }
}
