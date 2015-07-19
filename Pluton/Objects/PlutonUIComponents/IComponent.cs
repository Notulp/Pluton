using System;

namespace Pluton.PlutonUIPanel
{
    public interface IComponent
    {
        string type { get; }
        JSON.Object obj { get; }
    }
}

