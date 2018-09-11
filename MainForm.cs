using System;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle.CSharp
{

    public delegate void ShowMessageDelegate(string Message);
    public delegate void ReceiveShotDelegate(int X, int Y);
    public delegate void ReceiveCellShotResultDelegate(int X, int Y, BoardCellState State);
    public class MainForm : Form
    {
        private readonly Player _humanPlayer;
        private readonly Player _computerPlayer;

        private readonly Board _humanBoard;
        private readonly Board _computerBoard;
        
        private readonly GameController _controller;
        
        private readonly ScoreBoard _scoreboard;

        private readonly Button _shuffleButton;
        private readonly Button _startGameButton;
        private readonly Button _newGameButton;
        private readonly Button _connectButton;
        private readonly Button _sendButton;
        private RichTextBox _richTextBox;
        private readonly GroupBox _gbMySocket;
        private readonly GroupBox _gbFriendSocket;

        private TextBox tbMyIP;
        private TextBox tbMyPort;

        private TextBox friendIP;
        private TextBox friendPort;

        private Panel pnBot;
        private TextBox outMsg;

        private static readonly Color ButtonBackColor = Color.FromArgb(65, 133, 243);
        private const char ShuffleCharacter = (char)0x60;
        private const char StartGameCharacter = (char)0x55;
        private const char NewGameCharacter = (char)0x6C;
        private const char ConnectCharacter = (char)0xC2;
        private const char SendCharacter = (char)0x29;

        private Network network;

        public MainForm()
        {
            SuspendLayout();
            ShowMessageDelegate del = new ShowMessageDelegate(ShowMessage);
            ReceiveShotDelegate delShot = new ReceiveShotDelegate(Shot);
            ReceiveCellShotResultDelegate delShotRes = new ReceiveCellShotResultDelegate(ShotResults);
            _humanBoard = new Board();
            _computerBoard = new Board(false);

            _humanPlayer = new HumanPlayer("Me", _computerBoard);
            _computerPlayer = new ComputerPlayer("Friend");


            _scoreboard = new ScoreBoard(_humanPlayer, _computerPlayer, 10, 100);
            _controller = new GameController(_humanPlayer, _computerPlayer, _humanBoard, _computerBoard, _scoreboard);
            network = new Network(del, delShot, delShotRes);

            _humanBoard.network = network;
            _computerBoard.network = network;
            _shuffleButton = CreateButton(ShuffleCharacter.ToString(), ButtonBackColor);
            _newGameButton = CreateButton(NewGameCharacter.ToString(), ButtonBackColor);
            _connectButton = CreateButton(ConnectCharacter.ToString(), ButtonBackColor);
            _startGameButton = CreateButton(StartGameCharacter.ToString(), ButtonBackColor);
            _sendButton = CreateButton(SendCharacter.ToString(), ButtonBackColor);

            pnBot = new Panel { Dock = DockStyle.Bottom };
            
            _richTextBox = new RichTextBox();

            _gbMySocket = new GroupBox{Text = "Me"};
            _gbFriendSocket = new GroupBox{Text = "Friend"};
            friendIP = new TextBox{Text = "127.0.0.1"};
            tbMyIP = new TextBox{Text = "127.0.0.1"};
            tbMyPort = new TextBox { Text = "12345" };
            friendPort = new TextBox { Text = "12346" };

            pnBot = new Panel { Dock = DockStyle.Bottom };
            outMsg = new TextBox();
            _sendButton.Dock = DockStyle.Right;
            outMsg.Dock = DockStyle.Fill;
            _richTextBox.Dock = DockStyle.Top;
            pnBot.Controls.Add(outMsg);
            pnBot.Controls.Add(_sendButton);
            pnBot.Controls.Add(_richTextBox);
            SetupWindow();
            LayoutControls();

            _scoreboard.GameEnded += OnGameEnded;

            _shuffleButton.Click += OnShuffleButtonClick;
            _startGameButton.Click += OnStartGameButtonClick;
            _newGameButton.Click += OnNewGameButtonClick;
            _connectButton.Click += OnConnectButtonClick;
            _sendButton.Click += OnSendButtonClick;

            ResumeLayout();

            StartNewGame();
        }

        private void OnConnectButtonClick(object sender, System.EventArgs e)
        {
            network.Connect(tbMyIP.Text, friendIP.Text, System.Convert.ToInt32(tbMyPort.Text), System.Convert.ToInt32(friendPort.Text));
        }

        private void OnSendButtonClick(object sender, System.EventArgs e)
        {
            network.SendMessage(outMsg.Text);
            _richTextBox.AppendText("<< " + outMsg.Text + "\r\n");
        }
        private void OnNewGameButtonClick(object sender, System.EventArgs e)
        {
            StartNewGame();
        }
        private void Shot(int X, int Y)
        {
            _controller.shootResult(X, Y);
        }
        private void ShotResults(int X, int Y, BoardCellState State)
        {

        }

        private void StartNewGame()
        {
            _shuffleButton.Visible = true;
            _startGameButton.Visible = true;
            _newGameButton.Visible = false;
            _controller.NewGame();
        }


        private void OnStartGameButtonClick(object sender, System.EventArgs e)
        {
            _shuffleButton.Visible = false;
            _newGameButton.Visible = false;
            _startGameButton.Visible = false;
            _controller.StartGame();
        }

        private void OnShuffleButtonClick(object sender, System.EventArgs e)
        {
            _humanBoard.AddRandomShips();
        }

        private void OnGameEnded(object sender, System.EventArgs e)
        {
            _shuffleButton.Visible = false;
            _startGameButton.Visible = false;
            _newGameButton.Visible = true;
            _computerBoard.ShowShips();
        }




        private void SetupWindow()
        {
            AutoScaleDimensions = new SizeF(8, 19);
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Calibri", 10, FontStyle.Regular, GraphicsUnit.Point, 186);
            Margin = Padding.Empty;
            Text = "SeaBattle.NET";
            BackColor = Color.FromArgb(235, 235, 235);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
        }

        private static Button CreateButton(string text, Color backColor)
        {
            var button = new Button
                             {
                                 FlatStyle = FlatStyle.Flat,
                                 ForeColor = Color.White,
                                 BackColor = backColor,
                                 UseVisualStyleBackColor = false,
                                 Size = new Size(40, 40),
                                 Text = text,
                                 Font = new Font("Webdings", 24, FontStyle.Regular, GraphicsUnit.Point),
                                 TextAlign = ContentAlignment.TopCenter,
                             };
            button.FlatAppearance.BorderSize = 0;

            return button;
        }

        private void LayoutControls()
        {
            _humanBoard.Location = new Point(0, 0);
            _computerBoard.Location = new Point(_humanBoard.Right + 150, 0);
            _scoreboard.Location = new Point(25, _humanBoard.Bottom );
            _scoreboard.Width = _computerBoard.Right - 25;

            _connectButton.Location = new Point(_humanBoard.Right+25, _scoreboard.Bottom);
            _newGameButton.Location = new Point(_connectButton.Right, _scoreboard.Bottom);
            _startGameButton.Location = _newGameButton.Location;
            _shuffleButton.Location = new Point(_newGameButton.Right, _newGameButton.Location.Y);
            _gbMySocket.Location = new Point(_humanBoard.Left, _scoreboard.Bottom);
            _gbFriendSocket.Location = new Point(_computerBoard.Left, _scoreboard.Bottom);

            tbMyIP.Location = new Point (_humanBoard.Left + 25, _scoreboard.Bottom);
            tbMyPort.Location = new Point(_humanBoard.Left + 25, _scoreboard.Bottom + 25);

            friendIP.Location = new Point(_computerBoard.Left + 175, _scoreboard.Bottom);
            friendPort.Location = new Point(_computerBoard.Left + 175, _scoreboard.Bottom + 25);


            pnBot.Top = tbMyPort.Bottom;
            pnBot.Height = 138;
            
            Controls.AddRange(new Control[]
                                  {
                                      _humanBoard,
                                      _computerBoard,
                                      _scoreboard,
                                      _newGameButton,
                                      _startGameButton,
                                      _connectButton,
                                      _shuffleButton,
                                      //_richTextBox,
                                      //_gbMySocket,
                                      //_gbFriendSocket,
                                      tbMyIP,
                                      tbMyPort,
                                      friendIP,
                                      friendPort,
                                      pnBot
                                  });

            ClientSize = new Size(_computerBoard.Right+25, 570);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(620, 501);
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }

        private void ShowMessage(string Message)
        {
            _richTextBox.Invoke(new Action(() => { _richTextBox.AppendText(">> " + Message + "\r\n"); }));
        }
    }
}
