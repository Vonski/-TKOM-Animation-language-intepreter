using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using SFML;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Code
{
    public static class ProgramGlobals
    {
        public const int RESOLUTION_X = 800;
        public const int RESOLUTION_Y = 600;
    }
    static class Program
    {
        static void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }
        static void Main(string[] args)
        {
            RenderWindow app = new RenderWindow(new VideoMode(ProgramGlobals.RESOLUTION_X, ProgramGlobals.RESOLUTION_Y), "SFML Works!");
            app.Closed += new EventHandler(OnClose);

            Stopwatch sw = new Stopwatch();
            float dT;

            Color windowColor = new Color(255, 255, 255);

            Parser parser = new Parser("./testCode.txt");
            Queue<Statement> queue = parser.Run();

            ScopesManager sm = new ScopesManager();
            ExecutionPart(queue, sm);

            sw.Start();
            long lastCheck = 0;
            while (app.IsOpen)
            {
                app.DispatchEvents();
                dT = sw.ElapsedMilliseconds - lastCheck;
                lastCheck = sw.ElapsedMilliseconds;

                app.Clear(windowColor);

                sm.Update(dT);
                sm.Draw(app);

                app.Display();
            }
            Console.ReadKey();
        }

        public static void ExecutionPart(Queue<Statement> queue, ScopesManager sm)
        {
            foreach (Statement stat in queue)
            {
                stat.execute(sm);
                Console.WriteLine("{ " + stat + " }");
            }

        }
    }
}
