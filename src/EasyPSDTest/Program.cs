using System;
using System.IO;
using EasyPSD;

internal class Program
{
    private static void Main(string[] args)
    {
        var reader = new PsdReader();
        var file = new FileInfo(args[0]);
        var result = reader.ReadFile(file.FullName);
        if (result.HasErrors)
        {
            Console.WriteLine(false);
        }
        else
        {
            Console.WriteLine(true);
        }
    }
}