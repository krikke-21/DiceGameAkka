﻿using Akka.Actor;
using DiceGame.Akka.GameEngine.Actor;
using DiceGame.Akka.GameEngine.Infrastructure;
using DiceGame.Akka.Infrastructure.Akka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace DiceGame.Akka.GameEngine
{
    public class Startup
    {
        private const string GameEngineAllowSpecificOrigins = "_gameEngineSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy(GameEngineAllowSpecificOrigins,
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameEngine", Version = "v1" });
            });

            services.AddSingleton(_ => ConfigureActorSystem());

            services.AddSingleton<GameManagerActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();
                var gameManagerActor = actorSystem.ActorOf(GameManagerActor.GetProps());
                return () => gameManagerActor;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(GameEngineAllowSpecificOrigins);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });


            lifetime.ApplicationStarted.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>(); // start Akka.NET
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>()?.Terminate().Wait();
            });
        }

        private static ActorSystem ConfigureActorSystem()
        {
            return ActorSystem.Create("DiceGameSystem", ConfigurationLoader.Load());
        }
    }
}