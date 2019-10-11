using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTM
{
    class RemoteTasks
    {

        static void Main(string[] args)
        {
            var remoteds = new DATA.Remote();
            
            var adapter = new DATA.RemoteTableAdapters.TableAdapterManager();
            adapter.RemoteSourceTableAdapter = new DATA.RemoteTableAdapters.RemoteSourceTableAdapter();
            adapter.RemoteTaskGrupTableAdapter = new DATA.RemoteTableAdapters.RemoteTaskGrupTableAdapter();
            adapter.RemoteTasksTableAdapter = new DATA.RemoteTableAdapters.RemoteTasksTableAdapter();

            //adapter.RemoteTasksTableAdapter.Fill(remoteds.RemoteTasks);
            //adapter.RemoteTaskGrupTableAdapter.Fill(remoteds.RemoteTaskGrup);
            var remoteresource_id = adapter.RemoteSourceTableAdapter.GetIdByName("GOOGLETASKS");

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                           new ClientSecrets
                           {
                               ClientId = "917646541628-duv6g0pjfpmd5tkd89g6nv5han1grutv.apps.googleusercontent.com",
                               ClientSecret = "MkouuBsAZh0HNS-6RxM1cd7N",
                           },
                           new[] { TasksService.Scope.Tasks },
                           "user",
                           CancellationToken.None).Result;

            // Create the service.
            var service = new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Tasks API Sample",
            });
            // Define parameters of request.
            TasklistsResource.ListRequest listRequest = service.Tasklists.List();

            

            // List task lists.
            IList<TaskList> taskLists = listRequest.Execute().Items;
            Console.WriteLine("Task Lists:");
            if (taskLists != null && taskLists.Count > 0)
            {
                foreach (var taskList in taskLists)
                {   
                    adapter.RemoteTaskGrupTableAdapter.Insert(taskList.Title, false, taskList.Id, remoteresource_id, taskList.ETag);
                    var ss = service.Tasks.List(taskList.Id);
                    ss.ShowCompleted = true;
                    ss.ShowHidden = true;
                    Tasks tasks = ss.Execute();
                    if (tasks.Items != null)
                    {
                        foreach (var task in tasks.Items)
                        {
                            adapter.RemoteTasksTableAdapter.Insert(task.ETag, task.Title, null, task.Id, false, remoteresource_id, task.Completed, task.Notes);

                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No task lists found.");
            }
            Console.Read();

        }
    }
}
