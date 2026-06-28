namespace Opengate.Modules.Shared.Utils.Providers.ResilientProviders;

/// <summary>
/// Allow call multiple services, when one fails, the next one is called
/// </summary>
public static class ResilientProviderExtension
{
    /// <summary>
    /// Retrieves all registered services of type T.
    /// </summary>
    /// <typeparam name="T">The type of the service to retrieve.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <param name="availableServices">The list of available services.</param>
    /// <param name="shuffle">Whether to shuffle the list of available services.</param>
    /// <returns>An enumerable of the retrieved services.</returns>
    public static IEnumerable<T> GetAvailableServices<T>(
        this IServiceProvider sp,
        List<string> availableServices,
        bool shuffle = false)
    {
        List<string> servicesToIterate;

        if (shuffle)
        {
            servicesToIterate = availableServices
                .OrderBy(x => Guid.NewGuid())
                .ToList();
        }
        else
        {
            servicesToIterate = availableServices;
        }

        foreach (var sender in servicesToIterate)
        {
            var adapter = sp.GetKeyedService<T>(sender);

            if (adapter is null)
            {
                continue;
            }

            yield return adapter;
        }
    }

    /// <summary>
    /// Calls a specified action on the first available Provider
    /// </summary>
    /// <typeparam name="T">The type of the Provider service.</typeparam>
    /// <param name="sp"></param>
    /// <param name="action">The action to perform on the Provider.</param>
    /// <param name="availableServices">The list of available services.</param>
    /// <param name="shuffle">Whether to shuffle the list of available services.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Fin{Unit}"/> which is either success or failure.</returns>
    public static async Task<Unit> CallResilientService<T>(
        this IServiceProvider sp,
        Func<T, Task> action,
        List<string> availableServices,
        bool shuffle = false)
        => (await CallResilientServiceFin(sp, action, availableServices, shuffle)).ThrowIfFail();

    /// <summary>
    /// Calls a specified action on the first available Provider
    /// </summary>
    /// <typeparam name="T">The type of the Provider service.</typeparam>
    /// <param name="sp"></param>
    /// <param name="action">The action to perform on the Provider.</param>
    /// <param name="availableServices">The list of available services.</param>
    /// <param name="shuffle">Whether to shuffle the list of available services.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Fin{Unit}"/> which is either success or failure.</returns>
    public static Task<Fin<Unit>> CallResilientServiceFin<T>(
        this IServiceProvider sp,
        Func<T, Task> action,
        List<string> availableServices,
        bool shuffle = false)
        => CallResilientService<T, Unit>(sp, async sender =>
        {
            await action(sender);
            return unit;
        }, availableServices, shuffle);

    /// <summary>
    /// Calls a specified function on the first available provider service and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the provider service.</typeparam>
    /// <typeparam name="TOut">The return type of the function.</typeparam>
    /// <param name="sp"></param>
    /// <param name="action">The function to perform on the provider service.</param>
    /// <param name="availableServices">The list of available services.</param>
    /// <param name="shuffle">Whether to shuffle the list of available services.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Fin{TOut}"/> which is either the result of the function or a failure.</returns>
    public static Task<Fin<TOut>> CallResilientService<T, TOut>(
        this IServiceProvider sp,
        Func<T, Task<TOut>> action,
        List<string> availableServices,
        bool shuffle = false)
    {
        async Task<Fin<TOut>> Action(T t)
            => await Aff(async () => await action(t)).Run();

        var res = CallResilientServiceError<T, TOut>(
            sp,
            Action,
            availableServices,
            shuffle);

        return res;
    }

    /// <summary>
    /// Calls a specified function on the first available provider service and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the provider service.</typeparam>
    /// <typeparam name="TOut">The return type of the function.</typeparam>
    /// <param name="sp"></param>
    /// <param name="action">The function to perform on the provider service.</param>
    /// <param name="availableServices">The list of available services.</param>
    /// <param name="shuffle">Whether to shuffle the list of available services.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="Fin{TOut}"/> which is either the result of the function or a failure.</returns>
    public static async Task<Fin<TOut>> CallResilientServiceError<T, TOut>(
        this IServiceProvider sp,
        Func<T, Task<Fin<TOut>>> action,
        List<string> availableServices,
        bool shuffle = false)
        => await Aff(async () =>
        {
            var exceptions = new List<Exception>();

            foreach (var sender in GetAvailableServices<T>(sp, availableServices, shuffle))
            {
                var res = await action(sender);

                if (res.IsSucc)
                {
                    return res.ThrowIfFail();
                }

                var err = (Error)res;

                exceptions.Add(err.ToException());
            }

            if (exceptions.Count == 0)
            {
                throw new Exception("No providers found");
            }

            throw new AggregateException(exceptions);
        }).Run();
}