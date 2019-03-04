using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsoleAppWords
{
    class Program
    {

        struct WordInfo
        {
            public string Original;
            public string Translation;
            public bool Used;
            public WordInfo(string original, string translation)
            {
                Original = original;
                Translation = translation;
                Used = false;
            }
        }

        List<WordInfo> Dict = new List<WordInfo>();

        Random WordRandomizer = new Random();

        struct Record
        {
            public string Word;
            public bool Valid;
            public Record(string word, bool valid)
            {
                Word = word;
                Valid = valid;
            }
        }
        class Quiz
        {
            public string OriginalWord { get; set; }
            public List<Record> Variants { get; set; }
            public Quiz()
            {
                OriginalWord = "";
                Variants = new List<Record>();
            }

            private static Random rng = new Random();

            public void Shuffle()
            {
                int n = Variants.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    Record value = Variants[k];
                    Variants[k] = Variants[n];
                    Variants[n] = value;
                }
            }
        }

        bool IsDuplicate(WordInfo wordInfo)
        {
            foreach(var rec in Dict)
            {
                if(rec.Original.Equals(wordInfo.Original))
                {
                    return true;
                }
                if (rec.Translation.Equals(wordInfo.Translation))
                {
                    return true;
                }
            }
            return false;
        }
        // FreeDict TEI format naive parser
        void LoadDictionary(string source)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(source);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "text")
                    {
                        foreach (XmlNode textNode in node)
                        {
                            if (textNode.Name == "body")
                            {
                                foreach (XmlNode bodyNode in textNode)
                                {
                                    if (bodyNode.Name == "entry")
                                    {
                                        string orth = "";
                                        string cit = "";
                                        foreach (XmlNode entryNode in bodyNode)
                                        {
                                            if (entryNode.Name == "form")
                                            {
                                                foreach (XmlNode formNode in entryNode)
                                                {
                                                    if (formNode.Name == "orth")
                                                    {
                                                        orth = formNode.InnerText;
                                                    }
                                                }
                                            }
                                            if (entryNode.Name == "sense")
                                            {
                                                foreach (XmlNode senseNode in entryNode)
                                                {
                                                    if (senseNode.Name == "cit")
                                                    {
                                                        cit = senseNode.InnerText;
                                                    }
                                                }
                                            }
                                        }
                                        if ((orth.Length > 0) && (cit.Length > 0))
                                        {
//                                            Console.WriteLine(orth + " " + cit);
                                            WordInfo wordInfo = new WordInfo(orth, cit);
                                            if(!IsDuplicate(wordInfo))
                                            {
                                                Dict.Add(new WordInfo(orth, cit));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


            }
            catch (XmlException)
            {

            }
        }
        bool GetNotUsedWord(ref WordInfo wordInfo)
        {
            int rndIdx = WordRandomizer.Next(0, Dict.Count);
            for (int i = rndIdx; i < Dict.Count; ++i)
            {
                if (!Dict[i].Used)
                {
                    wordInfo = Dict[i];
                    wordInfo.Used = true;
                    Dict[i] = wordInfo;
                    return true;
                }
            }
            for (int i = 0; i < rndIdx; ++i)
            {
                if (!Dict[i].Used)
                {
                    wordInfo = Dict[i];
                    wordInfo.Used = true;
                    Dict[i] = wordInfo;
                    return true;
                }
            }
            return false;
        }
        void TestGetNotUsedWord()
        {
            int count = 0;
            WordInfo wordInfo = new WordInfo();
            while (GetNotUsedWord(ref wordInfo))
            {
                ++count;
                if ((count % 1000) == 0)
                {
                    Console.WriteLine("TestGetNotUsedWord " + count);
                }
            }
            Console.WriteLine("TestGetNotUsedWord finished " + count + " Dictionary size " + Dict.Count);
        }
        bool GetRandomQuiz(out Quiz quiz, int numVariants)
        {
            quiz = new Quiz();
            if (numVariants > 0)
            {
                WordInfo wordInfo = new WordInfo();
                if (GetNotUsedWord(ref wordInfo))
                {
                    quiz.OriginalWord = wordInfo.Original;
                    quiz.Variants.Add(new Record(wordInfo.Translation, true));
                    --numVariants;
                    while (numVariants > 0)
                    {
                        if (GetNotUsedWord(ref wordInfo))
                        {
                            quiz.Variants.Add(new Record(wordInfo.Translation, false));
                            --numVariants;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        void TestGetRandomQuiz()
        {
            int numVariants = 4;
            Quiz quiz;
            if (GetRandomQuiz(out quiz, numVariants))
            {
                quiz.Shuffle();
                Console.WriteLine(quiz.OriginalWord);
                foreach (var variant in quiz.Variants)
                {
                    Console.WriteLine(variant.Word + " " + variant.Valid);
                }
            }
            else
            {
                Console.WriteLine("TestGetRandomQuiz failed");
            }
        }
        int GetSelection(string header, List<string> variants)
        {
            Console.WriteLine(header);
            int lineNum = 1;
            foreach (var variant in variants)
            {
                Console.WriteLine(lineNum++ + " " + variant);
            }
            int idx = 0;
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                idx = key.KeyChar - '1';
            } while (!(idx >= 0 && idx < variants.Count));
            return idx;
        }
        void TestGetSelection()
        {
            List<string> variants = new List<string>();
            variants.Add(" press number key");
            variants.Add(" press number key");
            variants.Add(" press number key");
            variants.Add(" press number key");
            int idx = GetSelection("Hello", variants);
            Console.WriteLine("Variant " + idx + " selected");
        }
        bool PlayQuiz(ref bool won)
        {
            int numVariants = 4;
            Quiz quiz;
            if (GetRandomQuiz(out quiz, numVariants))
            {
                quiz.Shuffle();
                List<string> variants = new List<string>();
                foreach (var variant in quiz.Variants)
                {
                    variants.Add(variant.Word);
                }
                int idx = GetSelection("Proper translation of '" + quiz.OriginalWord + "'", variants);
                won = quiz.Variants[idx].Valid;
                return true;
            }
            return false;
        }

        void Play()
        {
            bool won = false;
            while (PlayQuiz(ref won))
            {
                if (won)
                {
                    Console.WriteLine("Right !");
                }
                else
                {
                    Console.WriteLine("Wrong...");
                }
            }
            Console.WriteLine("Game finished");
        }
        static void Main(string[] args)
        {
            Program program = new Program();
            try
            {
                program.LoadDictionary("eng-rus.tei");
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("Please download https://github.com/freedict/fd-dictionaries/blob/master/eng-rus/eng-rus.tei");
            }
            //"github\\fd-dictionaries\\eng-rus\\eng-rus.tei"
            //program.TestGetNotUsedWord();
            //program.TestGetRandomQuiz();
            //program.TestGetSelection();
            program.Play();
            Console.ReadKey(true);
        }
    }
}
