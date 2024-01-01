namespace MediatR.Application.Exceptions
{
    public class ModelPropertyException
    {
        public string PropertyName { get; set; } = null!;
        public string PropertyErrorMessage { get; set; } = null!;
    }

    public class ModelStateException : Exception
    {
        public ModelPropertyException[] Errors { get; private set; }

        public ModelStateException(ModelPropertyException[] errors)
        {
            Errors = errors;
        }
    }
}
