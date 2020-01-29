using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApplication4
{
    public partial class Form1 : Form
    {
        private GameField field = new GameField();
        private StateCell currentPlayer = StateCell.Red;
        private const int CELL_SIZE = 20;

        public Form1()
        {
            Logo fm = new Logo();
            fm.ShowDialog();
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            var p = new Point((int)Math.Round(1f * e.X / CELL_SIZE), (int)Math.Round(1f * e.Y / CELL_SIZE));
            if (field[p] == StateCell.Empty)
            {
                field.SetPoint(p, currentPlayer);
                currentPlayer = GameField.Inverse(currentPlayer);
                field.DecCountPoint(1);
                if (field.CountPoint <= 0)
                {
                    int redPoint = Convert.ToInt32(label1.Text);
                    int bluePoint = Convert.ToInt32(label2.Text);
                    if (bluePoint > redPoint)
                    {
                        MessageBox.Show("Игра окончена\n Cчет " + label1.Text +
                                        ":" + label2.Text + "\n Победа синих");
                    }
                    else
                    {
                        MessageBox.Show("Игра окончена\n Cчет " + label1.Text +
                                           ":" + label2.Text + "\n Победа красных");
                    }
                }
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.ScaleTransform(CELL_SIZE, CELL_SIZE);
            //рисуем сетку
            using (var pen = new Pen(Color.Gainsboro, 0.1f))
            {
                for (int x = 0; x < GameField.SIZE; x++)
                    e.Graphics.DrawLine(pen, x, 0, x, GameField.SIZE - 1);
                for (int y = 0; y < GameField.SIZE; y++)
                    e.Graphics.DrawLine(pen, 0, y, GameField.SIZE - 1, y);
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //рисуем занятые области
            foreach(var item in field.TakenAreas)
            {
                var state = item.Item1;
                var area = item.Item2;
                var contour = field.GetContour(area);

                //рисуем контур
                using (var pen = new Pen(Color.White, 0.1f))
                using (var brush = new SolidBrush(Color.White))
                {
                    pen.Color = StateToColor(state);
                    brush.Color = StateToColor(state, 100);
                    e.Graphics.FillPolygon(brush, contour.ToArray());
                    e.Graphics.DrawPolygon(pen, contour.ToArray());
                }
            }

            //рисуем выставленные точки
            using(var brush = new SolidBrush(Color.White))
            for (int x = 0; x < GameField.SIZE; x++)
            for (int y = 0; y < GameField.SIZE; y++)
            {
                var p = new Point(x, y);
                var cell = field[p];
                if (cell != StateCell.Empty)
                {
                    brush.Color = StateToColor(cell);
                    e.Graphics.FillEllipse(brush, x - 0.2f, y - 0.2f, 0.4f, 0.4f);
                }
            }
        }

        Color StateToColor(StateCell state, byte alpha = 255)
        {
            var res = state == StateCell.Blue ? Color.Blue : Color.Red;
            return Color.FromArgb(alpha, res);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 fm = new Form2();
            fm.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            History fm = new History();
            fm.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
