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
		ScriptTree scriptTree;

		Script CurrentScript
			{ get { return scriptTree.script; } }

		int CurrentAdvancementNum;
		public Advancement CurrentAdvancement
			{ get { return CurrentScript[CurrentAdvancementNum]; } }

		public List<AdvancementDirective> CurrentDirectives 
			= new List<AdvancementDirective>();

		public AdvancementDirective GetDirective(string name)
		{
			foreach (var adv in CurrentDirectives)
				if (adv.name == name)
					return adv;
			return new AdvancementDirective
				{ name = "N/A", value = "N/A" };
		}
		
		public List<string> Choices
		{
			get 
			{
				if (CurrentAdvancementNum >= CurrentScript.Count-1)
					return (scriptTree.Paths.Select(n => n.choice).ToList());
				else return new List<string>();
			}
		}

		public bool AtChoicePoint()
			{ return Choices.Count > 0; }

		Queue<int> plannedChoices = new Queue<int>();
		public void PlanChoice(int choice)
			{ plannedChoices.Enqueue(choice); }

		public Director(ScriptTree s)
		{ 
			scriptTree = s;
			CurrentAdvancementNum = 0;
			UpdateDirectives();
		}

		public DirectorStatus Advance()
		{
			return JumpToAdvancement(CurrentAdvancementNum + 1);
		}

		public DirectorStatus JumpToAdvancement(int advancement)
		{
			CurrentAdvancementNum = advancement;
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
				if (CurrentAdvancementNum >= CurrentScript.Count)
					CurrentAdvancementNum -= CurrentScript.Count;
				scriptTree = scriptTree.Paths[choice].tree;
			}
		}
		
		DirectorStatus UpdateDirectives()
		{
			DirectorStatus status = CheckAgainstBounds();
			if (status != DirectorStatus.Success)
				return status;

			CurrentDirectives = 
				new List<AdvancementDirective>(CurrentAdvancement.directives);
			CurrentDirectives.Add(new AdvancementDirective 
				{ name = "dialogue", value = CurrentAdvancement.dialogue });

			CopyPersistingDirectives();

			return status;
		}

		DirectorStatus CheckAgainstBounds()
		{
			if (CurrentAdvancementNum >= CurrentScript.Count)
			{
				if (scriptTree.Paths.Count > 0)
					return DirectorStatus.PendingChoice;
				else 
				{
					CurrentAdvancementNum = CurrentScript.Count-1;
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
				for (int i = CurrentAdvancementNum; i >= 0; --i)
					foreach (var dir in node.script[i].directives)
						if (dir.name == directiveName)
							return dir.value;
			return "";
		}
	}
}
