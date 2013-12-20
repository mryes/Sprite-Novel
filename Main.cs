using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;

namespace SpriteNovel
{
	class MainClass
	{
		static void Main()
		{
			Script scriptRoot = Script.Parse("n  music=lovely  \"Hello. What do you want to do today?\" n \"Shit on people?\"");
			Script scriptA    = Script.Parse("n \"You want to run? Okay, let's run.\"");
			Script scriptB    = Script.Parse("n \"Yeah? Just walking? That's fine. What do you want to eat?\" \"Shit?\"");
			Script scriptBA   = Script.Parse("n \"I'm tasty.\"");
			Script scriptBB   = Script.Parse("n \"I do like to eat you.\"");

			var scriptTreeRoot  = new ScriptTree(scriptRoot);
			var scriptTreeA 	= new ScriptTree(scriptA);
			var scriptTreeB 	= new ScriptTree(scriptB);
			var scriptTreeBA    = new ScriptTree(scriptBA);
			var scriptTreeBB    = new ScriptTree(scriptBB);

			scriptTreeRoot.AddPath(new ScriptPath(tree: scriptTreeA, choice: "Run."));
			scriptTreeRoot.AddPath(new ScriptPath(tree: scriptTreeB, choice: "Walk."));
			scriptTreeB.AddPath(new ScriptPath(tree: scriptTreeBA, choice: "You."));
			scriptTreeB.AddPath(new ScriptPath(tree: scriptTreeBB, choice: "Me."));

			/*do
			{
				Console.Write(String.Format("{0}", directorTest.GetDirective("dialogue").value));
				if (directorTest.AtChoicePoint())
					Console.WriteLine("\n");
				foreach(string option in directorTest.Choices)
					Console.WriteLine(option);
				Console.WriteLine();
				if (directorTest.AtChoicePoint())
					directorTest.PlanChoice(Convert.ToInt32(Console.ReadLine()));
				else Console.ReadLine();

			} while (directorTest.Advance() != DirectorStatus.EndOfScripts);*/

			var font = new Font("rix.ttf");

			var text = new WrappedText (
				"Helpers make helpers make further helpers",
				font, 15, 200
			);


		}
	}
}