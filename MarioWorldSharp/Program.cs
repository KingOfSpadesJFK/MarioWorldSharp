﻿using MarioWorldSharp.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
using KdTree;
using KdTree.Math;
using System.Reflection;
using System.IO;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;
using System.Threading;

namespace MarioWorldSharp
{
    /// <summary>
    /// The main class.
    /// </summary>
    /// 
    public static class Program
    {
        #region Console
        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();

        private const int STD_OUTPUT_HANDLE = -11;
        private static bool showConsole = false; //Or false if you don't want to see the console
        private static bool runGame = true;
        public static bool powerSavingMode = false;
        public static SMW Instance { get; set; }
        #endregion

        public static Thread MainThread;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "MainThread";
            MainThread = Thread.CurrentThread;
            if (args.Length > 0)
            {
                foreach (string s in args)
                {
                    switch (s)
                    {
                        case "-c":
                            showConsole = true;
                            break;
                        case "-p":
                            powerSavingMode = true;
                            break;
                    }
                }
            }

            if (showConsole)
            {
                AllocConsole();
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                Microsoft.Win32.SafeHandles.SafeFileHandle safeFileHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
            }

            if (runGame)
            {
                using (var game = new SMW())
                {
                    Instance = game;
                    game.Run();
                }
            }
            else
            {
                TestKdTree();
                while (true) ;
            }
        }

        public static byte[] ExtractEmbeddedBin(string filePath)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            using Stream s = assembly.GetManifestResourceStream($"MarioWorldSharp.{filePath}");
            using BinaryReader b = new BinaryReader(s);
            return b.ReadBytes((int)s.Length);
        }

        private static void TestKdTree()
        {
            KdTree<double, int> tree = new KdTree<double, int>(2, new DoubleMath());
            Random rand = new Random();

            for (int SampleSize = 0; SampleSize > 0; SampleSize--)
            {
                tree.Add(new[] { rand.NextDouble() * 100.0, rand.NextDouble() * 100.0 }, rand.Next() % 100);
            }
            tree.Add(new[] { rand.NextDouble() * 100.0, rand.NextDouble() * 100.0 }, 16);

            double x = rand.NextDouble() * 100.0;
            double y = rand.NextDouble() * 100.0;
            Console.WriteLine($"Finding: ({x}, {y})...");
            Console.WriteLine(tree.FindValue(15)?[0]);

            //Find Value returns null if no value can be found
        }
    }
}
