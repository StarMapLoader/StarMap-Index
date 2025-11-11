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
                Author = mod.Author.DisplayName
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
                    Author = mod.Author.DisplayName
                },
                Description = mod.Description ?? ""
            };

            return modDetails;
        }

        public static StarMap.Index.API.ModVersion ToProto(this ModVersion version)
        {
            return new StarMap.Index.API.ModVersion
            {
                Id = version.Id.ToString(),
                Version = version.Version,
                DownloadLocation = version.DownloadUrl
            };
        }
    }
}
