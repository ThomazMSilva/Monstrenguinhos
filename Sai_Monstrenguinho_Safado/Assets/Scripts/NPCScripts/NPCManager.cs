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
        //depois referemcia a um segundo transform que � da segunda posi��o, do lado. por cima, tanto faz
        //a� em script calcula a diferen�a entre esses dois, e determina a posi��o como "diferen�a * spawnedNPCs.Count"
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
