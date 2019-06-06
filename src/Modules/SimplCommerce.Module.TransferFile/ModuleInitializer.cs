using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using SimplCommerce.Infrastructure.Modules;
using SimplCommerce.Module.Core.Services;
/****************************************************************
*   Author：L
*   Time：2019/5/29 14:14:45
*   FrameVersion：2.0
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Module.TransferFile
{
    public class ModuleInitializer : IModuleInitializer
    {

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITransferFileService, TransferFileService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

        }
    }
}
