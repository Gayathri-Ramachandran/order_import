using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using Heliar.Composition.Core;

namespace Agr.Int.OrderImport
{
        public class HostDependencyRegistrar : IApplicationDependencyRegistrar
        {
            public void Register(RegistrationBuilder registrations, AggregateCatalog catalog)
            {
                registrations.ForType<OrderImportRunner>()
                    .SetCreationPolicy(CreationPolicy.Shared)
                    .Export();
            }
        }
}
