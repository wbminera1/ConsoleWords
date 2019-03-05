using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppBlank
{
    class Program
    {
        public struct Letter
        {
            public string[] Lines;
            public Letter(string[] strings)
            {
                Lines = strings;
            }

            public void Load(string path)
            {
                string text = System.IO.File.ReadAllText(path);
                Lines = text.Split('\n');
                Normalize();
            }
            public void Normalize()
            {
                // blank lines 
                // leading - trailing spaces
                int notBlankStarting = -1;
                int notBlankEnding = -1;
                for (int i = 0; i < Lines.Length; ++i)
                {
                    string str = Lines[i];
                    str = string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                    if (str.Length != 0)
                    {
                        if (notBlankStarting == -1)
                        {
                            notBlankStarting = i;
                        }
                        else
                        {
                            notBlankEnding = i;
                        }
                    }
                }
                if (notBlankStarting != -1 && notBlankEnding != -1)
                {
                    string[] newLines = new string[notBlankEnding - notBlankStarting + 1];
                    int trailingOffset = int.MaxValue;
                    int maxLen = -1;
                    for (int i = 0; i < newLines.Length; ++i)
                    {
                        string newStr = Lines[notBlankStarting + i].TrimEnd();
                        int offset = newStr.TakeWhile(c => char.IsWhiteSpace(c)).Count();
                        if (offset < trailingOffset)
                        {
                            trailingOffset = offset;
                        }
                        if(maxLen < newStr.Length)
                        {
                            maxLen = newStr.Length;
                        }
                        newLines[i] = newStr;
                    }
                    maxLen -= trailingOffset;
                    for (int i = 0; i < newLines.Length; ++i)
                    {
                        string str = newLines[i].Substring(trailingOffset);
                        str = str.PadRight(maxLen, ' ');
                        newLines[i] = str;
                    }
                    Lines = newLines;
                }
            }

            public bool SplitByVerticalSpaceLine(out Letter letter)
            {
                Normalize();
                letter = new Letter();
                return false;
            }
            public void Print()
            {
                foreach (var line in Lines)
                {
                    string str = line;
                    Console.WriteLine(str.Replace(' ', '.'));
                }
            }

        }

    public class StringOfLetters
        {
            public List<Letter> Letters = new List<Letter>();

            public bool Load(string path)
            {
                bool res = false;
                try
                {
                    string text = System.IO.File.ReadAllText(path);
                    string[] strings = text.Split('\n');
                    Letter letter = new Letter(strings);
                    letter.Normalize();
                    letter.Print();
                    res = true;
                }
                catch (System.IO.FileNotFoundException)
                {

                }
                return res;
            }
        }

        public class TraningSet
        {
            public Dictionary<char, Letter> Set = new Dictionary<char, Letter>();
            public void Load(string filepath, string ext)
            {
                for (char chr = 'A'; chr <= 'Z'; ++chr)
                {
                    try
                    {
                        string fileName = filepath + chr + ext;
                        Letter letter = new Letter();
                        letter.Load(fileName);
                        Set.Add(chr, letter);
                    }
                    catch (System.IO.FileNotFoundException)
                    {

                    }
                }
            }
        }

        public interface Integral
        {
            List<string> Calculate(Letter letter);
        }

        public class HScanIntegral : Integral
        {
            public List<string> Calculate(Letter letter)
            {
                List<string> res = new List<string>();
                string last = "";
                foreach(var line in letter.Lines)
                {
                    string lineRes = ScanLine(line);
                    if(lineRes != last)
                    {
                        //Console.WriteLine(lineRes);
                        last = lineRes;
                        res.Add(lineRes);
                    }
                }
                return res;
            }

            public string ScanLine(string line)
            {
                string res = "";
                foreach(var chr in line)
                {
                    if(chr == ' ')
                    {
                        if(res.Length == 0 || res.Last() != '0' )
                        {
                            res = res + '0';
                        }
                    }
                    else
                    {
                        if (res.Length == 0 || res.Last() != '1')
                        {
                            res = res +'1';
                        }
                    }
                }
                return res;
            }
        }

        public void TestTrainingSet()
        {
            TraningSet traningSet = new TraningSet();
            traningSet.Load(@"tests\test", @".txt");
            traningSet.Set['A'].Print();

            TraningSet traningSet2 = new TraningSet();
            traningSet2.Load(@"tests\test", @"2.txt");
            traningSet2.Set['A'].Print();

        }

        public void TestString()
        {
            StringOfLetters stringOfLetters = new StringOfLetters();
            if(stringOfLetters.Load(@"tests\teststr.txt"))
            {
                //stringOfLetters.
            }
        }

        public class Comparator
        {
            public char Compare(List<TraningSet> traningSets, Letter letter, Integral integral)
            {
                char res = '?';
                List<string> letterIntegral = integral.Calculate(letter);
                foreach (var trainingSet in traningSets)
                {
                    foreach(var setLetter in trainingSet.Set)
                    {
                        List<string> setIntegral = integral.Calculate(setLetter.Value);
                        if(Equal(setIntegral, letterIntegral))
                        {
                            return setLetter.Key;
                        }
                    }
                }
                return res;
            }

            bool Equal(List<string> set1, List<string> set2)
            {
                if(set1.Count == set2.Count)
                {
                    for(int i = 0; i < set1.Count; ++i)
                    {
                        if(set1[i] != set2[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }

        public void TestHScanIntegral()
        {
            HScanIntegral hScanIntegral = new HScanIntegral();
            //             Console.WriteLine(hScanIntegral.ScanLine("  *  "));
            //             Console.WriteLine(hScanIntegral.ScanLine(" *** "));
            //             Console.WriteLine(hScanIntegral.ScanLine("**  "));
            TraningSet traningSet = new TraningSet();
            traningSet.Load(@"tests\test", @".txt");
            hScanIntegral.Calculate(traningSet.Set['A']);

            Console.WriteLine("");

            TraningSet traningSet2 = new TraningSet();
            traningSet2.Load(@"tests\test", @"2.txt");
            hScanIntegral.Calculate(traningSet2.Set['A']);
        }

        void Run(string file)
        {
            List<TraningSet> traningSets = new List<TraningSet>();

            TraningSet traningSet = new TraningSet();
            traningSet.Load(@"tests\test", @"x.txt");
            traningSets.Add(traningSet);

//             TraningSet traningSet2 = new TraningSet();
//             traningSet2.Load(@"tests\test", @"2.txt");
//             traningSets.Add(traningSet2);

            try
            {
                Letter letter = new Letter();
                letter.Load(file);

                Comparator comparator = new Comparator();
                HScanIntegral integral = new HScanIntegral();
                
                char res = comparator.Compare(traningSets, letter, integral);

                Console.WriteLine("Result '" + res + "'");
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("File not found " + file);
            }

        }

        void TestCompareSets()
        {
            Console.WriteLine("testX2.txt vs textX.txt:");

            List<TraningSet> traningSets = new List<TraningSet>();

            TraningSet traningSet = new TraningSet();
            traningSet.Load(@"tests\test", @".txt");
            traningSets.Add(traningSet);

            TraningSet traningSet2 = new TraningSet();
            traningSet2.Load(@"tests\test", @"2.txt");

            Comparator comparator = new Comparator();
            HScanIntegral integral = new HScanIntegral();

            foreach(var letter in traningSet2.Set)
            {
                char res = comparator.Compare(traningSets, letter.Value, integral);
                Console.WriteLine("Result: symbol '" + letter.Key + "' == " + "'" + res + "'");
            }

        }

        static void Main(string[] args)
        {
            Program program = new Program();
            //program.TestTrainingSet();
            //program.TestString();
            //program.TestHScanIntegral();
            //program.TestCompareSets();
            if (args.Length > 0)
            {
                program.Run(args[0]);
            }
            else
            {
                Console.WriteLine("Usage: program filename");
            }
            Console.ReadKey();
        }
    }
}
