using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AStar
{
    public class Tile
    {
        #region Properties
        public int X { get; set; } // 타일의 X 좌표 (get:읽기, set:쓰기)

        public int Y { get; set; } // 타일의 Y 좌표

        // 이 타일이 장애물인지 여부를 나타내는 값 (true: 장애물, false: 장애물 아님)
        public bool IsBlock { get; set; }

        // 타일의 그리기 영역을 나타내는 사각형 (Rectangle 객체)
        public Rectangle Region { get; set; }

        // 타일 위에 그릴 텍스트 (그리기용)
        public string Text { get; set; }

        // ( F = G + H ) 경로 탐색에서 사용
        public int F { get { return G + H; } }

        // G 값:  START(시작 타일) ~ 현재 타일까지의 비용
        public int G { get; private set; }

        // H 값: 현재 타일 ~ END(목표 타일)까지의 예상 비용
        public int H { get; private set; }

        // 부모 타일: 경로 탐색 중 현재 타일에 도달하기 위해 바로 전에 방문한 타일
        public Tile Parent { get; private set; }
        #endregion

        #region Public Method
        // 경로 탐색 시 타일의 값을 계산하는 메서드
        // parent는 이 타일의 이전 타일(부모 타일), endTile은 목표 타일
        public void Execute(Tile parent, Tile endTile)
        {
            Parent = parent; // 현재 타일의 부모 타일을 설정
            // G 값을 계산 (부모 타일로부터 현재 타일까지의 이동 비용)
            G = CalcGValue(parent, this);

            // H 값을 계산 (현재 타일에서 목표 타일까지의 예상 이동 비용)
            // X, Y 좌표의 차이를 사용하여 맨해튼 거리로 계산 (diffX + diffY)
            int diffX = Math.Abs(endTile.X - X);
            int diffY = Math.Abs(endTile.Y - Y);
            // 각 타일 사이의 기본 이동 비용을 10으로 설정 (수직 또는 수평 이동 시)
            H = (diffX + diffY) * 10;
        }
        #endregion
        // G 값을 계산하는 정적 메서드
        // 부모 타일에서 현재 타일까지의 이동 비용을 계산
        #region Static Method
        public static int CalcGValue(Tile parent, Tile current)
        {
            int diffX = Math.Abs(parent.X - current.X); // 부모 타일과 현재 타일의 X 좌표 차이 계산
            int diffY = Math.Abs(parent.Y - current.Y); // 부모 타일과 현재 타일의 Y 좌표 차이 계산
            int value = 10; // 기본 이동 비용은 10 (수직 또는 수평 이동 시)

            // x, y 좌표 차이
            if (diffX == 1 && diffY == 0) value = 10; // 만약 타일이 수평으로 인접해 있을 경우 이동 비용은 10
            else if (diffX == 0 && diffY == 1) value = 10; // 만약 타일이 수직으로 인접해 있을 경우 이동 비용은 10
            else if (diffX == 1 && diffY == 1) value = 14; // 만약 타일이 대각선으로 인접해 있을 경우 이동 비용은 14 (대각선 이동은 더 많은 비용이 듦)

            return parent.G + value; // 부모 타일의 G 값에 현재 타일까지의 이동 비용을 더해 반환
        }

        public void DeleteParent()
        {
            this.Parent = null;
        }

        public int CompareTo(Tile other)
        {
            if (this.F != other.F)
                return this.F.CompareTo(other.F);

            if (this.X != other.X)
                return this.X.CompareTo(other.X);

            return this.Y.CompareTo(other.Y);
        }
        #endregion
    }
}