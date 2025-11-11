

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
            var mods = await _db.Mods.Include(m => m.Author).Select(mod => mod.ToModProto()).ToListAsync();

            var response = new GetModsResponse();
            response.Mods.AddRange(mods);
            return response;
        }

        public async override Task<GetModDetailsResponse> GetModDetails(GetModDetailsRequest request, ServerCallContext context)
        {
            var mod = await _db.Mods.Include(m => m.Author).Where(mod => mod.Id == Guid.Parse(request.Id)).FirstOrDefaultAsync();

            if (mod == null)
            {
                return new GetModDetailsResponse();
            }

            var versions = await _db.Versions
                .Where(v => v.ModId == mod.Id)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var protoMod = mod.ToModDetailsProto();
            protoMod.Versions.AddRange(versions.Select(v => v.ToProto()));

            return new GetModDetailsResponse()
            {
                Mod = protoMod
            };
        }
    }
}
