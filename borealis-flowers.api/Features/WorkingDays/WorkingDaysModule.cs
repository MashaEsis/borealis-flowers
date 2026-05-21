namespace borealis_flowers.api.Features.WorkingDays;

public static class WorkingDaysModule
{
    public static void WorkingDaysEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/workingdays", WorkingDaysHandler.GetWorkingDays()).WithTags("WorkingDays");
        endpoints.MapGet("/workingdays/master/{id}", WorkingDaysHandler.GetWorkingDaysForMonthRange()).WithTags("WorkingDays");
        endpoints.MapPost("/workingdays", WorkingDaysHandler.CreateWorkingDays()).WithTags("WorkingDays");
        endpoints.MapPut("/workingdays", WorkingDaysHandler.UpdateWorkingDays()).WithTags("WorkingDays");
        endpoints.MapPut("/workingdays-availability", WorkingDaysHandler.UpdateWorkingDaysAvailability()).WithTags("WorkingDays");
        endpoints.MapPut("/workingdays-soft", WorkingDaysHandler.SoftUpdateWorkingDays()).WithTags("WorkingDays");
    }
}
