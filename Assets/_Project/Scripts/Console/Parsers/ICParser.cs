using System;

namespace Mahou.Debugging
{
    public interface ICParser
    {
        int Priority { get; }
        bool CanParse(Type type);
        object Parse(string value, Type type, Func<string, Type, object> recursiveParser);
    }
}