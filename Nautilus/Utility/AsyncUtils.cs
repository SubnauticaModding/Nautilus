using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Nautilus.Utility;

/// <summary>
/// Utilities that make it easier to work with async methods in the context of Unity.
/// </summary>
public static class AsyncUtils
{
    /// <summary>
    /// Takes an async <see cref="Task"/> and returns an <see cref="IEnumerator"/> that only continues when the task
    /// has completed (successfully or unsuccessfully). Can be used to run an async method as part of a
    /// <see cref="Coroutine"/>.
    /// </summary>
    /// <param name="task">The async method to wait for.</param>
    /// <returns>An IEnumerator for use in e.g. a coroutine.</returns>
    public static IEnumerator WaitUntilTaskComplete(Task task)
    {
        yield return new WaitUntil(() => task.IsCompleted);
    }
}