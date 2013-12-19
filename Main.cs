using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNovel
{
	class MainClass
	{
		static void Main()
		{
			var adv = new Advancement();
			adv = Advancement.Parse(" n music=lovely \"How are you doing?\"");
			foreach (var dir in adv.directives)
			{
				Console.WriteLine(dir.name);
				Console.WriteLine(dir.value);
				Console.WriteLine();
			}
			Console.WriteLine(adv.dialogue);
		}
	}
}