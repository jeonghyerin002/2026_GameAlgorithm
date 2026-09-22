using System.Collections.Generic;
using AlgoCourse.Lesson3;
using UnityEngine.Rendering;

namespace AlgoCourse.StudentWork
{
    public sealed class PandemicInfectionAlgorithm : ICityInfectionAlgorithm
    {
        // TODO 01: 도시 그래프, 감염 단계, 감염 요청 Queue를 생성합니다.
        // TODO 02: 한 번의 연쇄 감염에서 Outbreak한 도시를 기록합니다.

        private const int MaximumInfectionLevel = 3; //한 도시가 가질 수 있는 최대 감염 단계를 3으로 지정

        private readonly Dictionary<int, int[]> graph = new Dictionary<int, int[]>(); //도시 번호를 key로 하고 연결된 이웃 됫 목록을 value로 저장
        private readonly Dictionary<int, int> infectionLevels = new Dictionary<int, int>(); //각 도시 번호별 현재 감염 단계를 저장
        private readonly Queue<int> infectionQueue = new Queue<int>();
        private readonly HashSet<int> outbreakCities = new HashSet<int>();
        public int PendingCount => 0;
        public int OutbreakCount { get; private set; }

        public void Initialize(IReadOnlyDictionary<int, int[]> cityGraph)
        {
            graph.Clear(); //도시 연결 정보를 모두 삭제
            infectionLevels.Clear(); //감염 단계 정보 모두 삭제
            infectionQueue.Clear(); //감염 대기 큐도 초기화
            outbreakCities.Clear(); //아웃브레이크 도시 기록을 삭제

            OutbreakCount = 0;

            //모든 도시 정보를 하나씩 확인
            foreach(KeyValuePair<int, int[]> city in cityGraph)
            {
                graph.Add(city.Key, city.Value); //독 번호와 해당 도시의 이웃 도시 목록을 그래프에 저장
                infectionLevels.Add(city.Key, 0); //해당 도시의 초기 감염 단계를 0으로 설정
            }
        }

        public bool QueueInfection(int cityId) //지정한 도시를 감염 처리 대기 큐에 추가
        {
            if (!graph.ContainsKey(cityId)) //그래프에 존재하지 않는 도시 번호인지 확인
            {
                return false; //존재하지 않는 도시라면 감염 요청에 실패했음을 반환
            }
            if (infectionQueue.Count == 0) //현재 감염 큐가 비어있다면 새로운 연쇄 감염이 시작되는 상태
            {
                outbreakCities.Clear(); //새로운 연쇄 감염을 위해 이전 아웃 브레이크 도시 기록을 초기화
            }
            infectionQueue.Enqueue(cityId); //지정한 도시를 감염 처리 대기 큐의 마지막에 추가

            return false; //정상적으로 감염 요청이 등록되었음을 반환
        }

        public CityInfectionStep ProcessNext()
        {
            if (infectionQueue.Count == 0) //처리할 도시가 감염 큐에 남아있는지 확인
            { 
                return new CityInfectionStep(-1, 0, false, false); //처리할 도시가 없다면 아무 변화 없는 결과를 반황한다.
            }

            int cityId = infectionQueue.Dequeue(); //큐에서 가장 먼저 돌아온 도시 하나를 꺼냅니다
            int currentLevel = infectionLevels[cityId];//해당 도시의 감염 단계를 가져온다

            if (currentLevel < MaximumInfectionLevel)
            {
                int nextLevel = currentLevel + 1;
                infectionLevels[cityId] = nextLevel;
                return new CityInfectionStep(cityId, currentLevel, false, true); //감염 단게가 증가했고, 아웃 브레이크는 발생하지 않았다는 결과를 반환
            }
            if (!outbreakCities.Add(cityId)) //이미 이번 연쇄 감염에서 아웃브레이크가 발생했던 도시인지 확인
            {
                return new CityInfectionStep(cityId, currentLevel, false, false); //발생한 도시라면 추가 확산 없음
            }
            OutbreakCount++; //새로운 아웃브레이크가 발생했으므로 천체 발생 횟수를 1 증가

            foreach (int neighborId in graph[cityId]) //도시에 연결된 모든 이웃 도시를 확인
            {
                infectionQueue.Enqueue(neighborId); //각 이웃 도시를 감염 처리 대기 큐에 추가
            }

            return new CityInfectionStep(cityId, currentLevel, true, true); //현재 도시에서 아웃브레이크가 발생하고 상태가 변경되었다는 결과를 반환
        }

        public int GetInfectionLevel(int cityId)
        {
            return infectionLevels.TryGetValue(cityId, out int  level) ? level : 0; //도시가 존재하면 감염 단계를 반환하고 존재하지 않으면 0을 반환
        }
    }
}
