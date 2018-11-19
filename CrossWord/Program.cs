using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossWordLibrary;

namespace CrossWordApp
{
	public class Program
	{
		private static void Main(string[] args)
		{
			var wordListsAll = new List<List<string>>()
			{
				new List<string>
				{
					"FRANK",
					"KRISTEN",
					"ZACH",
					"ALEXIS",
				},
				new List<string>
				{
					"FRANK",
					"KRISTEN",
					"ZACHARY",
					"ALEXIS",
				},
				new List<string>
				{
					"CHRISTY",
					"CASH",
					"ERIC",
					"COOPER",
				},
				new List<string>
				{
					"ZACHARY",
					"JESSICA",
					"ABIGAIL",
					"ELEANOR",
					"ANDREW",
				},
				new List<string>
				{
					"ZACH",
					"FRED",
					"HAROLD",
				},
			};

			List<string> wordList;

			for(;;)
			{
				PrintHelloBanner();

				string input = Console.ReadLine();

				input = input.ToUpper();

				if(input.Length == 0)
				{
					Console.WriteLine("Goodbye!");
					break;
				}

				wordList = input.Split(',').ToList();


				if(wordList.Count < 2)
				{
					Console.WriteLine("Input Error.  Try again.");
					continue;
				}

				var solver = new CrosswordSolver();
				solver.NewLogMessage += Solver_NewLogMessage;

				solver.Run(wordList,true);

				Console.WriteLine();
				Console.WriteLine();
			}
		}

		private static void Solver_NewLogMessage(object sender, LogMessageEventArgs e)
		{
			if(e.IncompleteMessage == true)
				Console.Write(e.Message);
			else
				Console.WriteLine(e.Message);
		}

		private static void PrintHelloBanner()
		{
			Console.WriteLine("+-------------------------------------+");
			Console.WriteLine("|                                     |");
			Console.WriteLine("| Welcome to the Crossword generator! |");
			Console.WriteLine("|                                     |");
			Console.WriteLine("+-------------------------------------+");
			Console.WriteLine();
			Console.WriteLine("Please Enter the Words to 'CrossWord' seperated by Commas.  Press 'Enter' to quit.");
			Console.WriteLine();
			Console.Write(": ");
		}

	}
}
