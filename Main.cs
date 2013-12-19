using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNovel
{
	class MainClass
	{
		static void Main()
		{
			string scriptString = "n \"Hello, folks.\"\nn sp=instant snd=ding flash " +
			                      "\"Welcome back, Bogoa!\" n music=lovely \"How are you doing?\"\nn \"Good!\" " +
			                      "\"This is all pretty incredible, huh?\"\nn \"Yeah!\"";
			Script scriptTest = Script.Parse(scriptString);

			foreach (var advancement in scriptTest)
			{
				foreach(var directive in advancement.directives)
					Console.Write(String.Format("{0}={1} ", 
							      directive.name, directive.value));
				Console.WriteLine(String.Format("{0}\n", advancement.dialogue));
			}

			Director directorTest = new Director(scriptTest);
			for (int i=0; i<scriptTest.Count; ++i)
			{
				foreach (var directive in directorTest.CurrentDirectives)
					Console.Write(String.Format("{0} ", directive.name));
				Console.WriteLine();
				directorTest.CurrentAdvancement += 1;
			}
		}
	}
}