using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AStar
{
    // A* 알고리즘에 사용되는 우선순위 큐(Priority Queue)를 구현한 클래스
    public class PriorityQueue
    {
        private List<Tile> _elements = new List<Tile>();

        public int Count => _elements.Count;

        // 새로운 타일을 우선순위 큐에 추가하는 함수
        public void Enqueue(Tile item)
        {
            _elements.Add(item); // 리스트 끝에 새로운 타일을 추가
            var childIndex = _elements.Count - 1;
            while (childIndex > 0)
            {
                var parentIndex = (childIndex - 1) / 2;

                if (_elements[childIndex].CompareTo(_elements[parentIndex]) >= 0) break;

                // 부모와 자식 위치를 교환
                (_elements[childIndex], _elements[parentIndex]) = (_elements[parentIndex], _elements[childIndex]);

                // 새로운 부모를 대상으로 계속 위로 올라감
                childIndex = parentIndex;
            }
        }

        // 우선순위 큐에서 가장 작은 우선순위를 가진 타일을 제거하고 반환하는 함수
        public Tile Dequeue()
        {
            if (_elements.Count == 0) throw new Exception("Custom PQ is empty.");

            int lastIndex = _elements.Count - 1;
            Tile firstItem = _elements[0]; // 최소값
            _elements[0] = _elements[lastIndex]; // 마지막 노드를 루트로 옮김
            _elements.RemoveAt(lastIndex);
            --lastIndex;

            int parentIndex = 0;
            while (true)
            {
                int leftChildIndex = parentIndex * 2 + 1;
                int rightChildIndex = parentIndex * 2 + 2;

                // 자식이 없으면 종료
                if (leftChildIndex > lastIndex) break;

                // 오른쪽 자식이 존재하고 F가 더 작으면 오른쪽 자식을 선택
                int smallestChildIndex = leftChildIndex;
                if (rightChildIndex <= lastIndex && _elements[rightChildIndex].CompareTo(_elements[leftChildIndex]) <= 0)
                {
                    smallestChildIndex = rightChildIndex;
                }

                // 부모가 자식보다 작거나 같으면 종료
                if (_elements[smallestChildIndex].CompareTo(_elements[parentIndex]) >= 0) break;

                // 부모와 가장 작은 자식의 위치를 교환
                (_elements[parentIndex], _elements[smallestChildIndex]) = (_elements[smallestChildIndex], _elements[parentIndex]);

                // 다음 단계로 진행
                parentIndex = smallestChildIndex;
            }

            return firstItem; // 가장 작은 값을 반환
        }

        // 큐에서 가장 작은 우선순위를 가진 타일을 제거하지 않고 반환하는 함수
        public Tile Peek()
        {
            if (_elements.Count == 0) throw new Exception("Custom PQ is empty.");
            return _elements[0];
        }

        // 큐 안에 특정 타일이 포함되어 있는지 확인하는 함수
        public bool Contains(Tile item)
        {
            return _elements.Contains(item);
        }

        // 큐에 있는 모든 타일을 제거하는 함수
        public void Clear()
        {
            _elements.Clear();
        }
    }
}
