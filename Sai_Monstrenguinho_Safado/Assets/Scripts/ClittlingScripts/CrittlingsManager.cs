using Assets.Scripts.Interactibles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ClittlingScripts
{
    public class CrittlingsManager : MonoBehaviour
    {
        [SerializeField] private GameObject crittlingPrefab;
        [SerializeField] private Transform crittlingParent;

        [SerializeField] private List<Transform> spawnablePositions = new();
        [SerializeField] private List<Plot> landPlots = new();
        private List<CrittlingBehaviour> spawnedCrittlings = new();

        public bool isSpawning = true;

        [SerializeField] private float plantedPlotCheckInterval = 1f;
        private WaitForSeconds WaitForPlantedPlot;
        public float minTime = 15f;
        public float maxTime = 20f;
        
        public int CurrentMinCrittling = 1;
        public int CurrentMaxCrittling = 3;

        [SerializeField] private float updateTime = .2f;
        private Coroutine spawnCrittlingRoutine;
        private Coroutine updateCrittlingRoutine;
       
        private void Start()
        {
            /*SpawnCrittling();
            SpawnCrittling();*/
            WaitForPlantedPlot = new(plantedPlotCheckInterval);
            spawnCrittlingRoutine = StartCoroutine(CrittlingSpawnHandler());
            updateCrittlingRoutine = StartCoroutine(UpdateCrittlingBehaviours());
        }

        private System.Collections.IEnumerator CrittlingSpawnHandler()
        {
            while (true)
            {
                if(!isSpawning) yield break;

                var spawnTime = Random.Range(minTime, maxTime);
                yield return new WaitForSeconds(spawnTime);

                while (GetPlantedPlots() == null) 
                    yield return WaitForPlantedPlot;

                var spawnAmount = Random.Range(CurrentMinCrittling, CurrentMaxCrittling + 1);

                for (int i = 0; i < spawnAmount; i++)
                {
                    SpawnCrittling();
                }
            }
        }

        private void SpawnCrittling()
        {
            var positionIndex = Random.Range(0, spawnablePositions.Count);
            var crittling = Instantiate
            (
                crittlingPrefab,
                spawnablePositions[positionIndex].position,
                Quaternion.identity,
                crittlingParent
            );
            crittling.name = $"{crittlingPrefab.name}_{spawnedCrittlings.Count}";
            var critterBehaviour = crittling.GetComponent<CrittlingBehaviour>();
            critterBehaviour.Manager = this;
            spawnedCrittlings.Add(critterBehaviour);
        }

        private System.Collections.IEnumerator UpdateCrittlingBehaviours()
        {
            while (true) 
            {
                yield return new WaitForSeconds(updateTime);

                foreach(var crittling in spawnedCrittlings)
                {
                    crittling.UpdateBehaviour();
                }
            }
        }

        public List<Plot> GetPlantedPlots()
        {
            var plantedPlots = landPlots.Where(p => p.currentCrop != null && !string.IsNullOrEmpty(p.currentCrop.CropName)).ToList();
            return plantedPlots.Count > 1 ? plantedPlots : null;
        }

        /// <summary>
        /// Retorna todos os terrenos com a colheita com o nome designado. <br></br>
        /// Se não acha nenhum terreno com o nome, retorna todos os terrenos com alguma planta
        /// </summary>
        /// <param name="cropName"></param>
        /// <returns></returns>
        public List<Plot> GetPlotsWithSpecificCrop(string cropName = "")
        {
            var allPlantedPlots = GetPlantedPlots();
            if (allPlantedPlots == null || allPlantedPlots.Count < 1) return null;

            var plotsWithDesiredCrop = allPlantedPlots.Where(p => p.currentCrop.CropName == cropName).ToList();

            return string.IsNullOrEmpty(cropName) || plotsWithDesiredCrop.Count < 1
                ? (allPlantedPlots ?? new())
                : plotsWithDesiredCrop;
                
        }

        public Plot GetClosestPlot(Vector3 position, string cropName = "")
        {
            var desirablePlots = GetPlotsWithSpecificCrop(cropName);

            if(desirablePlots == null || desirablePlots.Count < 1)
            {
                Debug.LogError("Lista de terrenos desejaveis ta vazia");
                return null;
            }

            List<Vector3> plotPositions = desirablePlots.Select(t => t.transform.position).ToList();

            int closestIndex = 0;

            float
            currentDestination,
            closestDestinationSoFar = (plotPositions[0] - position).sqrMagnitude; //a variavel começa como a distância entre o agente e a destinação[0].

            for (int i = 0; i < plotPositions.Count; i++)
            {

                currentDestination = (plotPositions[i] - position).sqrMagnitude;

                if (currentDestination < closestDestinationSoFar)
                {

                    closestDestinationSoFar = currentDestination;

                    closestIndex = i;
                }
            }
            return desirablePlots[closestIndex];
        }
    
        public void RemoveCritterFromList(CrittlingBehaviour crittling)
        {
            spawnedCrittlings.Remove(crittling);
        }
    }

    
}