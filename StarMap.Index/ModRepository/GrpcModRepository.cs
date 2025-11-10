

using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StarMap.Index.API;
using StarMapIndex.ModRepository;

namespace StarMapIndex.Endpoints
{
    public class GrpcModRepository : ModRepositoryService.ModRepositoryServiceBase
    {
        private readonly AppDbContext _db;

        public GrpcModRepository(AppDbContext db)
        {
            _db = db;
        }

        public async override Task<GetModsResponse> GetMods(GetModsRequest request, ServerCallContext serverCallContext)
        {
            var mods = await _db.Mods.Select(mod => mod.ToModProto()).ToListAsync();

            var response = new GetModsResponse();
            response.Mods.AddRange(mods);
            return response;
        }

        public override Task<GetModDetailsResponse> GetModDetails(GetModDetailsRequest request, ServerCallContext context)
        {
            return base.GetModDetails(request, context);
        }

        public override Task<GetModDownloadLocationResponse> GetModDownloadLocation(GetModDownloadLocationRequest request, ServerCallContext context)
        {
            return base.GetModDownloadLocation(request, context);
        }
    }
}
