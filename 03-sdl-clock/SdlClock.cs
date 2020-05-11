using System;
using SDL2;

namespace _03_sdl_clock
{
    class SdlClock
    {
        bool quit = false;
        IntPtr renderer = IntPtr.Zero;

        public void Run()
        {
            // Initialize SDL
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                String errorDescription = "SDL_Init" +
                    " failed: " + SDL.SDL_GetError();
                throw (new Exception(errorDescription));
            }

            // Get display resolution
            SDL.SDL_DisplayMode desktopDisplayMode;
            if (SDL.SDL_GetDesktopDisplayMode(0, out desktopDisplayMode) != 0)
            {
                String errorDescription = "SDL_GetDesktopDisplayMode" +
                    " failed: " + SDL.SDL_GetError();
                throw (new Exception(errorDescription));
            }

            // Set the window size to 1/3 of the display size
            int windowWidth = desktopDisplayMode.w / 3;
            int windowHeight = desktopDisplayMode.h / 3;

            // Create the window
            IntPtr window = SDL.SDL_CreateWindow("SDL-Clock",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                windowWidth,
                windowHeight,
                SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (window == IntPtr.Zero)
            {
                String errorDescription = "SDL_CreateWindow" +
                    " failed: " + SDL.SDL_GetError();
                throw (new Exception(errorDescription));
            }

            // Create the renderer
            renderer = SDL.SDL_CreateRenderer(window, -1, 0);
            if (renderer == IntPtr.Zero)
            {
                String errorDescription = "SDL_CreateRenderer" +
                    " failed: " + SDL.SDL_GetError();
                throw (new Exception(errorDescription));
            }

            // Join the event loop
            quit = false;
            EventLoop();
            
            // Cleanup
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        void EventLoop()
        {
            SDL.SDL_Event ev;

            while (!quit)
            {
                if (SDL.SDL_WaitEventTimeout(out ev, 1000) != 0)
                {
                    switch (ev.type)
                    {
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            switch (ev.key.keysym.sym)
                            {
                                case SDL.SDL_Keycode.SDLK_ESCAPE:
                                    HandleQuit();
                                    break;
                            }
                            break;
                        case SDL.SDL_EventType.SDL_QUIT:
                            HandleQuit();
                            break;
                    }
                }
                else
                {
                    HandleTimeout();
                }

                Render();
            }
        }

        void Render()
        {
            Console.WriteLine("Render()");

            // Clear the window to selected color
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(renderer);

            // Make changes visible
            SDL.SDL_RenderPresent(renderer);
        }

        void HandleTimeout()
        {
            Console.WriteLine("HandleTimeout()");
        }

        void HandleQuit()
        {
            Console.WriteLine("HandleQuit()");
            quit = true;
        }
    }
}