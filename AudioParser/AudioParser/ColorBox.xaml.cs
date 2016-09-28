using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioParser
{
    /// <summary>
    /// Логика взаимодействия для ColorBox.xaml
    /// </summary>
    public partial class ColorBox : UserControl
    {
        /// <summary>
        /// Конструктор класса.
        /// </summary>
        public ColorBox()
        {
            InitializeComponent();

            SetColor(0, 0, 0);
        }

        /// <summary>
        /// Функция установки цвета.
        /// </summary>
        /// <param name="r">Интенсивность красного цвета.</param>
        /// <param name="g">Интенсивность зеленого цвета.</param>
        /// <param name="b">Интенсивность голубого цвета.</param>
        public void SetColor(byte r, byte g, byte b)
        {
            BarR.Value = (int)(r / 2.55); ;
            BarG.Value = (int)(g / 2.55); ;
            BarB.Value = (int)(b / 2.55); ;
            Canvas.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
