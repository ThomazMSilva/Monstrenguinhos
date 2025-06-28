using UnityEngine;

namespace Assets.Scripts.NPCScripts
{
    [CreateAssetMenu(fileName = "ClientAttributes", menuName = "Client Attributes")]
    public class ClientAttributes : ScriptableObject
    {
        [Tooltip("Isso aqui nem eh usado, so acho divertido dar nominho pra cada kkkkk")]
        [SerializeField] private string clientName;

        [Tooltip("Tempo mínimo base do cálculo de quanto o Cliente tolera esperar, depois que é atendido.\nO valor final é um número randomizado entre o mínimo e o máximo, multiplicado pelo multiplicador atual da fase (pra diminuir a tolerância geral com o tempo).")]
        [SerializeField] private float baseToleranceTimeMin = 30f;
        
        [Tooltip("Tempo máximo base do cálculo de quanto o Cliente tolera esperar, depois que é atendido.\nO valor final é um número randomizado entre o mínimo e o máximo, multiplicado pelo multiplicador atual da fase  (pra diminuir a tolerância geral com o tempo).")]
        [SerializeField] private float baseToleranceTimeMax = 45f;

        [SerializeField] private int cropsOrderedMin = 1;
        [SerializeField] private int cropsOrderedMax = 2;

        [SerializeField] private Interactibles.CropType cropPreference;

        public float RandomizedToleranceTime => Random.Range(baseToleranceTimeMin, baseToleranceTimeMax);

        public static int WeightedRandomByPercentage(int minInlcusive, int maxInclusive, int preference, float percentage)
        {
            if (minInlcusive > maxInclusive)
            {
                (minInlcusive, maxInclusive) = (maxInclusive, minInlcusive);
            }

            percentage = Mathf.Clamp(percentage, 0f, 100f);

            if (minInlcusive == maxInclusive) return minInlcusive;
            if (preference < minInlcusive || preference > maxInclusive) return Random.Range(minInlcusive, maxInclusive + 1);

            float preferenceProbability = percentage / 100f;
            float randomValue = Random.value;

            if (randomValue < preferenceProbability)
            {
                return preference;
            }

            int totalOtherValues = maxInclusive - minInlcusive;
            //float otherValueProbability = (1f - preferenceProbability) / totalOtherValues;

            float adjustedRandom = (randomValue - preferenceProbability) / (1f - preferenceProbability);
            int selectedIndex = Mathf.FloorToInt(adjustedRandom * totalOtherValues);

            return selectedIndex < (preference - minInlcusive)
                ? minInlcusive + selectedIndex
                : minInlcusive + selectedIndex + 1;
        }
    }
}