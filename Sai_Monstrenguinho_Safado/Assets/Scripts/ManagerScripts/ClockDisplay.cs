using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ManagerScripts
{
    [System.Serializable]
    public class ClockHandAttributes
    {
        public string ClockHandName;
        public Transform transform;
        private Vector3 originalRotation;
        public Vector3 OriginalRotation { get => originalRotation; set => originalRotation = value; }
        private float maximumValue;
        public float MaximumValue { get => maximumValue; set => maximumValue = value; }
        private float currentZRotation;
        public float CurrentZRotation { get => currentZRotation; set => currentZRotation = value; }
        public float multiplier = 1f;

        [System.Serializable]
        public enum RotationCondition
        {
            AllTimeBased,
            AllSuccessBased,
            StageTimeBased,
            StageSuccessBased,
            CliemtSpawnBased,
            CrittlingSpawnBased
        }
        public List<RotationCondition> rotationConditions = new();

        public float HighestRotation(StageAttributes currentStage)
        {
            float highestValue = 0;

            var clients = currentStage.Clients;
            var conditions = currentStage.Conditions;
            var critters = currentStage.Crittlings;

            List<StageAttributes> allStages = new();

            if (rotationConditions.Contains(RotationCondition.AllTimeBased) || rotationConditions.Contains(RotationCondition.AllSuccessBased))
                allStages = GameManager.Instance.StageAttributes;

            foreach(var requiredCondition in rotationConditions)
            {
                switch (requiredCondition)
                {
                    case RotationCondition.StageTimeBased:
                        float rotationStepTime = 1 / conditions.TimeToPass;
                        var currentRotationTime = Mathf.Lerp(0, 360 * multiplier, conditions.TimeElapsed * rotationStepTime);
                        
                        if (currentRotationTime > highestValue) 
                            highestValue = currentRotationTime;
                        break;

                    case RotationCondition.StageSuccessBased:
                        var rotationStepSuccess = 1 / conditions.SuccessfulClientsToPass;
                        var currentRotationSuccess = Mathf.Lerp(0, 360 * multiplier, conditions.SuccessfulClientsPassed * rotationStepSuccess);

                        if (currentRotationSuccess > highestValue)
                            highestValue = currentRotationSuccess;
                        break;

                    case RotationCondition.CliemtSpawnBased:
                        float rotationStopClient = 1 / clients.Interval;
                        var currentRotationClient = Mathf.Lerp(0, 360, 1 - (clients.TimeRemaining * rotationStopClient));

                        if(currentRotationClient > highestValue)
                            highestValue = currentRotationClient;

                        break;

                    case RotationCondition.CrittlingSpawnBased:
                        float rotationStopCritter = 1 / clients.Interval;
                        var currentRotationCritter = Mathf.Lerp(0, 360, 1 - (critters.TimeRemaining * rotationStopCritter));

                        if (currentRotationCritter > highestValue)
                            highestValue = currentRotationCritter;
                        break;
                
                    case RotationCondition.AllTimeBased:
                        float allTimes = 0;
                        float allTimeElapsed = 0;
                        foreach(var stage in allStages)
                        {
                            allTimes += stage.Conditions.TimeToPass;
                            allTimeElapsed += stage.Conditions.TimeElapsed;
                        }
                        float allTimeStep = 1 / allTimes;
                        var currentRotationAllTime = Mathf.Lerp(0, 360 * multiplier, allTimeElapsed * allTimeStep);

                        if (currentRotationAllTime > highestValue)
                            highestValue = currentRotationAllTime;

                        break;

                    case RotationCondition.AllSuccessBased:
                        int allSuccesses = 0;
                        float allSuccessesElapsed = 0;
                        foreach (var stage in allStages)
                        {
                            allSuccesses += stage.Conditions.SuccessfulClientsToPass;
                            allSuccessesElapsed += stage.Conditions.SuccessfulClientsPassed;
                        }
                        float allSuccessStep = 1 / allSuccesses;
                        var currentRotationAllSuccess = Mathf.Lerp(0, 360 * multiplier, allSuccessesElapsed * allSuccessStep);

                        if(currentRotationAllSuccess > highestValue)
                            highestValue = currentRotationAllSuccess;

                        break;

                }

            }
            return highestValue;
        }
    }

    public class ClockDisplay : MonoBehaviour
    {
        [SerializeField] private List<ClockHandAttributes> clockHands = new();
        private GameManager game;

        void Start()
        {
            game = GameManager.Instance;

            foreach (var hand in clockHands) { hand.OriginalRotation = hand.transform.eulerAngles; }
        }

        void LateUpdate()
        {
            var conditions = game.CurrentStage.Conditions;
            var clients = game.CurrentStage.Clients;

            foreach (var hand in clockHands)
            {
                hand.transform.rotation = Quaternion.Euler
                (
                    hand.OriginalRotation.x,
                    hand.OriginalRotation.y,
                    hand.HighestRotation(game.CurrentStage)
                );
            }
        }
    }
}