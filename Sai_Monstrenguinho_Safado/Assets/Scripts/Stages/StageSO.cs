using System.Collections;
using UnityEngine;

namespace Assets.Scripts.ManagerScripts
{
    [CreateAssetMenu(fileName ="stages",menuName ="Stage")]
    public class StageSO : ScriptableObject
    {
        public System.Collections.Generic.List<StageAttributes> attributes = new(1);
    }
}