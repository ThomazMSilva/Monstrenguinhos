using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    public class NPCManager : MonoBehaviour
    {
        [SerializeField] private List<ClientAttributes> possibleClients = new();
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform spawnPosition;
        [SerializeField] private int maxClients = 5;
        private float spawnInterval = 15f;
        private float capCheckInterval = .5f;
        private List<NPCBehaviour> spawnedNPCs = new();
        private WaitForSeconds waitForCapCheck;

        //Fazer referencia a um transform do fundo com a posicao e rotacao do primeiro cliente na fila
        //depois referemcia a um segundo transform que é da segunda posição, do lado. por cima, tanto faz
        //aí em script calcula a diferença entre esses dois, e determina a posição como "diferença * spawnedNPCs.Count"
        //no script do behaviour atribuir um navmeshagent, e um metodo pra definir o alvo
        //organizar essas variaveis

        private void Start()
        {
            waitForCapCheck = new(capCheckInterval);

            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (spawnedNPCs.Count >= maxClients) yield return waitForCapCheck;

                yield return new WaitForSeconds(spawnInterval);

                var client = Instantiate(npcPrefab).GetComponent<NPCBehaviour>();
                spawnedNPCs.Add(client);
                client.onDestroy.AddListener(RemoveClientFromList);
                client.Attributes = possibleClients[Random.Range(0, possibleClients.Count)];
            }
        }
        private void RemoveClientFromList(NPCBehaviour client) => spawnedNPCs.Remove(client);
    }
}
