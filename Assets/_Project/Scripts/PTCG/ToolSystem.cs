using System.Linq;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// どうぐ（ポケモンのどうぐ）装備システム
    /// </summary>
    public class ToolSystem : MonoBehaviour
    {
        public static ToolSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 手札からどうぐを装備
        /// </summary>
        /// <param name="player">プレイヤー</param>
        /// <param name="target">装備先のポケモン</param>
        /// <returns>装備成功したか</returns>
        public bool AttachToolFromHand(PlayerController player, PokemonInstance target)
        {
            if (target == null)
            {
                return false;
            }

            // すでにどうぐが付いているか
            if (target.attachedTool != null)
            {
                return false;
            }

            // 手札からポケモンのどうぐカードを探す
            var toolCard = player.hand.OfType<TrainerCardData>()
                .FirstOrDefault(t => t.trainerType == TrainerType.Tool);

            if (toolCard == null)
            {
                return false;
            }

            // 手札から削除してポケモンに装備
            player.hand.Remove(toolCard);
            target.attachedTool = toolCard;


            // いさぎのふんどし（BraveryCharm）効果
            // HP計算はPokemonInstance.MaxHPプロパティで自動処理されるため、ここでは何もしない

            return true;
        }

        /// <summary>
        /// 指定したどうぐカードを装備
        /// </summary>
        public bool AttachTool(PlayerController player, PokemonInstance target, TrainerCardData toolCard)
        {
            if (target == null)
            {
                return false;
            }

            if (toolCard.trainerType != TrainerType.Tool)
            {
                return false;
            }

            if (target.attachedTool != null)
            {
                return false;
            }

            if (!player.hand.Remove(toolCard))
            {
                return false;
            }

            target.attachedTool = toolCard;

            return true;
        }

        /// <summary>
        /// どうぐを取り外す（交換・KO時など）
        /// </summary>
        public bool RemoveTool(PlayerController player, PokemonInstance pokemon)
        {
            if (pokemon == null || pokemon.attachedTool == null)
            {
                return false;
            }

            var tool = pokemon.attachedTool;
            pokemon.attachedTool = null;

            // トラッシュへ
            player.discard.Add(tool);

            return true;
        }
    }
}
