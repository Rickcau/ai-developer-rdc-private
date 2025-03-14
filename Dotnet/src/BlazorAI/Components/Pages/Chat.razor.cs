using Azure;
using Azure.Search.Documents.Indexes;
using BlazorAI.Plugins;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.TextToImage;
using System.Net.Sockets;

#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BlazorAI.Components.Pages;

public partial class Chat
{
    private ChatHistory? chatHistory;
    private Kernel? kernel;
    // RDC Added
    private OpenAIPromptExecutionSettings _openAIPromptExecutionSettings = new()
    {
        ChatSystemPrompt = "You're a virtual assistant that helps people find information. Ask followup questions if something is unclear or more data is needed to complete a task",
        Temperature = 0.9, // Set the temperature to 0.9
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()  // Auto invoke kernel functions
    };

    [Inject]
    public required IConfiguration Configuration { get; set; }
    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = null!;

    protected async Task InitializeSemanticKernel()
    {
        chatHistory = [];

        // Challenge 02 - Configure Semantic Kernel
        var kernelBuilder = Kernel.CreateBuilder();

        // Challenge 02 - Add OpenAI Chat Completion
        kernelBuilder.AddAzureOpenAIChatCompletion(
            Configuration["AOI_DEPLOYMODEL"]!,
            Configuration["AOI_ENDPOINT"]!,
            Configuration["AOI_API_KEY"]!);

        // Add Logger for Kernel
        kernelBuilder.Services.AddSingleton(LoggerFactory);

        // Challenge 03 and 04 - Services Required
        kernelBuilder.Services.AddHttpClient();

        // Challenge 05 - Register Azure AI Foundry Text Embeddings Generation
        kernelBuilder.Services.AddAzureOpenAITextEmbeddingGeneration(
            deploymentName: Configuration["EMBEDDINGS_DEPLOYMODEL"]!, 
            endpoint: Configuration["AOI_ENDPOINT"]!,           
            apiKey: Configuration["AOI_API_KEY"]!);

        // Challenge 05 - Register Search Index

        kernelBuilder.Services.AddSingleton(
                        _ => new SearchIndexClient(
                            new Uri(Configuration["AI_SEARCH_URL"]!),
                            new AzureKeyCredential(Configuration["AI_SEARCH_KEY"]!)));
        kernelBuilder.AddAzureAISearchVectorStore();

        // Challenge 07 - Add Azure AI Foundry Text To Image
        kernelBuilder.AddAzureOpenAITextToImage(
          Configuration["DALLE_DEPLOYMODEL"]!,
          Configuration["AOI_ENDPOINT"]!,
          Configuration["AOI_API_KEY"]!);

        // Challenge 02 - Finalize Kernel Builder
        kernel = kernelBuilder.Build();

        // Challenge 03, 04, 05, & 07 - Add Plugins
        await AddPlugins();

        // Challenge 02 - Chat Completion Service


        // Challenge 03 - Create OpenAIPromptExecutionSettings
        // I did this private variable
        // _openAIPromptExecutionSettings and in SendMessage we use it there.


    }


    private async Task AddPlugins()
    {
        // Challenge 03 - Add Time Plugin
        kernel!.Plugins.AddFromObject(new TimePlugin(Configuration),"TimePlugin");
        kernel!.Plugins.AddFromObject(new GeocodingPlugin(kernel.Services.GetRequiredService<IHttpClientFactory>(), Configuration), "GeocodingPlugin");
        kernel!.Plugins.AddFromObject(new WeatherPlugin(kernel.Services.GetRequiredService<IHttpClientFactory>(), Configuration), "WeatherPlugin");

        // Challenge 04 - Import OpenAPI Spec
        await kernel.ImportPluginFromOpenApiAsync(
           pluginName: "WorkItemPlugin",
           uri: new Uri("http://localhost:5115/swagger/v1/swagger.json"),
           executionParameters: new OpenApiFunctionExecutionParameters()
           {
               // Determines whether payload parameter names are augmented with namespaces.
               // Namespaces prevent naming conflicts by adding the parent parameter name
               // as a prefix, separated by dots
               EnablePayloadNamespacing = true
           }
        );

        // Challenge 05 - Add Search Plugin
        kernel.Plugins.AddFromType<ContosoSearchPlugin>("ConsotoSearchPlugin", kernel.Services);

        // Challenge 07 - Text To Image Plugin
        var plugin = new TextToImagePlugin(kernel.GetRequiredService<ITextToImageService>());
        kernel.Plugins.AddFromObject(plugin, "TextToImagePlugin");

    }

    private async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(newMessage) && chatHistory != null)
        {
            // This tells Blazor the UI is going to be updated.
            StateHasChanged();
            loading = true;
            // Copy the user message to a local variable and clear the newMessage field in the UI
            var userMessage = newMessage;
            newMessage = string.Empty;
            StateHasChanged();

            // Start Challenge 02 - Sending a message to the chat completion service

            // Your code goes here
            chatHistory.AddUserMessage(userMessage);
            var chatCompletionService = kernel!.GetRequiredService<IChatCompletionService>();
            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: _openAIPromptExecutionSettings,
                kernel: kernel
            );

            string? content = response.Content;
            chatHistory.AddAssistantMessage(content!);
            // End Challenge 02 - Sending a message to the chat completion service

            loading = false;
        }
    }
}
