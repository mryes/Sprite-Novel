using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteNovel
{
    public enum DirectorAdvancementStatus
    {
        EndOfScripts,
        PendingChoice,
        Success
    }

    class Director
    {
        public static readonly string DirectiveNotFoundError = "Directive Not Found";
        public List<AdvancementDirective> CurrentDirectives;
        public Script FlatScript
        {
            get {
                var oldDirectives = CurrentDirectives;
                var flatScript = new Script();
                for (var node = scriptTree; node != null; node = node.Parent)
                    for (int i = (node == scriptTree) ? currentAdvancementNum : node.Script.Count - 1; i >= 0; --i) {
                        CurrentDirectives = node.Script[i].Directives;
                        CopyPersistingDirectives();
                        flatScript.Add(node.Script[i]);
                }
                CurrentDirectives = oldDirectives;
                flatScript.Reverse();
                return flatScript;
            }
        }

        public List<string> Choices {
            get {
                if (currentAdvancementNum >= CurrentScript.Count - 1)
                    return (scriptTree.Paths.Select(n => n.Choice).ToList());
                return new List<string>();
            }
        }

        public Director(ScriptTree s)
        { 
            scriptTree = s;
            currentAdvancementNum = 0;
            CurrentDirectives = new List<AdvancementDirective>();
            UpdateDirectives();
        }

        public AdvancementDirective GetDirective(string name)
        {
            foreach (var adv in CurrentDirectives)
                if (adv.Name == name)
                    return adv;
            return new AdvancementDirective
			{ Name = DirectiveNotFoundError, Value = "N/A" };
        }

        public bool GaveDirective(string name)
        {
            return GetDirective(name).Name != DirectiveNotFoundError;
        }

        public DirectorAdvancementStatus AdvanceOnce()
        {
            return JumpToAdvancement(currentAdvancementNum + 1);
        }

        public DirectorAdvancementStatus JumpToAdvancement(int advancement)
        {
            int oldAdvancementNum = currentAdvancementNum;
            currentAdvancementNum = advancement;
            DirectorAdvancementStatus status = CheckAgainstBounds();
            while ((status == DirectorAdvancementStatus.PendingChoice)
            && (plannedChoices.Count > 0)) {
                TakeChoice();
                status = CheckAgainstBounds();
            }
            if (status == DirectorAdvancementStatus.Success)
                UpdateDirectives();
            else
                currentAdvancementNum = oldAdvancementNum;
            return status;
        }

        public void PlanChoice(int choice)
        {
            plannedChoices.Enqueue(choice); 
        }

        public bool AtChoicePoint()
        { 
            return Choices.Count > 0; 
        }

        ScriptTree scriptTree;

        Script CurrentScript { 
            get { return scriptTree.Script; }
        }

        int currentAdvancementNum;

        Advancement CurrentAdvancement {
            get { return CurrentScript[currentAdvancementNum]; } 
        }

        Queue<int> plannedChoices = new Queue<int>();

        DirectorAdvancementStatus UpdateDirectives()
        {
            DirectorAdvancementStatus status = CheckAgainstBounds();
            if (status != DirectorAdvancementStatus.Success)
                return status;

            CurrentDirectives = 
				new List<AdvancementDirective>(CurrentAdvancement.Directives);
            CurrentDirectives.Add(new AdvancementDirective { 
                Name = "dialogue", 
                Value = CurrentAdvancement.Dialogue
            });

            CopyPersistingDirectives();

            return status;
        }

        DirectorAdvancementStatus CheckAgainstBounds()
        {
            if (currentAdvancementNum >= CurrentScript.Count) {
                if (scriptTree.Paths.Count > 0)
                    return DirectorAdvancementStatus.PendingChoice;

                currentAdvancementNum = CurrentScript.Count - 1;
                return DirectorAdvancementStatus.EndOfScripts;
            }

            return DirectorAdvancementStatus.Success;
        }

        void TakeChoice()
        {
            if ((plannedChoices.Count > 0)
            && (plannedChoices.Peek() < scriptTree.Paths.Count)) {
                int choice = plannedChoices.Dequeue();
                if (currentAdvancementNum >= CurrentScript.Count)
                    currentAdvancementNum -= CurrentScript.Count;
                scriptTree = scriptTree.Paths[choice].Tree;
            }
        }

        readonly string[] persistingDirectives = { "music", "character" };

        void CopyPersistingDirectives()
        {
            var directiveNames = new List<string>();
            foreach (var curDir in CurrentDirectives)
                directiveNames.Add(curDir.Name);
            foreach (var perDir in persistingDirectives) {
                if (!directiveNames.Contains(perDir)) {
                    string lastValue = LastValueOfDirective(perDir);
                    if (lastValue != "")
                        CurrentDirectives.Add(new AdvancementDirective 
							{ Name = perDir, Value = lastValue });
                }
            }
        }

        string LastValueOfDirective(string directiveName)
        {
            for (var node = scriptTree; node != null; node = node.Parent)
                for (int i = (node == scriptTree) ? currentAdvancementNum : node.Script.Count-1; i >= 0; --i)
                    foreach (var dir in node.Script[i].Directives)
                        if (dir.Name == directiveName)
                            return dir.Value;
            return "";
        }
    }
}
