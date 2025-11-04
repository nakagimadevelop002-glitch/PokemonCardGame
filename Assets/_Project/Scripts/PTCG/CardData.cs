using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// カードデータの基底クラス（ScriptableObject）
    /// </summary>
    public abstract class CardData : ScriptableObject
    {
        [Header("Basic Info")]
        public string cardID;
        public string cardName;
        public CardType cardType;

        [Header("Visual")]
        public Sprite cardArt;

        public virtual string GetDisplayName()
        {
            return cardName;
        }
    }
}
