using Assets.Scripts.Interactibles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ClittlingScripts
{
    public class CrittlingsManager : MonoBehaviour
    {
        [SerializeField] private Transform crittlingParent;

        [SerializeField] private List<Transform> spawnablePositions = new();
        [SerializeField] private List<Plot> landPlots = new();
        private List<CrittlingBehaviour> spawnedCrittlings = new();

        public bool isSpawning = true;

        [SerializeField] private float plantedPlotCheckInterval = 1f;
        private WaitForSeconds WaitForPlantedPlot;

        [SerializeField] private float updateTime = .2f;
        private Coroutine spawnCrittlingRoutine;
        private Coroutine updateCrittlingRoutine;

        private GameManager game;
        private bool isGamePaused;
        private WaitUntil waitUntilGameUnpauses;

        private void Start()
        {
            /*SpawnCrittling();
            SpawnCrittling();*/
            game = GameManager.Instance;
            game.OnPause += PauseBehaviour;
            game.OnRestart += ReturnAllCrittlings;

            WaitForPlantedPlot = new(plantedPlotCheckInterval);
            spawnCrittlingRoutine = StartCoroutine(CrittlingSpawnHandler());
            updateCrittlingRoutine = StartCoroutine(UpdateCrittlingBehaviours());
        }

        private void OnDisable()
        {
            game.OnPause -= PauseBehaviour;
            game.OnRestart -= ReturnAllCrittlings;
        }

        private void PauseBehaviour(bool pause)
        {
            isGamePaused = pause;

            if(pause) waitUntilGameUnpauses = new(() => !isGamePaused);

        }

        private System.Collections.IEnumerator CrittlingSpawnHandler()
        {
            while (true)
            {
                if (isGamePaused) yield return waitUntilGameUnpauses; //slk

                if(!isSpawning) yield break;

                game.CurrentStage.Crittlings.Interval = Random.Range(game.CurrentStage.Crittlings.MinInterval, game.CurrentStage.Crittlings.MaxInterval);
                game.CurrentStage.Crittlings.TimeRemaining = game.CurrentStage.Crittlings.Interval;

                while (game.CurrentStage.Crittlings.TimeRemaining > 0)
                {
                    if (isGamePaused) yield return waitUntilGameUnpauses; //mano so taquei em tudo isso
                    game.CurrentStage.Crittlings.TimeRemaining -= Time.deltaTime;
                    yield return null;
                }

                Debug.Log("Terminou intervalo de spawn de monstrenguinho");
                var plantedPlots = GetPlantedPlots();
                while (plantedPlots == null || spawnedCrittlings.Count > game.CurrentStage.Crittlings.SpawnCap)
                {
                    if (isGamePaused) yield return waitUntilGameUnpauses; //mano so taquei em tudo isso

                    //Debug.Log("Aguardando condições...");
                    plantedPlots = GetPlantedPlots();
                    yield return WaitForPlantedPlot;
                }

                if (isGamePaused) yield return waitUntilGameUnpauses; //aqui tb

                var spawnAmount = Random.Range(game.CurrentStage.Crittlings.MinHorde, game.CurrentStage.Crittlings.MaxHorde + 1);

                for (int i = 0; i < spawnAmount; i++)
                {
                    //Debug.Log("Spawnando bicho");
                    SpawnCrittling();
                }
            }
        }

        private void SpawnCrittling()
        {
            int possibleCrittlingsCount = game.CurrentStage.Crittlings.SpawnableCrittlings.Count;

            if (possibleCrittlingsCount <= 0) return;
            
            int spawnIndex = Random.Range(0, possibleCrittlingsCount);

            int positionIndex = Random.Range(0, spawnablePositions.Count);
            
            var crittlingPrefab = game.CurrentStage.Crittlings.SpawnableCrittlings[spawnIndex];

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
                if (isGamePaused) yield return waitUntilGameUnpauses;

                yield return new WaitForSeconds(updateTime);

                foreach(var crittling in spawnedCrittlings)
                {
                    crittling.UpdateBehaviour();
                }
            }
        }

        private void ReturnAllCrittlings()
        {
            foreach(var critl in spawnedCrittlings)
            {
                critl.ScareAway();
            }
        } 

        public List<Plot> GetPlantedPlots()
        {
            var plantedPlots = landPlots.Where(p => p.currentCrop != null && p.currentCrop.IsPlanted).ToList();
            return plantedPlots.Count > 0 ? plantedPlots : null;
        }

        /// <summary>
        /// Retorna todos os terrenos com a colheita com o nome designado. <br></br>
        /// Se não acha nenhum terreno com o nome, retorna todos os terrenos com alguma planta
        /// </summary>
        /// <param name="cropName"></param>
        /// <returns></returns>
        public List<Plot> GetPlotsWithSpecificCrop(CropType cropName = CropType.None)
        {
            var allPlantedPlots = GetPlantedPlots();
            if (allPlantedPlots == null || allPlantedPlots.Count < 1) return null;

            var plotsWithDesiredCrop = allPlantedPlots.Where(p => p.currentCrop.CropName == cropName).ToList();

            return cropName == CropType.None || plotsWithDesiredCrop.Count < 1
                ? (allPlantedPlots ?? new())
                : plotsWithDesiredCrop;
                
        }

        public Plot GetClosestPlot(Vector3 position, CropType cropName = CropType.None)
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