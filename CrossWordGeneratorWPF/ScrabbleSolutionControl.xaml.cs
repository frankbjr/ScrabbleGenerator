// ---------------------------------------------------------------------------------------------
// FileName: ScrabbleSolutionControl.xaml.cs
// FileType: Visual C# Xaml Code behind Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Class that encapsulates all the behaviours of a ScrabbleSolution
// ---------------------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace Kranken.ScrabbleGenerator
{
	public partial class ScrabbleSolutionControl : UserControl
	{
		// Fields
		public static readonly DependencyProperty ScrabbleSolutionProperty = DependencyProperty.Register("ScrabbleSolution", typeof(ScrabbleSolution), typeof(ScrabbleSolutionControl),
			new PropertyMetadata(null, new PropertyChangedCallback(OnScrabbleSolutionChanged)));

		// cTors
		public ScrabbleSolutionControl() => InitializeComponent();

		// Properties
		public ScrabbleSolution ScrabbleSolution
		{
			get => (ScrabbleSolution)GetValue(ScrabbleSolutionProperty);
			set => SetValue(ScrabbleSolutionProperty, value);
		}

		// Methods
		private static void OnScrabbleSolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctl = d as ScrabbleSolutionControl;
			var solution = e.NewValue as ScrabbleSolution;

			ctl.ResetControl();

			if(solution.Count == 0)
				return;

			var rect = solution.Rectangle;
			int width = (int)rect.Width;
			int height = (int)rect.Height;
			int xStart = (int)rect.X;
			int yStart = (int)rect.Y;

			GenerateRowAndColDefinitions(ctl, width, height);

			for(int y = 0;y < height;y++)
			{
				for(int x = 0;x < width;x++)
				{
					var letter = solution.GetLetterAtPosition(x + xStart, y + yStart);
					if(letter.HasValue)
					{
						AddLetterToGrid(ctl.PuzzleGrid, x, y, letter.Value);
					}
				}
			}
		}

		private static void AddLetterToGrid(Grid grid, int x, int y, char letter)
		{
			var textBlock = new TextBlock();

			Grid.SetColumn(textBlock, x);
			Grid.SetRow(textBlock, y);
			textBlock.Text = $"{letter}";

			grid.Children.Add(textBlock);
		}

		private static void GenerateRowAndColDefinitions(ScrabbleSolutionControl ctl, int width, int height)
		{
			for(int i = 0;i < height;i++)
			{
				var rowDef = new RowDefinition
				{
					Height = GridLength.Auto
				};

				ctl.PuzzleGrid.RowDefinitions.Add(rowDef);
			}
			for(int i = 0;i < width;i++)
			{
				var colDef = new ColumnDefinition
				{
					Width = GridLength.Auto
				};

				ctl.PuzzleGrid.ColumnDefinitions.Add(colDef);
			}
		}

		private void ResetControl()
		{
			PuzzleGrid.Children.Clear();
			PuzzleGrid.RowDefinitions.Clear();
			PuzzleGrid.ColumnDefinitions.Clear();
		}
	}
}