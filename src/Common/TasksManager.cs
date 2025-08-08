namespace Common;

/// <summary>
/// Provides thread-safe mechanism for effective management of tasks pool.
/// </summary>
public abstract class TasksManager : IDisposable
{
    #region Properties
    private readonly List<Task> _managedTasks;
    #endregion

    #region Instantiation
    protected TasksManager()
    {
        _managedTasks = new List<Task>();
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Adds provided task to pool of managed tasks.
    /// </summary>
    /// <remarks>
    /// Additionally whenever this method is being invoked, completed tasks are removed
    /// from pool of managed tasks. It is simple yet effective mechanism of lazy-management of the pool.
    /// </remarks>
    /// <param name="task">
    /// Task, which shall be added to pool of managed event tasks.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    protected void AddTask(Task task)
    {
        #region Arguments validation
        if (task is null)
        {
            string argumentName = nameof(task);
            const string ErrorMessage = "Provided event task is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        lock (_managedTasks)
        {
            List<Task> compleatedTasks = _managedTasks.Where(managedTask => managedTask.IsCompleted).ToList();
            compleatedTasks.ForEach(completedTask => _managedTasks.Remove(completedTask));

            _managedTasks.Add(task);
        }
    }

    /// <summary>
    /// Waits for completion of every task present in managed pool. 
    /// </summary>
    public virtual void Dispose()
    {
        Task.WaitAll(_managedTasks);

        lock (_managedTasks)
        {
            _managedTasks.Clear();
        }
    }
    #endregion
}
