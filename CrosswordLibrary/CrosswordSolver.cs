using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kranken.ScrabbleLibrary
{
	public class CrosswordSolver
	{
		public event EventHandler<LogMessageEventArgs> NewLogMessage;
		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
		public DateTime StartTime { get; private set; }
		public DateTime EndTime { get; private set; }
		public ObservableCollection<List<CrossWord>> TotalSolutions { get; private set; } = new ObservableCollection<List<CrossWord>>();
		public int NonUniqueSolutions { get; private set; } = 0;
		public int CombosConsidered { get; private set; } = 0;
		private List<List<CrossWord>> CurrentComboSolutions { get; set; }

		public async void RunAsync(List<string> wordList, bool printSolutionsToConsole = false) => await Task.Run(() => Run(wordList, printSolutionsToConsole));

		public void Run(List<string> wordList, bool printSolutionsToConsole = false)
		{
			var nameCombos = GetPermutations(wordList, wordList.Count()).ToList();

			if(nameCombos.Count < 2)
			{
				OnLogMessage("ERROR: Must have at least two names.");
				return;
			}

			NonUniqueSolutions = 0;
			CombosConsidered = 0;
			TotalSolutions = new ObservableCollection<List<CrossWord>>();
			StartTime = DateTime.Now;
			int z = 0;

			ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0, null));
			OnLogMessage($"There are {nameCombos.Count} permutations of the names {{ {nameCombos.First().Aggregate((i, j) => i + ", " + j)} }} to consider.");

			foreach(var nameSet in nameCombos)
			{
				OnLogMessage(".",true);

				CurrentComboSolutions = new List<List<CrossWord>>();
				foreach(var name in nameSet)
				{
					AddWord(name);
					if(CurrentComboSolutions.Count == 0)
						break;
				}
				AddToTotalSolutions(TotalSolutions, CurrentComboSolutions);

				ProgressChanged?.Invoke(this,new ProgressChangedEventArgs(++z * 100 /nameCombos.Count, null));
			}

			EndTime = DateTime.Now;

			OnLogMessage("");
			ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(100, null));

			if(printSolutionsToConsole == true)
				PrintSolutionsToConsole();

			OnLogMessage($"Solutions considered: {CombosConsidered}");
			OnLogMessage($"Valid Solutions Found: {NonUniqueSolutions}");
			OnLogMessage($"Unique Valid Solutions: {TotalSolutions.Count}");
			OnLogMessage($"Elapsed Time: {(EndTime - StartTime).ToString()}");
		}

		private static List<List<T>> GetPermutations<T>(List<T> list, int length)
		{
			if(length == 1)
				return list.Select(t => new List<T> { t }).ToList();

			return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new List<T> { t2 }).ToList()).ToList();
		}

		private void AddToTotalSolutions(ObservableCollection<List<CrossWord>> totalSolutions, List<List<CrossWord>> solutions)
		{
			foreach(var solution in solutions)
			{
				if(solution.Count == 0)
					continue;

				NonUniqueSolutions++;

				bool foundMatch = false;
				foreach(var existingSolution in totalSolutions)
				{
					if(SolutionsAreEqual(existingSolution, solution))
					{
						foundMatch = true;
						break;
					}
				}
				if(foundMatch == false)
					totalSolutions.Add(solution);
			}
		}

		private bool SolutionsAreEqual(List<CrossWord> a, List<CrossWord> b)
		{
			if(a.Count != b.Count)
				return false;

			(int xStartA, int yStartA, int xEndA, int yEndA) = GetSolutionCorners(a);
			(int xStartB, int yStartB, int xEndB, int yEndB) = GetSolutionCorners(b);
			int deltaX = xStartB - xStartA;
			int deltaY = yStartB - yStartA;

			foreach(var wordA in a)
			{
				bool wordMatch = false;
				foreach(var wordB in b)
				{
					if(wordA.Word == wordB.Word
					&& wordA.XPosition == wordB.XPosition - deltaX
					&& wordA.YPosition == wordB.YPosition - deltaY
					&& wordA.Direction == wordB.Direction)
					{
						wordMatch = true;
						break;
					}
				}

				if(wordMatch == false)
					return false;
			}
			return true;
		}

		private void AddWord(string word)
		{
			//if(word == "KRISTEN")
			//	System.Diagnostics.Debugger.Break();

			// If this is the first word we are adding
			// We automatically have two solutions...Horiz & vertical
			if(CurrentComboSolutions.Count == 0)
			{
				var word1H = new CrossWord(word, 0, 0, DirectionEnum.Horizontal);
				var word1V = new CrossWord(word, 0, 0, DirectionEnum.Vertical);

				var solution1 = new List<CrossWord>();
				var solution2 = new List<CrossWord>();

				solution1.Add(word1H);
				solution2.Add(word1V);

				CurrentComboSolutions.Add(solution1);
				CurrentComboSolutions.Add(solution2);

				return;
			}

			var addSolutionCandidates = new List<List<CrossWord>>();

			foreach(var solution in CurrentComboSolutions)
			{
				var possibleSolutions = FindPossibleSolutions(word, solution);

				foreach(var pSolution in possibleSolutions)
				{
					CombosConsidered++;

					if(ValidateSolution(solution, pSolution) == true)
					{
						var newSolution = new List<CrossWord>(solution);
						;
						newSolution.Add(pSolution);

						addSolutionCandidates.Add(newSolution);
					}
				}
			}

			CurrentComboSolutions = addSolutionCandidates;
		}

		private List<CrossWord> FindPossibleSolutions(string word, List<CrossWord> solution)
		{
			var possibleSolutions = new List<CrossWord>();

			for(int z = 0;z < solution.Count();z++)
			{
				var existingWord = solution[z];

				for(int i = 0;i < existingWord.Length;i++)
				{
					for(int j = 0;j < word.Length;j++)
					{
						if(word[j] == existingWord.Word[i])
						{
							CrossWord maybe = null;
							switch(existingWord.Direction)
							{
							case DirectionEnum.Horizontal:
								maybe = new CrossWord(word, existingWord.XPosition + i, existingWord.YPosition - j, DirectionEnum.Vertical);
								break;
							case DirectionEnum.Vertical:
								maybe = new CrossWord(word, existingWord.XPosition - j, existingWord.YPosition + i, DirectionEnum.Horizontal);
								break;
							}
							possibleSolutions.Add(maybe);
						}
					}
				}
			}
			return possibleSolutions;
		}

		private bool ValidateSolution(List<CrossWord> solution, CrossWord pSolution)
		{
			for(int i = 0;i < pSolution.Word.Length;i++)
			{
				char? letterFound;
				int x, y;

				switch(pSolution.Direction)
				{
				case DirectionEnum.Horizontal:
					x = pSolution.XPosition + i;
					y = pSolution.YPosition;

					letterFound = GetCharAtPosition(x, y, solution);
					if(letterFound.HasValue == false)
					{
						if(GetCharAtPosition(x, y - 1, solution).HasValue
						|| GetCharAtPosition(x, y + 1, solution).HasValue)
							return false;
					}
					else if(letterFound != pSolution.Word[i])
					{
						return false;
					}

					if(i == 0 && GetCharAtPosition(x - 1, y, solution) != null)
						return false;

					if(i == pSolution.Word.Length - 1 && GetCharAtPosition(x + 1, y, solution).HasValue == true)
						return false;

					break;
				case DirectionEnum.Vertical:
					x = pSolution.XPosition;
					y = pSolution.YPosition + i;

					letterFound = GetCharAtPosition(x, y, solution);
					if(letterFound.HasValue == false)
					{
						if(GetCharAtPosition(x - 1, y, solution).HasValue
						|| GetCharAtPosition(x + 1, y, solution).HasValue)
							return false;
					}
					else if(letterFound != pSolution.Word[i])
					{
						return false;
					}

					if(i == 0 && GetCharAtPosition(x, y - 1, solution) != null)
						return false;

					if(i == pSolution.Word.Length - 1 && GetCharAtPosition(x, y + 1, solution).HasValue == true)
						return false;

					break;
				}
			}
			return true;
		}

		public static char? GetCharAtPosition(int x, int y, List<CrossWord> solution)
		{
			foreach(var cWord in solution)
			{
				switch(cWord.Direction)
				{
				case DirectionEnum.Horizontal:
					if(cWord.YPosition != y)
						continue;
					else
					{
						var xOffset = x - cWord.XPosition;
						if(xOffset >= 0 && xOffset < cWord.Length)
							return cWord.Word[xOffset];
					}
					break;

				case DirectionEnum.Vertical:
					if(cWord.XPosition != x)
						continue;
					else
					{
						var yOffset = y - cWord.YPosition;
						if(yOffset >= 0 && yOffset < cWord.Length)
							return cWord.Word[yOffset];
					}
					break;
				}
			}
			return null;
		}

		public static (int xStart, int yStart, int xEnd, int yEnd) GetSolutionCorners(List<CrossWord> solution)
		{
			int xStart = int.MaxValue;
			int yStart = int.MaxValue;
			int xEnd = int.MinValue;
			int yEnd = int.MinValue;

			foreach(var word in solution)
			{
				xStart = Math.Min(word.XPosition, xStart);
				yStart = Math.Min(word.YPosition, yStart);
				switch(word.Direction)
				{
				case DirectionEnum.Horizontal:
					xEnd = Math.Max(word.XPosition + word.Length, xEnd);
					yEnd = Math.Max(word.YPosition, yEnd);
					break;

				case DirectionEnum.Vertical:
					xEnd = Math.Max(word.XPosition, xEnd);
					yEnd = Math.Max(word.YPosition + word.Length, yEnd);
					break;
				}
			}

			return (xStart, yStart, xEnd, yEnd);
		}

		public void PrintSolutionsToConsole(IEnumerable<List<CrossWord>> solutions = null)
		{
			if(solutions == null)
				solutions = TotalSolutions;

			foreach(var solution in solutions)
			{
				PrintSolutionToConsole(solution);
			}
		}

		private void PrintSolutionToConsole(List<CrossWord> solution)
		{
			bool ShowBorders = false;
			bool ShowGridSize = false;

			char borderCorner, borderTop, borderSide, borderBottom;
			char letterPlaceHolder = ' ';

			if(ShowBorders == true)
			{
				borderCorner = '+';
				borderTop = '-';
				borderSide = '|';
				borderBottom = '-';
			}
			else
			{
				borderCorner = ' ';
				borderTop = ' ';
				borderSide = ' ';
				borderBottom = ' ';
			}

			(int xStart, int yStart, int xEnd, int yEnd) = GetSolutionCorners(solution);

			if(ShowGridSize)
			{
				Console.WriteLine($"Solution is {xEnd - xStart } x {yEnd - yStart }");
			}

			Console.WriteLine();

			Console.Write(borderCorner);
			for(int z = 0;z < xEnd - xStart;z++)
				Console.Write(borderTop);
			Console.Write(borderCorner);
			Console.WriteLine();

			for(int y = yStart;y < yEnd;y++)
			{
				Console.Write(borderSide);

				for(int x = xStart;x < xEnd;x++)
				{
					char? result = GetCharAtPosition(x, y, solution);

					if(result.HasValue)
						Console.Write(result);
					else
						Console.Write(letterPlaceHolder);
				}
				Console.Write(borderSide);

				Console.WriteLine();
			}

			Console.Write(borderCorner);
			for(int z = 0;z < xEnd - xStart;z++)
				Console.Write(borderBottom);
			Console.Write(borderCorner);
			Console.WriteLine();

			Console.WriteLine();
		}
		private void OnLogMessage(string message, bool incompleteMessage = false)
		{
			var e = new LogMessageEventArgs(message,incompleteMessage);

			NewLogMessage?.Invoke(this, e);
		}
	}
}
