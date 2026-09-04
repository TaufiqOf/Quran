namespace Quran.Helpers.Search.VectorSearch.Model;

public interface ITokenizer
{
    long PadTokenId { get; set; }

    TokenizedInput Encode(
        string text,
        int maxLength);
}