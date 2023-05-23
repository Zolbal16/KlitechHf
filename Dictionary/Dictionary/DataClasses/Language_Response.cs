using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dictionary.DataClasses
{
    public class LanguageResponse
    {
        public Head Head { get; set; }
        public List<Definition> Def { get; set; }
    }

    public class Head
    {
    }

    public class Definition
    {
        public string Text { get; set; }
        public string Pos { get; set; }
        public List<Translation> Tr { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public string Pos { get; set; }
        public List<Synonym> Syn { get; set; }
        public List<Meaning> Mean { get; set; }
        public List<Example> Ex { get; set; }
    }

    public class Synonym
    {
        public string Text { get; set; }
    }

    public class Meaning
    {
        public string Text { get; set; }
    }

    public class Example
    {
        public string Text { get; set; }
        public List<Translation> Tr { get; set; }
    }
}
