using System;
using System.Collections.Generic;
using SDL2;

namespace _03_sdl_clock
{
    struct Size
    {
        public int width;
        public int height;
    }

    class SdlClock
    {
        const int fontTileSize = 28;
        const int bufferTextureWidth = 10 * fontTileSize;
        const int bufferTextureHeight = 2 * fontTileSize;

        bool quit = false;
        IntPtr window = IntPtr.Zero;
        IntPtr renderer = IntPtr.Zero;
        IntPtr fontTexture = IntPtr.Zero;
        String clockTimeString = "";
        String clockDateString = "";
        Size currentWindowSize = new Size() { width = 0, height = 0 };
        Dictionary<Char, SDL.SDL_Rect> fontRectMap = new Dictionary<Char, SDL.SDL_Rect>();
        IntPtr bufferTexture;

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
            currentWindowSize.width = desktopDisplayMode.w / 3;
            currentWindowSize.height = desktopDisplayMode.h / 3;

            // Create the window
            window = SDL.SDL_CreateWindow("SDL-Clock",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                currentWindowSize.width,
                currentWindowSize.height,
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

            // Load a bitmap and create a texture out of it
            fontTexture = SDL_image.IMG_LoadTexture(renderer, "font.png");
            if (renderer == IntPtr.Zero)
            {
                String errorDescription = "IMG_LoadTexture" +
                    " failed: " + SDL.SDL_GetError();
                throw (new Exception(errorDescription));
            }

            // Create font texture mapping
            CreateFontRectMap();

            // Create buffer texture for text scaling
            bufferTexture = SDL.SDL_CreateTexture(
                renderer, SDL.SDL_PIXELFORMAT_BGRA8888,
                (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
                bufferTextureWidth,
                bufferTextureHeight);
            if (renderer == IntPtr.Zero)
            {
                String errorDescription = "SDL_CreateTexture" +
                    " failed: " + SDL.SDL_GetError();
                throw (new Exception(errorDescription));
            }

            // Join the event loop
            quit = false;
            EventLoop();

            // Cleanup
            SDL.SDL_DestroyTexture(fontTexture);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }

        void EventLoop()
        {
            SDL.SDL_Event ev;
            DateTime nextRenderTime = DateTime.UtcNow;
            TimeSpan timeAccumulator = TimeSpan.Zero;
            DateTime lastFrameTime = DateTime.UtcNow;

            Update();
            Render();

            while (!quit)
            {
                int loopIterationTime = 500;
                DateTime currentTime = DateTime.UtcNow;
                timeAccumulator += currentTime - lastFrameTime;
                lastFrameTime = currentTime;

                int waitTime = loopIterationTime - ((int)timeAccumulator.TotalMilliseconds);
                waitTime = waitTime < 0 ? 0 : waitTime;

                if (waitTime > 0)
                {
                    if (SDL.SDL_WaitEventTimeout(out ev, waitTime) != 0)
                    {
                        switch (ev.type)
                        {
                            case SDL.SDL_EventType.SDL_KEYDOWN:
                                switch (ev.key.keysym.sym)
                                {
                                    case SDL.SDL_Keycode.SDLK_ESCAPE:
                                        Quit();
                                        break;
                                }
                                break;
                            case SDL.SDL_EventType.SDL_QUIT:
                                Quit();
                                break;
                            case SDL.SDL_EventType.SDL_WINDOWEVENT:
                                switch (ev.window.windowEvent)
                                {
                                    case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                        Resized();
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        Timeout();
                    }
                }

                if ((int)timeAccumulator.TotalMilliseconds >= loopIterationTime)
                {
                    Update();
                    Render();
                    timeAccumulator = TimeSpan.FromMilliseconds(
                        (int)timeAccumulator.TotalMilliseconds % loopIterationTime);
                }
            }
        }

        void Resized()
        {
            SDL.SDL_GetWindowSize(window, out currentWindowSize.width, out currentWindowSize.height);
            Console.WriteLine("Resized(): to " + currentWindowSize.width + "x" + currentWindowSize.height);
        }

        void Update()
        {
            DateTime currentTime = DateTime.Now;

            Console.WriteLine("Update()");

            clockTimeString = currentTime.ToString("HH:mm:ss");
            clockDateString = currentTime.ToString("yyyy-MM-dd");
        }

        void RenderDateAndTime()
        {
            int x = fontTileSize;

            SDL.SDL_SetRenderTarget(renderer, bufferTexture);
            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL.SDL_RenderClear(renderer);
            foreach (Char c in clockTimeString)
            {
                var srcRect = fontRectMap[c];
                var dstRect = new SDL.SDL_Rect() { x = x, y = 0, w = fontTileSize, h = fontTileSize };
                SDL.SDL_RenderCopy(renderer, fontTexture, ref srcRect, ref dstRect);
                x += fontTileSize;
            }

            x = 0;

            foreach (Char c in clockDateString)
            {
                var srcRect = fontRectMap[c];
                var dstRect = new SDL.SDL_Rect() { x = x, y = fontTileSize, w = fontTileSize, h = fontTileSize };
                SDL.SDL_RenderCopy(renderer, fontTexture, ref srcRect, ref dstRect);
                x += fontTileSize;
            }

            SDL.SDL_SetRenderTarget(renderer, IntPtr.Zero);
            var srcRect2 = new SDL.SDL_Rect() { x = 0, y = 0, w = bufferTextureWidth, h = bufferTextureHeight };
            var dstRect2 = new SDL.SDL_Rect() { x = 0, y = 0, w = currentWindowSize.width, h = currentWindowSize.height };
            SDL.SDL_RenderCopy(renderer, bufferTexture, ref srcRect2, ref dstRect2);
        }

        void Render()
        {
            Console.WriteLine("Render()");

            // Clear the window to selected color
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(renderer);

            // Render date and time
            RenderDateAndTime();

            // Make changes visible
            SDL.SDL_RenderPresent(renderer);
        }

        void Timeout()
        {
            Console.WriteLine("Timeout()");
        }

        void Quit()
        {
            Console.WriteLine("Quit()");
            quit = true;
        }

        void CreateFontRectMap()
        {
            fontRectMap.Add('1', new SDL.SDL_Rect() { x = 0, y = 0, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('2', new SDL.SDL_Rect() { x = fontTileSize, y = 0, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('3', new SDL.SDL_Rect() { x = fontTileSize * 2, y = 0, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('4', new SDL.SDL_Rect() { x = fontTileSize * 3, y = 0, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('5', new SDL.SDL_Rect() { x = fontTileSize * 4, y = 0, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add(':', new SDL.SDL_Rect() { x = fontTileSize * 5, y = 0, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('6', new SDL.SDL_Rect() { x = 0, y = fontTileSize, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('7', new SDL.SDL_Rect() { x = fontTileSize, y = fontTileSize, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('8', new SDL.SDL_Rect() { x = fontTileSize * 2, y = fontTileSize, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('9', new SDL.SDL_Rect() { x = fontTileSize * 3, y = fontTileSize, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('0', new SDL.SDL_Rect() { x = fontTileSize * 4, y = fontTileSize, w = fontTileSize, h = fontTileSize });
            fontRectMap.Add('-', new SDL.SDL_Rect() { x = fontTileSize * 5, y = fontTileSize, w = fontTileSize, h = fontTileSize });
        }
    }
}