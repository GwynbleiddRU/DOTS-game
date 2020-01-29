using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication4
{
    /// <summary>
    /// Поле игры
    /// </summary>
    class GameField
    {
        public const int SIZE = 20;
        public StateCell[,] cells = new StateCell[SIZE, SIZE];
        public List<Tuple<StateCell, HashSet<Point>>> TakenAreas = new List<Tuple<StateCell, HashSet<Point>>>();
        public int CountPoint = SIZE*SIZE;
        public  void DecCountPoint (int n)
        {
            CountPoint -= n;
        }
        public StateCell this[Point p]
        {
            get
            {
                if (p.X < 0 || p.X >= SIZE || p.Y < 0 || p.Y >= SIZE)
                    return StateCell.OutOfGameField;
                return cells[p.X, p.Y];
            }

            set { cells[p.X, p.Y] = value; }
        }

        public IEnumerable<Point> GetFreeState4(Point p)
        {
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X, p.Y - 1);
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X, p.Y + 1);
        }

        public IEnumerable<Point> GetFreeState8(Point p)
        {
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X - 1, p.Y - 1);
            yield return new Point(p.X, p.Y - 1);
            yield return new Point(p.X + 1, p.Y - 1);
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X + 1, p.Y + 1);
            yield return new Point(p.X, p.Y + 1);
            yield return new Point(p.X - 1, p.Y + 1);
        }

        /// <summary>
        /// Поиск замкнутых областей
        /// </summary>
        private IEnumerable<HashSet<Point>> GetFullArea(Point lastPoint)
        {
            int count=0;
            var myState = this[lastPoint];
            //перебираем пустые точки в округе и пытаемся пробиться из них к краю поля
            foreach (var n in GetFreeState4(lastPoint))
            if (this[n] != myState)
            {
                //ищем замкнутую область
                var list = GetFullArea(n, myState);
                if (list != null)//найден контур
                {
                    yield return list;//возвращаем занятые точки
                   
                    // Тестовое сообщение
                    //MessageBox.Show("Count point-" + list.Count.ToString());
                    if (myState == StateCell.Red)
                    {
                        count = 0;
                        foreach (var pn in list)
                            if (this[pn] == StateCell.Empty)
                            {
                                count++;
                            }
                            else
                                this[pn] = StateCell.Red;
                        MessageBox.Show("Окружено " + (list.Count - count).ToString() + " синих точек");
                        (Application.OpenForms[0] as Form1).label1.Text = (Convert.ToInt32((Application.OpenForms[0] as Form1).label1.Text)+(list.Count - count)).ToString();
                    }
                    if (myState == StateCell.Blue)
                    {
                        count = 0;
                        foreach (var pn in list)
                            if (this[pn] == StateCell.Empty)
                            {
                                count++;
                            }
                            else
                                this[pn] = StateCell.Blue;
                        MessageBox.Show("Окружено " + (list.Count - count).ToString() + " красных точек");
                        (Application.OpenForms[0] as Form1).label2.Text = (Convert.ToInt32((Application.OpenForms[0] as Form1).label2.Text) + (list.Count - count)).ToString();
                    }
                    DecCountPoint(list.Count);
					//Тестовое сообщение
                    //MessageBox.Show("Free Point " + CountPoint.ToString());
                }
            }
        }

        /// <summary>
        /// Заливка области, начинаем начальной точки (точка затравки), 
        /// если не получается выйти к краю игрового поля - возвращается набор точек в замкной
        /// области
        /// </summary>
        private HashSet<Point> GetFullArea(Point pos, StateCell myState)
        {
            //рекурсивный алгоритм заливки
            var stack = new Stack<Point>();
            var visit_Point = new HashSet<Point>();
            stack.Push(pos);
            visit_Point.Add(pos);
            while (stack.Count > 0)
            {
                var p = stack.Pop();
                var state = this[p];
                //если найден выход к границе игрового поля - область не замкнута, возвращается значение null
                if (state == StateCell.OutOfGameField)
                    return null;
                //рекурсивный перебор соседних точек
                foreach (var n in GetFreeState4(p))
                if (this[n] != myState)
                if (!visit_Point.Contains(n))
                {
                    visit_Point.Add(n);
                    stack.Push(n);
                }
            }

            return visit_Point;
        }
        // Функция замены активного игрока
        public static StateCell Inverse(StateCell state)
        {
            return state == StateCell.Blue ? StateCell.Red : StateCell.Blue;
        }
        // Функция изменения состояния точек
        public void SetPoint(Point pos, StateCell state)
        {
            this[pos] = state;

            foreach (var captured in GetFullArea(pos))
                TakenAreas.Add(new Tuple<StateCell, HashSet<Point>>(state, captured));
        }

        /// <summary>
        /// определение контура области
        /// </summary>
        public IEnumerable<Point> GetContour(HashSet<Point> captured)
        {
            //ищем любую точку из контура
            var start = new Point();
            foreach (var p in captured)
            foreach (var n in GetFreeState4(p))
            if (!captured.Contains(n))
            {
                start = n;
                goto next;
            }

        next:

            //делаем обход по часовой стрелке вдоль области
            yield return start;
            var pp = GetNext(start, captured);
            while (pp != start)
            {
                yield return pp;
                pp = GetNext(pp, captured);
            }
        }
        // Выбор следующей точки для контура
        Point GetNext(Point p, HashSet<Point> captured)
        {
            var temp = GetFreeState8(p).ToList();
            var list = new List<Point>(temp);
            list.AddRange(temp);
            for (int i = 0; i < list.Count - 1; i++)
            if (!captured.Contains(list[i]) && captured.Contains(list[i + 1]))
                return list[i];

            throw new Exception("Error!!!");
        }
    }
    // Перечисление состояний точки игрового поля
    enum StateCell
    {
        Empty, Red, Blue, OutOfGameField
    }
}
