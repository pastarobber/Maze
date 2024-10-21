using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AStar.MainForm;
using static System.Net.Mime.MediaTypeNames;

namespace AStar
{
    // MainForm 클래스는 A* 알고리즘을 사용한 길찾기 기능을 담당하는 폼을 구현
    public partial class MainForm : Form
    {
        // 길찾기 관련 상태 업데이트 타입을 정의한 열거형
        public enum UpdateType { None, Init, Create, Build, Move, Moving };
        public enum ToolType { None, RectShape, HeartShape, Eraser, SetStart, SetEnd }; //사각형, 하트모양, 지우개 선택했는지

        private bool _isCreated; // 맵 생성 여부 
        private bool _isStarted; // 길찾기 시작 여부

        private int _mapSizeX; // 맵의 X축 크기
        private int _mapSizeY; // 맵의 Y축 크기
        private int _width, _height;

        // 각종 데이터를 저장하는 리스트들
        private List<Tile> _tiles; // 맵의 타일들을 저장
        private List<Tile> _path; // 길찾기 경로를 저장
        //private List<Tile> _openList; 
        private PriorityQueue _openList; // A* 알고리즘의 open list
        private List<Tile> _closeList; // A* 알고리즘의 close list
        private UpdateType _updateType; // 현재 맵의 업데이트 상황
        /*---------------------------------------------------------------------------------------*/
        private Tile _startTile, _endTile;
        private HashSet<Point> _toggledTiles;
        private bool _isDrag;//드래그 여부
        private ToolType _toolType;

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
        private Font _menufont;


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
            //_openList = new List<Tile>(); // 전 코드
            _openList = new PriorityQueue();
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
            _menufont = new Font("맑은 고딕", 13);

            numericUpDown_x.Minimum = 5;
            numericUpDown_x.Maximum = 100;
            numericUpDown_y.Minimum = 5;
            numericUpDown_y.Maximum = 100;

            _toolType = ToolType.None;

            // 맵을 초기화
            UpdateMap(UpdateType.Init);
        }

        /*이 컨트롤은 숫자 값을 입력할 수 있게 해주는 UI 요소입니다. 
         * 사용자는 직접 숫자를 입력하거나, 위/아래 화살표 버튼을 사용하여 숫자 값을 증가 또는 감소시킬 수 있습니다.*/
        // X축 크기가 변경될 때 호출
        private void NumericUpDown_x_ValueChanged(object sender, EventArgs e)
        {
            if (_isStarted)  // 길찾기가 시작된 상태라면 값을 변경하지 않음
            {
                // 길찾기가 시작된 경우, 값을 기존 맵 크기 값으로 돌려놓음
                numericUpDown_x.Value = _mapSizeX;
            }
            else
            {   // 만약 값이 2 이하라면, 값이 3으로 고정됨
                int newSizeX = (int)numericUpDown_x.Value; // 맵 크기 값을 NumericUpDown의 현재 값으로 설정
                // 맵 크기를 무조건 홀수로 만들기 위해 처리
                if (newSizeX > _mapSizeX)
                    _mapSizeX = (newSizeX % 2 == 0 ? 2 * newSizeX / 2 + 1 : newSizeX);
                else
                    _mapSizeX = (newSizeX % 2 == 0 ? 2 * newSizeX / 2 - 1 : newSizeX);

                numericUpDown_x.Value = _mapSizeX;
            }
        }

        // Y축 크기가 변경될 때 호출
        private void NumericUpDown_y_ValueChanged(object sender, EventArgs e)
        {
            if (_isStarted) // 길찾기가 시작된 상태라면 값을 변경하지 않음
            {
                numericUpDown_y.Value = _mapSizeY;
            }
            else
            {
                int newSizeY = (int)numericUpDown_y.Value; // 변경된 값 적용

                if (newSizeY > _mapSizeY)
                    _mapSizeY = (newSizeY % 2 == 0 ? 2 * newSizeY / 2 + 1 : newSizeY);
                else
                    _mapSizeY = (newSizeY % 2 == 0 ? 2 * newSizeY / 2 - 1 : newSizeY);

                numericUpDown_y.Value = _mapSizeY;
            }
        }

        // "맵 생성" 버튼을 클릭했을 때 호출
        private void Button_createMap_Click(object sender, EventArgs e)
        {
            CreateMap();
            // 맵 업데이트
            UpdateMap(UpdateType.Create);
        }

