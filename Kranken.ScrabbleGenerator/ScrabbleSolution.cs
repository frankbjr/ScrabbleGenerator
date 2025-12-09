// ---------------------------------------------------------------------------------------------
// FileName: ScrabbleSolution.cs
// FileType: Visual C# Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Class that encapsulates all the behaviours of a ScrabbleSolution
// ---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;

namespace Kranken.ScrabbleGenerator
{
	public class ScrabbleSolution : List<ScrabbleWordPlacement>
	{
		//cTors
		public ScrabbleSolution() : base()
		{
		}

		public ScrabbleSolution(IEnumerable<ScrabbleWordPlacement> collection) : base(collection)
		{
		}

		//Properties
		public Rect Rectangle
		{
			get
			{
				var result = new Rect();

				foreach(var word in this)
					result.Union(word.Rectangle);

				return result;
			}
		}

		//Methods
		public override bool Equals(object? solution)
		{
			if(solution is ScrabbleSolution solutionB)
			{
				// If they don't have the same number of words in the solution, they can't match!
				if(Count != solutionB.Count)
					return false;

				// Could be the same solution, but their Origin could be offset.  In order to
				// consider their equality, we need to make sure that we use a common Origin.
				var rectA = Rectangle;
				var rectB = solutionB.Rectangle;
				var offset = new Vector(rectA.Left - rectB.Left, rectA.Top - rectB.Top);

				// Since we have already ensured that each solution has the same numer of elements (words)
				// We simply iterate through them making sure that all the elements in A are also in B.
				foreach(var wordA in this)
				{
					bool elementMatch = false;
					foreach(var wordB in solutionB)
					{
						var tempRect = new Rect(wordB.Rectangle.TopLeft, wordB.Rectangle.BottomRight);
						tempRect.Offset(offset);

						if(wordA.Word == wordB.Word && wordA.Rectangle == tempRect)
						{
							elementMatch = true;
							break;
						}
					}

					if(elementMatch == false)
						return false;
				}
				return true;
			}
			else
				return false;
		}

		public override int GetHashCode()
		{
			int runningHash = 0;

			foreach(var element in this)
			{
				runningHash += element.GetHashCode();
			}
			return runningHash;
		}

		internal char? GetLetterAtPosition(Point p) => GetLetterAtPosition((int)p.X, (int)p.Y);

		internal char? GetLetterAtPosition(int x, int y)
		{
			foreach(var cWord in this)
			{
				switch(cWord.Direction)
				{
				case DirectionEnum.Horizontal:
					if(cWord.Rectangle.Top != y)
						continue;
					else
					{
						var xOffset = x - (int)cWord.Rectangle.Left;
						if(xOffset >= 0 && xOffset < cWord.Length)
							return cWord.Word[xOffset];
					}
					break;

				case DirectionEnum.Vertical:
					if((int)cWord.Rectangle.X != x)
						continue;
					else
					{
						var yOffset = y - (int)cWord.Rectangle.Top;
						if(yOffset >= 0 && yOffset < cWord.Length)
							return cWord.Word[yOffset];
					}
					break;
				}
			}
			return null;
		}

		internal List<ScrabbleWordPlacement> FindIntersectionCandidates(string word)
		{
			var intersectionCandidates = new List<ScrabbleWordPlacement>();

			foreach(var existingWord in this)
			{
				for(int x = 0;x < existingWord.Length;x++)
				{
					for(int y = 0;y < word.Length;y++)
					{
						if(word[y] == existingWord.Word[x])
						{
							ScrabbleWordPlacement? wordPlacementCandidate = null;
							switch(existingWord.Direction)
							{
							case DirectionEnum.Horizontal:
								wordPlacementCandidate = new ScrabbleWordPlacement(word, (int)existingWord.Rectangle.Left + x, (int)existingWord.Rectangle.Top - y, DirectionEnum.Vertical);
								break;

							case DirectionEnum.Vertical:
								wordPlacementCandidate = new ScrabbleWordPlacement(word, (int)existingWord.Rectangle.Left - y, (int)existingWord.Rectangle.Top + x, DirectionEnum.Horizontal);
								break;
							}

							// Only add this placement it the rest of word doesn't have any intersection problems.
							if(AreWordIntersectionsValid(wordPlacementCandidate!))
								intersectionCandidates.Add(wordPlacementCandidate!);
						}
					}
				}
			}
			return intersectionCandidates;
		}

		private bool AreWordIntersectionsValid(ScrabbleWordPlacement proposedPlacement)
		{
			// This function DOES NOT check if a word placement is valid.
			// It checks that the letters meeting at all intersections match.
			// It DOES NOT check if the words are perpendicular
			int dx = 0, dy = 0;
			if(proposedPlacement.Direction == DirectionEnum.Horizontal)
				dx = 1;
			else
				dy = 1;

			for(int i = 0;i < proposedPlacement.Word.Length;i++)
			{
				var p = new Point(proposedPlacement.Rectangle.X + dx * i,
					proposedPlacement.Rectangle.Y + dy * i);

				var letter = GetLetterAtPosition(p);
				if(letter.HasValue && letter != proposedPlacement.Word[i])
					return false;
			}
			return true;
		}

		internal bool IsPlacementValid(ScrabbleWordPlacement proposedPlacement)
		{
			/*
			 * We are using the follwing rules to validate word placement.
			 * ----------------------------------------------------
			 * #1- Placed word that intersect existing words must intersect on common letter.
			 * #2- Intersections must be perpendicular to each other.
			 * #3- Placed word must have nothing to the sides of each letter *
			 * #4- Placed word must have nothing just before the first letter *
			 * #5- Placed word must have nothing just after the last letter *
			 *  ( * = except at the intersection point. )
			 */
			int dx = 0, dy = 0;

			if(proposedPlacement.Direction == DirectionEnum.Horizontal)
				dx = 1;
			else
				dy = 1;

			for(int i = 0;i < proposedPlacement.Word.Length;i++)
			{
				char? letterFound;
				int x, y;

				x = (int)proposedPlacement.Rectangle.Left + dx * i;
				y = (int)proposedPlacement.Rectangle.Top + dy * i;

				letterFound = GetLetterAtPosition(x, y);

				// Is Just before the first letter clear?
				if(i == 0 && GetLetterAtPosition(x - dx, y - dy) != null)
					return false;

				// Is Just after the last letter clear?
				if(i == proposedPlacement.Word.Length - 1 && GetLetterAtPosition(x + dx, y + dy).HasValue == true)
					return false;

				// Is this a junction/intersection point?
				if(letterFound.HasValue == false)
				{
					// No junction-- Are the sides clear?
					if(GetLetterAtPosition(x - dy, y - dx).HasValue
					|| GetLetterAtPosition(x + dy, y + dx).HasValue)
						return false;
				}
				else if(letterFound != proposedPlacement.Word[i])
				{
					// Yes junction -- if we don't match, we punt
					return false;
				}
			}
			return true;
		}

		internal ScrabbleWordPlacement? FindWordAtCoordinates(int x, int y)
		{
			var point = new Point(x, y);

			foreach(var word in this)
			{
				if(word.Rectangle.Contains(point))
					return word;
			}

			return null;
		}
	}
}