namespace borealis_flowers.api.Features.Images;

public static class ImagesModule
{
    public static void ImagesEndpointsRegistration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMethods("/user-images/{imageName}", new[] { "HEAD" }, ImagesHandler.CheckImageExists()).WithTags("Images");
        endpoints.MapGet("/user-images", ImagesHandler.GetAllImages()).WithTags("Images");
        endpoints.MapGet("/user-images/{imageName}/file", ImagesHandler.GetImageFile()).WithTags("Images");
        endpoints.MapGet("/user-images/{imageName}/base64", ImagesHandler.GetImageBase64()).WithTags("Images");
        endpoints.MapPost("/user-images", ImagesHandler.UploadImage()).WithTags("Images").DisableAntiforgery();
        endpoints.MapDelete("/user-images", ImagesHandler.DeleteImage()).WithTags("Images");
    }
}
