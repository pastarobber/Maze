namespace AStar
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox_map = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_randMap = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            this.numericUpDown_y = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_x = new System.Windows.Forms.NumericUpDown();
            this.button_createMap = new System.Windows.Forms.Button();
            this.button_start = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tool_full_square = new System.Windows.Forms.ToolStripMenuItem();
            this.tool_full_heart = new System.Windows.Forms.ToolStripMenuItem();
            this.tool_Eraser = new System.Windows.Forms.ToolStripMenuItem();
            this.Tool_Change_Start = new System.Windows.Forms.ToolStripMenuItem();
            this.Tool_Change_End = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_map)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_x)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox_map
            // 
            this.pictureBox_map.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_map.Location = new System.Drawing.Point(185, 48);
            this.pictureBox_map.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.pictureBox_map.Name = "pictureBox_map";
            this.pictureBox_map.Size = new System.Drawing.Size(1739, 1006);
            this.pictureBox_map.TabIndex = 0;
            this.pictureBox_map.TabStop = false;
            this.pictureBox_map.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox_map_Paint);
            this.pictureBox_map.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox_map_MouseDown);
            this.pictureBox_map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox_map_MouseMove);
            this.pictureBox_map.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox_map_MouseUp);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button_randMap);
            this.panel1.Controls.Add(this.button_close);
            this.panel1.Controls.Add(this.numericUpDown_y);
            this.panel1.Controls.Add(this.numericUpDown_x);
            this.panel1.Controls.Add(this.button_createMap);
            this.panel1.Controls.Add(this.button_start);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 48);
            this.panel1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(185, 1006);
            this.panel1.TabIndex = 1;
            // 
            // button_randMap
            // 
            this.button_randMap.Location = new System.Drawing.Point(23, 147);
            this.button_randMap.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.button_randMap.Name = "button_randMap";
            this.button_randMap.Size = new System.Drawing.Size(140, 46);
            this.button_randMap.TabIndex = 5;
            this.button_randMap.Text = "Random";
            this.button_randMap.UseVisualStyleBackColor = true;
            this.button_randMap.Click += new System.EventHandler(this.Button_randMap_Click);
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(23, 322);
            this.button_close.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(140, 46);
            this.button_close.TabIndex = 4;
            this.button_close.Text = "close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.Button_close_Click);
            // 
            // numericUpDown_y
            // 
            this.numericUpDown_y.Location = new System.Drawing.Point(23, 78);
            this.numericUpDown_y.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.numericUpDown_y.Name = "numericUpDown_y";
            this.numericUpDown_y.Size = new System.Drawing.Size(140, 35);
            this.numericUpDown_y.TabIndex = 1;
            this.numericUpDown_y.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numericUpDown_y.ValueChanged += new System.EventHandler(this.NumericUpDown_y_ValueChanged);
            // 
            // numericUpDown_x
            // 
            this.numericUpDown_x.Location = new System.Drawing.Point(23, 24);
            this.numericUpDown_x.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.numericUpDown_x.Name = "numericUpDown_x";
            this.numericUpDown_x.Size = new System.Drawing.Size(140, 35);
            this.numericUpDown_x.TabIndex = 0;
            this.numericUpDown_x.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.numericUpDown_x.ValueChanged += new System.EventHandler(this.NumericUpDown_x_ValueChanged);
            // 
            // button_createMap
            // 
            this.button_createMap.Location = new System.Drawing.Point(23, 205);
            this.button_createMap.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.button_createMap.Name = "button_createMap";
            this.button_createMap.Size = new System.Drawing.Size(140, 46);
            this.button_createMap.TabIndex = 2;
            this.button_createMap.Text = "Create";
            this.button_createMap.UseVisualStyleBackColor = true;
            this.button_createMap.Click += new System.EventHandler(this.Button_createMap_Click);
            // 
            // button_start
            // 
            this.button_start.Location = new System.Drawing.Point(23, 262);
            this.button_start.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.button_start.Name = "button_start";
            this.button_start.Size = new System.Drawing.Size(140, 46);
            this.button_start.TabIndex = 3;
            this.button_start.Text = "Start";
            this.button_start.UseVisualStyleBackColor = true;
            this.button_start.Click += new System.EventHandler(this.Button_start_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tool_full_square,
            this.tool_full_heart,
            this.tool_Eraser,
            this.Tool_Change_Start,
            this.Tool_Change_End});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1924, 48);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tool_full_square
            // 
            this.tool_full_square.Name = "tool_full_square";
            this.tool_full_square.Size = new System.Drawing.Size(58, 42);
            this.tool_full_square.Text = "■";
            this.tool_full_square.Click += new System.EventHandler(this.Tool_full_square_Click);
            // 
            // tool_full_heart
            // 
            this.tool_full_heart.Name = "tool_full_heart";
            this.tool_full_heart.Size = new System.Drawing.Size(58, 42);
            this.tool_full_heart.Text = "♥";
            this.tool_full_heart.Click += new System.EventHandler(this.Tool_full_heart_Click);
            // 
            // tool_Eraser
            // 
            this.tool_Eraser.Name = "tool_Eraser";
            this.tool_Eraser.Size = new System.Drawing.Size(106, 42);
            this.tool_Eraser.Text = "지우개";
            this.tool_Eraser.Click += new System.EventHandler(this.Tool_Eraser_Click);
            // 
            // Tool_Change_Start
            // 
            this.Tool_Change_Start.Name = "Tool_Change_Start";
            this.Tool_Change_Start.Size = new System.Drawing.Size(194, 42);
            this.Tool_Change_Start.Text = "출발 위치 변경";
            this.Tool_Change_Start.Click += new System.EventHandler(this.Tool_Change_Start_Click);
            // 
            // Tool_Change_End
            // 
            this.Tool_Change_End.Name = "Tool_Change_End";
            this.Tool_Change_End.Size = new System.Drawing.Size(194, 42);
            this.Tool_Change_End.Text = "도착 위치 변경";
            this.Tool_Change_End.Click += new System.EventHandler(this.Tool_Change_End_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1924, 1054);
            this.Controls.Add(this.pictureBox_map);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "MainForm";
            this.Text = "A* Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_map)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_x)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_map;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_start;
        private System.Windows.Forms.Button button_createMap;
        private System.Windows.Forms.NumericUpDown numericUpDown_y;
        private System.Windows.Forms.NumericUpDown numericUpDown_x;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tool_full_heart;
        private System.Windows.Forms.ToolStripMenuItem tool_full_square;
        private System.Windows.Forms.Button button_randMap;
        private System.Windows.Forms.ToolStripMenuItem tool_Eraser;
        private System.Windows.Forms.ToolStripMenuItem Tool_Change_Start;
        private System.Windows.Forms.ToolStripMenuItem Tool_Change_End;
    }
}

