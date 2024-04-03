using System;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Sys = Cosmos.System;

namespace CosmosKernel4
{
    public class Kernel : Sys.Kernel
    {

        protected override void BeforeRun()
        {
            // Dummy Invocation, Cosmos does not include uncalled methods of types and as everyone knows, ctors are methods...xd
            // Maybe an option should be added so that you can decide if you want the all the ctors of referenced types.
            (new MyGuiWriter()).WriteText("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override unsafe void Run()
        {
            //Console.ReadKey();
            Console.SetCursorPosition(0, 0);
            var input = "asdasdasd";

            //IWriter bazService = new MyGuiWriter();
            IWriter bazService = Get<MyGuiWriter>();
            bazService.WriteText((bazService.Text is null).ToString());
            bazService.WriteText(bazService.Text);
            bazService.WriteText(bazService.GetType().Name);
            bazService.WriteText(string.Format("Text typed: {0}", input));
            Console.SetCursorPosition(0, 23);
            Console.WriteLine("Free RAM: {0}/{1}", GCImplementation.GetAvailableRAM(), CPU.GetAmountOfRAM());
            Console.Write("Used RAM: {0}", GCImplementation.GetUsedRAM());

            //Heap.Free(GCImplementation.GetPointer(bazService));
            Heap.Collect();
            //Thread.Sleep(1000);
        }

        public static T Get<T>() where T : class, new()
        {
            return new T();
        }
    }
    internal interface IWriter
    {
        public string Text { get; }
        public void WriteText(string text);
    }
    internal class MyGuiWriter : IWriter
    {
        public MyGuiWriter(string text, string ram)
        {
            Text = text;
            Ram = ram;
        }
        public MyGuiWriter(string text) : this(text, "ram")
        {
            Text = text;
        }

        public MyGuiWriter() : this("asd")
        {
        }


        public string Text { get; }
        public string Ram { get; }

        public void WriteText(string text)
        {
            Console.WriteLine(text);
        }
    }
}
