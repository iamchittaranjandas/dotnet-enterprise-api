namespace DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore
{
    /// <summary>
    /// Reciprocal Rank Fusion over two ranked lists (vector + keyword).
    /// RRF score = Σ 1 / (k + rank_i) where k=60 is the standard constant.
    /// Higher RRF score = more relevant.
    /// </summary>
    internal static class HybridRanker
    {
        private const int K = 60;

        /// <summary>
        /// Combines a vector-ranked list and a keyword-ranked list into a single
        /// hybrid-ranked list via Reciprocal Rank Fusion.
        /// </summary>
        /// <typeparam name="TId">Identifier type (e.g. int for TaskId).</typeparam>
        /// <param name="vectorRanked">Items ordered by descending cosine similarity.</param>
        /// <param name="keywordRanked">Items ordered by descending BM25 score.</param>
        /// <param name="getId">Extracts the identifier from an item.</param>
        /// <param name="topK">Max results to return.</param>
        public static IReadOnlyList<(T Item, float RrfScore)> Fuse<T, TId>(
            IEnumerable<T> vectorRanked,
            IEnumerable<T> keywordRanked,
            Func<T, TId> getId,
            int topK)
            where TId : notnull
        {
            var scores = new Dictionary<TId, float>();
            var items  = new Dictionary<TId, T>();

            var rank = 1;
            foreach (var item in vectorRanked)
            {
                var id = getId(item);
                scores[id]  = scores.GetValueOrDefault(id) + 1f / (K + rank);
                items[id]   = item;
                rank++;
            }

            rank = 1;
            foreach (var item in keywordRanked)
            {
                var id = getId(item);
                scores[id]  = scores.GetValueOrDefault(id) + 1f / (K + rank);
                items[id]   = item;
                rank++;
            }

            return scores
                .OrderByDescending(kv => kv.Value)
                .Take(topK)
                .Select(kv => (items[kv.Key], kv.Value))
                .ToList();
        }
    }

    /// <summary>
    /// Lightweight BM25 scorer over a collection of documents.
    /// Uses k1=1.5, b=0.75 — standard BM25 defaults.
    /// Operates purely in C# on a pre-loaded corpus — no external dependency.
    /// </summary>
    internal class Bm25Scorer<T>
    {
        private const float K1 = 1.5f;
        private const float B  = 0.75f;

        private readonly IReadOnlyList<(T Item, string[] Terms)> _corpus;
        private readonly float _avgDocLen;
        private readonly Dictionary<string, int> _df; // document frequency per term

        public Bm25Scorer(IEnumerable<T> items, Func<T, string> getText)
        {
            var tokenised = items
                .Select(item => (Item: item, Terms: Tokenise(getText(item))))
                .ToList();

            _corpus    = tokenised;
            _avgDocLen = _corpus.Count == 0 ? 1f
                : (float)_corpus.Average(d => d.Terms.Length);

            _df = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var (_, terms) in _corpus)
            {
                foreach (var term in terms.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    _df[term] = _df.GetValueOrDefault(term) + 1;
                }
            }
        }

        /// <summary>Scores every document against <paramref name="query"/> and returns
        /// them ordered by descending BM25 score (zero-score items excluded).</summary>
        public IEnumerable<(T Item, float Score)> Score(string query)
        {
            if (_corpus.Count == 0)
                yield break;

            var queryTerms = Tokenise(query);
            var n          = _corpus.Count;

            foreach (var (item, terms) in _corpus)
            {
                var docLen = terms.Length;
                var score  = 0f;

                foreach (var qt in queryTerms)
                {
                    if (!_df.TryGetValue(qt, out var df))
                        continue;

                    var tf = terms.Count(t => t.Equals(qt, StringComparison.OrdinalIgnoreCase));
                    if (tf == 0) continue;

                    // IDF component (with smoothing)
                    var idf = MathF.Log((n - df + 0.5f) / (df + 0.5f) + 1f);

                    // TF component
                    var tfNorm = (tf * (K1 + 1f))
                                 / (tf + K1 * (1f - B + B * docLen / _avgDocLen));

                    score += idf * tfNorm;
                }

                if (score > 0f)
                    yield return (item, score);
            }
        }

        private static string[] Tokenise(string text) =>
            text.ToLowerInvariant()
                .Split([' ', '\t', '\n', '\r', '.', ',', '!', '?', '-', '_', '/', '\\'],
                    StringSplitOptions.RemoveEmptyEntries);
    }
}
