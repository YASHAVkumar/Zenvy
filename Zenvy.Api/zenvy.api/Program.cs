using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using zenvy.application;
using zenvy.api.Infrastructure;
using zenvy.infrastructure;
using zenvy.shared.Reponses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ApiResponseFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .ToDictionary(
                    item => item.Key,
                    item => item.Value!.Errors.Select(error => error.ErrorMessage).ToArray());
            return new BadRequestObjectResult(ApiResponse<object>.Failed(
                "One or more validation errors occurred.",
                context.HttpContext.TraceIdentifier,
                errors));
        };
    });

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Zenvy API",
        Version = "v1"
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Zenvy API",
        Version = "v2"
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        return apiDesc.GroupName == docName;
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(
        document => new OpenApiSecurityRequirement 
        { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Zenvy API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Zenvy API v2");

        options.RoutePrefix = "swagger";
        options.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

app.UseStatusCodePages(async statusContext =>
{
    var response = statusContext.HttpContext.Response;
    if (response.ContentLength is > 0 || !string.IsNullOrEmpty(response.ContentType)) return;

    var message = response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => "Authentication is required.",
        StatusCodes.Status403Forbidden => "You are not authorized to perform this action.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        _ => "The request could not be completed."
    };
    response.ContentType = "application/json";
    await response.WriteAsJsonAsync(ApiResponse<object>.Failed(
        message,
        statusContext.HttpContext.TraceIdentifier));
});

app.UseMiddleware<ApiAuditMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
