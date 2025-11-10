using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarMap.Index.API
{
    public interface IModRespositoryClient : IDisposable
    {
        Task<Mod[]> GetMods();
        Task<ModDetails?> GetModDetails(Guid id);
        Task<string> GetModDownloadLocation(Mod mod);
    }

    public class ModRepositoryClient : IModRespositoryClient
    {
        private GrpcChannel _channel;
        private ModRepositoryService.ModRepositoryServiceClient _client;
        //Force build
        public ModRepositoryClient(string repositoryUrl)
        {
            _channel = GrpcChannel.ForAddress(repositoryUrl);
            _client = new(_channel);
        }

        public async Task<Mod[]> GetMods()
        {
            var request = new GetModsRequest();
            var response = await _client.GetModsAsync(request);
            return response.Mods.ToArray();
        }

        public async Task<ModDetails?> GetModDetails(Guid id)
        {
            var request = new GetModDetailsRequest()
            {
                Id = id.ToString()
            };
            var response = await _client.GetModDetailsAsync(request);
            return response.Mod;
        }

        public Task<string> GetModDownloadLocation(Mod mod)
        {
            return Task.FromResult("");
        }

        public void Dispose()
        {
            _channel.Dispose();
        }


    }
}
