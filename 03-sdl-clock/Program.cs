using System;

namespace _03_sdl_clock
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SdlClock sdlClock = new SdlClock();
                sdlClock.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
