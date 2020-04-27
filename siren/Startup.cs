using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;

namespace CatSiren
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new SirenSettings();
            Configuration.Bind("Siren", config);
            services.AddSingleton(config);

            services.AddControllers();

            services.AddHostedService<MotionHost>();

            services.AddSingleton<IMotionSiren, MotionSiren>();

            bool isTargetPlatform = true;

#if DEBUG
            isTargetPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif

            if (isTargetPlatform)
            {
                services.AddSingleton<IGpioService, GpioService>();
                services.AddSingleton<ICameraService, CameraService>();
            }
            else
            {
                services.AddSingleton<IGpioService, SimulatedGpioService>();
                services.AddSingleton<ICameraService, SimulatedCameraService>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
