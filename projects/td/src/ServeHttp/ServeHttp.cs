using System.Net;
using System.Text;

namespace gdt.projects.td.ServeHttp;

public class ServeHttp {
	private static HttpListener _listener;

	public static async Task HandleIncomingConnections() {
		bool runServer = true;
		
		while (runServer) {
			var ctx = await _listener.GetContextAsync();
			var req = ctx.Request;
			var res = ctx.Response;
			
			if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown")) {
				Console.WriteLine("Shutdown requested");
				runServer = false;
			}
			
			string disableSubmit = !runServer ? "disabled" : "";
			byte[] data = Encoding.UTF8.GetBytes(State.ToTxt());
			res.ContentType = "text/html";
			res.ContentEncoding = Encoding.UTF8;
			res.ContentLength64 = data.LongLength;

			
			await res.OutputStream.WriteAsync(data, 0, data.Length);
			res.Close();
		}
	}

	public static void Run() {
		_listener = new HttpListener() {
			Prefixes = {
				"http://localhost:1234/",
			},
		};
		_listener.Start();
		Log.LastCall(nameof(ServeHttp), "http://localhost:1234/");
		Task listenTask = HandleIncomingConnections();
		listenTask.GetAwaiter().GetResult();
		
		_listener.Close();
	}
}