        private void CreateMap()
        {
            if (_isStarted) return; // 길찾기가 시작된 상태라면 맵을 생성하지 않음

            _isCreated = false;
            _width = (pictureBox_map.Size.Width - 10) / _mapSizeX; // 타일의 너비 계산
            _height = (pictureBox_map.Size.Height - 10) / _mapSizeY; // 타일의 높이 계산
            // (작은 정사각형)맵의 한 타일 가로와 세로 비율을 동일하게 맞추기 위해 (정사각형)
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
            SetStartTile(new Point(0, 0)); // 시작 타일 설정
            SetEndTile(new Point(_mapSizeX - 1, _mapSizeY - 1));
        }

        // "시작" 버튼을 클릭했을 때 호출
        private void Button_start_Click(object sender, EventArgs e)
        {
            if (!_isCreated) return; // 맵이 생성되지 않았다면 길찾기 시작 불가

            // 기존 길찾기 정보 초기화
            _tiles.ForEach(o =>
            {
                if (o.Text == "START" || o.Text == "END") return;
                o.Text = null;
            });
            _openList.Clear();
            _closeList.Clear();
            _path.Clear();

            var directions = new List<Point> { new Point(-1, -1), new Point(0, -1), new Point(1, -1),
                                               new Point(-1,0), new Point(1,0),
                                               new Point(-1,1),new Point(0,1),new Point(1,1)};

            Tile startTile = _startTile, endTile = _endTile;
            Tile tile = null;

            _openList.Enqueue(startTile);
            //_openList.Add(startTile);
            do
            {
                if (_openList.Count == 0) break;

                tile = _openList.Dequeue();//PQ는 Min Heap으로 구현, Dequeue()을 수행하면 Min F를 delete 및 return.
                //tile = _openList.OrderBy(o => o.F).First();//기존코드
                //_openList.Remove(tile);//기존코드
                _closeList.Add(tile);
                //tile.Text = "C_" + _closeList.Count;//디버깅

                if (tile == endTile) break;

                // 타일 주변의 타일을 검사
                foreach (var dir in directions)
                {
                    int nx = tile.X + dir.X;
                    int ny = tile.Y + dir.Y;
                    if (!IsInBound(new Point(nx, ny))) continue;

                    Tile target = FindTile(new Point(nx, ny));

                    if (target.IsBlock == true) continue;
                    if (_closeList.Contains(target)) continue;

                    if (!_openList.Contains(target))
                    {
                        //_openList.Add(target)
                        target.Execute(tile, endTile);
                        //기존 코드랑 다르게 F값을 계산하고 가야함.
                        //Priority Queue를 min heap으로 구현했고,
                        //pq에 넣으면서 다시 heapify를 하는데, 이때 계산안하고 넣으면 F, G, H 다 쓰레기값 들어가있어서
                        //pq가 제대로 동작을안함.
                        //이거 떄문에 2시간 씀
                        _openList.Enqueue(target);
                    }
                    else
                    {
                        if (Tile.CalcGValue(tile, target) < target.G)
                        {
                            target.Execute(tile, endTile);
                        }
                        //target.Text = "O_" + _openList.Count; //디버깅
                    }
                }
            } while (tile != null);

            //foreach (var target in _tiles)
            //{
            //    if (target.IsBlock) continue; // 장애물이 있는 타일은 패스
            //    if (_closeList.Contains(target)) continue; // 이미 close 리스트에 있는 타일은 패스
            //    if (!IsNearLoc(tile, target)) continue; // 인접하지 않은 타일은 패스
            //    if (!_openList.Contains(target)) // target이 없으면
            //    {
            //        _openList.Add(target); // 새로운 타일을 open 리스트에 추가
            //        target.Execute(tile, endTile); // 각 타일의 F, G, H 값을 계산하고 경로 정보를 업데이트하는 데 사용
            //    }
            //    else // 이미 오픈리스트에 있으면
            //    {   // G 값이 더 작은 경우 타일을 업데이트
            //        if (Tile.CalcGValue(tile, target) < target.G) // G 값을 계산하는 정적 메서드
            //        { // 타일을 업데이트할 때 G 값을 비교하는 이유는 더 최적화된 경로를 찾기 위해
            //            target.Execute(tile, endTile);
            //        }
            //    }
            //}

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
            } while (tile != null);
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
        private void PictureBox_map_Paint(object sender, PaintEventArgs e)
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
                        "\r\n5.메뉴바에서 출발&도착을 임의로 설정 가능\r\n6.메뉴바에서 네모/하트 도장 찍기 가능\r\n7.메뉴바에 지우개 기능이 있음(5*5)" +
                        "\r\n8.Random 버튼 클릭시 미로가 랜덤으로 생성됨";
                    // 맵의 크기를 계산
                    int width = pictureBox_map.Size.Width - 10;
                    int height = pictureBox_map.Size.Height - 10;
                    if (width < height) height = width; // 가로와 세로 중 작은 값으로 맞춤
                    else if (height < width) width = height;
                    // 배경을 검은색으로 채우고 메시지를 출력
                    e.Graphics.FillRectangle(_backgroundBrush, new Rectangle(0, 0, width, height));
                    e.Graphics.DrawString(waitMsg, _menufont, _textBrush, 0, 0);
                    break;
                // 맵이 생성된 후의 상태 처리
                case UpdateType.Create:
                    //모든 타일을 순회하며 각 타일을 회색으로 그린 후 테두리를 그림
                    foreach (var loc in _tiles)
                    {
                        if (loc.Text == "START" || loc.Text == "END") e.Graphics.FillRectangle(new SolidBrush(Color.Green), loc.Region);
                        else e.Graphics.FillRectangle(_normalBrush, loc.Region); // 타일 영역을 회색으로 채움

                        e.Graphics.DrawRectangle(_pen, loc.Region); // 타일 테두리를 그림
                    }

