using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CloudInvent.Cheetah.Data.Geometry;

namespace Cheetah.ExampleViewer
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        List<Type> _availableExample;

        Type _typeSelected;

        ICheetahExample _currentExample;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load all example class
            // These class need to implement "ICheetahExample" 
            _availableExample = Assembly.GetExecutingAssembly()
                                         .GetTypes()
                                         .Where(t => typeof(ICheetahExample).IsAssignableFrom(t) &&
                                         t != typeof(ICheetahExample))
                                         .OrderBy(t => t.Name)
                                         .ToList();

            var exampleListName = new List<string>();

            foreach (var exampleClass in _availableExample)
            {
                var name = exampleClass.GetAttributeValue((DisplayNameAttribute att) => att.DisplayName);

                if (string.IsNullOrWhiteSpace(name))
                    name = exampleClass.Name;

                exampleListName.Add(name);
            }

            //Sort by name
            exampleListName = exampleListName.OrderBy(ex => ex).ToList();

            cbExampleList.ItemsSource = exampleListName;

            //Pick the first as default
            cbExampleList.SelectedItem = exampleListName.FirstOrDefault();

        }

        /// <summary>
        /// 
        /// </summary>
        private void cbExampleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var exampleName = cbExampleList.SelectedValue.ToString();

            if (string.IsNullOrWhiteSpace(exampleName))
                return;

            foreach (var exampleClass in _availableExample)
            {
                var name = exampleClass.GetAttributeValue((DisplayNameAttribute att) => att.DisplayName);

                if (string.IsNullOrWhiteSpace(name))
                    name = exampleClass.Name;

                if (exampleName == name)
                {
                    _typeSelected = exampleClass;

                    //Call reset
                    ResetBtnClik(null, null);
                }
            }
        }

        /// <summary>
        /// Called on example property changed
        /// </summary>
        private void propertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            UpdateScene();
        }

        private void UpdateScene()
        {
            ClearViewport();

            if (_currentExample == null) return;

            _currentExample.Run();

            var entities = _currentExample.GetCurrentElements();

            PrintElements(entities);

            Viewport1.ZoomExtents();


        }

        private void PrintElements(List<CheetahCurve> entities)
        {
            if (entities == null) return;

            foreach (var e in entities)
            {
                var cLine = e as CheetahLine2D;
                var cArc = e as CheetahArc2D;

                if (cLine != null)
                {
                    var line = new LinesVisual3D { Color = Colors.Blue, Thickness = 3 };
                    line.Points.Add(new Point3D(cLine.Start.X, cLine.Start.Y, 0));
                    line.Points.Add(new Point3D(cLine.End.X, cLine.End.Y, 0));
                    Viewport1.Children.Add(line);
                }

                else if (cArc != null)
                {
                    var arc = new LinesVisual3D { Color = Colors.DarkBlue, Thickness = 3 };
                    var pnts = GetPoints(cArc);
                    foreach (var p in pnts)
                        arc.Points.Add(p);
                    Viewport1.Children.Add(arc);
                }
            }
        }


        private static List<Point3D> GetPoints(CheetahArc2D arc)
        {
            var rad = .05;

            var pntList = new List<Point3D>();

            var center = arc.Center;

            var startAngle = Math.Atan2(arc.Start.Y - center.Y, arc.Start.X - center.X);

            var endAngle = Math.Atan2(arc.End.Y - center.Y, arc.End.X - center.X);

            // pensavo di fare questo valore proporzionato alla misura del raggio,
            // per ora lo lascio cosi..

            if (arc.IsClockwise)
            {
                /* 
                 * Se il senso è antiorario l'angolo finale dovra essere maggiore
                 */

                if (startAngle < 0)
                {
                    startAngle += Math.PI * 2;
                    endAngle += Math.PI * 2;
                }

                if (endAngle >= startAngle)
                    endAngle -= 2 * Math.PI;

                var deltaAngle = endAngle - startAngle;

                if (Math.Abs(deltaAngle) < .00001)
                {
                    /* è un cerchio completo*/
                    endAngle -= Math.PI * 2;
                    //     deltaAngle = endAngle - startAngle;

                }


                var ampiezza = startAngle - endAngle;

                int nSpicchi;
                var angleIncrement = GetIncremento(ampiezza, rad, out nSpicchi);


                for (var j = 0; j <= nSpicchi; j++)
                {
                    var angoloCorrente = startAngle - (j * angleIncrement);

                    var x1 = (Math.Cos(angoloCorrente) * arc.Radius) + center.X;
                    var y1 = (Math.Sin(angoloCorrente) * arc.Radius) + center.Y;

                    var lastPnt = pntList.LastOrDefault();

                    var p = new Point3D()
                    {
                        X = x1,
                        Y = y1,
                    };

                    pntList.Add(p);
                }

            }
            else
            {
                if (startAngle < 0)
                {
                    startAngle += Math.PI * 2;
                    endAngle += Math.PI * 2;
                }

                if (endAngle < startAngle)
                    endAngle += 2 * Math.PI;

                var deltaAngle = endAngle - startAngle;

                if (deltaAngle == 0)
                {
                    /* è un cerchio completo*/
                    /* */

                    endAngle += Math.PI * 2;
                    deltaAngle = endAngle - startAngle;

                }

                var ampiezza = endAngle - startAngle;

                int nSpicchi;
                var angleIncrement = GetIncremento(ampiezza, rad, out nSpicchi);

                for (var j = 0; j <= nSpicchi; j++)
                {
                    var angoloCorrente = (j * angleIncrement) + startAngle;

                    var x1 = (Math.Cos(angoloCorrente) * arc.Radius) + center.X;
                    var y1 = (Math.Sin(angoloCorrente) * arc.Radius) + center.Y;


                    var lastPnt = pntList.LastOrDefault();

                    var p = new Point3D()
                    {
                        X = x1,
                        Y = y1,
                    };

                    pntList.Add(p);
                }

            }

            return pntList;
        }
        private static double GetIncremento(double ampiezza, double rad, out int nSpicchi)
        {
            nSpicchi = (int)Math.Ceiling(Math.Round(ampiezza, 5) / rad);

            return ampiezza / nSpicchi;
        }



        private void ClearViewport()
        {
            var grid = Viewport1.Children.OfType<GridLinesVisual3D>().FirstOrDefault();
            Viewport1.Children.Clear();
            if (grid != null)
                Viewport1.Children.Add(grid);
        }

        private void ResetBtnClik(object sender, RoutedEventArgs e)
        {
            _currentExample = Activator.CreateInstance(_typeSelected) as ICheetahExample;

            if (_currentExample == null) return;

            _currentExample.Reset();

            propertyGrid.SelectedObject = _currentExample;

            UpdateScene();

        }

        //private void btnRun_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateScene();
        //}
    }

    //http://stackoverflow.com/questions/2656189/how-do-i-read-an-attribute-on-a-class-at-runtime
    public static class AttributeExtensions
    {
        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }
    }

}
