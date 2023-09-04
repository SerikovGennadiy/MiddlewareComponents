var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();



// MW должен или отправлять ответ или передавать обработку дальше
// вызов Response.WriteAsync() и затем передача управления в след MW
// где изменяется ответ, след-но, состав HTTP объекта ответв (нарушается протокол HTTP)
// закомментить context.Response.statusCode = 200 - ошибок нет. Но лучше после отправки ответа обработку запроса пректратить!!!

//public static IApplicationBuilder Use(this IApplicationBuilder app, Func<HttpContext,
//Func<Task>, Task> middleware);
app.Use(async (context, next) =>
{
    Console.WriteLine("Logging before execution next delegate in the Use method");
    //await context.Response.WriteAsync("Hello from middleware component in USE");
    await next.Invoke();
    Console.WriteLine("Loggin after executing next delegate in the Use method");
});

//public static IApplicationBuilder Map(this IApplicationBuilder app, PathString
//pathMatch, Action<IApplicationBuilder> configuration)
app.Map("/usebranchmap", builder =>
{
    builder.Use(async (context, next) =>
    {
        Console.WriteLine("Mapping answer before next.Invoke in Branch Use method");
        await next.Invoke();
        Console.WriteLine("Mapping answer after next.Invoke in Branch Use method");
    });
    builder.Run(async (context) =>
    {
        Console.WriteLine("Response from Branch Run method");
        await context.Response.WriteAsync("Response to client agent from Branch RUN method");
    });
    // even Run method won't be here, the pipeline stop here. This line like a terminal. Further handling i'll go back.
});

//public static IApplicationBuilder MapWhen(this IApplicationBuilder app,
//Func<HttpContext, bool> predicate, Action<IApplicationBuilder> configuration)
app.MapWhen(context => context.Request.Query.ContainsKey("testmapwhere"), builder =>
{
    builder.Run(async (context) =>
    {
        await context.Response.WriteAsync("Hello from Map When Middleware component!");
    });
});

// accepts as parametr public delegate Task RequestDelegate(HttpContext context);
app.Run(async (context) =>
{
    Console.WriteLine("Writing response to the client from the Run method");
   // context.Response.StatusCode = 200;
    await context.Response.WriteAsync("Hello from middleware component in RUN");
});

app.MapControllers();

app.Run();
