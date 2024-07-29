# Weather report agent example

This example demos how to invoke tool call with local model using Ollama and AutoGen.Net

## Prerequisites
- Ollama >= 0.3.0
- dotnet sdk >= 8.0

## How to run
1. Clone this repository
2. start `ollama` server. Suppose the server is running at `http://localhost:11434`
3. run `dotnet run` in this directory. And you will see the output like below:

```

TextMessage from assistant
--------------------
The current temperature in New York is 25°C (77°F), and it's a sunny day.
--------------------
```

## Further reading
- [Ollama](https://ollama.com/)
- [AutoGen.Net](https://microsoft.github.io/autogen-for-net/index.html)