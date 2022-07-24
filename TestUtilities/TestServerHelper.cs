using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace YadaYadaSoftware.TestUtilities;

public class TestServerHelper
{
    public static TestServer GetServer<TTestStartup,TProductionStartup>([CallerMemberName] string testName = null) where TTestStartup : class where TProductionStartup : class
    {

        var builder = new WebHostBuilder()
                .UseEnvironment($"Testing-{testName}")
                .UseStartup<TTestStartup>()
                .UseSetting(WebHostDefaults.ApplicationKey, typeof(TProductionStartup).GetTypeInfo().Assembly.GetName().Name) // Set the "right" application after the startup's been registerted
                .ConfigureAppConfiguration(z=>z.AddJsonFile("appsettings.json"))
            //.ConfigureTestServices(collection => collection.AddSingleton<IAmazonS3>(new MockAmazonS3()))
            ;

        return new TestServer(builder);
    }
}