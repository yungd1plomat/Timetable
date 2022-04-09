using Microsoft.EntityFrameworkCore;
using Timetable.BotCore.Abstractions;
using Timetable.Helpers;
using Timetable.BotCore;
using Timetable.Models;

namespace Timetable
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(); // Для парсинга json
            services.AddEndpointsApiExplorer(); // Для Swagger
            services.AddSwaggerGen(); // Для Swagger

            using (DatabaseContext context = new DatabaseContext())
            {
                context.Database.Migrate(); // Мигрируем
                var user = context.Users.Where(x => x.UserId == 137778129).FirstOrDefault();
                if (user == null)
                {
                    context.Users.Add(new BotUser()
                    {
                        UserId = 137778129,
                        admin = true,
                        Subscribtion = DateTime.MaxValue,
                    });
                } else
                {
                    user.admin = true;
                    user.Subscribtion = DateTime.MaxValue;
                }
                context.SaveChanges();
            }

            services.AddSingleton<IVkBot, Bot>(sp => new Bot(Configuration["Access_token"])); // Создаем экземпляр бота (1 раз)

            QiwiPayment.Secret = Configuration["QiwiSecret"];
            QiwiPayment.Number = Configuration["QiwiNumber"];
            QiwiPayment.ThemeCode = Configuration["QiwiTheme"];
            QiwiPayment.OauthToken = Configuration["QiwiOauth"];
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) // Для отладки
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => // Используем контроллеры
            {
                endpoints.MapControllers();
            });
        }
    }
}
