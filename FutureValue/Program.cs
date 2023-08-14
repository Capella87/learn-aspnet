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
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Register a custom middleware that runs at the start of the pipeline.
// app.Use()

// Route matching middleware to run after the custom middleware.
app.UseRouting();

// The endpoint registered with MapGet
// MapGet adds a RouteEndPoint to the IEndpointRouteBuilder that matches HTTP GET requests
// app.MapGet("/", () => "Hello Pearl Abyss!"); // It returns a string..

app.UseAuthorization();

// MapControllerRoute adds endpoints for controller actions to the IEndpointRouteBuilder and
// specifies a route with the given name, pattern, defaults, constraints and dataTokens.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Running the WebApplication object using Generic Host.
app.Run();
