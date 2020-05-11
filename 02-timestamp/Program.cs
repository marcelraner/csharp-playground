using System;

namespace _02_timestamp
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime time = DateTime.UtcNow;
            String timeString = time.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Console.WriteLine(timeString);
        }
    }
}
