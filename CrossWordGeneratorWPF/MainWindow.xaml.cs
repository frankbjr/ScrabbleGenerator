using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kranken.ScrabbleGenerator
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private ScrabbleSolverEngine _solver;
		private WrapPanel _wrapPanel;
		private double _solutionFontSize = 30;

		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
			_solver = new ScrabbleSolverEngine();
			_solver.NewLogMessage += Solver_NewLogMessage;
			_solver.ProgressChanged += Solver_OnProgressChanged;

			Messages = new ObservableCollection<string>();

			InitializeComponent();

			DataContext = this;
		}

		public int Progress { get; set; } = 0;

		public double SolutionFontSize
		{
			get => _solutionFontSize;
			set
			{
				// Let's clamp the values
				value = Math.Min(120, value);
				value = Math.Max(10, value);
				value = Math.Round(value);

				_solutionFontSize = value;
				OnNotifyPropertyChanged(nameof(SolutionFontSize));
			}
		}

		public string NamesInput { get; set; } = "FRANK, KRISTEN, ZACHARY, ALEXIS";

		public ObservableCollection<string> Messages { get; }

		public ObservableCollection<ScrabbleSolution> Solutions => _solver.UniqueSolutions;

		private void GenerateButton_Click(object sender, RoutedEventArgs e)
		{
			var wordList = NamesInput.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

			_solver.RunAsync(wordList);
		}

		private void QuitButton_Click(object sender, RoutedEventArgs e) => Close();

		private void Solver_NewLogMessage(object sender, LogMessageEventArgs e)
		{
			if(string.IsNullOrWhiteSpace(e.Message))
				return;

			Dispatcher.Invoke(() =>
			{
				Messages.Add($"[{(e.Timestamp - _solver.StartTime).TotalSeconds:n3}]: {e.Message}");
				OnNotifyPropertyChanged(nameof(Messages));
			});
		}

		private void Solver_OnProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				Progress = e.ProgressPercentage;
				OnNotifyPropertyChanged(nameof(Progress));

				if(e.ProgressPercentage == 100)
				{
					OnNotifyPropertyChanged(nameof(Solutions));
					if(Solutions.Count == 0)
						System.Media.SystemSounds.Hand.Play();
					else
						System.Media.SystemSounds.Exclamation.Play();
				}
			});
		}

		private void SolutionLV_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// Total Kludge but it works.

			if(_wrapPanel != null)
				_wrapPanel.Width = e.NewSize.Width - 25;

			e.Handled = true;
		}

		private void WrapPanel_Loaded(object sender, RoutedEventArgs e)
		{
			_wrapPanel = sender as WrapPanel;
			e.Handled = true;
			Width += 1;
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Messages.Clear();
			_solver.UniqueSolutions.Clear();
		}

		private void OnNotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		private void Print_SelectedSolutions(object sender, RoutedEventArgs e)
		{
			var set = SolutionLV.SelectedItems.Cast<ScrabbleSolution>().ToList();

			if(set.Count > 0)
			{
				Printing.PrintSolutions(set, SolutionFontSize);
			}
		}

		private void Print_AllSolutions(object sender, RoutedEventArgs e) => Printing.PrintSolutions(Solutions.ToList(), SolutionFontSize);
	}
}