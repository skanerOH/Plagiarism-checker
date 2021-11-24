using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;

namespace Antiplag
{
    static class AntiplagChecker
    {
        static string dataDirectoryPath = "../../../data";
        static string stopwordsFilePath = "../../../stopwords.txt";
        static char[] splitSympols = new char[] { ' ', '\t', '\r', '\n', '.', ',', '!', '?', ':', ';', '-', '(', ')' };
        static HashSet<string> stopwordsSet;

        static AntiplagChecker()
        {
            string fileText = File.ReadAllText(stopwordsFilePath);
            stopwordsSet = fileText.Split(new char[] { '\n' }).Select(p => p.Trim(new char[] { '\r' })).ToHashSet();
        }

        public static string CheckPlagiarism(string inpPath, int shingleSize)
        {
            var inpWordList = FileToWordList(inpPath);
            var inpShingles = WordListToShingles(inpWordList, shingleSize);
            var dataFilesNames = Directory.GetFiles(dataDirectoryPath);
            var report = new List<ReportItem>();

            foreach (var filePath in dataFilesNames)
            {
                var testWords = FileToWordList(filePath);
                var testShingleHashes = WordListToShingles(testWords, shingleSize).Select(s => s.GetHashCode()).ToHashSet();

                var reportItem = new ReportItem(filePath);
                bool inColl = false;
                int collCounter = 0;
                string currColl = "";

                foreach (var inpSh in inpShingles)
                    if (testShingleHashes.Contains(inpSh.GetHashCode()))
                    //{
                    //    collCounter++;
                    //    reportItem.Collisions.Add(inpSh);
                    //}
                    {
                        collCounter++;
                        if (inColl)
                        {
                            currColl += ' ' + inpSh.Split(' ').Last();
                        }
                        else
                        {
                            inColl = true;
                            currColl += inpSh;
                        }
                    }
                    else
                    {
                        inColl = false;
                        if (!String.IsNullOrEmpty(currColl))
                        {
                            reportItem.Collisions.Add(currColl);
                            currColl = "";
                        }
                    }

                reportItem.PlagiarismCoef = (double)collCounter / inpShingles.Count();

                report.Add(reportItem);
            }

            return JsonConvert.SerializeObject(report, Formatting.Indented);
        }

        static IList<string> FileToWordList(string path)
        {
            string fileText = File.ReadAllText(path);
            return fileText.Split(splitSympols)
                .Select(w => w.ToLower().Trim(' ', '\t'))
                .Where(w => !String.IsNullOrWhiteSpace(w))
                .Where(w => !stopwordsSet.Contains(w))
                .ToList();
        }

        static IList<string> WordListToShingles(IList<string> words, int wordsInShingleCount)
        {
            var endIndex = words.Count - wordsInShingleCount + 1;
            var wordsArr = words.ToArray();
            var res = new string[endIndex];

            for (int i = 0; i < res.Length; i++)
                res[i] = String.Join(" ", SubArray(wordsArr, i, wordsInShingleCount));

            return res;
        }

        static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

    class ReportItem
    {
        public ReportItem(string name)
        {
            FileName = name;
            PlagiarismCoef = 0;
            Collisions = new List<string>();
        }


        public string FileName { get; set; }

        public double PlagiarismCoef { get; set; }

        public List<string> Collisions { get; set; }
    }
}
