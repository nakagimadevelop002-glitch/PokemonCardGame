using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// トレーナーカードのデータ定義
    /// </summary>
    [CreateAssetMenu(fileName = "TrainerCard", menuName = "PTCG/Trainer Card")]
    public class TrainerCardData : CardData
    {
        [Header("Trainer Info")]
        public TrainerType trainerType;

        [Header("Effect")]
        public string effectID; // for code lookup
        [TextArea(3, 10)]
        public string effectDescription;
    }
}
