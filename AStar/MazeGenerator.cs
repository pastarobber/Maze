using System;
using System.Collections.Generic;
using System.Drawing;

namespace AStar
{
    public class MazeGenerator
    {
        private List<Tile> _tiles; // 타일 목록
        private Random _random; // 랜덤 객체
        private int _width; // 미로 너비
        private int _height; // 미로 높이

        // 생성자: 타일 목록과 미로의 크기를 받아옴
        public MazeGenerator(List<Tile> tiles, int height, int width)
        {
            _tiles = tiles; // 미로를 구성할 타일 리스트를 받아옵니다.
            _height = height;
            _width = width;
            _random = new Random(); // _random: 랜덤 객체를 초기화합니다.
        }

        // 미로 생성 메서드
        public void GenerateMaze(out Tile start, out Tile end)
        {
            // 모든 타일을 벽으로 초기화
            foreach (var tile in _tiles)
            {
                tile.IsBlock = true; // 모든 타일을 벽으로 설정
            }

            // 미로 생성 시작
            CarveMaze(1, 1); // (1, 1)에서 시작, 태두리는 경계(벽)임.

            start = _tiles[1*_height + 1];//왼쪽위
            end = _tiles[_height * _width - _height - 2];//오른쪽 아래
        }

        // 미로 길뚫기 메서드
        private void CarveMaze(int x, int y)
        {
            // 이동할 방향: 오른쪽, 아래, 왼쪽, 위
            //물론 랜덤하게 섞음.
            var directions = new Point[]
            {
                new Point(2, 0), // 오른
                new Point(0, 2), // 아래
                new Point(-2, 0), // 왼
                new Point(0, -2) // 위
            };

            // 방향 무작위 섞기
            Shuffle(directions);

            // 각 방향으로 경로 조각내기
            foreach (var dir in directions)
            {
                int newX = x + dir.X; // 새로운 X 좌표
                int newY = y + dir.Y; // 새로운 Y 좌표

                // 새로운 위치가 경계 안에 있고 이미 조각내지 않은 경우
                if (IsInBounds(newX, newY) && IsTileBlocked(newX, newY))
                {
                    // 경로로 만들기
                    CarvePath(x, y, newX, newY);
                    //백트레킹, 재귀로 미로 생성
                    CarveMaze(newX, newY);
                }
            }
        }

        // 두 타일 사이의 통로를 뚫는 메서드
        private void CarvePath(int fromX, int fromY, int toX, int toY)
        {
            // 두 타일 사이의 중간 점 계산
            int midX = (fromX + toX) / 2; // 중간 X 좌표
            int midY = (fromY + toY) / 2; // 중간 Y 좌표

            // 경로 타일을 장애물(벽)이 아니도록 설정
            GetTile(fromX, fromY).IsBlock = false; // 현재 타일
            GetTile(midX, midY).IsBlock = false; // 중간 타일
            GetTile(toX, toY).IsBlock = false; // 목적지 타일
        }

        // 주어진 좌표의 타일을 반환하는 메서드
        // GetTile 메서드를 사용하면 타일 리스트 _tiles에서 특정 좌표에 해당하는 타일을 쉽게 찾을 수 있습니다. 
        private Tile GetTile(int x, int y)
        {
            return _tiles.Find(t => t.X == x && t.Y == y); // 좌표가 일치하는 타일 찾기
        }

        // 주어진 좌표의 타일이 벽인지 확인하는 메서드
        private bool IsTileBlocked(int x, int y)
        {
            Tile tile = GetTile(x, y);
            return tile != null && tile.IsBlock; // 타일이 벽인지 확인
        }

        // 주어진 좌표가 테두리(경계) 안에 있는지 확인하는 메서드
        private bool IsInBounds(int x, int y)
        {
            return x > 0 && x < _width && y > 0 && y < _height; // 경계 검사
        }

        // 배열의 요소를 무작위로 섞는 메서드입니다.
        // 배열(또는 리스트)의 요소를 무작위로 섞는 알고리즘인 **피셔–예이츠 셔플(Fisher-Yates Shuffle)**을 구현한 것
        // 처음에 n은 배열의 전체 길이입니다. 각 반복마다 n을 1씩 줄여가며, 무작위로 선택한 인덱스와 현재 인덱스를 교환합니다.
        private void Shuffle(Point[] list)
        {
            int n = list.Length; // n은 배열의 길이를 저장하고, 그 값은 배열의 인덱스 범위 (0부터 n-1)를 나타냅니다.
            while (n > 1)
            {
                int k = _random.Next(n--); // 0부터 n-1까지의 무작위 정수를 생성하여 k에 할당합니다.
                (list[n], list[k]) = (list[k], list[n]); // 배열의 인덱스 n과 k에 해당하는 두 요소의 값을 교환합니다.
            } // n은 1씩 줄어들고 있음 랜덤으로 생성된 k와 맞바꿈
        }
    }
}
