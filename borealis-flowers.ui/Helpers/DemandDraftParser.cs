namespace borealis_flowers.ui.Helpers;

using borealis_flowers.ui.Models;

public static class DemandDraftParser
{
    public static List<FlowerDemandLine> ParseDraft(string? raw)
    {
        List<FlowerDemandLine> list = [];

        if (string.IsNullOrWhiteSpace(raw))
            return list;

        string normalized = raw.Replace('\r', '\n');

        IEnumerable<string> pieces = normalized
            .Split([';', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string fragment in pieces)
        {
            string[] tokens =
                fragment.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (tokens.Length < 2)
                continue;

            if (!int.TryParse(tokens[^1], out int qty))
                continue;

            string name = string.Join(' ', tokens[..^1]).Trim();

            if (name.Length == 0)
                continue;

            list.Add(new FlowerDemandLine { FlowerName = name, Quantity = Math.Max(qty, 1) });
        }

        return list;
    }
}
