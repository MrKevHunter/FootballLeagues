namespace FL.Mediator
{
    public interface IPreRequestHandler<in TRequest>
    {
        void Handle(TRequest request);
    }
}