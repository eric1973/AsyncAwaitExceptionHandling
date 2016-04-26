using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitSample
{

    /*
     * Related documents: 
     * 
     * Best Practices in Asynchronous Programming
     * https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
     * 
     * Await, SynchronizationContext, and Console Apps
     * http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx
     * 
     * */
    class Program
    {
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AsyncExceptionTester asyncTester = new AsyncExceptionTester();

            /*
             * 1. RULE: NEVER compose a method the has a signature of 'async void'!
             * Use 'async Task' instead. 'async void' is intended to make async event
             * handlers possible.
             * 
             * The exception is caught in the AppDomain UnhandleException handler because
             * the 'async' method returns 'void'. So the exception can not be captured in 
             * a 'Task' and goes up the current SynchronizationContext.
             * */

            //try
            //{
            //    asyncTester.AsyncVoidExceptionsCannotBeCaught();
            //}
            //catch (Exception e)
            //{

            //    Console.WriteLine("Async Void Exception not caught here, too.");
            //}


            try
            {
                asyncTester.AsyncTaskExceptionCaught();
            }
            catch (Exception e)
            {

                Console.WriteLine("Async Task Exception not caught here. It is caught in invoked method.");
            }


            try
            {
                asyncTester.DoSomeAsyncTaskOnConsoleMain();
            }
            catch (Exception e)
            {

                Console.WriteLine("Async Task Exception caught here.");
            }


            SomeAsyncAction(/*async*/ (taskId) =>
                            {
                                //try
                                //{
                                    Console.WriteLine("ActionTask '{0}' - before started.", taskId);
                                    /*await*/ asyncTester.DoSomeAction(taskId);
                                    Console.WriteLine("ActionTask '{0}' - after started.", taskId);
                                //}
                                //catch (Exception e)
                                //{
                                //    // Catch Exception from 'finally' block in DoSomeAction.
                                //    Console.WriteLine(e.Message);
                                //    if (e.InnerException != null)
                                //    {
                                //        Console.WriteLine(e.InnerException.Message);
                                //    }
                                //}
                                
                            });

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Main Thread doing other work ...");
            }


            // Fire and Forget the 'async' method ComputeSomeAsync just for fun
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ComputeSomeAsync(async (number) =>
             {
                 int doubled = number * 2;
                 await Task.Delay(500* doubled); // simulate external I/O operation
                 Console.WriteLine($"In continuation of calculateFunc for number:{doubled} in thread:{Thread.CurrentThread.ManagedThreadId}");
                 return doubled;
             });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed


            Console.WriteLine($"Main Thread other work finished. Waiting for ComputeSomeAsync to complete in thread:{Thread.CurrentThread.ManagedThreadId}");
            Console.ReadLine();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        internal static void SomeAsyncAction(Action<int> action)
        {
            // invoke a few times async method
            for (int i = 0; i < 10; i++)
            {
                action(i);
            }
        }

        private static async Task ComputeSomeAsync(Func<int, Task<int>> calculateFunc)
        {
            for(int i=0;i < 20;i++)
            {
                int result = await calculateFunc(i);
                Console.WriteLine($"In continuation of ComputeSomeAsync for result:{ result} in thread:{Thread.CurrentThread.ManagedThreadId}");               
            }
        } 

        static async Task DemoAsync()
        {
            var d = new Dictionary<int, int>();
            for (int i = 0; i < 10000; i++)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int count;
                d[id] = d.TryGetValue(id, out count) ? count + 1 : 1;

                await Task.Yield();
            }
            foreach (var pair in d) Console.WriteLine(pair);
        }
    }
}
