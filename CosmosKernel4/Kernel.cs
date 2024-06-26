﻿using System;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Sys = Cosmos.System;

namespace CosmosKernel4
{
    public class Kernel : Sys.Kernel
    {

        protected override void BeforeRun()
        {
            Console.Clear();
            // Dummy Invocation, Cosmos does not include uncalled methods of types and as everyone knows, ctors are methods...xd
            // Maybe an option should be added so that you can decide if you want the all the ctors of referenced types.
            var dummy = new MyGuiWriter();
            dummy.WriteText("Cosmos booted successfully. Type a line of text to get it echoed back.");
            dummy.WriteText(dummy.Ram);
            var dummy2 = new MyOtherWriter();
            dummy2.WriteText("Cosmos booted successfully. Type a line of text to get it echoed back.");
            var dummy3 = new Artesa();
            dummy.WriteText(string.Format("Struct value: {0} {1}", dummy3.are, dummy3.are2));
            var dummy4 = (Artesa)Activator.CreateInstance(typeof(Artesa))!;
            //var dummy4 = (Artesa)Activator.CreateInstance<Artesa>();
            //Cosmos.Debug.Kernel.Debugger.DoBochsBreak();
            Console.WriteLine(dummy4.GetType().AssemblyQualifiedName);
            dummy.WriteText(string.Format("Struct value: {0} {1}", dummy4.are, dummy4.are2));
            Console.ReadKey();
            Console.Clear();
        }

        protected override unsafe void Run()
        {
            //Console.ReadKey();
            Console.SetCursorPosition(0, 3);
            var input = "asdasdasd";

            //IWriter bazService = new MyGuiWriter();
            IWriter bazService = (MyOtherWriter)Activator.CreateInstance(typeof(MyOtherWriter));
            //IWriter bazService = Activator.CreateInstance<MyOtherWriter>();
            bazService.WriteText((bazService.Text is null).ToString());
            bazService.WriteText(bazService.Text);
            bazService.WriteText(bazService.GetType().Name);
            bazService.WriteText(string.Format("Text typed: {0}", input));
            Console.WriteLine(Activator.CreateInstance<Artesa>().are);
            Console.SetCursorPosition(0, 23);
            Console.WriteLine("Free RAM: {0}/{1}", GCImplementation.GetAvailableRAM(), CPU.GetAmountOfRAM());
            Console.Write("Used RAM: {0}", GCImplementation.GetUsedRAM());

            //Heap.Free(GCImplementation.GetPointer(bazService));
            //Heap.Collect();
            //Thread.Sleep(1000);
        }

        public static T Get<T>() where T : IWriter, new()
        {
            return new T();
        }
    }

    internal struct Artesa
    {
        public Artesa()
        {
            Console.WriteLine("LOL");
            are = "asds";
            are2 = "asds";
            are3 = "asds";
            are4 = "asds";
            are5 = "asds";
            are6 = "asds";
            are7 = "asds";
        }
        public string are { get; }
        public string are2 { get; }
        public string are3 { get; }
        public string are4 { get; }
        public string are5 { get; }
        public string are6 { get; }
        public string are7 { get; }
    }
    public interface IWriter
    {
        public string Text { get; }
        public void WriteText(string text);
    }
    internal class MyGuiWriter : IWriter
    {
        public MyGuiWriter(string text, string ram)
        {
            Console.WriteLine($"{text} {ram}");
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

    internal class MyOtherWriter : IWriter
    {
        private readonly IWriter writer;

        public string Text => writer.Text;



        public MyOtherWriter() : this(Kernel.Get<MyGuiWriter>())
        {
            
        }

        public MyOtherWriter(IWriter writer)
        {
            this.writer = writer;
            Console.WriteLine("Done!");
        }

        public void WriteText(string text)
        {
            writer.WriteText(text);
        }
    }
}
