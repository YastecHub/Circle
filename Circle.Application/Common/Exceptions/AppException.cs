namespace Circle.Application.Common.Exceptions;

public sealed class AppException(string message) : Exception(message);
