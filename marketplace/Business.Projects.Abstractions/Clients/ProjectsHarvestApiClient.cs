﻿using Flurl.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Projects.Abstractions.Clients
{
    public class ProjectsHarvestApiClient
    {
        const string harvestId = "239876";
        const string harvestDeveloperToken = "1602045.pt.veF3cqkrn-dcj-ko7w_CX4XQXbq0jRwomksp9SVL9Xh9S7q2gjG-idaAiDHCep7sUPZLqI_GRT9smwlEovsKrw";

        public async Task<IEnumerable<dynamic>> GetProjects(int pageNumber = 1)
        {
            var harvestApiUrl = $"https://api.harvestapp.com/v2/projects?page={pageNumber}";
            var data = await GetData(harvestApiUrl);
            var projects = (data as IDictionary<string, object>)["projects"] as IEnumerable<dynamic>;

            return projects;
        }

        private async Task<dynamic> GetData(string url)
            => await url
            .WithOAuthBearerToken(harvestDeveloperToken)
            .WithHeader("Harvest-Account-ID", harvestId)
            .WithHeader("User-Agent", "blazor")
            .GetJsonAsync();

        private IEnumerable<string> GetColumnNames(IEnumerable<dynamic> records)
           => (records.First() as IDictionary<string, object>).Keys;
    }
}
