using System;
using System.Collections.Generic;
using System.Linq;

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

			ScriptTree scriptTreeRoot = new ScriptTree(scriptRoot);
			ScriptTree scriptTreeA 	  = new ScriptTree(scriptA);
			ScriptTree scriptTreeB 	  = new ScriptTree(scriptB);
			ScriptTree scriptTreeBA   = new ScriptTree(scriptBA);
			ScriptTree scriptTreeBB   = new ScriptTree(scriptBB);

			scriptTreeRoot.AddPath(new ScriptPath(tree: scriptTreeA, choice: "Run."));
			scriptTreeRoot.AddPath(new ScriptPath(tree: scriptTreeB, choice: "Walk."));
			scriptTreeB.AddPath(new ScriptPath(tree: scriptTreeBA, choice: "You."));
			scriptTreeB.AddPath(new ScriptPath(tree: scriptTreeBB, choice: "Me."));

			Director directorTest = new Director(scriptTreeRoot);

			do
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

			} while (directorTest.Advance() != DirectorStatus.EndOfScripts);
		}
	}
}