using SampleMVCSolution.DataAccessLayer.ExchangeResources;
using SampleMVCSolution.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace SampleMVCSolution.DataAccessLayer
{
    /* ExchangeContext is a UnitOfWork, so it is a collection of Repositories.
     * `Exchanges` DbSet is a repo backed by a db. It is initialized by a DbContext constructor.
    **/
    public class ExchangeContext : DbContext
    {
        private ExchangeBackedByExternalResourcesRepository exchangesBackedRepository;
        public ExchangeContext() : base("ExchangeContext")
        {}

        public ExchangeBackedByExternalResourcesRepository ExchangeRepository
        {
            get {
                if (this.exchangesBackedRepository == null)
                    this.exchangesBackedRepository = new ExchangeBackedByExternalResourcesRepository(this);
                return this.exchangesBackedRepository;
            }
        }

        private DbSet<Exchange> Exchanges { get; set; } // This property is required by `this.Set<Exchange>.Add` so it's not just another property for convinience of a client.
        
    }
}