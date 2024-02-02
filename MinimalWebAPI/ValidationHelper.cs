using System.Reflection;

public static class ValidationHelper
{
    internal static async ValueTask<object?> ValidateId(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next) // Second one is the next endpoint filter or endpoint middleware. It is a delegate type.
    {
        // Retrieve the method arguments from the context
        var id = context.GetArgument<string>(0);

        // Return error response if filtered
        if (string.IsNullOrEmpty(id) || !id.StartsWith('f'))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    {"id", new[]{"Invalid format. Id must start with 'f'"}}
                });
        }

        // Invoke next one if there's no trouble.
        return await next(context);
    }

    internal static EndpointFilterDelegate ValidateIdFactory(EndpointFilterFactoryContext context,
        EndpointFilterDelegate next)
    {
        // Getting parameter information with reflection
        ParameterInfo[] parameters = context.MethodInfo.GetParameters();

        int? idPosition = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            // Find the parameter named id with string type (string id)
            if (parameters[i].Name == "id" &&
                parameters[i].ParameterType == typeof(string))
            {
                idPosition = i;
                break;
            }
        }

        // Not adding the filter if the condition did not meet.
        // Then, return to the pipeline.
        if (!idPosition.HasValue)
        {
            return next;
        }

        // Return a filter function. (Equivalent to AddEndpointFilter parameter and the above method in this class)
        return async (invocationContext) =>
        {
            var id = invocationContext.GetArgument<string>(idPosition.Value);

            if (string.IsNullOrEmpty (id) || !id.StartsWith('f'))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    {"id", new[]{"Id must start with 'f'"}}
                });
            }

            return await next(invocationContext);
        };
    }
}
