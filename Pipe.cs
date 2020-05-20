using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

using Smod2.Events;

namespace EventManager
{
    public class Pipe
    {
        public static string PipeServerName { private set; get; }

        public async Task StartThread()
        {
            //Task.Delay is because server port is setting late
            await Task.Delay(5000);
            PipeServerName = "EventManagerPipe_" + PluginHandler.Shared.Server.Port;

            NamedPipeServerStream pipeServer =
                new NamedPipeServerStream("testpipe", PipeDirection.In);
            await pipeServer.WaitForConnectionAsync();
            while(pipeServer.IsConnected)
            {
                byte[] buffer = new byte[1024];
                await pipeServer.ReadAsync(buffer, 0, buffer.Length);
                PluginHandler.Shared.ExecuteCommand(Encoding.UTF8.GetString(buffer));
            }
        }
    }
}
