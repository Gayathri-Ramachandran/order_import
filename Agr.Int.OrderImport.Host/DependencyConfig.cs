using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Heliar.Composition.Core;

namespace Agr.Int.OrderImport
{
    [ExcludeFromCodeCoverage]
    internal static class DependencyConfig
    {
        public static CompositionContainer Configure()
        {
            var bootstrapper = new ContainerBootstrapper();
            return bootstrapper.Bootstrap();
        }
    }
}
