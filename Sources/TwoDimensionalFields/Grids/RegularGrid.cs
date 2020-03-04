﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TwoDimensionalFields.Drawing;
using TwoDimensionalFields.MapObjects;
using TwoDimensionalFields.Maps;

namespace TwoDimensionalFields.Grids
{
    public class RegularGrid : Grid
    {
        private readonly double?[,] grid;

        public RegularGrid(double?[,] matrix, Node<double> position, double edge) : this()
        {
            grid = matrix;
            Edge = edge;
            Position = position;
        }

        private RegularGrid()
        {
            Visible = true;
            Name = "Regular grid";
            GridBitmap = new RegularGridBitmap(this);
        }

        public int ColumnCount => grid.GetLength(1);
        public double Edge { get; }
        public RegularGridBitmap GridBitmap { get; }
        public double Height => (RowCount - 1) * Edge;

        /// <summary>
        /// Xmin, Ymax
        /// </summary>
        public Node<double> Position { get; }

        public int RowCount => grid.GetLength(0);
        public double Width => (ColumnCount - 1) * Edge;

        public double? this[int i, int j] => grid[i, j];

        public (double i, double j) CoordinatesToIndexes(double x, double y)
        {
            double i = (Position.Y - y) / Edge;
            double j = (x - Position.X) / Edge;

            return (i, j);
        }

        public override double? GetValue(double x, double y)
        {
            (double i, double j) = CoordinatesToIndexes(x, y);

            return GetValueByIndex(i, j);
        }

        public override double? GetValue(Node<double> searchPoint) => GetValue(searchPoint.X, searchPoint.Y);

        public double? GetValueByIndex(double i, double j)
        {
            if (i < 0 || i >= RowCount || j < 0 || j >= ColumnCount)
            {
                return null;
            }

            int iMin = Convert.ToInt32(Math.Floor(i));
            int iMax = iMin + 1;
            int jMin = Convert.ToInt32(Math.Floor(j));
            int jMax = jMin + 1;

            double? z1 = grid[iMin, jMax];
            double? z2 = grid[iMax, jMax];
            double? z3 = grid[iMax, jMin];
            double? z4 = grid[iMin, jMin];

            if (!z1.HasValue || !z2.HasValue || !z3.HasValue || !z4.HasValue)
            {
                return null;
            }

            double? z5 = (j - jMin) * (z2 - z1) + z1;
            double? z6 = (j - jMin) * (z4 - z3) + z3;
            double? value = (i - iMin) * (z6 - z5) + z5;

            return value;
        }

        public Node<double> IndexesToCoordinates(double i, double j)
        {
            double x = j * Edge + Position.X;
            double y = Position.Y - i * Edge;

            return new Node<double>(x, y);
        }

        public override void SetColors(Dictionary<double, Color> colors) => GridBitmap.Colors = colors;

        public void SetValue(int i, int j, double? value)
        {
            grid[i, j] = value;
            ValueChanged(value);
        }

        protected override Bounds GetBounds()
        {
            return new Bounds(
                Position.X,
                Position.Y,
                Position.X + Width,
                Position.Y - Height
            );
        }

        protected override double? GetMaxValue() => grid.Cast<double?>().Where(value => value.HasValue).Max();
        protected override double? GetMinValue() => grid.Cast<double?>().Where(value => value.HasValue).Min();

        private void ValueChanged(double? newValue)
        {
            MinValue = MinValue.HasValue ? newValue < MinValue ? newValue : MinValue : newValue;
            MaxValue = MaxValue.HasValue ? newValue > MaxValue ? newValue : MaxValue : newValue;
            GridBitmap.Clear();
        }
    }
}