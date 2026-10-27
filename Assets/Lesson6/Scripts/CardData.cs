using System;

namespace AlgoLesson6
{
    public enum CardSuit { Spades, Hearts, Diamonds, Clubs }
    public enum HandRank { HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush }

    [Serializable]
    public readonly struct CardData
    {
        public readonly int rank;
        public readonly CardSuit suit;
        public CardData(int rank, CardSuit suit) { this.rank = rank; this.suit = suit; }
        public int ChipValue => rank == 14 ? 11 : rank >= 10 ? 10 : rank;
        public string RankText => rank switch { 14 => "A", 13 => "K", 12 => "Q", 11 => "J", _ => rank.ToString() };
        public string SuitText => suit switch { CardSuit.Spades => "♠", CardSuit.Hearts => "♥", CardSuit.Diamonds => "♦", _ => "♣" };
        public override string ToString() => RankText + SuitText;
    }
}
