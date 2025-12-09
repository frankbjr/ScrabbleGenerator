// ---------------------------------------------------------------------------------------------
// FileName: ScrabbleSolverEngineDebuggingExtensions.cs
// FileType: Visual C# Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Class that encapsulates a Scrabble Word Placement.
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Kranken.ScrabbleGenerator
{
	public static class ScrabbleSolverEngineDebuggingExtensions
	{
		/// <summary>
		/// Prints a set of Solutions to the console.
		/// </summary>
		/// <param name="solutions">An IEnumerable of solutions.  If you set this to null, it will print all the valid solutions the engine must recently found.</param>
		public static void PrintSolutionsToConsole(this ScrabbleSolverEngine engine, IEnumerable<ScrabbleSolution>? solutions = null)
		{
			solutions ??= engine.UniqueSolutions;

			foreach(var solution in solutions!)
			{
				solution.PrintSolutionToConsole();
			}
		}

		/// <summary>
		/// Prints a ScrabbleSolution to the Console.
		/// </summary>
		/// <param name="showBorders">If true, will print a box around the solution.</param>
		/// <param name="showGridSize">If true, will print a line displaying the size of the solution.</param>
		public static void PrintSolutionToConsole(this ScrabbleSolution solution, bool showBorders = false, bool showGridSize = false)
		{
			char borderCorner = ' ', borderTop = ' ', borderSide = ' ', borderBottom = ' ';
			char letterPlaceHolder = ' ';

			if(showBorders == true)
			{
				borderCorner = '+';
				borderTop = '-';
				borderSide = '|';
				borderBottom = '-';
			}

			var rect = solution.Rectangle;

			if(showGridSize)
			{
				Console.WriteLine($"Solution is {rect.Width } x {rect.Height }");
			}

			Console.WriteLine();

			Console.Write(borderCorner);
			for(int z = 0;z < rect.Width;z++)
				Console.Write(borderTop);
			Console.Write(borderCorner);
			Console.WriteLine();

			for(int y = (int)rect.Top;y < rect.Bottom;y++)
			{
				Console.Write(borderSide);

				for(int x = (int)rect.Left;x < rect.Width;x++)
				{
					char? result = solution.GetLetterAtPosition(x, y);

					if(result.HasValue)
						Console.Write(result);
					else
						Console.Write(letterPlaceHolder);
				}
				Console.Write(borderSide);

				Console.WriteLine();
			}

			Console.Write(borderCorner);
			for(int z = 0;z < rect.Width;z++)
				Console.Write(borderBottom);
			Console.Write(borderCorner);
			Console.WriteLine();

			Console.WriteLine();
		}
	}
}