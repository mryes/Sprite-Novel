using System;
using System.Collections.Generic;

namespace SpriteNovel
{
	class Director
	{
		public List<AdvancementDirective> CurrentDirectives;

		int advNumber;
		public int CurrentAdvancement
		{
			get { return advNumber; }
			set
			{
				advNumber = value;
				CurrentDirectives.Clear();
				CurrentDirectives = script[value].directives;
				CurrentDirectives.Add(new AdvancementDirective 
					{ name = "dialogue", value = script[value].dialogue });

				var directiveNames = new List<string>();
				foreach (var curDir in CurrentDirectives)
					directiveNames.Add(curDir.name);
				foreach (var perDir in persistingDirectives)
				{
					if (!directiveNames.Contains(perDir))
					{
						string lastValue = LastValueOfDirective(perDir);
						if (lastValue != "")
							CurrentDirectives.Add(new AdvancementDirective 
								{ name = perDir, value = lastValue });
					}
				}
			}
		}

		public Director(Script s) { script = s; }

		Script script;

		string[] persistingDirectives = 
		{ 
			"music" 
		};

		string LastValueOfDirective(string directiveName)
		{
			for (int i = CurrentAdvancement; i >= 0; --i)
				foreach (var dir in script[i].directives)
					if (dir.name == directiveName)
						return dir.value;
			return "";
		}
	}
}
