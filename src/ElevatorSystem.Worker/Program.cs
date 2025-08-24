using ElevatorSystem.Worker;
using ElevatorSystem.Worker.Domain.Interfaces;
using ElevatorSystem.Worker.Presentation;
using ElevatorSystem.Worker.Repository;
using ElevatorSystem.Worker.Services;
using ElevatorSystem.Worker.Services.Interfaces;
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