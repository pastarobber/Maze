using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace AStar
{
    // MainForm 클래스는 A* 알고리즘을 사용한 길찾기 기능을 담당하는 폼을 구현
    public partial class MainForm : Form
    {
        // 길찾기 관련 상태 업데이트 타입을 정의한 열거형
        public enum UpdateType { None, Init, Create, Build, Move };

        private bool _isCreated; // 맵 생성 여부 
        private bool _isStarted; // 길찾기 시작 여부

        private int _mapSizeX; // 맵의 X축 크기
        private int _mapSizeY; // 맵의 Y축 크기

        // 각종 데이터를 저장하는 리스트들
        //KdTree같은거 쓰면 성능 향상 있을거 같긴 함 - ㅇㅅㅂ
        private List<Tile> _tiles; // 맵의 타일들을 저장
        private List<Tile> _path; // 길찾기 경로를 저장
        private List<Tile> _openList; // A* 알고리즘의 open list
        private List<Tile> _closeList; // A* 알고리즘의 close list
        private UpdateType _updateType; // 현재 맵의 업데이트 상태
        
        /*------------------------------------------------------------------*/
        private HashSet<Point> _toggledTiles;
        private Tile _start, _end;
        private bool _isDrag;
        private bool _heartShape, _rectShape, _isEraser;
        int _width, _height;

        /*------------------------------------------------------------------*/

        /*
         * 1.열린 목록은 아직 탐색하지 않았거나, 탐색 도중 다시 고려해야 할 노드들을 저장하는 목록
         * 즉, 앞으로 탐색할 후보 노드들을 관리하는 공간
         * 2.닫힌 목록은 이미 탐색을 마친 노드들을 저장하는 목록
         * 즉, 이미 방문하고 더이상 다시 고려하지 않아도 되는 노드들을 관리하는 공간
         */

        // 다양한 그리기 속성들 (브러시, 펜, 폰트)
        private Brush _backgroundBrush;
        private Brush _normalBrush;
        private Brush _blockBrush;
        private Brush _pathBrush;
        private Brush _textBrush;
        private Pen _pen;
        private Font _font;

        #region Constructor
        // MainForm의 생성자. 폼을 초기화
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Control Event
        // 폼이 로드될 때 호출되는 메서드
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;  // 디자인 모드에서 로드 시 아무것도 하지 않음

            _mapSizeX = (int)numericUpDown_x.Value; // 초기 맵 X축 크기 설정
            _mapSizeY = (int)numericUpDown_y.Value; // 초기 맵 Y축 크기 설정

            // 리스트와 브러시, 폰트 등 초기화
            _tiles = new List<Tile>();
            _path = new List<Tile>();
            _openList = new List<Tile>();
            _closeList = new List<Tile>();
            _isDrag = false;
            _toggledTiles = new HashSet<Point>();

            _backgroundBrush = new SolidBrush(Color.Black);
            _normalBrush = new SolidBrush(Color.Gray);
            _blockBrush = new SolidBrush(Color.DarkSlateGray);
            _pathBrush = new SolidBrush(Color.Red);
            _textBrush = new SolidBrush(Color.Yellow);
            _pen = new Pen(Color.DarkGray);
            _font = new Font("맑은 고딕", 10);

            _rectShape = false;
            _heartShape = false;

            numericUpDown_x.Maximum = 99;
            numericUpDown_y.Maximum = 99;

            // 맵을 초기화
            UpdateMap(UpdateType.Init);
        }

        /*이 컨트롤은 숫자 값을 입력할 수 있게 해주는 UI 요소입니다. 
         * 사용자는 직접 숫자를 입력하거나, 위/아래 화살표 버튼을 사용하여 숫자 값을 증가 또는 감소시킬 수 있습니다.*/
        // X축 크기가 변경될 때 호출
        private void numericUpDown_x_ValueChanged(object sender, EventArgs e)
        {
            if (_isStarted)  // 길찾기가 시작된 상태라면 값을 변경하지 않음
            {
                // 길찾기가 시작된 경우, 값을 기존 맵 크기 값으로 돌려놓음
                numericUpDown_x.Value = _mapSizeX;
            }
            else
            {   // 만약 값이 2 이하라면, 값이 3으로 고정됨
                if (numericUpDown_x.Value <= 2) numericUpDown_x.Value = 3;

                _mapSizeX = (int)numericUpDown_x.Value; // 맵 크기 값을 NumericUpDown의 현재 값으로 설정
                // 맵 크기를 무조건 홀수로 만들기 위해 처리
                _mapSizeX = 2 * (_mapSizeX / 2) + 1; // (_mapSizeX / 2) * 2 는 짝수, +1 을 하면 홀수가 됨
                // 수정된 값을 NumericUpDown 컨트롤에 다시 반영
                numericUpDown_x.Value = _mapSizeX;
            }
        }

        // Y축 크기가 변경될 때 호출
        private void numericUpDown_y_ValueChanged(object sender, EventArgs e)
        {
            if (_isStarted) // 길찾기가 시작된 상태라면 값을 변경하지 않음
            {
                numericUpDown_y.Value = _mapSizeY;
            }
            else
            {
                if (numericUpDown_x.Value <= 2) numericUpDown_y.Value = 3;

                _mapSizeY = (int)numericUpDown_y.Value; // 변경된 값 적용
                _mapSizeY = 2 * (_mapSizeY / 2) + 1; //무조건 홀수값만.
                numericUpDown_y.Value = _mapSizeY;
            }
        }

        // "맵 생성" 버튼을 클릭했을 때 호출
        private void button_createMap_Click(object sender, EventArgs e)
        {
            createMap();
            // 맵 업데이트
            UpdateMap(UpdateType.Create);
        }

        private void createMap()
        {
            if (_isStarted) return; // 길찾기가 시작된 상태라면 맵을 생성하지 않음

            _isCreated = false;
            _width = (pictureBox_map.Size.Width - 10) / _mapSizeX; // 타일의 너비 계산
            _height = (pictureBox_map.Size.Height - 10) / _mapSizeY; // 타일의 높이 계산
            // 맵의 가로와 세로 비율을 동일하게 맞추기 위해 (정사각형)
            if (_width < _height) _height = _width;
            else if (_height < _width) _width = _height;

            _tiles.Clear(); // 기존 타일 리스트 초기화
            for (int x = 0; x < _mapSizeX; x++)
            {
                for (int y = 0; y < _mapSizeY; y++)
                {
                    // 타일 객체 생성 및 좌표 설정
                    var loc = new Tile()
                    {
                        X = x,
                        Y = y,
                        Region = new Rectangle(new Point(x * _width, y * _height), new Size(_width, _height)),
                    };// Point: 각 타일이 겹치지 않도록 위치를 설정합니다. , Size: 타일의 크기를 설정합니다.
                    _tiles.Add(loc); // 타일 리스트에 추가
                }
            }
            _start = _tiles[0]; // 시작 타일 설정
            _end = _tiles[_tiles.Count - 1]; // 마지막 타일 설정 (항상 바뀌니까 카운트해서 -1)
        }

        // "시작" 버튼을 클릭했을 때 호출
        private void button_start_Click(object sender, EventArgs e)
        {
            if (!_isCreated) return; // 맵이 생성되지 않았다면 길찾기 시작 불가

            // 기존 길찾기 정보 초기화
            _tiles.ForEach(o => o.Text = null); //_tiles 리스트의 각 타일 객체를 순차적으로 o라는 이름으로 받아서, 그 타일의 Text 속성을 null로 설정하는 역할
            _openList.Clear();
            _closeList.Clear();
            _path.Clear();

            Tile startTile = _start, endTile = _end;

            _openList.Add(startTile); // 시작 타일을 open 리스트에 추가
            Tile tile = null;
            do
            {
                if (_openList.Count == 0) break; // open 리스트가 비어 있으면 종료

                // OrderBy는 C#에서 LINQ (Language Integrated Query)의 메서드 중 하나로, 컬렉션을 정렬하는 데 사용됩니다.
                // OrderBy는 지정된 조건에 따라 오름차순으로 요소들을 정렬합니다.
                tile = _openList.OrderBy(o => o.F).First(); // F 값이 가장 낮은 타일 선택
                _openList.Remove(tile); // open 리스트에서 제거
                _closeList.Add(tile); // close 리스트에 추가

                if (tile == endTile) break; // 목적지에 도착하면 종료

                // 타일 주변의 타일을 검사
                foreach (var target in _tiles)
                {
                    if (target.IsBlock) continue; // 장애물이 있는 타일은 패스
                    if (_closeList.Contains(target)) continue; // 이미 close 리스트에 있는 타일은 패스
                    if (!IsNearLoc(tile, target)) continue; // 인접하지 않은 타일은 패스

                    if (!_openList.Contains(target)) // target이 없으면
                    {
                        _openList.Add(target); // 새로운 타일을 open 리스트에 추가
                        target.Execute(tile, endTile); // 각 타일의 F, G, H 값을 계산하고 경로 정보를 업데이트하는 데 사용
                    }
                    else // 이미 오픈리스트에 있으면
                    {   // G 값이 더 작은 경우 타일을 업데이트
                        if (Tile.CalcGValue(tile, target) < target.G) // G 값을 계산하는 정적 메서드
                        { // 타일을 업데이트할 때 G 값을 비교하는 이유는 더 최적화된 경로를 찾기 위해
                            target.Execute(tile, endTile);
                        }
                    }
                }
            }
            while (tile != null);

            if (tile != endTile)
            {
                MessageBox.Show("길막힘"); // 경로를 찾을 수 없는 경우 메시지 출력
                return;
            }
            // 경로를 거꾸로 추적하여 저장
            do
            {
                _path.Add(tile);
                tile = tile.Parent;
            }
            while (tile != null);
            _path.Reverse(); // 경로를 뒤집어 올바른 순서로 설정

            // 경로에 따라 타일에 텍스트 표시
            for (int i = 0; i < _path.Count; i++)
            {
                if (i == 0) _path[i].Text = "START";
                else if (i == _path.Count - 1) _path[i].Text = "END";
                else _path[i].Text = i.ToString();
            }

            _isStarted = true;
            UpdateMap(UpdateType.Move); // 경로 표시
        }

        // 맵을 다시 그리는 메서드
        private void pictureBox_map_Paint(object sender, PaintEventArgs e)
        {
            // 업데이트할 내용이 없으면 함수를 종료
            if (_updateType == UpdateType.None) return;

            // 업데이트 타입에 따라 처리 방식을 다르게 적용
            switch (_updateType)
            {
                // 초기화 상태일 때 맵에 초기 안내 메시지를 출력
                case UpdateType.Init:
                    // 안내 메시지를 정의
                    string waitMsg = "1.맵 크기 설정 (범위: 2<홀수<100)\r\n2.Create 클릭\r\n3.맵에 마우스 좌클릭하여 장애물(벽) 생성\r\n4.Start 클릭" +
                        "\r\n5.메뉴바에서 네모/하트 도장 찍기 가능\r\n6.메뉴바에 지우개 기능이 있음(5*5)\r\n7.Random 버튼 클릭시 미로가 랜덤으로 생성됨";
                    // 맵의 크기를 계산
                    int width = pictureBox_map.Size.Width - 10;
                    int height = pictureBox_map.Size.Height - 10;
                    if (width < height) height = width; // 가로와 세로 중 작은 값으로 맞춤
                    else if (height < width) width = height;
                    // 배경을 검은색으로 채우고 메시지를 출력
                    e.Graphics.FillRectangle(_backgroundBrush, new Rectangle(0, 0, width, height));
                    e.Graphics.DrawString(waitMsg, _font, _textBrush, 0, 0);
                    break;
                // 맵이 생성된 후의 상태 처리
                case UpdateType.Create:
                    // 모든 타일을 순회하며 각 타일을 회색으로 그린 후 테두리를 그림
                    foreach (var loc in _tiles)
                    {
                        e.Graphics.FillRectangle(_normalBrush, loc.Region); // 타일 영역을 회색으로 채움
                        e.Graphics.DrawRectangle(_pen, loc.Region); // 타일 테두리를 그림
                    }

                    // 맵이 생성되었음을 표시
                    _isCreated = true;
                    break;
                // 장애물 생성 후 맵 상태 처리
                case UpdateType.Build:
                    // 각 타일을 검사하여 장애물 타일은 어두운 색, 일반 타일은 회색으로 채움
                    foreach (var loc in _tiles)
                    {
                        if (loc.IsBlock) e.Graphics.FillRectangle(_blockBrush, loc.Region); // 장애물은 어두운 회색
                        else e.Graphics.FillRectangle(_normalBrush, loc.Region); // 일반 타일은 회색
                        e.Graphics.DrawRectangle(_pen, loc.Region); // 타일 테두리 그리기
                    }
                    break;
                // 경로를 따라 이동 중일 때의 상태 처리
                case UpdateType.Move:
                    // 각 타일을 순회하며 장애물과 일반 타일을 그린 후 텍스트가 있으면 출력
                    foreach (var loc in _tiles)
                    {
                        if (loc.IsBlock) e.Graphics.FillRectangle(_blockBrush, loc.Region); // 장애물 타일은 어두운 회색
                        else e.Graphics.FillRectangle(_normalBrush, loc.Region); // 일반 타일은 회색
                        e.Graphics.DrawRectangle(_pen, loc.Region); // 타일 테두리 그리기
                        // 타일에 텍스트가 있으면 출력
                        if (!string.IsNullOrWhiteSpace(loc.Text))
                        {
                            e.Graphics.DrawString(loc.Text, _font, _textBrush, loc.Region.X, loc.Region.Y);
                        }
                    }
                    // 경로 리스트에 있는 타일을 따라 빨간색으로 경로를 그림
                    foreach (var loc in _path)
                    {
                        e.Graphics.FillRectangle(_pathBrush, loc.Region); // 경로 타일은 빨간색으로 채움
                        e.Graphics.DrawRectangle(_pen, loc.Region); // 타일 테두리 그리기
                        // 경로 타일에 텍스트가 있으면 출력
                        if (!string.IsNullOrWhiteSpace(loc.Text))
                        {
                            e.Graphics.DrawString(loc.Text, _font, _textBrush, loc.Region.X, loc.Region.Y);
                        }
                    }
                    // 경로 이동이 완료되었음을 표시
                    _isStarted = false;
                    break;
            }
            // 업데이트 타입을 None으로 설정하여 더 이상 그리지 않음
            _updateType = UpdateType.None;
        }
        private void button_randMap_Click(object sender, EventArgs e) // 랜덤 생성
        {
            if (_isStarted) return; // 길찾기가 시작된 상태라면 맵을 생성하지 않음

            createMap();

            MazeGenerator mazeGen = new MazeGenerator(_tiles, _mapSizeY, _mapSizeX);
            mazeGen.GenerateMaze(out _start, out _end);
            // out은 C#에서 메서드의 매개변수를 정의할 때 사용하는 키워드로, 해당 매개변수를 통해 메서드에서 값을 반환할 수 있도록 해줍니다. 

            _isCreated = true;
            UpdateMap(UpdateType.Build);
        }

        // 맵을 클릭하여 장애물 추가/삭제(토글)
        private void pictureBox_map_MouseDown(object sender, MouseEventArgs e)
        {
            HandleMouseAction(e.Location, true);
        }
        // 마우스가 드래그(움직일 때) 호출되는 메서드
        private void pictureBox_map_MouseMove(object sender, MouseEventArgs e)
        {
            HandleMouseAction(e.Location, false);
        }
        // 마우스클릭을 뗄 때 호출되는 메서드
        private void pictureBox_map_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isCreated || _isStarted) return; // 맵이 생성되지 않았거나 길찾기가 시작되었으면 무시

            // 마우스 클릭이 해제되면 타일 토글 동작을 멈춤
            if (_isDrag)
            {
                _isDrag = false;
                _toggledTiles.Clear(); // 마우스를 뗄 때 토글된 타일 리스트 초기화
            }
        }

        // 사용자가 마우스를 클릭하거나 드래그할 때 특정 작업을 수행하는 메서드
        private void HandleMouseAction(Point mousePos, bool isOnceClicked)
        {   // 현재 마우스 포인터의 위치를 나타내는 점, 마우스가 한 번 클릭되었는지 여부를 나타내는 불리언 값
        
            if (!_isCreated || _isStarted) return; // 맵이 생성되지 않았거나 길찾기가 시작되었으면 무시

            Point pos = ConverRelativePos(mousePos); // mousePos를 상대 좌표로 변환한 후, 해당 위치가 유효한지 검사
            if (!IsInBound(pos)) return; // 마우스가 picture box 밖을 클릭했으면 무시

            if (_rectShape && isOnceClicked)
            {
                CreateRectObstacle(pos);
            }
            else if (_heartShape && isOnceClicked)
            {
                CreateHeartObstacle(pos);
            }
            else if (isOnceClicked) //마우스 한번만 클릭 했을 때
            {
                _isDrag = true;
                UpdateSingleTIle(pos);
            }
            else if (_isDrag) // 클릭 후 moving(drag 이벤트)
            {
                UpdateSingleTIle(pos);
            }
            
            //위에 Create~ 함수들에서 만약에 그려지면 true, 안그려지면 false리턴해서
            //그려졌을때만 Update하는식으로 수정해도 됨.
            UpdateMap(UpdateType.Build);
        }

        // 마우스 좌표를 기반으로 장애물 타일을 그리는 메서드
        private void UpdateSingleTIle(Point pos)
        {
            // 모든 타일을 순회하여 마우스 좌표가 해당 타일 영역 안에 있는지 확인
            // 이미 토글된 타일이 아닌 경우
            if (!_toggledTiles.Contains(pos))
            {
                Tile target = FindTile(pos);
                if (_isEraser)
                    EraseBlockedTile(pos);
                else
                    target.IsBlock = !target.IsBlock;
                _toggledTiles.Add(pos); // 토글된 타일 기록
            }
        }
            
        private void UpdatePolyTile(Point pos, Point[] offsets, bool blockState)
        {
            foreach (var off in offsets) // 각 오프셋에 대해
            {
                Point tilePos = new Point(pos.X + off.X, pos.Y + off.Y); // 현재 타일의 위치 계산
                if (!IsInBound(tilePos)) continue; // 타일 위치가 맵의 경계 밖이거나, 이미 벽인경우 continue

                Tile target = FindTile(tilePos);
                if (target.IsBlock == blockState) continue;
                // 유효한 위치라면 타일을 장애물로 설정
                target.IsBlock = blockState;
            }
        }
        // 사용자가 선택한 위치(pos)를 중심으로 사각형 장애물을 생성
        private void CreateRectObstacle(Point pos)
        { // 오프셋(offset)은 주어진 기준점 또는 위치에서의 상대적인 거리나 위치를 나타내는 용어
            // 장애물을 만들기 위한 상대적인 오프셋 배열
            Point[] offsets = { new Point(-1,-1), new Point(0, -1) ,new Point(1, -1), // 위쪽 3개
                                new Point(-1,0),  new Point(0,0),  new Point(1,0),  // 중앙 3개
                                new Point(-1,1),  new Point(0,1),  new Point(1,1) }; // 아래쪽 3개

            UpdatePolyTile(pos, offsets, blockState: true);
        }

        // 사용자가 선택한 위치(pos)를 중심으로 하트모양 장애물을 생성
        private void CreateHeartObstacle(Point pos)
        { 
            Point[] offsets = {new Point(-1,-2),new Point(1,-2), // 2개
                                new Point(-2,-1), new Point(-1,-1), new Point(0, -1) ,new Point(1, -1), new Point(2,-1), // 5개
                                new Point(-2,0), new Point(-1,0),  new Point(0,0),  new Point(1,0), new Point(2,0),  // 5개
                                new Point(-1,1),  new Point(0,1),  new Point(1,1), // 3개
                                new Point(0,2)}; // 1개
            UpdatePolyTile(pos, offsets, blockState: true);
        }
        private void EraseBlockedTile(Point pos)
        { 
            Point[] offsets = {new Point(-2,-2), new Point(-1,-2), new Point(0,-2), new Point(1,-2), new Point(2,-2), // 5개
                                new Point(-2,-1), new Point(-1,-1), new Point(0, -1), new Point(1, -1), new Point(2,-1), // 5개
                                new Point(-2,0), new Point(-1,0), new Point(0,0), new Point(1,0), new Point(2,0),  // 5개
                                new Point(-2,1),  new Point(-1,1), new Point(0,1), new Point(1,1), new Point(2,1),  // 5개
                                new Point(-2,2),  new Point(-1,2), new Point(0,2), new Point(1,2), new Point(2,2) }; // 5개
            UpdatePolyTile(pos, offsets, blockState: false);
        }

        private void tool_full_square_Click(object sender, EventArgs e)
        {
            _rectShape = !_rectShape;
            ToggleBtnHandler(rectShape: _rectShape);
        }

        private void tool_full_heart_Click(object sender, EventArgs e)
        {
            _heartShape = !_heartShape;
            ToggleBtnHandler(heartShape: _heartShape);
        }

        private void tool_Eraser_Click(object sender, EventArgs e)
        {
            _isEraser = !_isEraser;
            ToggleBtnHandler(erase: _isEraser);
        }

        private void ToggleBtnHandler(bool rectShape = false, bool heartShape = false, bool erase = false)
        {
            // 공통 동작: 다른 기능들을 비활성화
            _isEraser = erase;
            _rectShape = rectShape;
            _heartShape = heartShape;
        }
        #endregion


        #region Private Method
        // 맵을 갱신하는 메서드
        private void UpdateMap(UpdateType type)
        {
            _updateType = type; // 새로운 업데이트 타입 설정
            pictureBox_map.Invalidate(); // 맵 영역을 다시 그리도록 요청
        }

        // 두 타일이 인접해 있는지 확인하는 메서드
        // 최단 경로를 찾기 위해 주로 인접한 타일(또는 노드)을 탐색합니다
        private bool IsNearLoc(Tile srcLoc, Tile targetLoc)
        {
            // X축과 Y축 좌표 차이를 계산하여 인접 여부를 확인
            int diffX = Math.Abs(srcLoc.X - targetLoc.X);
            int diffY = Math.Abs(srcLoc.Y - targetLoc.Y);
            return diffX <= 1 && diffY <= 1; // 차이가 1 이하면 인접한 것으로 판단
        }

        // 마우스 클릭 위치(mousePos)를 현재 타일의 상대 좌표로 변환
        // 다른 점을 기준으로 얼마나 떨어져 있는지
        private Point ConverRelativePos(Point mousePos)
        {
            int x = (int)mousePos.X / _width, y = (int)mousePos.Y / _height;
            return new Point(x,y); // 계산된 타일 인덱스를 Point 객체로 반환
        }

        // 주어진 좌표(Point pos)를 기반으로 해당 위치에 있는 타일(Tile)을 반환
        private Tile FindTile(Point pos)
        {
            return _tiles[pos.X * _mapSizeY + pos.Y];
        }

        // 주어진 좌표(pos)가 현재 맵의 경계 내에 있는지를 확인
        private bool IsInBound(Point pos)
        {
            if (pos.X < 0 || pos.Y < 0 ||
                pos.X >= _mapSizeX ||
                pos.Y >= _mapSizeY) return false;
            return true;
        }

        // 창 닫기
        private void button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}