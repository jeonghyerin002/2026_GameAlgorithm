using System.Collections.Generic;

namespace AlgoLesson6
{
    /// <summary>
    /// 손패에서 1~5장 조합을 모두 확인해 가장 높은 점수를 찾습니다.
    /// 학생은 재귀와 백트래킹으로 Search 메서드를 완성합니다.
    /// </summary>
    public sealed class BestHandAdvisor
    {
        private readonly IPokerHandEvaluator evaluator;

        public BestHandAdvisor(IPokerHandEvaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        public PokerHandResult FindBest(IReadOnlyList<CardData> hand)
        {
            PokerHandResult best = new PokerHandResult();

            if (hand == null || hand.Count == 0)
                return best;

            List<CardData> selected = new List<CardData>();

            // TODO 09: Search를 호출해 1~5장 조합을 모두 탐색하세요.

            return best;
        }

        private void Search(
            IReadOnlyList<CardData> hand,
            int start,
            int maxCount,
            List<CardData> selected,
            ref PokerHandResult best)
        {
            // TODO 10: 현재 selected가 비어 있지 않으면 점수를 평가하세요.

            // TODO 11: 5장을 선택했으면 재귀를 종료하세요.

            // TODO 12: start부터 손패 끝까지 카드를 하나씩 선택하세요.

            // TODO 13: 다음 인덱스부터 Search를 재귀 호출하세요.

            // TODO 14: 재귀가 끝나면 마지막 카드를 제거해 상태를 복원하세요.
        }
    }
}
