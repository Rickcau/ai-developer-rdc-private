using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BlazorAI.Components.Pages
{
    public partial class MultiAgent
    {
        private ChatHistory? chatHistory;
        private IChatCompletionService? chatCompletionService;
        private OpenAIPromptExecutionSettings? openAIPromptExecutionSettings;
        private Kernel? kernel;

        [Inject]
        public required IConfiguration Configuration { get; set; }
        [Inject]
        private ILoggerFactory LoggerFactory { get; set; } = null!;

        private List<ChatCompletionAgent> Agents { get; set; } = [];

        private AgentGroupChat? AgentGroupChat;


        protected void InitializeSemanticKernel()
        {
            chatHistory = [];

            var kernelBuilder = Kernel.CreateBuilder();

            kernelBuilder.AddAzureOpenAIChatCompletion(
                Configuration["AOI_DEPLOYMODEL"] ?? "gpt-35-turbo",
                Configuration["AOI_ENDPOINT"]!,
                Configuration["AOI_API_KEY"]!);

            kernelBuilder.AddAzureOpenAITextToImage(
                Configuration["DALLE_DEPLOYMODEL"]!,
                Configuration["AOI_ENDPOINT"]!,
                Configuration["AOI_API_KEY"]!);

            kernelBuilder.Services.AddSingleton(LoggerFactory);

            kernel = kernelBuilder.Build();

            AddPlugins();

            CreateAgents();

            // create Agent Group Chat
            AgentGroupChat = new AgentGroupChat(Agents.ToArray())
            {
                ExecutionSettings = new()
                {
                    TerminationStrategy = new ApprovalTerminationStrategy()
                    {
                        Agents = [Agents.Last()],
                        MaximumIterations = 6,
                        AutomaticReset = true
                    }
                }
            };
        }

        private void CreateAgents()
        {
            if (kernel is null)
            {
                throw new InvalidOperationException("Kernel must be initialized before creating agents.");
            }
            // Create a Business Analyst Agent
            Agents.Add(new ChatCompletionAgent()
            {
                Instructions = """
				                  You are a Business Analyst which will take the requirements from the user (also known as a 'customer')
				                  and create a project plan for creating the requested app. The Business Analyst understands the user
				                  requirements and creates detailed documents with requirements and costing. The documents should be 
				                  usable by the SoftwareEngineer as a reference for implementing the required features, and by the 
				                  Product Owner for reference to determine if the application delivered by the Software Engineer meets
				                  all of the user's requirements.
				               """,
                Name = "BusinessAnalyst", // Do not use spaces!
                Kernel = kernel,
            });

            // Create a Software Engineer Agent
            Agents.Add(new ChatCompletionAgent()
            {
                Instructions = """
				                   You are a Software Engineer, and your goal is create a web app using HTML and JavaScript
				                   by taking into consideration all the requirements given by the Business Analyst. The application should
				                   implement all the requested features. Deliver the code to the Product Owner for review when completed.
				                   You can also ask questions of the BusinessAnalyst to clarify any requirements that are unclear.
				               """,
                Name = "SoftwareEngineer",
                Kernel = kernel,
            });

            // Create a Product Owner Agent
            Agents.Add(new ChatCompletionAgent()
            {
                Instructions = """
				               You are the Product Owner which will review the software engineer's code to ensure all user 
				               requirements are completed. You are the guardian of quality, ensuring the final product meets
				               all specifications and receives the green light for release. Once all client requirements are
				               completed, you can approve the request by just responding "approve". Do not ask any other agent
				               or the user for approval. If there are missing features, you will need to send a request back
				               to the SoftwareEngineer or BusinessAnalyst with details of the defect. To approve, respond with
				               the token %APPR%.
				               """,
                Name = "ProductOwner",
                Kernel = kernel,
            });
        }

        private void AddPlugins()
        {

        }

        private async Task SendMessage()
        {
            if (AgentGroupChat is null)
            {
                throw new InvalidOperationException("AgentGroupChat must be initialized before sending messages.");
            }

            // Copy the message from the user input - just like in Chat.razor.cs
            var userMessage = MessageInput;
            MessageInput = string.Empty;
            loading = true;
            chatHistory!.AddUserMessage(userMessage);
            StateHasChanged();

            // Add a new ChatMessageContent to the AgentGroupChat with the User role, and userMessage contents
            AgentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userMessage));
            StateHasChanged();

            try
            {
                // Use async foreach to iterate over the messages from the AgentGroupChat
                await foreach (var message in AgentGroupChat.InvokeAsync())
                {
                    // Add each message to the chatHistory
                    chatHistory.Add(message);
                    StateHasChanged();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while trying to send message to agents.");
            }

            loading = false;
        }
    }

    sealed class ApprovalTerminationStrategy : TerminationStrategy
    {
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken = default)
            => Task.FromResult(history[^1].Content?.Contains("%APPR%", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
