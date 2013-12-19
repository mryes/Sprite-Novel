using System;
using System.Collections.Generic;
using System.Linq;

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

		public List<string> Choices
		{
			get 
			{
				if (CurrentAdvancement >= CurrentScript.Count-1)
					return (scriptTree.Paths.Select(n => n.choice).ToList());
				else return new List<string>();
			}
		}
		public bool AtChoicePoint()
		{
			return Choices.Count > 0;
		}

		Queue<int> plannedChoices = new Queue<int>();
		public void PlanChoice(int choice)
			{ plannedChoices.Enqueue(choice); }


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
			while ((status == DirectorStatus.PendingChoice)
				&& (plannedChoices.Count > 0))
			{
				TakeChoice();
				status = CheckAgainstBounds();
			}
			UpdateDirectives();
			return status;
		}


		void TakeChoice()
		{
			if  ((plannedChoices.Count > 0)
				&& (plannedChoices.Peek() < scriptTree.Paths.Count))
			{
				int choice = plannedChoices.Dequeue();
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

		string[] persistingDirectives = { "music" };
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
