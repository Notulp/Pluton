
namespace Pluton
{
    public interface ISingleton
    {
        bool CheckDependencies();

        void Initialize();
    }
}

