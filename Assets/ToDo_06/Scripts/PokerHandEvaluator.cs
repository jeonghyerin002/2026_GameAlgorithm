using System.Collections.Generic;
using System.Linq;

namespace AlgoLesson6
{
    /// <summary>
    /// 학생이 직접 완성하는 포커 족보 판정기입니다.
    /// 시작 상태에서도 Unity가 컴파일되도록 모든 메서드는 안전한 기본값을 반환합니다.
    /// </summary>
    public sealed class PokerHandEvaluator : IPokerHandEvaluator
    {
        public PokerHandResult Evaluate(IReadOnlyList<CardData> cards)
        {
            List<CardData> safeCards = cards == null
                ? new List<CardData>()
                : cards.ToList();

            // TODO 01: BuildRankCounts를 호출해 rank별 장수를 구하세요.
            Dictionary<int, int> rankCounts = BuildRankCounts(safeCards);

            // TODO 02: IsFlush를 호출해 같은 무늬 5장인지 확인하세요.
            bool isFlush = IsFlush(safeCards);

            // TODO 03: IsStraight를 호출해 연속된 숫자 5장인지 확인하세요.
            bool isStraight = IsStraight(safeCards);

            // TODO 04: 가장 많이 겹친 rank의 장수와 Pair 개수를 구하세요.
            int maxSameRank = rankCounts.Count == 0 ? 0 : rankCounts.Values.Max();
            int pairCount = rankCounts.Values.Count(value => value == 2);

            // TODO 05: DetermineRank 안에 강한 족보부터 판정하는 조건을 작성하세요.
            HandRank rank = DetermineRank(
                safeCards.Count,
                isFlush,
                isStraight,
                maxSameRank,
                pairCount);

            // TODO 06: 선택 카드의 ChipValue 합을 구하세요.
            int cardChips = safeCards.Sum(card => card.ChipValue);
            GetBaseScore(rank, out int baseChips, out int multiplier);

            return new PokerHandResult
            {
                rank = rank,
                baseChips = baseChips,
                cardChips = cardChips,
                multiplier = multiplier,
                cards = safeCards
            };
        }

        private Dictionary<int, int> BuildRankCounts(List<CardData> cards)
        {
            // TODO 01 구현 위치: Dictionary에 rank별 등장 횟수를 저장하세요.
            Dictionary<int, int> counts = new Dictionary<int, int>();

            foreach (CardData card in cards)
            {
                if (counts.ContainsKey(card.rank))
                {
                    counts[card.rank]++;
                }
                else
                {
                    counts.Add(card.rank, 1);
                }
            }
            return counts;
        }

        private bool IsFlush(List<CardData> cards)
        {
            // TODO 02 구현 위치: 정확히 5장인지 먼저 확인하세요.
            if (cards.Count != 5) return false;

            CardSuit suit = cards[0].suit;

            return cards.All(card => card.suit == suit);
        }

        private bool IsStraight(List<CardData> cards)
        {
            if (cards.Count != 5) return false;
            // TODO 03 구현 위치: 중복 제거 후 오름차순으로 정렬하세요.
            List<int> ranks = cards.Select(card => card.rank).Distinct().OrderBy(rank => rank).ToList();
            if(ranks.Count != 5) return false;
            // TODO 07: A, 2, 3, 4, 5를 별도의 Straight로 처리하세요.
            if (ranks.SequenceEqual(new[] { 2, 3, 4, 5, 14 })) return true;

            for (int i = 0; i < ranks.Count; i++)
            {
                if (ranks[i] != ranks[0] + i)
                    return false;
            }

            return true;
        }

        private HandRank DetermineRank(
            int count,
            bool flush,
            bool straight,
            int maxSame,
            int pairs)
        {
            // TODO 05 구현 위치: 강한 족보부터 순서대로 검사하세요.
            
            if (count == 5 && straight && flush) return HandRank.StraightFlush;
            if (maxSame == 4) return HandRank.FourOfAKind;
            if (count == 5 && maxSame == 3 && pairs == 1) return HandRank.FullHouse;
            if (count == 5 && flush) return HandRank.Flush;
            if (count == 5 && straight) return HandRank.Straight;
            if (maxSame == 3) return HandRank.ThreeOfAKind;
            if (pairs >= 2) return HandRank.TwoPair;
            if (pairs == 1) return HandRank.Pair;

            return HandRank.HighCard;
        }

        private void GetBaseScore(
            HandRank rank,
            out int chips,
            out int multiplier)
        {
            // TODO 08: 족보별 기본 Chips와 Multiplier를 연결하세요.
            (chips, multiplier) = rank switch
            {
                HandRank.StraightFlush => (100, 8),
                HandRank.FourOfAKind => (60, 7),
                HandRank.FullHouse => (40, 4),
                HandRank.Flush => (35, 4),
                HandRank.Straight => (30, 4),
                HandRank.ThreeOfAKind => (30, 3),
                HandRank.TwoPair => (20, 2),
                HandRank.Pair => (10, 2),
                _ => (5, 1)
            };
        }
    }
}
