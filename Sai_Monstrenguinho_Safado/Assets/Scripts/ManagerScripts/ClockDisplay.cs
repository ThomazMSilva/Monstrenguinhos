using System.Collections;
using UnityEngine;

namespace Assets.Scripts.ManagerScripts
{
    public class ClockDisplay : MonoBehaviour
    {
        [SerializeField] private Transform clockHand1;
        [SerializeField] private Transform clockHand2;
        [SerializeField] private Transform clockHand3;
        public float multiplier = 1;
        private Vector3 hand1Rotation;
        private Vector3 hand2Rotation;
        private Vector3 hand3Rotation;

        private float timeBasedRotationZ;
        private float successBasedRotationZ;
        private float maximumTime;
        private float maximumSuccess;
        private GameManager game;

        void Start()
        {
            game = GameManager.Instance;
            hand1Rotation = clockHand1.eulerAngles;
            hand2Rotation = clockHand2.eulerAngles;
            hand3Rotation = clockHand3.eulerAngles;
        }

        void LateUpdate()
        {
            var conditions = game.CurrentStage.Conditions;
            var clients = game.CurrentStage.Clients;

            //time based
            maximumTime = 1 / conditions.TimeToPass;
            timeBasedRotationZ = Mathf.Lerp(0, 360 * multiplier, conditions.TimeElapsed * maximumTime);
            clockHand1.rotation = Quaternion.Euler(hand1Rotation.x, hand1Rotation.y, timeBasedRotationZ);
            //clockHand2.rotation = Quaternion.Euler(hand2Rotation.x, hand2Rotation.y, timeBasedRotationZ * 60);

            //until client
            var maximumInterval = 1 / clients.Interval;
            var npcIntervalRotationZ = Mathf.Lerp(0, 360, 1 - (clients.TimeRemaining / maximumInterval));
            clockHand2.rotation = Quaternion.Euler(hand2Rotation.x, hand2Rotation.y, npcIntervalRotationZ);

            //success based
            maximumSuccess = 1 / conditions.SuccessfulClientsToPass;
            successBasedRotationZ = Mathf.Lerp(0, 360 * multiplier, conditions.SuccessfulClientsPassed * maximumTime);
            clockHand3.rotation = Quaternion.Euler(hand3Rotation.x, hand3Rotation.y, successBasedRotationZ);
        }
    }
}