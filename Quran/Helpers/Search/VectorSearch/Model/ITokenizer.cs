namespace Quran.Helpers.Search.VectorSearch.Model;

public interface ITokenizer
{
    TokenizedInput Encode(
        string text,
        int maxLength);
}

