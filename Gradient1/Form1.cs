using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gradient1
{
    public partial class Form1 : Form
    {

        class ColorPoint
        {
            public ColorPoint(int x, int y, int r, int g, int b)
            {
                Point = new Point(x, y);
                Color = Color.FromArgb(r, g, b);
            }


            public Point Point { get; set; }
            public Color Color { get; set; }
        }

        class ColorTriangle
        {
            /// <summary>
            /// вершина, из которой идут отрезки до точек в основании треугольника
            /// </summary>
            public ColorPoint Top { get; set; }

            /// <summary>
            /// первая точка основания треугольника
            /// </summary>
            public ColorPoint Base1 { get; set; }

            /// <summary>
            /// вторая точка основания треугольника
            /// </summary>
            public ColorPoint Base2 { get; set; }
        }

        const int matrixSize = 400;
        
        List<ColorTriangle> _triangles;

        public Form1()
        {
            InitializeComponent();


            _triangles = new List<ColorTriangle>() {

                new ColorTriangle() { //     x    y    r    g    b
                    Top =   new ColorPoint(200,   0, 255,   0,   0),
                    Base1 = new ColorPoint(  0, 300,   0, 255,   0),
                    Base2 = new ColorPoint(380, 380,   0,   0, 255)
                },
                new ColorTriangle() {
                    Top =   new ColorPoint(  0, 300,   0, 255,   0),
                    Base1 = new ColorPoint(380, 380,   0,   0, 255),
                    Base2 = new ColorPoint(  0, 380, 255,   0,   0)
                },
                new ColorTriangle() {
                    Top =   new ColorPoint(  0,   0,   0,   0, 255),
                    Base1 = new ColorPoint(200,   0, 255,   0,   0),
                    Base2 = new ColorPoint(  0, 300,   0, 255,   0)
                },
                new ColorTriangle() {
                    Top =   new ColorPoint(200,   0, 255,   0,   0),
                    Base1 = new ColorPoint(380, 380,   0,   0, 255),
                    Base2 = new ColorPoint(380,   0,   0, 255,   0)
                },
            };


        }

        /// <summary>
        /// Вычитание Point
        /// </summary>
        Point PointSubstract(Point p1, Point p2) => new Point(p2.X - p1.X, p2.Y - p1.Y);

        /// <summary>
        /// Сложение Point
        /// </summary>
        Point PointSumm(Point p1, Point p2) => new Point(p2.X + p1.X, p2.Y + p1.Y);

        /// <summary>
        /// Градусы в радианы
        /// </summary>        
        double DegToRad(double a) => Math.PI * a / 180.0;
        
        /// <summary>
        /// расстояние между точками
        /// </summary>
        double PointDistance(Point p1, Point p2)
        {
            var diff = PointSubstract(p1, p2);
            return Math.Sqrt(Math.Pow(diff.X, 2) + Math.Pow(diff.Y, 2));
        }
        
        /// <summary>
        /// угол между точками в градусах
        /// </summary>
        double PointAngleDeg(Point p1, Point p2)
        {
            var diff = PointSubstract(p1, p2);
            return Math.Atan2(diff.Y, diff.X) * 180 / Math.PI;
        }

        /// <summary>
        /// вычисление промежуточных между cp1 и cp2 точек и цветов с последующей передачей этих данных в callback colorPointProcessingAction
        /// </summary>
        void DrawGradientLine(ColorPoint cp1, ColorPoint cp2, Action<ColorPoint> colorPointProcessingAction)
        {
            
            var distance = PointDistance(cp1.Point, cp2.Point);

            var dr = (cp2.Color.R - cp1.Color.R) / distance;
            var dg = (cp2.Color.G - cp1.Color.G) / distance;
            var db = (cp2.Color.B - cp1.Color.B) / distance;

            double r = cp1.Color.R;
            double g = cp1.Color.G;
            double b = cp1.Color.B;

            var angle = PointAngleDeg(cp1.Point, cp2.Point);

            var p1 = cp1.Point;

            var c = cp1.Color;


            for (int radius = 0; radius < Math.Round(distance); radius++)
            {
                var p2 = new Point(
                    (int)Math.Round(Math.Cos(DegToRad(angle)) * radius + cp1.Point.X),
                    (int)Math.Round(Math.Sin(DegToRad(angle)) * radius + cp1.Point.Y)
                    );

                if (p2.X != p1.X || p2.Y != p1.Y)
                {
                    r += dr;
                    g += dg;
                    b += db;

                    c = Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
                }
                
                colorPointProcessingAction(new ColorPoint(p2.X, p2.Y, c.R, c.G, c.B));

                p1 = p2;

            }
        }



        private void Button1_Click(object sender, EventArgs e)
        {

            var bmp = new Bitmap(matrixSize, matrixSize);
            pictureBox1.Refresh();

            // меряем время
            Stopwatch sw = new Stopwatch();

            sw.Start();

            foreach (var t in _triangles)
            {
                // приблизительная иллюстрация происходящего
                // https://i.imgur.com/sWXGYpF.png
                // https://files.catbox.moe/7xo2cp.webm

                // вычисляем точки и цвета для основания треугольника и передаем их дальше для рисования отрезка от вершины
                DrawGradientLine(t.Base1, t.Base2, (basecp) =>
                {
                    // рисуем отрезок от вершины до вычисленной выше точки на основании треугольника
                    DrawGradientLine(t.Top, basecp, (toptobasecp) => bmp.SetPixel(toptobasecp.Point.Y, toptobasecp.Point.X, toptobasecp.Color));

                    // debug points
                    // DrawGradientLine(t.Top, basecp, (toptobasecp) => {
                    //     _bmp.SetPixel(toptobasecp.Point.Y, toptobasecp.Point.X, toptobasecp.Color);
                    //     pictureBox1.Image = bmp;
                    //     pictureBox1.Refresh();
                    // });

                    // debug lines
                    // pictureBox1.Image = bmp;
                    // pictureBox1.Refresh();
                });

            }

            sw.Stop();

            Text = $"Generated and drawn in {sw.ElapsedMilliseconds} ms";


            pictureBox1.Image = bmp;
        }
    }
}
