using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwaitSample
{
    internal class AsyncExceptionTester
    {
        internal async void ThrowExceptionAsyncVoid()
        {
            throw new InvalidOperationException();
        }

        internal void AsyncVoidExceptionsCannotBeCaught()
        {
            try
            {
                this.ThrowExceptionAsyncVoid();
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Exception is never caught here." +
                    Environment.NewLine +
                    "It is passed to the thread pooled SynchronizationContext where it is never caught" +
                    Environment.NewLine +
                    "and the Console App terminates.");
            }
        }

        internal async Task SomeAsync()
        {
            await Task.Delay(2000);
            Console.WriteLine("Some Async completed.");
            throw new InvalidOperationException();
        }

        internal async Task ThrowExceptionAsyncTask()
        {
            throw new InvalidOperationException();
        }

        internal async Task AsyncTaskExceptionCaught()
        {
            try
            {
                await this.ThrowExceptionAsyncTask();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception of ThrowExceptionAsyncTask caught here.");                
            }
        }


        /// <summary>
        /// Does some asynchronous task on console main. Task.Wait() is
        /// only allowed on Console Applications because Console Main can't
        /// be 'async' and a Console Application has a threadpool SynchronizationContext.
        /// Continuation of awaited Task continues on another thread of the threadpool
        /// and not the one with is blocked through task.Wait().
        /// 
        /// If using that in a GUI or ASP.NET Application same code would produce
        /// a deadlock. 
        /// In GUI and ASP.NET there is a SynchronizationContext on the
        /// UI Thread which allows only to run one chunk of code at a time(Single Threaded). 
        /// 
        /// If calling task.Wait() the UI Thread is blocked. If the awaited
        /// Task finishes it can't obtain the SynchronizationContext because the UI Thread
        /// in it was blocked with task.Wait(). 
        /// 
        /// So the task.Wait() is waiting for the Task to finish and the finished task 
        /// is waiting for the SynchronizationContext to be unblocked. 
        /// Deadlock!!!
        /// </summary>
        internal void DoSomeAsyncTaskOnConsoleMain()
        {
            Task task = this.SomeAsync();
            task.Wait();
        }

        internal async Task DoSomeAction(int taskId)
        {
            try
            {
                await this.ActionTask(taskId, part: 1);

                await this.ActionTask(taskId, part: 2);

            }
            catch (Exception e)
            {
                Console.WriteLine("TRY BLOCK: " + e.Message);                
            }
            finally
            {
                // Would deadlock in Service, GUI ?
                this.ActionFinallyTask(taskId, 3).Wait();
            }

            Console.WriteLine("Task '{0}' completed.", taskId);
        }

        internal async Task ActionTask(int taskId, int part)
        {
            await Task.Delay(2000);

            // simulate an exception state within that async method in its try block.
            if (part == 2)
            {
                throw new InvalidOperationException(
                    string.Format("Task '{0}' part '{1}' throw an exception.", taskId, part));
            }
        }

        internal async Task ActionFinallyTask(int taskId, int part)
        {
            try
            {
                await Task.Delay(2000).ConfigureAwait(false);

                if (taskId % 2 == 0)
                {
                    // simulate an exception state within that async method in callers finally block.
                    throw new InvalidOperationException(
                        string.Format("Task '{0}' part '{1}' throw an exception.", taskId, part));
                }

                Console.WriteLine("Task '{0}' part '{1}' completed...", taskId, part);
            }
            catch (Exception e)
            {
                Console.WriteLine("FINALLY BLOCK: " + e.Message);
            }
        }



    }
}
