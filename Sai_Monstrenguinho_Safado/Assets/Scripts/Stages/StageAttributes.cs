using Assets.Scripts.NPCScripts;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageAttributes
{
    public string stageName;
    public int stageID;
    public bool isTutorial;
    public bool isFinal;

    [System.Serializable]
    public class NpcAttributes
    {
        [Space(8f)]

        public List<ClientAttributes> SpawnableClients = new(3);

        [Space(8f), Header("Spawn"), Space(5f)]

        public bool PreSpawn = true;
        public float MinInterval = 50f;
        public float MaxInterval = 60f;
        private float currentInterval;
        public float Interval { get => currentInterval; set { currentInterval = value; } }
        private float timeRemaining;
        public float TimeRemaining { get => timeRemaining; set { timeRemaining = value; } }

        public bool ForceClientOnScreen;
        public float waitToForce = 5f;
        public bool waitForStageToEndBeforeSpawning;

        private float currentTimeWaitedWithNoClients;
        public float CurrentTimeWaitedWithNoClients { get => currentTimeWaitedWithNoClients; set => currentTimeWaitedWithNoClients = value; }
        private float totalTimeWaitedWithNoClients;
        public float TotalTimeWaitedWithNoClients { get => totalTimeWaitedWithNoClients; set => totalTimeWaitedWithNoClients = value; }
        private float totalTimeWaitedWithClients;
        public float TotalTimeWaitedWithClients { get => totalTimeWaitedWithClients; set => totalTimeWaitedWithClients = value; }

        private float timeTakenFromNextStage;
        public float TimeTakenFromNextStage { get => timeTakenFromNextStage; set => timeTakenFromNextStage = value; }

        public int SpawnCap = 4;
        private int spawnedAmount;
        public int SpawnedAmount { get => spawnedAmount; set => spawnedAmount = value; }
        private int clientsCurrentlyActive;
        public int ClientsRemaining { get => clientsCurrentlyActive; set => clientsCurrentlyActive = value; }

        private List<float> serviceTime;
        public List<float> ServiceTime { get => serviceTime; set => serviceTime = value; }
        private float averageServiceTime;
        public float AverageServiceTime { get => averageServiceTime; set => averageServiceTime = value; }

        public float AverageTime() => ServiceTime.Average();
    }

    [System.Serializable]
    public class EnemyAttributes
    {
        public List<GameObject> SpawnableCrittlings;
        [Space(8f), Header("Spawn"), Space(5f)]

        public float MinInterval = 15f;
        public float MaxInterval = 20f;
        private float currentInterval;
        public float Interval { get => currentInterval; set { currentInterval = value; } }
        private float timeRemaining;
        public float TimeRemaining { get => timeRemaining; set { timeRemaining = value; } }

        private float timeWaitedBetweenSpawns;
        public float TimeWaitedBetweenSpawns { get => timeWaitedBetweenSpawns; set => timeWaitedBetweenSpawns = value; }

        public int MinHorde = 1;
        public int MaxHorde = 2;
        public int SpawnCap = 6;
        private int spawnedAmount;
        public int SpawnedAmount { get => spawnedAmount; set => spawnedAmount = value; }
    }

    [System.Serializable]
    public class StageConditions
    {
        [Space(8f), Header("Condições de Passagem"), Space(8f)]

        public bool TimeBased;
        [Tooltip("Em segundos")]
        public float TimeToPass = 300;

        [Space(8f)]

        public bool SuccessBased = true;
        public int SuccessfulClientsToPass = 1;

        [Space(8f), Header("Estado da Condição"), Space(8f)]
        private float timeElapsed;
        public float TimeElapsed { get => timeElapsed; set { timeElapsed = value; } }
        private int successfulClientsPassed;
        public int SuccessfulClientsPassed { get => successfulClientsPassed; set { successfulClientsPassed = value; } }
        private int failedClientsPassed;
        public int FailedClientsPassed { get => failedClientsPassed; set { failedClientsPassed = value; } }
    }

    public NpcAttributes Clients;
    public EnemyAttributes Crittlings;
    public StageConditions Conditions;

    private float stageTime;
    public float StageTime { get => stageTime; set => stageTime = value; }

    [Space(8f)]

    public int nextStageID;
    public UnityEngine.Events.UnityEvent OnCompleted;

    public void Reset()
    {
        Conditions.TimeElapsed = 0;
        Conditions.SuccessfulClientsPassed = 0;
        Conditions.FailedClientsPassed = 0;
    }
}