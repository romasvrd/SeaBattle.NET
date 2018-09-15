using System;

namespace SeaBattle.CSharp
{
    public class GameController
    {
        private readonly Board _board1;
        private readonly Board _board2;
        private readonly ScoreBoard _scoreBoard;

        public GameController(Board board1, Board board2, ScoreBoard scoreBoard)
        {
            _board1 = board1;
            _board2 = board2;
            _scoreBoard = scoreBoard;
        }   

        //новая игра - переключение полей в режим дизайна и добавление случайных кораблей
        public void NewGame()
        {
            _board1.Mode = BoardMode.Design;
            _board2.Mode = BoardMode.Design;
            _board1.AddRandomShips();
            _board2.ClearBoard();
            _scoreBoard.NewGame();
        }

        //запуск игры - переключение полей в режим игры
        public void StartGame()
        {
            _board1.Mode = BoardMode.Game;
            _board2.Mode = BoardMode.Game;
            _scoreBoard.NewGame();
        }

        //выстрел от противника
        public void shootResult(int X, int Y)
        {
            ShotResult res = _board1.OpenentShotAt(X, Y);
            if(res == ShotResult.Missed)            //если противник промахнулся
            {
                _board2.Mode = BoardMode.Game;      //разрешаем себе стрелять в поле противника
                _scoreBoard.TakeControl();          //принимаем ход
            }
            else if(res == ShotResult.ShipDrowned)  //иначе, если противник потопил корабль
            {
                _scoreBoard.EnemyDrownedMe();       //выполняем обработчик потопления своего корабля
            }
        }

        //результат выстрела в поле противника
        public void friendShootResult(int X, int Y, ShotResult State)
        {
            switch(State)
            {
                case ShotResult.Missed:             //если я промахнулся
                    _board2._cells[X, Y].State = BoardCellState.MissedShot;     //помечаю ячейку промахом
                    _board2.Mode = BoardMode.Yeld;  //запрещаем себе стрелять в поле противника
                    _scoreBoard.YeldControl();      //передаем ход противнику
                    break;
                case ShotResult.ShipHit:            //если я попал в противника
                    _board2._cells[X, Y].State = BoardCellState.ShotShip;       //помечаю ячейку подстреленой
                    break;
                case ShotResult.ShipDrowned:        //если я потопил корабль
                    _board2._cells[X, Y].State = BoardCellState.ShowDrowned;    //помечаю ячейку потопленной
                    _scoreBoard.IDrownedEnemy();    //вызываю обработчик потопления корабля противника
                    break;
            }
            _board2.Invoke(new System.Action(() => _board2.Refresh())); //перерисовываем поле противника
        }
    }
}