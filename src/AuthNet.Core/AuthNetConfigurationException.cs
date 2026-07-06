namespace AuthNet.Core;

public sealed class AuthNetConfigurationException(string message) : InvalidOperationException(message);

