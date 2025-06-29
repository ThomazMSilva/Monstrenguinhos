using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    [CreateAssetMenu(fileName = "ClientAttributes", menuName = "Client Attributes")]
    public class ClientAttributes : ScriptableObject
    {
        [Tooltip("Isso aqui nem eh usado, so acho divertido dar nominho pra cada kkkkk")]
        [SerializeField] private string clientName;
        public string ClientName => clientName;
        
        [Tooltip("Tempo base do cálculo de quanto o Cliente tolera esperar, depois que é atendido.\nO valor final é um número randomizado entre o mínimo e o máximo, multiplicado pelo multiplicador atual da fase (pra diminuir a tolerância geral com o tempo).")]
        [SerializeField] private float 
        baseToleranceTimeMin = 30f,
        baseToleranceTimeMax = 45f;
        public float RandomizedToleranceTime => Random.Range(baseToleranceTimeMin, baseToleranceTimeMax);

        [SerializeField] private int cropsOrderedMin = 1;
        [SerializeField] private int cropsOrderedMax = 2;
        public int RandomizedOrderAmount => Random.Range(cropsOrderedMin, cropsOrderedMax + 1);

        [SerializeField] private Interactibles.CropType cropPreference;
        public Interactibles.CropType Preference => cropPreference;
    }
}