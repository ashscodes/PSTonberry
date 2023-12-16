namespace EasyPSD;

public interface IPsdInlineComment
{
    CommentValue Comment { get; set; }

    bool HasInlineComment { get; }
}