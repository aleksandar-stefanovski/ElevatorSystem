using ElevatorSystem;
using ElevatorSystem.Domain.Interfaces;
using ElevatorSystem.Presentation;
using ElevatorSystem.Repository;
using ElevatorSystem.Services;
using ElevatorSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton<IElevatorRepository, ElevatorRepositoryMock>()
    .AddSingleton<IElevatorService, ElevatorService>()
    .AddSingleton<IElevatorDisplay, ElevatorDisplay>()
    .AddSingleton<IElevatorMover, ElevatorMover>()
    .AddSingleton<IRequestProducer, RequestProducer>()
    .AddHostedService<ElevatorRequestWorker>();

var app = builder.Build();
await app.RunAsync();