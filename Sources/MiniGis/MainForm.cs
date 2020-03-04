﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TwoDimensionalFields.Drawing.Styling;
using TwoDimensionalFields.Grids;
using TwoDimensionalFields.MapObjects;
using TwoDimensionalFields.Maps;
using TwoDimensionalFields.Parsers;
using Point = TwoDimensionalFields.MapObjects.Point;

namespace MiniGis
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ButtonSelect.Checked = true;
            toolStripStatusLabelMouse.Text = string.Empty;
            toolStripStatusLabelArea.Text = string.Empty;
        }

        private void AddLayersFromFiles(object sender, EventArgs e)
        {
            openFileDialog.Filter = "MIF|*.mif";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string[] filePathName = openFileDialog.FileNames;
            string[] filename = openFileDialog.SafeFileNames;
            for (int i = 0; i < filePathName.Length; ++i)
            {
                var layerName = Path.GetFileNameWithoutExtension(filename[i]);
                var layer = new Layer(layerName);
                var parser = new MifParser(filePathName[i]);

                foreach (var mapObject in parser.Data)
                {
                    layer.Add(mapObject);
                }

                mapControl.AddLayer(layer);
                layersControl.UpdateLayers();
            }
        }

        private void ButtonCalcRegular_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonPan_Click(object sender, EventArgs e)
        {
            mapControl.ActiveTool = MapToolType.Pan;
            ButtonSelect.Checked = false;
            ButtonPan.Checked = true;
            ButtonZoomIn.Checked = false;
            ButtonZoomOut.Checked = false;
        }

        private void ButtonSelect_Click(object sender, EventArgs e)
        {
            mapControl.ActiveTool = MapToolType.Select;
            ButtonSelect.Checked = true;
            ButtonPan.Checked = false;
            ButtonZoomIn.Checked = false;
            ButtonZoomOut.Checked = false;
        }

        private void ButtonZoomAll_Click(object sender, EventArgs e)
        {
            if (layersControl.SelectedItemsCount == 0)
            {
                mapControl.ZoomAll();
            }
            else
            {
                mapControl.ZoomLayers(layersControl.GetSelectedLayers());
            }
        }

        private void ButtonZoomIn_Click(object sender, EventArgs e)
        {
            mapControl.ActiveTool = MapToolType.ZoomIn;
            ButtonSelect.Checked = false;
            ButtonPan.Checked = false;
            ButtonZoomIn.Checked = true;
            ButtonZoomOut.Checked = false;
        }

        private void ButtonZoomOut_Click(object sender, EventArgs e)
        {
            mapControl.ActiveTool = MapToolType.ZoomOut;
            ButtonSelect.Checked = false;
            ButtonPan.Checked = false;
            ButtonZoomIn.Checked = false;
            ButtonZoomOut.Checked = true;
        }

        private void DisplayGridValue()
        {
            toolStripStatusLabelValue.Text = string.Empty;

            if (!mapControl.SelectedValues.Any())
            {
                return;
            }

            foreach (var gridValue in mapControl.SelectedValues)
            {
                toolStripStatusLabelValue.Text += $"\"{gridValue.Key.Name}\": {Math.Round(gridValue.Value, 4)}";
            }
        }

        private void DisplayPolygonArea()
        {
            toolStripStatusLabelArea.Text = string.Empty;

            if (mapControl.SelectedObjects.Count != 1)
            {
                return;
            }

            if (!(mapControl.SelectedObjects.First() is Polygon polygon))
            {
                return;
            }

            toolStripStatusLabelArea.Text += $"Polygon's Area = {Math.Round(polygon.Area(), 4)}";
        }

        private void InitTestGrid()
        {
            var colors = new Dictionary<double, Color>
            {
                [0] = Color.Blue,
                [25] = Color.Cyan,
                [50] = Color.Green,
                [75] = Color.Yellow,
                [100] = Color.Red
            };

            var nodes = new[]
            {
                new Node3d<double>(10, 30, 1),
                new Node3d<double>(20, 40, 2),
                new Node3d<double>(20, 30, 3),
                new Node3d<double>(50, 10, 4),
                new Node3d<double>(30, 50, 5),
            };

            var grid1 = new IrregularGrid(nodes);
            var grid2 = RegularGridFactory.Create(nodes, 2);
            var grid3 = RegularGridFactory.CreateTestGrid();

            grid1.SetColors(colors);
            grid2.SetColors(colors);
            grid3.SetColors(colors);

            grid1.Name = "Нерегулярная матрица";
            grid2.Name = "Расчитанная регулярная матрица";
            grid3.Name = "Тестовая матрица";

            mapControl.AddLayer(grid3);
            mapControl.AddLayer(grid2);
            mapControl.AddLayer(grid1);
        }

        private void InitTestLayers()
        {
            var layer1 = new Layer("Test1");
            var layer2 = new Layer("Test2");
            var layer3 = new Layer("Test3");

            var point = new Point(0, 500);
            var line1 = new Line(0, -50, 0, 50);
            var line2 = new Line(-50, 0, 50, 0);
            var lineStyle = new Style
            {
                Pen = new Pen(Color.Red, 2)
                {
                    DashStyle = DashStyle.Dash
                }
            };
            line1.Style = lineStyle;
            line2.Style = lineStyle;

            var polyline = new Polyline();
            polyline.AddNode(0, 0);
            polyline.AddNode(200, 100);
            polyline.AddNode(200, -100);

            var polygon1 = new Polygon();
            polygon1.AddNode(0, 0);
            polygon1.AddNode(-200, 100);
            polygon1.AddNode(-200, -100);
            var polygonStyle = new Style
            {
                Brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Red, Color.Green)
            };
            polygon1.Style = polygonStyle;
            layer1.Add(polygon1);

            var polygon2 = new Polygon();
            polygon2.AddNode(0, 0.111);
            polygon2.AddNode(0.55, 0.99);
            polygon2.AddNode(0.8, 0.7);
            polygon2.Style = polygonStyle;
            layer1.Add(polygon2);

            int b = -349;
            for (int x = 0; x < 100; x++)
            {
                int a = -349;
                for (int j = 0; j < 100; j++)
                {
                    Polygon polygon = new Polygon();
                    polygon.AddNode(a, b);
                    polygon.AddNode(a + 5, b);
                    polygon.AddNode(a + 5, b + 5);
                    polygon.AddNode(a, b + 5);
                    layer1.Add(polygon);
                    a += 7;
                }

                b += 7;
            }

            layer2.Add(line1);
            layer2.Add(line2);
            layer2.Add(polyline);
            layer3.Add(point);

            mapControl.AddLayer(layer1);
            mapControl.AddLayer(layer2);
            mapControl.AddLayer(layer3);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitTestLayers();
            InitTestGrid();

            layersControl.MapControl = mapControl;
            layersControl.AddLayer += AddLayersFromFiles;
            layersControl.UpdateLayers();
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            (double x, double y) = mapControl.ScreenToMap(e.Location);
            toolStripStatusLabelMouse.Text = $"x = {Math.Round(x, 4)}; y = {Math.Round(y, 4)}";
        }

        private void map_MouseUp(object sender, MouseEventArgs e)
        {
            DisplayPolygonArea();
            DisplayGridValue();
        }
    }
}
