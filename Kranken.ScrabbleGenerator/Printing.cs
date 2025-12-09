// ---------------------------------------------------------------------------------------------
// FileName: Printing.cs
// FileType: Visual C# Source file
// Author: frankjr
// Created On: 11/18/2018
// Last Modified On: 11/18/2018
// Copywrite: Kranken Software
// Description: Class that encapsulates visual Printing code for Scrabble Solutions
// ---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Kranken.ScrabbleGenerator
{
	public class Printing
	{
		internal static void PrintSolutions(List<ScrabbleSolution> solutions, double? fontSize = null)
		{
			var printDialog = new PrintDialog();
			if(printDialog.ShowDialog() == false)
				return;

			var fixedDocument = new FixedDocument();
			fixedDocument.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

			var it = solutions.GetEnumerator();
			it.MoveNext();

			var fixedPages = new List<FixedPage>();

			for(;;)
			{
				var page = new FixedPage
				{
					Width = printDialog.PrintableAreaWidth,
					Height = printDialog.PrintableAreaHeight
				};

				LayoutPage(ref it, page, fontSize);

				if(page.Children.Count == 0)
					break;

				var pc = new PageContent
				{
					Child = page
				};
				fixedDocument.Pages.Add(pc);
			}

			if(fixedDocument.Pages.Count != 0)
				printDialog.PrintDocument(fixedDocument.DocumentPaginator, "ScrabbleGenerator Solutions");
			
		}

		private static void LayoutPage(ref List<ScrabbleSolution>.Enumerator it, FixedPage page, double? fontSize)
		{
			double marginX = 48;
			double marginY = 48;
			var sz = new Size(8.5 * 96 - 2 * marginX, 11 * 96 - 2 * marginY);

			// This is how we know we are done.
			// No more pages to layout if there
			// aren't any more solutions to show
			if(it.Current == null)
				return;

			//Do Page Header
			var headerTextBlock = new TextBlock
			{
				Text = "Scrabble Generator Solutions",
				FontSize = 25
			};

			headerTextBlock.Measure(sz);

			FixedPage.SetLeft(headerTextBlock, sz.Width / 2 - headerTextBlock.DesiredSize.Width / 2);  // Center the Header
			FixedPage.SetTop(headerTextBlock, 48);  // Put it at 1/2 inch down

			page.Children.Add(headerTextBlock);

			var separator = new Separator
			{
				Width = sz.Width,
				Height = 5
			};
			separator.Measure(sz);

			FixedPage.SetLeft(separator, marginX);
			FixedPage.SetTop(separator, marginY + headerTextBlock.DesiredSize.Height);

			page.Children.Add(separator);

			double x = marginX;
			double y = marginY + headerTextBlock.DesiredSize.Height + separator.DesiredSize.Height;
			double largestY = 0;

			do
			{
				var ctl = new ScrabbleSolutionControl
				{
					ScrabbleSolution = it.Current,
					Margin = new Thickness(10),
				};

				if(fontSize.HasValue)
					ctl.FontSize = fontSize.Value;

				ctl.Measure(sz);

				// Page starts with two controls (Heading & separator.. Count==2).
				// If we haven't placed a Scrabble Solution (i.e Count==2), and the current
				// solution is too damn big to fit, lets keep shrinking the fontsize
				// by one until the solution fits!  Yes, it will be one solution per page,
				// but the good news is that we will manage to print every solution for
				// the user, as close to as big as they wanted as possible.
				if(page.Children.Count == 2)
				{
					var newFontSize = ctl.FontSize;

					while(ctl.DesiredSize.Width > sz.Width - 2 * marginX || ctl.DesiredSize.Height > sz.Height - marginY)
					{
						ctl.FontSize = newFontSize--;

						ctl.Measure(sz);
					}
				}

				// When we run out of X space, we move down and start over in the x direction
				if(x + ctl.DesiredSize.Width > page.Width - 2 * marginX)
				{
					x = marginX;
					y += largestY;
					largestY = 0;
				}

				// When we run out of Y space, the page is full.
				if(y + ctl.DesiredSize.Height > page.Height - marginY)
					break;

				FixedPage.SetLeft(ctl, x);
				FixedPage.SetTop(ctl, y);
				page.Children.Add(ctl);

				x += ctl.DesiredSize.Width;

				largestY = Math.Max(largestY, ctl.DesiredSize.Height);
			}
			while(it.MoveNext() == true);
		}
	}
}
