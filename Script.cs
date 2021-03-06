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
            if (dirString.Contains("=")) {
                return new AdvancementDirective {	
                    Name = dirString.Substring(0, dirString.IndexOf('=')), 
                    Value = dirString.Substring(dirString.IndexOf('=') + 1)
                };
            }

            return new AdvancementDirective { 
                Name = dirString,
                Value = "N/A" 
            };
        }
    }

    struct Advancement
    {
        public List<AdvancementDirective> Directives;
        public string Dialogue;
        public static readonly char DialogueDelimiter = '`';
        public static readonly char[] Whitespace = { ' ', '\n', '\r' };

        public static Advancement Parse(string advString)
        {
            // Every advancement needs dialogue
            if (advString.IndexOf(DialogueDelimiter) < 0)
                throw new FormatException();

            var directiveStrings = new List<string>();
            if (advString.IndexOf(DialogueDelimiter) > 0)
                directiveStrings = 
                    advString.TrimStart(Whitespace).Substring(0, advString.IndexOf(DialogueDelimiter) - 1)
                             .Trim(Whitespace).Split(' ').ToList();

//            // A newline character is the same as a "clear" directive
//            if (advString[0] == Environment.NewLine)
//                directiveStrings.Insert(0, "clear");

            var dirs = new List<AdvancementDirective>();
            foreach (string dir in directiveStrings)
                if (dir != "")
                    dirs.Add(AdvancementDirective.Parse(dir));

            return new Advancement { 
                Directives = dirs, 
                Dialogue = advString.Substring(advString.IndexOf(DialogueDelimiter) + 1)
				             		  .TrimEnd(DialogueDelimiter)
            };
        }
    }

    class Script : List<Advancement>
    {
        public static Script Parse(string scriptString)
        {
            List<string> advStrings = 
                MiscUtilities.SplitEveryNthOccurrence(
                    scriptString, 
                    Advancement.DialogueDelimiter, 2);

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
            Paths = new List<ScriptPath>();
        }

        public void AddPath(ScriptPath path)
        {
            Paths.Add(path);
            path.Tree.Parent = this;
        }
    }
}
