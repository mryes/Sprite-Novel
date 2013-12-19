using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNovel
{
	struct AdvancementDirective
	{
		public string name;
		public string value;

		public static AdvancementDirective Parse(string dirString)
		{
			if (dirString.Contains("="))
			{
				return new AdvancementDirective 
				{	
					name  = dirString.Substring(0, dirString.IndexOf('=')), 
					value = dirString.Substring(dirString.IndexOf('=')+1)
				};
			}
			else
			{
				return new AdvancementDirective 
				{ 
					name  = dirString,
					value = "N/A" 
				};
			}
		}
	}

	struct Advancement
	{
		public List<AdvancementDirective> directives;
		public string dialogue;

		public static Advancement Parse(string advString)
		{
			// Every advancement needs dialogue
			if (advString.IndexOf('"') < 0)
				throw new ArgumentException();

			var directiveStrings = new List<string>();
			if (advString.IndexOf('"') > 0)
				directiveStrings = 
					advString.TrimStart('\n').Substring(0, advString.IndexOf('"')-1).Trim(' ')
							 .Split(' ').ToList();

			// A newline character is the same as a "clear" directive
			if (advString[0] == '\n')
				directiveStrings.Insert(0, "clear");

			var dirs = new List<AdvancementDirective>();
			foreach(string dir in directiveStrings)
				if (dir != "")
					dirs.Add(AdvancementDirective.Parse(dir));

			return new Advancement 
			{ 
				directives = dirs, 
				dialogue   = advString.Substring(advString.IndexOf('"')+1).TrimEnd('"')
			};
		}
	}

	class Script : List<Advancement>
	{
		public static Script Parse(string scriptString)
		{
			List<string> advStrings = 
				MiscUtilities.SplitEveryNthOccurrence(scriptString, '"', 2);

			var script = new Script();
			foreach (string str in advStrings)
			{
				Console.WriteLine(str);
				Advancement adv = Advancement.Parse(str);
                script.Add(adv);
			}

			return script;
		}
	}
}
