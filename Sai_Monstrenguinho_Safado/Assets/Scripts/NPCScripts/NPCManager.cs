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
        [Tooltip("As posições pra onde os npcs vão quando spawnam (a filazinha). Eles spawnam e são designados uma posição pra ir. \nVão na ordem da lista, então se uma posição x tá na frente da y no mundo, mas tá embaixo da y na lista, os npcs vão primeiro pra y.")]
        [SerializeField] private List<Transform> targetPositions = new();
        [SerializeField] private int maxClients = 5;
        public float spawnInterval = 15f;
        [Tooltip("Se chega no número máximo de npcs permitidos, vai checar a cada [esse tanto] de segundos se liberou. Depois de terminar todos os eventos, dá pra tirar isso aqui.")]
        [SerializeField] private float capCheckInterval = .5f;

        private Vector3 spawnOffsetDirection;
        [Tooltip("Se estão serializadas menos de duas posições de alvo para os npcs irem atrás, \nspawna novas a partir desse offset em relação à primeira posição.")]
        [SerializeField] private Vector3 defaultOffset = Vector3.forward;
        private WaitForSeconds waitForCapCheck;
        private List<NPCBehaviour> spawnedNPCs = new();
        private Dictionary<Transform, NPCBehaviour> positionsOccupationDict = new();
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
                Debug.LogError("Não há posição de spawn serializada no NPCManager; Retornando Start()");
                return false;
            }

            if (targetPositions.Count < maxClients)
            {
                //Inicializa cadeia se só tiver 1 ponto
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

                //Pra não alterar a lista que tá referenciada no loop
                List<Transform> copyTargetPositions = new(targetPositions);

                for (int i = targetPositions.Count + 1; i <= maxClients; i++)
                {
                    var newPosition = new GameObject($"TargetPosition ({i - 1})").transform;
                    newPosition.SetParent(targetPositionParent);
                    newPosition.SetPositionAndRotation
                    (
                        targetPositions[^1].position + spawnOffsetDirection * (i - targetPositions.Count),
                        newPosition.rotation = targetPositions[^1].rotation
                    );

                    copyTargetPositions.Add(newPosition);
                }
                targetPositions = copyTargetPositions;
            }

            foreach (var position in targetPositions)
            {
                if (!positionsOccupationDict.ContainsKey(position))
                {
                    positionsOccupationDict.Add(position, null);
                }
            }

            return true;
        }

        private void SpawnClient()
        {
            var client = Instantiate(npcPrefab, transform.position, transform.rotation, npcParent).GetComponent<NPCBehaviour>();
            spawnedNPCs.Add(client);
            client.SetAttributes(possibleClients[Random.Range(0, possibleClients.Count)]);
            client.gameObject.name = $"Cliente ({spawnedNPCs.Count - 1}) - {client.Name}";

            Transform furthestAvailablePosition = targetPositions[0];

            for (int i = 0; i < targetPositions.Count; i++)
            {
                var position = targetPositions[i];
                if (positionsOccupationDict.ContainsKey(position) && positionsOccupationDict[position] == null)
                {
                    furthestAvailablePosition = position;
                    break;
                }
            }

            positionsOccupationDict[furthestAvailablePosition] = client;
            client.SetTarget(furthestAvailablePosition, client.OrderCrops);
            
            client.onDestroy.AddListener(RemoveClientFromList);

        }

        private void RemoveClientFromList(NPCBehaviour client)
        {
            spawnedNPCs.Remove(client);
            foreach(var kvp in positionsOccupationDict)
            {
                if (kvp.Value != client) continue;
                
                positionsOccupationDict[kvp.Key] = null;
                break;
            }
        }

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
