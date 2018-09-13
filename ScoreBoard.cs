using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle.CSharp
{
    public class ScoreBoard : TableLayoutPanel
    {
        private readonly Player _player1;
        private readonly Player _player2;
        private readonly int _shipsPerGame;
        private readonly int _shotsPerGame;
        private readonly Label _scoreLabel;

        private const string PlayerStatsTemplate = "Ships left: {0}";
        private const string ScoreTemplate = "{0} : {1}";

        private static readonly Color ActivePlayerColor = Color.FromArgb(255,174,0);
        private static readonly Color InactivePlayerColor = Color.FromArgb(128, 128, 128);
        private static readonly Color PlayerStatsColor = Color.FromArgb(128, 128, 128);
        private static readonly Color WinnerColor = Color.FromArgb(32, 167, 8);
        private static readonly Color LooserColor = Color.FromArgb(222, 0, 0);

        private static readonly Color ScoreColor = Color.Black;

        private readonly Pair<Label, Label> _playerNames;
        private readonly Pair<Label, Label> _playerStats;

        private Point _score;
        private Point _shipsLeft;



        public ScoreBoard(Player player1, Player player2, int shipsPerGame, int shotsPerGame)
        {
            SuspendLayout();
            _player1 = player1;
            _player2 = player2;
            _shipsPerGame = shipsPerGame;
            _shotsPerGame = shotsPerGame;
            

            _player1.Shot += OnPlayerMadeShot;
            _player2.Shot += OnPlayerMadeShot;


            var firstPlayerNameLabel = CreateLabel(_player1.Name, InactivePlayerColor);
            var secondPlayerNameLabel = CreateLabel(_player2.Name, InactivePlayerColor);
            _playerNames = new Pair<Label, Label>(firstPlayerNameLabel, secondPlayerNameLabel);


            var firstPlayerStatsLabel = CreateLabel(string.Empty, PlayerStatsColor);
            var secondPlayerStatsLabel = CreateLabel(string.Empty, PlayerStatsColor);
            _playerStats = new Pair<Label, Label>(firstPlayerStatsLabel, secondPlayerStatsLabel);

            _scoreLabel = CreateLabel("", ScoreColor);

            RefreshScore();
            InitPlayerStats();

            ResumeLayout();

        }


        private void InitPlayerStats()
        {
            _shipsLeft = new Point(_shipsPerGame, _shipsPerGame);
            RefreshPlayerStats();
        }

        private static Label CreateLabel(string text, Color color)
        {
            return new Label
                       {
                           AutoSize = true,
                           Text = text,
                           Dock = DockStyle.Fill,
                           Margin = Padding.Empty,
                           Padding = Padding.Empty,
                           ForeColor = color,
                           TextAlign = ContentAlignment.TopLeft
                       };
        }

        private void OnPlayerMadeShot(object sender, ShootingEventArgs e)
        {
            if (sender == _player1)
            {
                if (e.Result == ShotResult.ShipDrowned)
                    _shipsLeft.Y--;
            }
            else
            {
                if (e.Result == ShotResult.ShipDrowned)
                    _shipsLeft.X--;
            }
            TrackResult();
            RefreshPlayerStats();
        }

        private void RefreshPlayerStats()
        {
            if(_shipsLeft.X == 10 && _shipsLeft.Y == 10)
            {
                _playerStats.First.Text = string.Format(PlayerStatsTemplate, _shipsLeft.X);
                _playerStats.Second.Text = string.Format(PlayerStatsTemplate, _shipsLeft.Y);
            }
            else
            {
                _playerStats.First.Invoke(new Action(() => _playerStats.First.Text = string.Format(PlayerStatsTemplate, _shipsLeft.X)));
                _playerStats.Second.Invoke(new Action(() => _playerStats.Second.Text = string.Format(PlayerStatsTemplate, _shipsLeft.Y)));

            }
        }

        public bool GameHasEnded()
        {
            return _shipsLeft.X == 0 || _shipsLeft.Y == 0;
        }

        private void TrackResult()
        {
            if (!GameHasEnded())
                return;

            Color color1;
            Color color2;

            if (_shipsLeft.X == 0)
            {
                _score.Y++;
                color1 = LooserColor;
                color2 = WinnerColor;
            }
            else
            {
                _score.X++;
                color1 = WinnerColor;
                color2 = LooserColor;
            }

            _playerNames.First.ForeColor = color1;
            _playerNames.Second.ForeColor = color2;

            OnGameEnded();

            var handler = GameEnded;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void RefreshScore()
        {
            if (_score.X == 0 && _score.Y == 0)
            {
                _scoreLabel.Text = string.Format(ScoreTemplate, _score.X, _score.Y);
            }
            else
            {
                _scoreLabel.Invoke(new Action(() => _scoreLabel.Text = string.Format(ScoreTemplate, _score.X, _score.Y)));
            }
        }

        private void OnGameEnded()
        {
            RefreshScore();
        }

        public event EventHandler GameEnded;


        public void NewGame()
        {
            InitPlayerStats();
            RefreshPlayerStats();
            _playerNames.First.ForeColor = InactivePlayerColor;
            _playerNames.Second.ForeColor = InactivePlayerColor;
        }

        public void YeldControl()
        {
            _playerNames.First.ForeColor = InactivePlayerColor;
            _playerNames.Second.ForeColor = ActivePlayerColor;
        }

        public void TakeControl()
        {
            _playerNames.First.ForeColor = ActivePlayerColor;
            _playerNames.Second.ForeColor = InactivePlayerColor;
        }

        public void IDrownedEnemy()
        {
            _shipsLeft.Y--;
            TrackResult();
            RefreshPlayerStats();
        }

        public void EnemyDrownedMe()
        {
            _shipsLeft.X--;
            TrackResult();
            RefreshPlayerStats();
        }
        #region Layout

        private void AddLayoutColumns()
        {

            ColumnCount = 3;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        }

        private void AddLayoutRows()
        {
            RowCount = 2;
            RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
            RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
        }

        protected override void InitLayout()
        {
            base.InitLayout();
            Padding = Margin = Padding.Empty;
            Font = Parent.Font;
            
            AddLayoutColumns();
            AddLayoutRows();

            _playerNames.First.Font = new Font(Font.FontFamily,30);
            Controls.Add(_playerNames.First, 0, 0);

            _playerNames.Second.Font = new Font(Font.FontFamily, 30);
            _playerNames.Second.TextAlign = ContentAlignment.TopRight;
            Controls.Add(_playerNames.Second, 2, 0);

            _scoreLabel.Font = new Font(Font.FontFamily, 30, FontStyle.Bold);
            _scoreLabel.TextAlign = ContentAlignment.TopCenter;
            Controls.Add(_scoreLabel, 1, 0);

            _playerStats.First.Font = Font;
            Controls.Add(_playerStats.First, 0, 1);

            _playerStats.Second.Font = Font;
            _playerStats.Second.TextAlign = ContentAlignment.TopRight;
            Controls.Add(_playerStats.Second, 2, 1);

            Debug.WriteLine(_playerStats.First.Font.Name);


            Height = _playerNames.First.Height + _playerStats.First.Height;
        }



        #endregion

    }
}