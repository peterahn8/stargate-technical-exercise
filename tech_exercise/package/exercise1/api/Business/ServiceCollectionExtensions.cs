using MediatR;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Logging;

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
            services.AddLogging();
            services.AddScoped<IProcessLogWriter, ProcessLogWriter>();
            // Registered before AddMediatR, which adds its own pre-processor step: pipeline steps run in
            // registration order, so logging wraps everything, including validation (D17).
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProcessLoggingBehavior<,>));

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
