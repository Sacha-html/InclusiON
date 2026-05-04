using InclusiON.Application.Interfaces.Infrastructure;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace InclusiON.SemanticSearch.Services;

/// <summary>
/// Servicio de embeddings usando paraphrase-multilingual-MiniLM-L12-v2 (384 dims).
/// Tokenización: SentencePieceTokenizer (LlamaTokenizer.Create) con el archivo .model nativo.
/// Inferencia: Microsoft.ML.OnnxRuntime con mean-pooling + normalización L2.
/// </summary>
public sealed class OnnxEmbeddingService : IEmbeddingService, IDisposable
{
    private const int MaxSequenceLength = 128;

    private readonly InferenceSession _session;
    private readonly SentencePieceTokenizer _tokenizer;

    public OnnxEmbeddingService(string modelPath, string sentencePieceModelPath)
    {
        _session = new InferenceSession(modelPath);

        using var stream = File.OpenRead(sentencePieceModelPath);
        // LlamaTokenizer.Create es la fábrica de SentencePieceTokenizer en Microsoft.ML.Tokenizers 0.22.x
        // addBeginningOfSentence=true  → agrega <s> (id=0 en XLM-RoBERTa)
        // addEndOfSentence=true        → agrega </s>
        // Create(Stream, bool addBeginningOfSentence, bool addEndOfSentence, IReadOnlyDictionary? specialTokens)
        _tokenizer = LlamaTokenizer.Create(stream, true, true, null);
    }

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // 1. Tokenizar (incluye <s> y </s> automáticamente por la config del tokenizador)
        var rawIds = _tokenizer.EncodeToIds(
            text,
            considerNormalization:    true,
            considerPreTokenization:  true,
            addBeginningOfSentence:   true,
            addEndOfSentence:         true);

        // 2. Truncar a MaxSequenceLength
        int seqLen = Math.Min(rawIds.Count, MaxSequenceLength);

        var inputIds      = new long[seqLen];
        var attentionMask = new long[seqLen];
        var tokenTypeIds  = new long[seqLen];   // siempre 0 para single sentence

        for (int i = 0; i < seqLen; i++)
        {
            inputIds[i]      = rawIds[i];
            attentionMask[i] = 1L;
            // tokenTypeIds ya inicializado en 0
        }

        // 3. Crear tensores ONNX [batch=1, seqLen]
        var dims = new[] { 1, seqLen };
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids",
                new DenseTensor<long>(inputIds, dims)),
            NamedOnnxValue.CreateFromTensor("attention_mask",
                new DenseTensor<long>(attentionMask, dims)),
            NamedOnnxValue.CreateFromTensor("token_type_ids",
                new DenseTensor<long>(tokenTypeIds, dims)),
        };

        // 4. Inferencia ONNX
        using var results  = _session.Run(inputs);
        var outputMap      = results.ToDictionary(r => r.Name);

        float[] embedding;

        // Algunos exports de sentence-transformers ya incluyen sentence_embedding (pooled + normalized)
        if (outputMap.TryGetValue("sentence_embedding", out var sentEmb))
        {
            embedding = [.. sentEmb.AsEnumerable<float>()];
        }
        else
        {
            // Mean pooling sobre last_hidden_state [1, seqLen, hiddenDim]
            var hiddenOut = outputMap.TryGetValue("last_hidden_state", out var lhs) ? lhs
                          : outputMap.TryGetValue("token_embeddings",  out var te)  ? te
                          : results[0];

            var hidden    = hiddenOut.AsTensor<float>();
            int hiddenDim = hidden.Dimensions[2];
            embedding     = new float[hiddenDim];
            int maskSum   = 0;

            for (int i = 0; i < seqLen; i++)
            {
                if (attentionMask[i] == 0) continue;
                maskSum++;
                for (int j = 0; j < hiddenDim; j++)
                    embedding[j] += hidden[0, i, j];
            }

            if (maskSum > 0)
                for (int j = 0; j < hiddenDim; j++)
                    embedding[j] /= maskSum;
        }

        // 5. Normalización L2
        float norm = MathF.Sqrt(embedding.Sum(x => x * x));
        if (norm > 1e-9f)
            for (int i = 0; i < embedding.Length; i++)
                embedding[i] /= norm;

        return Task.FromResult(embedding);
    }

    public void Dispose() => _session.Dispose();
}
