using System;

namespace Pluton.PlutonUI
{
    public interface IComponent
    {
        string type { get; }
        JSON.Object obj { get; }
    }
}

