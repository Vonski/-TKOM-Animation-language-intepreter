using System;
using System.IO;

namespace Code 
{
    class Scanner
    {
        public Scanner(string path)
        {
            if (File.Exists(path))
                reader = new StreamReader(path);
            else

                throw new ScannerException("There is no file with path: " + path);
        }

        public char? GetNextChar()
        {
            if (reader.EndOfStream)
                return null;
            return (char?)reader.Read();
        }

        private StreamReader reader;
    }

    class ScannerException : Exception
    {
        public ScannerException() {}
        public ScannerException(string message) : base(message) { Console.WriteLine(message); }
        public ScannerException(string message, Exception inner) : base(message,inner) { Console.WriteLine(message); }
    }
}