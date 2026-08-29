using System.Collections.Generic;
using System.Threading.Tasks;
using Quran.Models;

namespace Quran.Helpers.Search;

public interface ISearch
{
    bool GetSearchMode(string searchText);
    Task InitializeAsync();
    List<Surah> PerformSearch(string searchText);
}