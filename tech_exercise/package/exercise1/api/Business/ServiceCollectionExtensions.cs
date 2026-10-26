using StargateAPI.Business.Commands;

namespace StargateAPI.Business
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers MediatR with the request pipeline (pre-processors, handlers). Shared by the API
        /// and the tests, so a missing registration fails in both rather than only in production.
        /// </summary>
        public static IServiceCollection AddStargateMediatR(this IServiceCollection services)
        {
            return services.AddMediatR(cfg =>
            {
                cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
                cfg.AddRequestPreProcessor<UpdatePersonPreProcessor>();
                cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
                cfg.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly);
            });
        }
    }
}
