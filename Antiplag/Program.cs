using System;

namespace Antiplag
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(AntiplagChecker.CheckPlagiarism("../../../input.txt", 2));
        }
    }
}
