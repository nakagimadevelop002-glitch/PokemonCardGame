using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// エネルギーカードのデータ定義
    /// </summary>
    [CreateAssetMenu(fileName = "EnergyCard", menuName = "PTCG/Energy Card")]
    public class EnergyCardData : CardData
    {
        [Header("Energy Info")]
        public bool isBasic;
        public PokemonType providesType;
        public int providesAmount = 1;

        [Header("Special Energy")]
        public bool isSpecial;
        [TextArea(2, 5)]
        public string specialEffectDescription;

        public override string GetDisplayName()
        {
            return isBasic ? $"基本{cardName}エネルギー" : cardName;
        }
    }
}
