namespace Colosoft.Reflection
{
    public interface IAssemblyPackageValidator
    {
        bool[] Validate(IAssemblyPackage[] assemblyPackages);
    }
}
