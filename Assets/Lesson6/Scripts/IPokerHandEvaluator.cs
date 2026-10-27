using System.Collections.Generic;

namespace AlgoLesson6
{
    public interface IPokerHandEvaluator
    {
        PokerHandResult Evaluate(IReadOnlyList<CardData> cards);
    }
}
