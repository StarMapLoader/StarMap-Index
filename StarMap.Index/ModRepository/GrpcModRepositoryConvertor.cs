using Google.Protobuf;

namespace StarMapIndex.ModRepository
{
    public static class GrpcModRepositoryConvertor
    {
        public static StarMap.Index.API.Mod ToModProto(this Mod mod)
        {
            return new StarMap.Index.API.Mod
            {
                Id = mod.Id.ToString(),
                Name = mod.Name,
                Version = "",
            };
        }

        public static StarMap.Index.API.ModDetails ToModDetailsProto(this Mod mod)
        {
            var modDetails = new StarMap.Index.API.ModDetails
            {
                Mod = new StarMap.Index.API.Mod
                {
                    Id = mod.Id.ToString(),
                    Name = mod.Name,
                    Version = "",
                },
                Description = mod.Description ?? ""
            };
            modDetails.Versions.AddRange(mod.Versions.Select(v => v.Version));
            return modDetails;
        }
    }
}
