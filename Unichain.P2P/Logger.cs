using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P; 
public class Logger{

    private string name;

    public Logger(string name = "") {
        this.name = name;
    }

    [DebuggerStepThrough]
    public void Log(string message) {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{name}] {message}");
        Console.ResetColor();
    }

    [DebuggerStepThrough]
    public void LogError(string message) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"[{name}] {message}");
        Console.ResetColor();
    }

    [DebuggerStepThrough]
    public void LogWarning(string message) {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{name}] {message}");
        Console.ResetColor();
    }
}
