using assetlen.Shared.Models.Models.ViewModels.RemoteSiteDtos;

namespace assetlen.Shared.Components.Media;

/// <summary>How one markup layer is drawn: its marks, and whether it is the question, someone else's, or a draft.</summary>
public sealed record MarkupLayerView(IReadOnlyList<AnnotationShapeDto> Shapes, MarkupTone Tone);

public enum MarkupTone
{
    /// <summary>A question on the register — the accent, used sparingly.</summary>
    Question,

    /// <summary>Anyone else's layer: present, but it does not shout.</summary>
    Other,

    /// <summary>What the reader is drawing now, not yet saved.</summary>
    Draft
}
