// ---------------------------------------------------------------------------------------------
// FileName: ScrabbleSolverEngine.cs
// FileType: Visual C# Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Engine to generate all possible "Scrabble" solutions for a set of words.
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kranken.ScrabbleGenerator
{
	public class ScrabbleSolverEngine
	{
		//Fields
		private int _progressPercentage;

		public event EventHandler<LogMessageEventArgs> NewLogMessage;

		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

		//Properties
		public DateTime StartTime { get; private set; }

		public DateTime EndTime { get; private set; }
		public ObservableCollection<ScrabbleSolution> UniqueSolutions { get; private set; }
		public int ValidSolutionsFound { get; private set; }
		public int SolutionsConsidered { get; private set; }

		public int ProgressPercentage
		{
			get => _progressPercentage;
			set
			{
				// Clip our value to between 0 and 100
				value = Math.Min(100, value);
				value = Math.Max(0, value);

				if(value != _progressPercentage)
				{
					_progressPercentage = value;

					// Tell anyone that cares
					ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(_progressPercentage, null));
				}
			}
		}

		//Methods
		/// <summary>
		/// Asynchronous Wrapper for calling Run
		/// </summary>
		/// <param name="wordList">List of words to find Scrabble solutions for.</param>
		public async void RunAsync(List<string> wordList) => await Task.Run(() => Run(wordList));

		/// <summary>
		/// Generates all "Scrabble" solutions for a set of words
		/// </summary>
		/// <param name="wordList">List of words to find Scrabble solutions for.</param>
		public void Run(List<string> wordList)
		{
			ResetStatistics();

			var namesetPermutations = GetAllNamesetPermutations(wordList, wordList.Count()).ToList();
			if(namesetPermutations.Count < 2)
			{
				OnLogMessage("ERROR: Must have at least two names.");
				return;
			}

			OnLogMessage($"There are {namesetPermutations.Count} permutations of the names {{ {namesetPermutations.First().Aggregate((i, j) => i + ", " + j)} }} to consider.");

			for(int namesetIndex = 0;namesetIndex < namesetPermutations.Count;namesetIndex++)
			{
				var nameset = namesetPermutations[namesetIndex];
				var currentNamesetSolutions = new List<ScrabbleSolution>();

				for(int nameIndex = 0;nameIndex < nameset.Count;nameIndex++)
				{
					var name = nameset[nameIndex];

					ProgressPercentage = ((namesetIndex * nameset.Count + nameIndex) * 100) / (namesetPermutations.Count * nameset.Count);

					currentNamesetSolutions = AddWordToAllExistingSolutions(name, currentNamesetSolutions);

					// If there are no solutions after adding the newest word,
					//  we can abort.  There are no solutions for these words.
					if(currentNamesetSolutions.Count == 0)
						break;
				}

				// Now take any solutions we just found, and only keep the ones that are unique
				// as compared to previously found solutions.
				AddAnyUniqueSolutions(UniqueSolutions, currentNamesetSolutions);
			}

			EndTime = DateTime.Now;
			ProgressPercentage = 100;

			LogExecutionStatistics();
		}

		private void ResetStatistics()
		{
			StartTime = DateTime.Now;
			ValidSolutionsFound = 0;
			SolutionsConsidered = 0;
			ProgressPercentage = 0;

			// Observable collection needs to be created on the engine's thread
			// otherwise updating it will cause an exception.
			// We can ensure that by creating the thread here.
			UniqueSolutions = new ObservableCollection<ScrabbleSolution>();
		}

		private void LogExecutionStatistics()
		{
			OnLogMessage($"Solutions considered: {SolutionsConsidered}");
			OnLogMessage($"Valid Solutions Found: {ValidSolutionsFound}");
			OnLogMessage($"Unique Valid Solutions: {UniqueSolutions.Count}");
			OnLogMessage($"Elapsed Time: {(EndTime - StartTime).ToString()}");
		}

		private static List<List<string>> GetAllNamesetPermutations(List<string> list, int length)
		{
			// Little bit of LINQ hocus pocus I found on SO
			// Recursion is so elegant sometimes!

			if(length == 1)
				return list.Select(t => new List<string> { t }).ToList();

			return GetAllNamesetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new List<string> { t2 }).ToList()).ToList();
		}

		private List<ScrabbleSolution> AddWordToAllExistingSolutions(string word, List<ScrabbleSolution> currentNamesetSolutions)
		{
			// If this is the first word we are adding (meaning: we have no solutions yet)
			// We automatically have two solutions...Placing the word Horiz & vertical
			if(currentNamesetSolutions.Count == 0)
			{
				var wordElementHorizontal = new ScrabbleWordPlacement(word, 0, 0, DirectionEnum.Horizontal);
				var wordElementVertical = new ScrabbleWordPlacement(word, 0, 0, DirectionEnum.Vertical);

				var solution1 = new ScrabbleSolution();
				var solution2 = new ScrabbleSolution();

				solution1.Add(wordElementHorizontal);
				solution2.Add(wordElementVertical);

				currentNamesetSolutions.Add(solution1);
				currentNamesetSolutions.Add(solution2);

				return currentNamesetSolutions;
			}
			else
			{
				//Strategy:  We enumerator all the possible connection points to add the word at.
				// Then we examine each possible placement and validate that it doesn't break
				// the rules of scrabble.
				var newSolutions = new List<ScrabbleSolution>();

				foreach(var solution in currentNamesetSolutions)
				{
					// Find all the possible connection points for the word we are adding.
					var possiblePlacements = solution.FindIntersectionCandidates(word);

					// Examine each possible placement, determine if it would result in a
					// valid Scrabble layout.
					foreach(var possiblePlacement in possiblePlacements)
					{
						SolutionsConsidered++;

						if(solution.IsPlacementValid(possiblePlacement) == true)
						{
							// Placement is valid, so lets create a new solution and add the
							// placement to it
							// IMPORTANT:  The next line creates a SHALLOW copy of the list.
							//  DO NOT FORGET THAT ANY LIST ELEMENTS WE MANIPULATE WILL
							//  LIKELY EXIST IN MULTIPLE LISTS, MEANING THAT MANIPULATION
							//  WILL LIKELY DO SOMETHING YOU DIDN'T INTEND IN ANOTHER LIST
							var newSolution = new ScrabbleSolution(solution);
							;
							newSolution.Add(possiblePlacement);

							newSolutions.Add(newSolution);
						}
					}
				}

				// Return the new solutions that exist after adding the new word.
				return newSolutions;
			}
		}

		private void AddAnyUniqueSolutions(ObservableCollection<ScrabbleSolution> uniqueSolutions, List<ScrabbleSolution> newUniqueSolutionCandidates)
		{
			// Iterate throw all new solutions comparing them
			// to our existing solutions.  Any solutions that
			// are unique are added to our collection.

			foreach(var solution in newUniqueSolutionCandidates)
			{
				if(solution.Count == 0)
					continue;

				ValidSolutionsFound++;

				bool matchesExistingSolution = false;
				foreach(var existingSolution in uniqueSolutions)
				{
					if(existingSolution.Equals(solution))
					{
						matchesExistingSolution = true;
						break;
					}
				}
				if(matchesExistingSolution == false)
					uniqueSolutions.Add(solution);
			}
		}

		private void OnLogMessage(string message)
		{
			// Notify any LogMessage event subscribers about a new LogMessage.
			var e = new LogMessageEventArgs(message);

			NewLogMessage?.Invoke(this, e);
		}
	}
}