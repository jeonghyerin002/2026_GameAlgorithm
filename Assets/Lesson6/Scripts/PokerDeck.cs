using System.Collections.Generic;
using UnityEngine;

namespace AlgoLesson6
{
    public sealed class PokerDeck
    {
        private readonly List<CardData> cards = new List<CardData>();
        private int drawIndex;
        public int Remaining => cards.Count - drawIndex;

        public void ResetAndShuffle(int seed)
        {
            cards.Clear();
            for (int suit = 0; suit < 4; suit++)
                for (int rank = 2; rank <= 14; rank++)
                    cards.Add(new CardData(rank, (CardSuit)suit));

            Random.InitState(seed);
            // Fisher–Yates: 아직 섞지 않은 범위에서 하나를 골라 뒤와 교환합니다.
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
            drawIndex = 0;
        }

        public bool TryDraw(out CardData card)
        {
            if (drawIndex >= cards.Count) { card = default; return false; }
            card = cards[drawIndex++];
            return true;
        }
    }
}
