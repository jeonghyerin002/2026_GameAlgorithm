using System.Collections.Generic;

namespace AlgoLesson6
{
    public sealed class PokerHandResult
    {
        public HandRank rank;
        public int baseChips;
        public int cardChips;
        public int multiplier;
        public int Score => (baseChips + cardChips) * multiplier;
        public List<CardData> cards = new List<CardData>();
        public string Name => rank switch
        {
            HandRank.StraightFlush => "스트레이트 플러시",
            HandRank.FourOfAKind => "포 카드",
            HandRank.FullHouse => "풀 하우스",
            HandRank.Flush => "플러시",
            HandRank.Straight => "스트레이트",
            HandRank.ThreeOfAKind => "트리플",
            HandRank.TwoPair => "투 페어",
            HandRank.Pair => "원 페어",
            _ => "하이 카드"
        };
    }
}
