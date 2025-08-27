namespace GigaSharp;

public class Program
{
    static WebApplication app;
    public static bool shutdownApproved = false;
    public static void Main(string[] args)
    {
        // Hey reader
        // You're probably looking for the MasterProcess class, not this.
        // Don't mess with this function unless you're really sure of it.

        MasterProcess.RunMasterProcess();

        while(!shutdownApproved){
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            MasterProcess.StartBots();

            app.Run();
        }
    }

    public static void Shutdown(){
        shutdownApproved = true;
        Console.WriteLine("---- VERIFIED SHUTDOWN REQUEST RECEIVED ----");
        app.StopAsync();
    }
}
