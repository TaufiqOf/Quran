using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search;

public interface ISearch
{
    bool GetSearchMode(string searchText);
    Task InitializeAsync();
    Task<List<SurahResult>> PerformSearch(string searchText, CancellationToken cancellationToken = default);
}