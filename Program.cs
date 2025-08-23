namespace GigaSharp;

public class Program
{
    public static void Main(string[] args)
    {
        // Hey reader
        // You're probably looking for the MasterProcess class, not this.
        // Don't mess with this function unless you're really sure of it.

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

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

        //This is the line that actually matters!!!
        //Go see the MasterProcess to see what it does.
        _ = MasterProcess.StartBots();

        //WARNING: THIS LINE BLOCKS THE THREAD UNTIL SHUTDOWN
        //DO NOT PLACE CODE AFTER THIS LINE
        app.Run();
    }

    public static void Shutdown(){
        Environment.Exit(0);
    }
}