                    e.Graphics.DrawString(_startTile.Text, _font, _textBrush, _startTile.Region.X, _startTile.Region.Y);
                    e.Graphics.DrawString(_endTile.Text, _font, _textBrush, _endTile.Region.X, _endTile.Region.Y);

                    // 맵이 생성되었음을 표시
                    _isCreated = true;
                    break;
                // 장애물 생성 후 맵 상태 처리
                case UpdateType.Build:
                    // 각 타일을 검사하여 장애물 타일은 어두운 색, 일반 타일은 회색으로 채움
                    foreach (var loc in _tiles)
                    {
                        if (loc.IsBlock) e.Graphics.FillRectangle(_blockBrush, loc.Region); // 장애물은 어두운 회색
                        else if (loc.Text == "START" || loc.Text == "END") e.Graphics.FillRectangle(new SolidBrush(Color.Green), loc.Region);
                        else e.Graphics.FillRectangle(_normalBrush, loc.Region); // 일반 타일은 회색
                        e.Graphics.DrawRectangle(_pen, loc.Region); // 타일 테두리 그리기
                    }
                    e.Graphics.DrawString(_startTile.Text, _font, _textBrush, _startTile.Region.X, _startTile.Region.Y);
                    e.Graphics.DrawString(_endTile.Text, _font, _textBrush, _endTile.Region.X, _endTile.Region.Y);

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
                    _isStarted = false;
                    break;
            }
            // 업데이트 타입을 None으로 설정하여 더 이상 그리지 않음
            _updateType = UpdateType.None;
        }
        private void Button_randMap_Click(object sender, EventArgs e) // 랜덤 생성
        {
            if (_isStarted) return; // 길찾기가 시작된 상태라면 맵을 생성하지 않음

            CreateMap();
            MazeGenerator mazeGen = new MazeGenerator(_tiles, _mapSizeY, _mapSizeX);
            mazeGen.GenerateMaze(out Tile start, out Tile end);

            SetStartTile(new Point(start.X, start.Y));
            SetEndTile(new Point(end.X, end.Y));

            _isCreated = true;
            UpdateMap(UpdateType.Build);
        }

        // 맵을 클릭하여 장애물 추가/삭제(토글)
        private void PictureBox_map_MouseDown(object sender, MouseEventArgs e)
        {
            HandleMouseAction(e.Location, isOnceClicked: true);
        }
        // 마우스가 드래그(움직일 때) 호출되는 메서드
        private void PictureBox_map_MouseMove(object sender, MouseEventArgs e)
        {
            HandleMouseAction(e.Location, isOnceClicked: false);
        }
        // 마우스클릭을 뗄 때 호출되는 메서드
        private void PictureBox_map_MouseUp(object sender, MouseEventArgs e)
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

            string actionType = "";

            if (_toolType == ToolType.RectShape && isOnceClicked) actionType = "CreateRectObstacle";
            else if (_toolType == ToolType.HeartShape && isOnceClicked) actionType = "CreateHeartObstacle";
            else if (_toolType == ToolType.SetStart && isOnceClicked) actionType = "SetStartTile";
            else if (_toolType == ToolType.SetEnd && isOnceClicked) actionType = "SetEndTile";
            else if (isOnceClicked) actionType = "SingleClick";
            else if (_isDrag) actionType = "Drag";

            switch (actionType)
            {
                case "CreateRectObstacle":
                    CreateRectObstacle(pos);
                    UpdateMap(UpdateType.Build);
                    break;
                case "CreateHeartObstacle":
                    CreateHeartObstacle(pos);
                    UpdateMap(UpdateType.Build);
                    break;
                case "SetStartTile":
                    SetStartTile(pos);
                    UpdateMap(UpdateType.Build);
                    break;
                case "SetEndTile":
                    SetEndTile(pos);
                    UpdateMap(UpdateType.Build);
                    break;
                case "SingleClick":
                    _isDrag = true;
                    UpdateSingleTIle(pos);
                    UpdateMap(UpdateType.Build);
                    break;
                case "Drag":
                    UpdateSingleTIle(pos);
                    UpdateMap(UpdateType.Build);
                    break;
                default:
                    break;
            }
        }

        // 마우스 좌표를 기반으로 장애물 타일을 그리는 메서드
        private void UpdateSingleTIle(Point pos)
        {
            if (_toggledTiles.Contains(pos)) return;

            if (_toolType == ToolType.Eraser) EraseBlockedTile(pos);
            else
            {
                Tile tile = FindTile(pos);
                if (tile.Text == "START" || tile.Text == "END") return;
                tile.IsBlock = !tile.IsBlock;
            }
            _toggledTiles.Add(pos); // 토글된 타일 기록
        }

        private void UpdateTileBlockState(Point pos, Point[] offsets, bool blockState)
        {
            foreach (var off in offsets) // 각 오프셋에 대해
            {
                Point curPos = new Point(pos.X + off.X, pos.Y + off.Y); // 현재 타일의 위치 계산
                if (!IsInBound(curPos)) continue; // 타일 위치가 맵의 경계 밖이거나, 이미 벽인경우 continue

                Tile tile = FindTile(curPos);
                if (tile.IsBlock == blockState || tile.Text == "START" || tile.Text == "END") continue;
                // 유효한 위치라면 타일을 장애물로 설정
                tile.IsBlock = blockState;
            }
        }
        // 사용자가 선택한 위치(pos)를 중심으로 사각형 장애물을 생성
        private void CreateRectObstacle(Point pos)
        { // 오프셋(offset)은 주어진 기준점 또는 위치에서의 상대적인 거리나 위치를 나타내는 용어
            // 장애물을 만들기 위한 상대적인 오프셋 배열
            Point[] offsets = { new Point(-1,-1), new Point(0, -1) ,new Point(1, -1), // 위쪽 3개
                                new Point(-1,0),  new Point(0,0),  new Point(1,0),  // 중앙 3개
                                new Point(-1,1),  new Point(0,1),  new Point(1,1) }; // 아래쪽 3개

            UpdateTileBlockState(pos, offsets, blockState: true);
        }

        // 사용자가 선택한 위치(pos)를 중심으로 하트모양 장애물을 생성
        private void CreateHeartObstacle(Point pos)
        {
            Point[] offsets = {new Point(-1,-2),new Point(1,-2), // 2개
                                new Point(-2,-1), new Point(-1,-1), new Point(0, -1) ,new Point(1, -1), new Point(2,-1), // 5개
                                new Point(-2,0), new Point(-1,0),  new Point(0,0),  new Point(1,0), new Point(2,0),  // 5개
                                new Point(-1,1),  new Point(0,1),  new Point(1,1), // 3개
                                new Point(0,2)}; // 1개
            UpdateTileBlockState(pos, offsets, blockState: true);
        }
        private void EraseBlockedTile(Point pos) // 지우개 
        {
            Point[] offsets = {new Point(-2,-2), new Point(-1,-2), new Point(0,-2), new Point(1,-2), new Point(2,-2), // 5개
                                new Point(-2,-1), new Point(-1,-1), new Point(0, -1), new Point(1, -1), new Point(2,-1), // 5개
                                new Point(-2,0), new Point(-1,0), new Point(0,0), new Point(1,0), new Point(2,0),  // 5개
                                new Point(-2,1),  new Point(-1,1), new Point(0,1), new Point(1,1), new Point(2,1),  // 5개
                                new Point(-2,2),  new Point(-1,2), new Point(0,2), new Point(1,2), new Point(2,2) }; // 5개
            UpdateTileBlockState(pos, offsets, blockState: false);
        }

        private void Tool_full_square_Click(object sender, EventArgs e)
        {
            ToggleToolBtnHandler(ToolType.RectShape);
        }

        private void Tool_full_heart_Click(object sender, EventArgs e)
        {
            ToggleToolBtnHandler(ToolType.HeartShape);
        }

        private void Tool_Eraser_Click(object sender, EventArgs e)
        {
            ToggleToolBtnHandler(ToolType.Eraser);
        }

        private void Tool_Change_Start_Click(object sender, EventArgs e)
        {
            ToggleToolBtnHandler(ToolType.SetStart);
        }

        private void Tool_Change_End_Click(object sender, EventArgs e)
        {
            ToggleToolBtnHandler(ToolType.SetEnd);
        }

        private void ToggleToolBtnHandler(ToolType toolType)
        {
            _toolType = (_toolType == toolType ? ToolType.None : toolType);
        }

        private void SetStartTile(Point pos)
        {
            Tile tile = FindTile(pos);

            if (tile.IsBlock || tile.Text == "END")
            {
                MessageBox.Show("출발지 설정 불가 다시 설정하세요.");
                return;
            }

            if (_startTile != null)
            {
                _startTile.Text = null;
            }

            _startTile = tile;
            _startTile.Text = "START";
            _startTile.DeleteParent();
            _toolType = ToolType.None;
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                if (item.Name == "Tool_Change_Start")
                    item.BackColor = Color.Transparent;
            }
        }
        private void SetEndTile(Point pos)
        {
            var tile = FindTile(pos);

            if (tile.IsBlock || tile.Text == "START")
            {
                MessageBox.Show("목적지 설정 불가 다시 설정하세요.");
                return;
            }
            if (_endTile != null)
            {
                _endTile.Text = null;
            }

            _endTile = tile;
            _endTile.Text = "END";
            _endTile.DeleteParent();
            _toolType = ToolType.None;

            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                if (item.Name == "Tool_Change_End")
                    item.BackColor = Color.Transparent;
            }
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
            int x = (int)(mousePos.X / _width), y = (int)(mousePos.Y / _height);
            return new Point(x, y); // 계산된 타일 인덱스를 Point 객체로 반환
        }

        // 주어진 좌표(Point pos)를 기반으로 해당 위치에 있는 타일(Tile)을 반환
        private Tile FindTile(Point pos)
        {
            return _tiles[pos.X * _mapSizeY + pos.Y];
        }

        private void menuStrip1_ItemClicked_1(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripMenuItem clickedItem = e.ClickedItem as ToolStripMenuItem;

            if (clickedItem != null)
            {
                // 선택된 메뉴 항목의 시각적 표시 (배경색과 테두리 변경)
                if (clickedItem.BackColor == Color.LightBlue)
                {
                    clickedItem.BackColor = Color.Transparent; // 선택 해제 (기본 색으로 변경)
                    clickedItem.Paint -= MenuItem_Paint; // 테두리 제거
                    return;
                }

                clickedItem.BackColor = Color.LightBlue;
                clickedItem.Paint += MenuItem_Paint; // 테두리 그리기 이벤트 추가

                // 다른 메뉴 항목들의 배경색 및 테두리 초기화
                foreach (ToolStripMenuItem item in menuStrip1.Items)
                {
                    if (item != clickedItem)
                    {
                        item.BackColor = Color.Transparent; // 기본 색으로 초기화
                        item.Paint -= MenuItem_Paint; // 기존에 있던 테두리 제거
                    }
                }
            }
        }

        // 선택된 메뉴 항목에 대해 직선 테두리를 그리는 Paint 이벤트 핸들러
        private void MenuItem_Paint(object sender, PaintEventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null && menuItem.BackColor == Color.LightBlue)
            {
                // 직선 사각형 테두리 그리기
                using (Pen pen = new Pen(Color.DarkBlue, 1)) // 1px 굵기의 검정색 펜
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None; // 직선 테두리
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, menuItem.Width - 1, menuItem.Height - 1));
                }
            }
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
        private void Button_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}