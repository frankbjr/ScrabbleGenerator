// ---------------------------------------------------------------------------------------------
// FileName: ScrabbleWordElement.cs
// FileType: Visual C# Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Class that encapsulates a Scrabble Word Placement.
// ---------------------------------------------------------------------------------------------

using System.Windows;

namespace Kranken.ScrabbleGenerator
{
	public class ScrabbleWordPlacement
	{
		//cTors
		public ScrabbleWordPlacement(string word, int x, int y, DirectionEnum direction)
		{
			Word = word;
			Direction = direction;

			Rectangle = new Rect(x, y,
					direction == DirectionEnum.Horizontal ? Length : 1,
					direction == DirectionEnum.Horizontal ? 1 : Length);
		}

		//Properties
		public string Word { get; set; }

		public Rect Rectangle { get; }

		public DirectionEnum Direction { get; set; }

		public int Length => Word.Length;
	}
}