using System.Collections.Generic;
using Quran.Models;

namespace Quran.Helpers.Search;

public interface ISearch
{
    bool GetSearchMode(string searchText);
    List<Surah> PerformSearch(string searchText);
}