using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P; 

/// <summary>
/// A class that represents a route and contains a collecion of routes
/// </summary>
public class Route {

    /// <summary>
    /// The path of the route
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Creates a new route
    /// </summary>
    /// <param name="path">The path for this route</param>
    private Route(string path) {
        Path = path;
    }

    #region Statics

    public static Route Root => "";
    public static Route Peers => "peers";
    public static Route Peers_Join => Peers / "join";
    public static Route Broadcast => "broadcast";

    #endregion

    #region Overloads

    public static Route operator /(Route route, string path) {
        return new Route($"{route.Path}/{path}");
    }

    public static Route operator /(Route route, Route path) {
        return new Route($"{route.Path}/{path.Path}");
    }

    public static bool operator ==(Route route, string path) {
        return route.Path == path;
    }

    public static bool operator !=(Route route, string path) {
        return route.Path != path;
    }

    public static implicit operator Route(string path) {
        return new Route(path);
    }

    public static implicit operator string(Route route) {
        return route.Path;
    }

    public override string ToString() {
        return Path;
    }

    public override bool Equals(object? obj) {
        return obj is Route route &&
               Path == route.Path;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Path);
    }

    #endregion
}
