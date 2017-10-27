using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;

using Heliar.Composition.Core;
using Agr.Int.OrderImport.Common.Data.Sql;

namespace Agr.Int.Orderimport.Data.Access
{
        /// <summary>
        /// Class DataAccessDependencyRegistrar
        /// </summary>
        /// /// <seealso cref="Heliar.Composition.Core.ILibraryDependencyRegistrar" />
        public class DataAccessDependencyRegistrar : ILibraryDependencyRegistrar
        {
            /// <summary>
            /// Registers required types within the Data project. 
            /// </summary>
            /// <param name="registrations"></param>
            /// <param name="catalog"></param>
            public void Register(RegistrationBuilder registrations, AggregateCatalog catalog)
            {
                //Implement DataAccessDependencyRegistrar: register library/services if needed

                registrations.ForTypesMatching(t => t.Name.EndsWith("Repository") &&
                                                   t.Namespace.StartsWith(typeof(DataAccessDependencyRegistrar).Namespace))
                   .SetCreationPolicy(CreationPolicy.Shared)
                   .ExportInterfaces()
                   .Export();

            registrations.ForType<SqlConnectionFactory>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .ExportInterfaces()
                .Export();

            registrations.ForType<ResourceLoader>()
                .SetCreationPolicy(CreationPolicy.Shared)
                .ExportInterfaces()
                .Export();
        }
        }
}
