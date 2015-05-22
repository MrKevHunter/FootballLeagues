using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FL.Commands;
using FL.Mediator;
using MediatR;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;

namespace FL.StructureMap
{
    public static class StructuremapRegistration
    {
        public static IContainer Populate(IEnumerable<ServiceDescriptor> descriptors, IConfiguration configuration)
        {
            var registry = new Registry();

            registry.For<IServiceScope>().Use<StructureMapServiceScope>();
            registry.For<IServiceProvider>().Use<StructureMapServiceProvider>();
            registry.For<IServiceScopeFactory>().Use<StructureMapServiceScopeFactory>();
            registry.For<IConfiguration>().Use(configuration);
            BuildBuildInRegistry(registry, descriptors);
            AddMediatr(registry);

            return new Container(registry);
        }

        private static void AddMediatr(Registry registry)
        {
            registry.Scan(scanner =>
            {
                scanner.AssemblyContainingType<IMediator>();
                scanner.AssemblyContainingType<AreaCommand>();
                scanner.WithDefaultConventions();
                scanner.ConnectImplementationsToTypesClosing(typeof (IRequestHandler<,>));
                scanner.ConnectImplementationsToTypesClosing(typeof (IAsyncRequestHandler<,>));
                scanner.ConnectImplementationsToTypesClosing(typeof (INotificationHandler<>));
                scanner.ConnectImplementationsToTypesClosing(typeof (IAsyncNotificationHandler<>));
            });

            registry.For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            registry.For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));

            registry.For(typeof (IRequestHandler<,>))
                .DecorateAllWith(typeof (MediatorPipeline<,>));

            var handlerType = registry.For(typeof (IRequestHandler<,>));
            handlerType.DecorateAllWith(typeof (ValidatorHandler<,>));
        }

        private static void BuildBuildInRegistry(Registry registry, IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    // Test if the an open generic type is being registered
                    var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        registry.For(descriptor.ServiceType).Use(descriptor.ImplementationType);
                    }
                    else
                    {
                        registry.For(descriptor.ServiceType)
                            .Use(descriptor.ImplementationType)
                            .SetLifecycleTo(LifetimeConvertor(descriptor.Lifetime));
                    }
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    registry.For(descriptor.ServiceType)
                        .Use($"Implementation Factory for {descriptor.ServiceType.Name}", context =>
                            descriptor.ImplementationFactory(context.GetInstance<IServiceProvider>()));
                }
                else
                {
                    registry.For(descriptor.ServiceType)
                        .Use(descriptor.ImplementationInstance);
                }
            }
        }

        private static LifecycleBase LifetimeConvertor(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return new SingletonLifecycle();
                case ServiceLifetime.Transient:
                case ServiceLifetime.Scoped:
                    return new TransientLifecycle();
            }
            return new TransientLifecycle();
        }

        public class ValidatorHandler<TRequest, TResponse>
            : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            private readonly IRequestHandler<TRequest, TResponse> _inner;
            private readonly IValidator<TRequest>[] _validators;

            public ValidatorHandler(IRequestHandler<TRequest, TResponse> inner,
                IValidator<TRequest>[] validators)
            {
                _inner = inner;
                _validators = validators;
            }

            public TResponse Handle(TRequest request)
            {
                var context = new ValidationContext(request);

                var failures = _validators
                    .Select(v => v.Validate(context))
                    .SelectMany(result => result.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Any())
                    throw new ValidationException(failures);

                return _inner.Handle(request);
            }
        }
    }
}