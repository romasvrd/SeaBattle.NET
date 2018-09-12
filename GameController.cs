using System;

namespace SeaBattle.CSharp
{
    public class GameController
    {
        private readonly Player _player1;
        private readonly Player _player2;
        private readonly Board _board1;
        private readonly Board _board2;
        private readonly ScoreBoard _scoreBoard;

        public GameController(Player player1, Player player2, Board board1, Board board2, ScoreBoard scoreBoard)
        {
            _player1 = player1;
            _player2 = player2;
            _board1 = board1;
            _board2 = board2;
            _scoreBoard = scoreBoard;
        }   

        public void NewGame()
        {
            _board1.Mode = BoardMode.Design;
            _board2.Mode = BoardMode.Design;
            _board1.AddRandomShips();
            _scoreBoard.NewGame();
        }

        public void StartGame()
        {
            _board1.Mode = BoardMode.Game;
            _board2.Mode = BoardMode.Game;
            _scoreBoard.NewGame();
        }

        public void shootResult(int X, int Y)
        {
            ShotResult res = _board1.OpenentShotAt(X, Y);
            if(res == ShotResult.Missed)
            {
                _scoreBoard.TakeControl();
            }
        }

        public void friendShootResult(int X, int Y, ShotResult State)
        {
            switch(State)
            {
                case ShotResult.Missed:
                    _board2._cells[X, Y].State = BoardCellState.MissedShot;
                    break;
                case ShotResult.ShipHit:
                    _board2._cells[X, Y].State = BoardCellState.ShotShip;
                    break;
                case ShotResult.ShipDrowned:
                    _board2._cells[X, Y].State = BoardCellState.ShowDrowned;
                    break;
            }
            if(State == ShotResult.Missed)
            {
                _scoreBoard.YeldControl();
            }
            _board2.Invoke(new System.Action(() => _board2.Refresh()));
        }
    }
}