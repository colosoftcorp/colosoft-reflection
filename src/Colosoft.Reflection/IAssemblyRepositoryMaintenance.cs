using System;

namespace Colosoft.Reflection
{
    public interface IAssemblyRepositoryMaintenance : IDisposable
    {
        string Name { get; }

        AssemblyRepositoryMaintenanceExecuteResult Execute();
    }
}
