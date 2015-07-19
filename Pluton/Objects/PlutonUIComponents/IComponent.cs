using System;

namespace Pluton.PlutonUI
{
    public interface IComponent
    {
        float fadeIn { get; }
        JSON.Object obj { get; }
        string type { get; }
    }
}

