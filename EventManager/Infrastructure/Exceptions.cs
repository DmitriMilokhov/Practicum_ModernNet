namespace EventManager.Infrastructure;

public class NotFoundException(string message) : Exception(message);
