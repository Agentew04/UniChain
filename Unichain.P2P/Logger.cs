using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P; 
public class Logger{

    private static readonly object lockObj = new();

    private readonly string name;

    public Logger(string name = "") {
        this.name = name;
    }

    [DebuggerStepThrough]
    public void Log(string message) {
        lock (lockObj)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{name}] {message}");
            Console.ResetColor();
        }
    }

    [DebuggerStepThrough]
    public void LogError(string message) {
        lock (lockObj) { 
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{name}] {message}");
        Console.ResetColor();
        }
    }

    [DebuggerStepThrough]
    public void LogWarning(string message) {
        lock (lockObj) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{name}] {message}");
            Console.ResetColor();
        }
    }
}
