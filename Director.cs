using System;
using System.Collections.Generic;

namespace SpriteNovel
{
	public enum DirectorStatus
	{
		EndOfScripts,
		PendingChoice,
		Success
	}

	class Director
	{
		public List<AdvancementDirective> CurrentDirectives 
			= new List<AdvancementDirective>();
		
		public int CurrentAdvancement
			{ get; private set; }

		ScriptTree scriptTree;
		public Script CurrentScript
			{ get { return scriptTree.script; } }

		Stack<int> plannedChoices = new Stack<int>();
		public void PlanChoice(int choice)
			{ plannedChoices.Push(choice); }

		public Director(ScriptTree s)
		{ 
			scriptTree = s;
			CurrentAdvancement = 0;
			UpdateDirectives();
		}

		public DirectorStatus Advance()
		{
			return JumpToAdvancement(CurrentAdvancement + 1);
		}

		public DirectorStatus JumpToAdvancement(int advancement)
		{
			CurrentAdvancement = advancement;
			DirectorStatus status = CheckAgainstBounds();
			while (status == DirectorStatus.PendingChoice)
			{
				TakeChoice();
				status = CheckAgainstBounds();
			}
			UpdateDirectives();
			return status;
		}
		
		string[] persistingDirectives = 
		{ 
			"music" 
		};

		void TakeChoice()
		{

			if (plannedChoices.Peek() < scriptTree.Paths.Count)
			{
				int choice = plannedChoices.Pop();
				if (CurrentAdvancement >= CurrentScript.Count)
					CurrentAdvancement -= CurrentScript.Count;
				scriptTree = scriptTree.Paths[choice].tree;
			}
		}
		
		DirectorStatus UpdateDirectives()
		{
			DirectorStatus status = CheckAgainstBounds();
			if (status != DirectorStatus.Success)
				return status;

			CurrentDirectives = 
				new List<AdvancementDirective>(CurrentScript[CurrentAdvancement].directives);
			CurrentDirectives.Add(new AdvancementDirective 
				{ name = "dialogue", value = CurrentScript[CurrentAdvancement].dialogue });

			CopyPersistingDirectives();

			return status;
		}

		DirectorStatus CheckAgainstBounds()
		{
			if (CurrentAdvancement >= CurrentScript.Count)
			{
				if (scriptTree.Paths.Count > 0)
					return DirectorStatus.PendingChoice;
				else 
				{
					CurrentAdvancement = CurrentScript.Count-1;
					return DirectorStatus.EndOfScripts;
				}
			}

			return DirectorStatus.Success;
		}

		void CopyPersistingDirectives()
		{
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

		string LastValueOfDirective(string directiveName)
		{
			for (var node = scriptTree; node != null; node = node.Parent)
				for (int i = CurrentAdvancement; i >= 0; --i)
					foreach (var dir in node.script[i].directives)
						if (dir.name == directiveName)
							return dir.value;
			return "";
		}
	}
}
