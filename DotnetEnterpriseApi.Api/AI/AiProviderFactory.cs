using Anthropic;
using Google.GenAI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace DotnetEnterpriseApi.Api.AI
{
    /// <summary>
    /// Builds an <see cref="IChatClient"/> and optionally an
    /// <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/> from configuration.
    ///
    /// Supported providers (set AI:Provider in appsettings):
    ///   openai      — OpenAI (api.openai.com)
    ///   claude      — Anthropic Claude
    ///   gemini      — Google Gemini
    ///   groq        — Groq (OpenAI-compatible)
    ///   openrouter  — OpenRouter (OpenAI-compatible)
    ///   nvidia      — NVIDIA NIM (OpenAI-compatible)
    ///   grok        — xAI Grok (OpenAI-compatible)
    /// </summary>
    public static class AiProviderFactory
    {
        private const string ProviderKey = "AI:Provider";

        // OpenAI-compatible provider endpoints
        private static readonly Dictionary<string, string> CompatEndpoints = new(StringComparer.OrdinalIgnoreCase)
        {
            ["groq"]       = "https://api.groq.com/openai/v1",
            ["openrouter"] = "https://openrouter.ai/api/v1",
            ["nvidia"]     = "https://integrate.api.nvidia.com/v1",
            ["grok"]       = "https://api.x.ai/v1",
        };

        public static IChatClient CreateChatClient(IConfiguration configuration)
        {
            var provider = configuration[ProviderKey] ?? "openai";

            return provider.ToLowerInvariant() switch
            {
                "claude"     => CreateClaude(configuration),
                "gemini"     => CreateGemini(configuration),
                "openai"     => CreateOpenAiCompat(configuration, "openai"),
                "groq"       => CreateOpenAiCompat(configuration, "groq"),
                "openrouter" => CreateOpenAiCompat(configuration, "openrouter"),
                "nvidia"     => CreateOpenAiCompat(configuration, "nvidia"),
                "grok"       => CreateOpenAiCompat(configuration, "grok"),
                var p        => throw new InvalidOperationException(
                    $"Unsupported AI provider '{p}'. Valid values: openai, claude, gemini, groq, openrouter, nvidia, grok"),
            };
        }

        public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(IConfiguration configuration)
        {
            var provider = configuration[ProviderKey] ?? "openai";

            // Only OpenAI-compatible providers support text embeddings via this interface.
            // Claude and Gemini do not expose embeddings through their chat SDKs.
            return provider.ToLowerInvariant() switch
            {
                "openai"     => CreateOpenAiEmbedding(configuration, "openai"),
                "nvidia"     => CreateOpenAiEmbedding(configuration, "nvidia"),
                "openrouter" => CreateOpenAiEmbedding(configuration, "openrouter"),
                "claude" or "gemini" or "groq" or "grok" =>
                    // Fall back to OpenAI embeddings when the chat provider doesn't support them.
                    // Requires AI:OpenAI:ApiKey and AI:OpenAI:EmbeddingModelId to be set.
                    CreateOpenAiEmbedding(configuration, "openai"),
                var p => throw new InvalidOperationException(
                    $"Unsupported AI provider '{p}' for embeddings."),
            };
        }

        // ── OpenAI-compatible (OpenAI, Groq, OpenRouter, NVIDIA NIM, xAI Grok) ──

        private static IChatClient CreateOpenAiCompat(IConfiguration configuration, string provider)
        {
            var section = GetSection(configuration, provider);
            var apiKey  = Require(section, "ApiKey", provider);
            var modelId = section["ModelId"] ?? DefaultModel(provider);

            var options = new OpenAIClientOptions();
            if (CompatEndpoints.TryGetValue(provider, out var endpoint))
                options.Endpoint = new Uri(endpoint);

            return new OpenAIClient(new ApiKeyCredential(apiKey), options)
                .GetChatClient(modelId)
                .AsIChatClient();
        }

        private static IEmbeddingGenerator<string, Embedding<float>> CreateOpenAiEmbedding(
            IConfiguration configuration, string provider)
        {
            var section        = GetSection(configuration, provider);
            var apiKey         = Require(section, "ApiKey", provider);
            var embeddingModel = section["EmbeddingModelId"] ?? "text-embedding-3-small";

            var options = new OpenAIClientOptions();
            if (CompatEndpoints.TryGetValue(provider, out var endpoint))
                options.Endpoint = new Uri(endpoint);

            return new OpenAIClient(new ApiKeyCredential(apiKey), options)
                .GetEmbeddingClient(embeddingModel)
                .AsIEmbeddingGenerator();
        }

        // ── Anthropic Claude ──

        private static IChatClient CreateClaude(IConfiguration configuration)
        {
            var section = GetSection(configuration, "claude");
            var apiKey  = Require(section, "ApiKey", "claude");
            var modelId = section["ModelId"] ?? "claude-sonnet-4-5";

            return new AnthropicClient(new Anthropic.Core.ClientOptions { ApiKey = apiKey })
                .AsIChatClient(modelId);
        }

        // ── Google Gemini ──

        private static IChatClient CreateGemini(IConfiguration configuration)
        {
            var section = GetSection(configuration, "gemini");
            var apiKey  = Require(section, "ApiKey", "gemini");
            var modelId = section["ModelId"] ?? "gemini-2.0-flash";

            return new Client(apiKey: apiKey).AsIChatClient(modelId);
        }

        // ── Helpers ──

        private static IConfigurationSection GetSection(IConfiguration configuration, string provider) =>
            configuration.GetSection($"AI:{Capitalise(provider)}");

        private static string Require(IConfigurationSection section, string key, string provider) =>
            section[key] ?? throw new InvalidOperationException(
                $"AI:{Capitalise(provider)}:{key} is not configured. Set it via user secrets or environment variables.");

        private static string Capitalise(string s) =>
            s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..].ToLowerInvariant();

        private static string DefaultModel(string provider) => provider.ToLowerInvariant() switch
        {
            "openai"     => "gpt-4o-mini",
            "groq"       => "llama-3.3-70b-versatile",
            "openrouter" => "openai/gpt-4o-mini",
            "nvidia"     => "meta/llama-3.1-70b-instruct",
            "grok"       => "grok-3-mini",
            _            => "gpt-4o-mini",
        };
    }
}
