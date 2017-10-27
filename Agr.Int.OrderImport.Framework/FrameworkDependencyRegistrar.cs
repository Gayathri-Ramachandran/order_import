using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using Heliar.Composition.Core;
using Agr.Int.OrderImport.Framework.Settings;

namespace Agr.Int.OrderImport.Framework
{
    public class FrameworkDependencyRegistrar : ILibraryDependencyRegistrar
    {
        public void Register(RegistrationBuilder registrations, AggregateCatalog catalog)
        {
            registrations.ForTypesMatching(t => t.Name.EndsWith("Service") &&
                                           t.Namespace.StartsWith(typeof(FrameworkDependencyRegistrar).Namespace))
                .SetCreationPolicy(CreationPolicy.Shared)
                .ExportInterfaces()
                .Export();
            registrations.ForType<Import>()
                .AddMetadata("ApplicationScoped", true)
                .SetCreationPolicy(CreationPolicy.NonShared)
                .Export();
            registrations.ForType<AppSettings>()
                .AddMetadata("ApplicationScoped", true)
                .SetCreationPolicy(CreationPolicy.NonShared)
                .ExportInterfaces()
                .Export();
        }
    }
}
