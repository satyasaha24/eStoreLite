﻿namespace CatalogService;

public static class CatalogApi
{
    public static RouteGroupBuilder MapCatalogApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/catalog");

        group.WithTags("Catalog");

        group.MapGet("items/type/all/brand/{catalogBrandId?}", async (int? catalogBrandId, CatalogDbContext catalogContext, int? before, int? after, int pageSize = 10) =>
        {
            var itemsOnPage = await catalogContext.GetCatalogItemsAsync(catalogBrandId, before, after, pageSize);

            var (firstId, lastId) = itemsOnPage switch
            {
                [] => (0, 0),
                [var only] => (only.Id, only.Id),
                [var first, .., var last] => (first.Id, last.Id)
            };

            return new Catalog(
                firstId,
                lastId,
                itemsOnPage.Count < pageSize,
                itemsOnPage.Take(pageSize));
        });

        group.MapGet("items/{catalogItemId:int}/image", async (int catalogItemId, CatalogDbContext catalogDbContext, IHostEnvironment environment) =>
        {
            var item = await catalogDbContext.CatalogItems.FindAsync(catalogItemId);

            if (item is null)
            {
                return Results.NotFound();
            }

            var path = Path.Combine(environment.ContentRootPath, "Images", item.PictureFileName);

            if (!File.Exists(path))
            {
                return Results.NotFound();
            }

            return Results.File(path, "image/jpeg");
        })
        .Produces(404)
        .Produces(200, contentType: "image/jpeg");

        return group;
    }
}