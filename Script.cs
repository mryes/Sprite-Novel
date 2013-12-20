using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNovel
{
	struct AdvancementDirective
	{
		public string Name;
		public string Value;

		public static AdvancementDirective Parse(string dirString)
		{
			if (dirString.Contains("="))
			{
				return new AdvancementDirective {	
					Name  = dirString.Substring(0, dirString.IndexOf('=')), 
					Value = dirString.Substring(dirString.IndexOf('=')+1)
				};
			}

			return new AdvancementDirective { 
				Name  = dirString,
				Value = "N/A" 
			};
		}
	}

	struct Advancement
	{
		public List<AdvancementDirective> Directives;
		public string Dialogue;

		public static Advancement Parse(string advString)
		{
			// Every advancement needs dialogue
			if (advString.IndexOf('"') < 0)
				throw new FormatException();

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

			return new Advancement { 
				Directives = dirs, 
				Dialogue   = advString.Substring(advString.IndexOf('"')+1).TrimEnd('"')
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
			foreach (string str in advStrings) {
				Advancement adv = Advancement.Parse(str);
				script.Add(adv);
			}

			return script;
		}
	}

	struct ScriptPath
	{
		public ScriptTree Tree;
		public string Choice;

		public ScriptPath(ScriptTree tree, string choice)
		{
			Tree = tree;
			Choice = choice;
		}
	}

	class ScriptTree
	{
		public Script Script;
		public List<ScriptPath> Paths { get; private set; }
		public ScriptTree Parent { get; private set; }

		public ScriptTree(Script stScript)
		{
			Script = stScript;
			Paths  = new List<ScriptPath>();
		}

		public void AddPath(ScriptPath path)
		{
			Paths.Add(path);
			path.Tree.Parent = this;
		}
	}
}
